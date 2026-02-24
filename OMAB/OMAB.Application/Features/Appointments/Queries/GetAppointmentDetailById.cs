using System;
using AutoMapper;
using MediatR;
using OMAB.Application.Cores;
using OMAB.Application.Features.Appointments.DTOs;
using OMAB.Application.Interfaces;
using OMAB.Domain.Enums;

namespace OMAB.Application.Features.Appointments.Queries;

public class GetAppointmentDetailById
{
    public record Query(int AppointmentId) : IRequest<Result<AppointmentDetailDto>>;

    public class Handler(IAppointmentRepository appointmentRepository, IUserAccessor userAccessor, IMapper mapper) : IRequestHandler<Query, Result<AppointmentDetailDto>>
    {
        public async Task<Result<AppointmentDetailDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var currentUserRole = userAccessor.GetCurrentUserRole();
            var currentUserId = userAccessor.GetCurrentUserId();
            var appointment = await appointmentRepository.GetFullDetailAsync(request.AppointmentId, cancellationToken);

            if (appointment == null)
                return Result<AppointmentDetailDto>.Failure("Appointment not found", 404);

            if (currentUserRole != UserRole.Admin && currentUserId != appointment.PatientId && currentUserId != appointment.DoctorId)
                return Result<AppointmentDetailDto>.Failure("Not Authorized", 403);


            var appointmentDto = mapper.Map<AppointmentDetailDto>(appointment);
            return Result<AppointmentDetailDto>.Success(appointmentDto);
        }
    }
}
