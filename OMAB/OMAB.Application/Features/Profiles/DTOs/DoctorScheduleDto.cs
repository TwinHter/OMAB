using System;

namespace OMAB.Application.Features.Profiles.DTOs;

public record DoctorScheduleDto
{
    public int ScheduleId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int SlotDurationInMinutes { get; set; } = 30;
}
