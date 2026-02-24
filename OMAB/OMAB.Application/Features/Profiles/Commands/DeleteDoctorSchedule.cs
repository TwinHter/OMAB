using System;
using MediatR;
using OMAB.Application.Cores;
using OMAB.Application.Interfaces;
using OMAB.Domain.Entities;

namespace OMAB.Application.Features.Profiles.Commands;

public class DeleteDoctorSchedule
{
    public record Command(int Id) : IRequest<Result<Unit>>;

    public class Handler(IGenericRepository<DoctorSchedule> doctorScheduleRepository, IUserAccessor userAccessor, IUnitOfWork unitOfWork) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            var userId = userAccessor.GetCurrentUserId();
            var schedule = await doctorScheduleRepository.GetByIdAsync(request.Id, ct);
            if (schedule == null)
            {
                return Result<Unit>.Failure("Doctor schedule not found", 404);
            }
            if (userId == null || userId != schedule?.DoctorId)
            {
                return Result<Unit>.Failure("Unauthorized to delete this schedule", 403);
            }

            doctorScheduleRepository.Delete(schedule);
            var result = await unitOfWork.SaveChangesAsync(ct);
            if (result <= 0)
                return Result<Unit>.Failure("Failed to delete doctor schedule", 400);
            return Result<Unit>.Success(Unit.Value);
        }
    }
}
