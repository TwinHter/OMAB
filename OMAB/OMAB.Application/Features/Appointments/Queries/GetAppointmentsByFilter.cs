using System;
using AutoMapper;
using FluentValidation;
using MediatR;
using OMAB.Application.Cores;
using OMAB.Application.Features.Appointments.DTOs;
using OMAB.Application.Interfaces;
using OMAB.Application.Models;

namespace OMAB.Application.Features.Appointments.Queries;

public class GetAppointmentByFilter
{
    public record Query(AppointmentFilter Filter) : IRequest<Result<IEnumerable<AppointmentItemDto>>>;

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Filter.PaymentStatus)
                .IsInEnum()
                .When(x => x.Filter?.PaymentStatus != null);

            RuleFor(x => x.Filter.Status)
                .IsInEnum()
                .When(x => x.Filter?.Status != null);
        }
    }


    public class Handler(IAppointmentRepository appointmentRepo, IMapper mapper, IUserAccessor userAccessor) : IRequestHandler<Query, Result<IEnumerable<AppointmentItemDto>>>
    {
        public async Task<Result<IEnumerable<AppointmentItemDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var currentUserRole = userAccessor.GetCurrentUserRole();
            var currentUserId = userAccessor.GetCurrentUserId();

            if (currentUserRole != Domain.Enums.UserRole.Admin && currentUserId != request.Filter?.PatientId && currentUserId != request.Filter?.DoctorId)
            {
                return Result<IEnumerable<AppointmentItemDto>>.Failure("Unauthorized access to appointments.", 403);
            }
            var filter = new AppointmentFilter
            {
                PatientId = request.Filter?.PatientId,
                DoctorId = request.Filter?.DoctorId,
                Status = request.Filter?.Status,
                PaymentStatus = request.Filter?.PaymentStatus
            };

            var appointments = await appointmentRepo.GetByFilterAsync(filter, cancellationToken);


            var appointmentDtos = mapper.Map<IEnumerable<AppointmentItemDto>>(appointments);

            return Result<IEnumerable<AppointmentItemDto>>.Success(appointmentDtos);
        }
    }
}
