using FluentValidation;
using MediatR;
using OMAB.Application.Cores;
using OMAB.Application.Interfaces;
using OMAB.Domain.Constants;

public class UpdateReview
{
    // Cần truyền vào ID cuộc hẹn, Rating mới và Comment mới
    public record Command(int AppointmentId, int Rating, string? Comment) : IRequest<Result<int>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Rating)
                .InclusiveBetween(SystemConstants.MinRating, SystemConstants.MaxRating)
                .WithMessage($"Rating must be between {SystemConstants.MinRating} and {SystemConstants.MaxRating}.");

            RuleFor(x => x.Comment)
                .MaximumLength(500)
                .WithMessage("Comment cannot exceed 500 characters.");
        }
    }

    public class Handler(
        IAppointmentRepository appointmentRepository,
        IDoctorRepository doctorRepository,
        IUserAccessor userAccessor,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<Command, Result<int>>
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
                return Result<int>.Failure("Not Authorized. You can only update your own review.", 403);

            if (appointment.Review == null)
                return Result<int>.Failure("Review not found. Please create a review first.", 404);

            int oldRating = appointment.Review.Rating;

            appointment.Review.Update(request.Rating, request.Comment);

            if (oldRating != request.Rating)
            {
                var doctor = await doctorRepository.GetByIdAsync(appointment.DoctorId, ct);
                if (doctor == null)
                    return Result<int>.Failure("Doctor not found.", 404);

                doctor.UpdateReviewScore(oldRating, request.Rating);
            }
            var result = await unitOfWork.SaveChangesAsync(ct) > 0;

            if (!result)
                return Result<int>.Failure("Failed to update review.", 500);

            return Result<int>.Success(appointment.Review.Id);
        }
    }
}