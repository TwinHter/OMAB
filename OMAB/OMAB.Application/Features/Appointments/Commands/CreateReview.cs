using System;
using FluentValidation;
using MediatR;
using OMAB.Application.Cores;
using OMAB.Application.Interfaces;
using OMAB.Domain.Constants;
using OMAB.Domain.Entities;
using OMAB.Domain.Enums;

namespace OMAB.Application.Features.Appointments.Commands;

public class CreateReview
{
    public record Command(int AppointmentId, int Rating, string? Comment) : IRequest<Result<int>>;
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Rating)
                .InclusiveBetween(SystemConstants.MinRating, SystemConstants.MaxRating)
                .WithMessage($"Rating must be between {SystemConstants.MinRating} and {SystemConstants.MaxRating}.");
        }
    }
    public class Handler(IAppointmentRepository appointmentRepository, IUserAccessor userAccessor, IDoctorRepository doctorRepository, IUnitOfWork unitOfWork) : IRequestHandler<Command, Result<int>>
    {
        public async Task<Result<int>> Handle(Command request, CancellationToken ct)
        {
            int? userId = userAccessor.GetCurrentUserId();
            if (userId == null)
                return Result<int>.Failure("User not authenticated.", 401);
            var appointment = await appointmentRepository.GetWithReviewAsync(request.AppointmentId, ct);
            if (appointment is null)
                return Result<int>.Failure("Appointment not found.", 404);
            if (appointment.PatientId != userId)
                return Result<int>.Failure("Not Authorized.", 403);

            if (appointment.Status != AppointmentStatus.Completed)
                return Result<int>.Failure("Cannot review an appointment that is not completed.", 400);

            var review = new Review(request.Rating, request.Comment, request.AppointmentId);
            var result = appointment.AddReview(review);

            if (result != null && !result.IsSuccess)
                return Result<int>.Failure(result.ErrorMessage ?? "Failed to add review.", 400);

            var doctor = await doctorRepository.GetByIdAsync(appointment.DoctorId, ct);

            if (doctor == null)
                return Result<int>.Failure("Doctor not found.", 404);

            doctor.AddReview(request.Rating);

            var saveResult = await unitOfWork.SaveChangesAsync(ct);
            if (saveResult <= 0)
                return Result<int>.Failure("Failed to create review.", 400);
            return Result<int>.Success(appointment.Review!.Id);
        }
    }
}
