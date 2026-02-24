using System;
using AutoMapper;
using MediatR;
using OMAB.Application.Cores;
using OMAB.Application.Features.Profiles.DTOs;
using OMAB.Application.Interfaces;
using OMAB.Domain.Entities;
using OMAB.Domain.Enums;

namespace OMAB.Application.Features.Identities.Queries;

public class GetCurrentUser
{
    public record Query() : IRequest<Result<UserProfileDto>>;

    public class Handler(IUserAccessor userAccessor, IUserRepository userRepository, IDoctorRepository doctorRepository, IGenericRepository<Patient> patientRepository, IMapper mapper)
        : IRequestHandler<Query, Result<UserProfileDto>>
    {
        public async Task<Result<UserProfileDto>> Handle(Query request, CancellationToken ct)
        {
            var userId = userAccessor.GetCurrentUserId();
            if (userId == null)
                return Result<UserProfileDto>.Failure("User not authenticated.", 401);
            var user = await userRepository.GetByIdAsync(userId.Value, ct);

            if (user == null)
                return Result<UserProfileDto>.Failure("User not found", 404);

            switch (user.UserRole)
            {
                case UserRole.Doctor:
                    var doctor = await doctorRepository.GetDoctorWithSpecialtyAsync(userId.Value, ct);

                    if (doctor == null)
                        return Result<UserProfileDto>.Failure("Doctor profile data mismatch", 404);

                    return Result<UserProfileDto>.Success(
                        UserProfileDto.Create(user, mapper, doctor: doctor)
                    );

                case UserRole.Patient:
                    var patient = await patientRepository.GetByIdAsync(userId.Value, ct);

                    if (patient == null)
                        return Result<UserProfileDto>.Failure("Patient profile data mismatch", 404);

                    return Result<UserProfileDto>.Success(
                        UserProfileDto.Create(user, mapper, patient: patient)
                    );
                default:
                    return Result<UserProfileDto>.Success(
                        UserProfileDto.Create(user, mapper)
                    );
            }
        }

    }
}
