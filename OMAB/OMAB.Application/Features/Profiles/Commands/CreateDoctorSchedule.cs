using System;
using FluentValidation;
using MediatR;
using OMAB.Application.Cores;
using OMAB.Application.Interfaces;

namespace OMAB.Application.Features.Profiles.Commands;

public class CreateDoctorSchedule
{
    public record Command(DayOfWeek DayOfWeek, TimeSpan StartTime, TimeSpan EndTime, int SlotDurationInMinutes = 30) : IRequest<Result<Unit>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.DayOfWeek).IsInEnum().WithMessage("Invalid DayOfWeek value");
            RuleFor(x => x.StartTime)
                .LessThan(x => x.EndTime).WithMessage("StartTime must be earlier than EndTime");
            RuleFor(x => x.SlotDurationInMinutes)
                .GreaterThan(0).WithMessage("SlotDurationInMinutes must be greater than 0");
        }
    }

    public class Handler(IUserAccessor userAccessor, IDoctorRepository doctorRepository, IUnitOfWork unitOfWork) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            int? currentUserId = userAccessor.GetCurrentUserId();
            if (currentUserId == null)
                return Result<Unit>.Failure("User not authenticated", 401);
            var currentUserRole = userAccessor.GetCurrentUserRole();
            if (currentUserRole != Domain.Enums.UserRole.Doctor)
                return Result<Unit>.Failure("Only doctors can create schedules", 403);

            var doctor = await doctorRepository.GetByIdWithSchedulesAsync(currentUserId.Value, cancellationToken);

            if (doctor == null)
                return Result<Unit>.Failure("Doctor not found", 404);

            var addResult = doctor.AddSchedule(request.DayOfWeek, request.StartTime, request.EndTime, request.SlotDurationInMinutes);

            if (addResult != null && !addResult.IsSuccess)
                return Result<Unit>.Failure(addResult.ErrorMessage ?? "Failed to add schedule", 400);

            var result = await unitOfWork.SaveChangesAsync(cancellationToken);

            if (result <= 0) return Result<Unit>.Failure("Failed to create doctor schedule", 400); return Result<Unit>.Success(Unit.Value);
        }
    }
}
