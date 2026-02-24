using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OMAB.Application.Interfaces;
using OMAB.Infrastructure.Persistence.Repositories;
using OMAB.Infrastructure.Persistence.Services;
using OMAB.Infrastructure.Services;

namespace OMAB.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
            ));
        services.AddScoped<AppDbContextInitialise>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();

        // DevUserAccessor for dev purposes, not implement JWT.
        services.AddScoped<IUserAccessor, UserAccessor>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDoctorRepository, DoctorRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IJwtGenerator, JwtGenerator>();
    }
}

