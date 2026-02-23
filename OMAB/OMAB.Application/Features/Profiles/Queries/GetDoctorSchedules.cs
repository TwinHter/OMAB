using System;
using MediatR;
using OMAB.Application.Cores;
using OMAB.Application.Features.Profiles.DTOs;
using OMAB.Application.Interfaces;

namespace OMAB.Application.Features.Profiles.Queries;

public class GetDoctorSchedules
{
    public record Query(int? DoctorId) : IRequest<Result<List<DoctorScheduleDto>>>;

    public class Handler(IDoctorRepository doctorRepository, IUserAccessor userAccessor)
        : IRequestHandler<Query, Result<List<DoctorScheduleDto>>>
    {
        public async Task<Result<List<DoctorScheduleDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            int doctorId;
            if (request.DoctorId.HasValue) doctorId = request.DoctorId.Value;
            else
            {
                var currentUser = await userAccessor.GetCurrentUserAsync();
                if (currentUser.UserRole != Domain.Enums.UserRole.Doctor)
                    return Result<List<DoctorScheduleDto>>.Failure("Only doctors can view their schedules", 403);
                doctorId = currentUser.Id;
            }

            var doctor = await doctorRepository.GetByIdWithSchedulesAsync(doctorId, cancellationToken);
            if (doctor == null)
                return Result<List<DoctorScheduleDto>>.Failure("Doctor not found", 404);

            var scheduleDtos = doctor.DoctorSchedules.Select(s => new DoctorScheduleDto
            {
                ScheduleId = s.Id,
                DayOfWeek = s.DayOfWeek,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                SlotDurationInMinutes = s.SlotDurationInMinutes
            }).ToList();

            return Result<List<DoctorScheduleDto>>.Success(scheduleDtos);
        }
    }
}
