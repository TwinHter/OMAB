using System;
using FluentValidation;
using MediatR;
using OMAB.Application.Cores;
using OMAB.Application.Interfaces;
using OMAB.Domain.Entities;

namespace OMAB.Application.Features.Appointments.Commands;

public class CreateAppointment
{
    public record Command(int DoctorId, DateTime AppointmentDate, DateTime AppointmentEndTime) : IRequest<Result<int>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.AppointmentDate).LessThan(x => x.AppointmentEndTime).WithMessage("Appointment end time must be after appointment date.");
            // Appointment date must be in the future
            RuleFor(x => x.AppointmentDate).GreaterThan(DateTime.UtcNow).WithMessage("Appointment date must be in the future.");

            // Appointment date must be :30 or :00
            RuleFor(x => x.AppointmentDate.Minute)
                .Must(minute => minute == 0 || minute == 30)
                .WithMessage("Appointment date must be on the hour or half hour.");
            RuleFor(x => x.AppointmentEndTime.Minute)
                .Must(minute => minute == 0 || minute == 30)
                .WithMessage("Appointment end time must be on the hour or half hour.");
        }
    }

    public class Handler(IAppointmentRepository appointmentRepository, IUserAccessor userAccessor, IDoctorRepository doctorRepository, IUnitOfWork unitOfWork) : IRequestHandler<Command, Result<int>>
    {
        public async Task<Result<int>> Handle(Command request, CancellationToken cancellationToken)
        {
            var currentUserId = userAccessor.GetCurrentUserId();
            if (currentUserId == null)
                return Result<int>.Failure("User not authenticated.", 401);
            var doctor = await doctorRepository.GetByIdAsync(request.DoctorId, cancellationToken);
            if (doctor == null)
                return Result<int>.Failure("Doctor not found.", 404);

            var isSlotTaken = await appointmentRepository.IsSlotAvailableAsync(request.DoctorId, request.AppointmentDate, request.AppointmentEndTime, cancellationToken);

            if (!isSlotTaken)
                return Result<int>.Failure("The selected time slot is not available.", 400);

            var apppointment = new Appointment
            (
                patientId: currentUserId.Value,
                doctorId: request.DoctorId,
                appointmentDate: request.AppointmentDate,
                appointmentEndTime: request.AppointmentEndTime,
                fee: doctor.ConsultationFee,
                diseaseId: null
            );

            await appointmentRepository.AddAsync(apppointment, cancellationToken);
            var result = await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<int>.Success(apppointment.Id);
        }
    }
}
