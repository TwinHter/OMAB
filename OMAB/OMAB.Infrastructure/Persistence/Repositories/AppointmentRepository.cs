using Microsoft.EntityFrameworkCore;
using OMAB.Application.Interfaces;
using OMAB.Application.Models;
using OMAB.Domain.Entities;
using OMAB.Domain.Enums;

namespace OMAB.Infrastructure.Persistence.Repositories;

public class AppointmentRepository : GenericRepository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(AppDbContext context) : base(context) { }

    public Task<Appointment?> GetFullDetailAsync(int appointmentId, CancellationToken ct = default)
    {
        return _dbSet.Include(a => a.Prescriptions).Include(a => a.Review).Include(a => a.Patient).ThenInclude(a => a.User).Include(a => a.Doctor).ThenInclude(a => a.User).FirstOrDefaultAsync(a => a.Id == appointmentId, ct);
    }

    public async Task<IEnumerable<Appointment>> GetByFilterAsync(AppointmentFilter filter, CancellationToken ct = default)
    {
        var query = _dbSet.AsQueryable();

        if (filter.PatientId.HasValue)
        {
            query = query.Where(a => a.PatientId == filter.PatientId.Value);
        }

        if (filter.DoctorId.HasValue)
        {
            query = query.Where(a => a.DoctorId == filter.DoctorId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(a => a.Status == filter.Status.Value);
        }

        if (filter.PaymentStatus.HasValue)
        {
            query = query.Where(a => a.PaymentStatus == filter.PaymentStatus.Value);
        }
        return await query.AsNoTracking().Include(a => a.Patient).ThenInclude(a => a.User).Include(a => a.Doctor).ThenInclude(a => a.User).ToListAsync(ct);
    }

    public Task<Appointment?> GetWithPrescriptionAsync(int appointmentId, CancellationToken ct = default)
    {
        return _dbSet.Include(a => a.Prescriptions).FirstOrDefaultAsync(a => a.Id == appointmentId, ct);
    }

    public Task<Appointment?> GetWithReviewAsync(int appointmentId, CancellationToken ct = default)
    {
        return _dbSet.Include(a => a.Review).FirstOrDefaultAsync(a => a.Id == appointmentId, ct);
    }

    public async Task<List<Appointment>> GetDoctorAppointmentsOnDateAsync(int doctorId, DateOnly date, CancellationToken ct = default)
    {
        var startDate = date.ToDateTime(TimeOnly.MinValue);
        var endDate = date.ToDateTime(TimeOnly.MaxValue);

        return await _dbSet
            .Where(a => a.DoctorId == doctorId && a.AppointmentDate >= startDate && a.AppointmentDate <= endDate && a.Status == AppointmentStatus.Scheduled)
            .ToListAsync(ct);
    }
    public async Task<bool> IsSlotAvailableAsync(int doctorId, DateTime startTime, DateTime endTime, CancellationToken ct = default)
    {
        return !await _dbSet.AnyAsync(a =>
            a.DoctorId == doctorId &&
            a.Status == AppointmentStatus.Scheduled &&
            (a.AppointmentDate < endTime) && (a.AppointmentEndTime > startTime), ct);
    }
}
