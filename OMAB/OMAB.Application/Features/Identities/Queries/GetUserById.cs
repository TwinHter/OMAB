using AutoMapper;
using MediatR;
using OMAB.Application.Cores;
using OMAB.Application.Features.Profiles.DTOs;
using OMAB.Application.Interfaces;
using OMAB.Domain.Entities;
using OMAB.Domain.Enums;

namespace OMAB.Application.Features.Identities.Queries;

public class GetUserById
{
    public record Query(int UserId) : IRequest<Result<UserProfileDto>>;

    public class Handler(IUserRepository userRepository, IDoctorRepository doctorRepository, IGenericRepository<Patient> patientRepository, IUserAccessor userAccessor, IMapper mapper)
        : IRequestHandler<Query, Result<UserProfileDto>>
    {
        public async Task<Result<UserProfileDto>> Handle(Query request, CancellationToken ct)
        {
            var requester = await userAccessor.GetCurrentUserAsync();
            if (requester == null)
                return Result<UserProfileDto>.Failure("Unauthorized", 401);

            var targetUser = await userRepository.GetByIdAsync(request.UserId, ct);
            if (targetUser == null)
                return Result<UserProfileDto>.Failure("User not found", 404);

            bool isAllowed = userAccessor.CanViewUser(targetUser.UserRole, requester.UserRole);
            if (!isAllowed)
                return Result<UserProfileDto>.Failure("You do not have permission to view this profile", 403);

            switch (targetUser.UserRole)
            {
                case UserRole.Doctor:
                    var doctor = await doctorRepository.GetDoctorWithSpecialtyAsync(request.UserId, ct);

                    if (doctor == null)
                        return Result<UserProfileDto>.Failure("Doctor profile data is missing", 404);

                    return Result<UserProfileDto>.Success(
                        UserProfileDto.Create(targetUser, mapper, doctor: doctor)
                    );

                case UserRole.Patient:
                    var patient = await patientRepository.GetByIdAsync(request.UserId, ct);

                    if (patient == null)
                        return Result<UserProfileDto>.Failure("Patient profile data is missing", 404);

                    return Result<UserProfileDto>.Success(
                        UserProfileDto.Create(targetUser, mapper, patient: patient)
                    );

                default:
                    return Result<UserProfileDto>.Success(
                        UserProfileDto.Create(targetUser, mapper)
                    );
            }
        }

    }
}
