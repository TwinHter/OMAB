using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using OMAB.Domain.Constants;
using OMAB.Domain.Entities;
using OMAB.Domain.Enums;

namespace OMAB.Infrastructure.Persistence;

public class AppDbContextInitialise
{
    private readonly ILogger<AppDbContextInitialise> _logger;
    private readonly AppDbContext _context;
    public AppDbContextInitialise(ILogger<AppDbContextInitialise> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }
    public async Task InitialiseAsync()
    {
        try
        {
            if (_context.Database.IsSqlite())
            {
                await _context.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🚀 Starting database seeding...");

            await SeedUsersAsync(cancellationToken);
            await SeedSpecialtiesAsync(cancellationToken);
            await SeedDoctorsPatientsAsync(cancellationToken);
            await SeedDiseasesAsync(cancellationToken);
            await SeedMedicinesAsync(cancellationToken);
            await SeedAppointmentsAsync(cancellationToken);
            await SeedDoctorSchedulesAsync(cancellationToken);
            await SeedReviewsAsync(cancellationToken);

            _logger.LogInformation("✅ Database seeding completed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task SeedDoctorSchedulesAsync(CancellationToken cancellationToken)
    {
        if (await _context.DoctorSchedules.AnyAsync(cancellationToken))
            return;

        var doctor = await _context.Doctors.FirstAsync(cancellationToken);

        var schedule1 = new DoctorSchedule(
            DayOfWeek.Monday,
            TimeSpan.FromHours(9),
            TimeSpan.FromHours(17),
            doctor.UserId
        );
        var schedule2 = new DoctorSchedule(
            DayOfWeek.Wednesday,
            TimeSpan.FromHours(10),
            TimeSpan.FromHours(16),
            doctor.UserId
        );

        _context.DoctorSchedules.AddRange(schedule1, schedule2);
        await _context.SaveChangesAsync(cancellationToken);
    }
    private async Task SeedUsersAsync(CancellationToken cancellationToken)
    {
        if (await _context.Users.AnyAsync(cancellationToken)) return;
        var admin = new User("admin@admin.com", SystemConstants.DefaultPassword, UserRole.Admin);
        var doctor1 = new User("doctor1@doctor.com", SystemConstants.DefaultPassword, UserRole.Doctor);
        var patient1 = new User("nguyen@patient.com", SystemConstants.DefaultPassword, UserRole.Patient);
        var doctor2 = new User("doctor2@doctor.com", SystemConstants.DefaultPassword, UserRole.Doctor);
        var patient2 = new User("quan@patient.com", SystemConstants.DefaultPassword, UserRole.Patient);

        doctor1.UpdatePersonalInfo("Bac si 1", Gender.Male, "012-345-6789", DateTime.Parse("1980-01-01"));
        doctor2.UpdatePersonalInfo("Bac si 2", Gender.Female, "987-654-3210", DateTime.Parse("1985-05-15"));
        patient1.UpdatePersonalInfo("Nguyen Huu Dang Nguyen", Gender.Male, "333-444-5555", DateTime.Parse("2005-09-10"));
        patient2.UpdatePersonalInfo("Tran Quan", Gender.Male, "444-555-6666", DateTime.Parse("2003-12-20"));
        admin.UpdatePersonalInfo("Admin User", Gender.Other, "000-111-2222", DateTime.Parse("1990-06-15"));

        _context.Users.AddRange(admin, doctor1, patient1, doctor2, patient2);
        await _context.SaveChangesAsync(cancellationToken);
    }
    private async Task SeedSpecialtiesAsync(CancellationToken cancellationToken)
    {
        if (await _context.Specialties.AnyAsync(cancellationToken)) return;

        var specialty = new Specialty(
            "Tim mạch",
            "Chuyên khoa tim mạch"
        );

        _context.Specialties.Add(specialty);
        await _context.SaveChangesAsync(cancellationToken);
    }
    private async Task SeedDoctorsPatientsAsync(CancellationToken cancellationToken)
    {
        if (await _context.Doctors.AnyAsync(cancellationToken))
            return;

        var doctorUser1 = await _context.Users
            .FirstAsync(u => u.Email == "doctor1@doctor.com", cancellationToken);
        var doctorUser2 = await _context.Users
            .FirstAsync(u => u.Email == "doctor2@doctor.com", cancellationToken);

        var patientUser1 = await _context.Users
            .FirstAsync(u => u.Email == "nguyen@patient.com", cancellationToken);
        var patientUser2 = await _context.Users
            .FirstAsync(u => u.Email == "quan@patient.com", cancellationToken);
        var doctor1 = new Doctor(
            doctorUser1.Id,
            experienceYears: 10,
            consultationFee: 200000
        );
        var doctor2 = new Doctor(
            doctorUser2.Id,
            experienceYears: 8,
            consultationFee: 180000
        );
        var patient1 = new Patient(
            patientUser1.Id,
            BloodType.APlus,
            diseaseHistory: "Không có tiền sử bệnh nghiêm trọng",
            relativePhoneNumber: "333-444-5555"
        );
        var patient2 = new Patient(
            patientUser2.Id,
            BloodType.APlus,
            diseaseHistory: "Không có tiền sử bệnh nghiêm trọng",
            relativePhoneNumber: "333-444-5555"
        );

        _context.Doctors.AddRange(doctor1, doctor2);
        _context.Patients.AddRange(patient1, patient2);

        await _context.SaveChangesAsync(cancellationToken);
    }
    private async Task SeedDiseasesAsync(CancellationToken cancellationToken)
    {
        if (await _context.Diseases.AnyAsync(cancellationToken))
            return;

        _context.Diseases.Add(
            new Disease("I10", "Tăng huyết áp")
        );

        _context.Diseases.Add(
            new Disease("I11", "Đau nửa đầu")
        );

        await _context.SaveChangesAsync(cancellationToken);
    }
    private async Task SeedMedicinesAsync(CancellationToken cancellationToken)
    {
        if (await _context.Medicines.AnyAsync(cancellationToken))
            return;

        _context.Medicines.Add(
            new Medicine("Atorvastatin", "Thuốc hạ mỡ máu")
        );
        _context.Medicines.Add(
            new Medicine("Paracetanol", "Thuốc chống đau đầu")
        );

        await _context.SaveChangesAsync(cancellationToken);
    }
    private async Task SeedAppointmentsAsync(CancellationToken cancellationToken)
    {
        if (await _context.Appointments.AnyAsync(cancellationToken))
            return;
        var doctor = await _context.Doctors.FirstAsync(cancellationToken);
        var patient = await _context.Patients.FirstAsync(cancellationToken);
        var disease = await _context.Diseases.FirstAsync(cancellationToken);
        _context.Appointments.Add(
            new Appointment(
                patient.UserId,
                doctor.UserId,
                disease.Id,
                DateTime.Parse("5/2/2026 9:00 AM"),
                200000,
                "Chẩn đoán ban đầu",
                DateTime.Parse("5/2/2026 9:00 AM").AddHours(1)
            )
        );
        await _context.SaveChangesAsync(cancellationToken);
    }
    private async Task SeedReviewsAsync(CancellationToken cancellationToken)
    {
        if (await _context.Reviews.AnyAsync(cancellationToken))
            return;

        var appointment = await _context.Appointments.FirstAsync(cancellationToken);

        var review = new Review(
            rating: 5,
            comment: "Bác sĩ rất tận tâm và chuyên nghiệp.",
            appointment.Id
        );

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
