using System;
using MediatR;
using OMAB.Application.Cores;
using OMAB.Application.Interfaces;

namespace OMAB.Application.Features.Profiles.Commands;

public class UpdateDoctorSpecialty
{
    public record Command(List<int> SpecialtyIds) : IRequest<Result<int>>;
    public class Handler(IUserAccessor userAccessor, IDoctorRepository doctorRepository, IUnitOfWork unitOfWork) : IRequestHandler<Command, Result<int>>
    {
        public async Task<Result<int>> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = userAccessor.GetCurrentUserId();
            if (userId == null)
                return Result<int>.Failure("User not authenticated.", 401);
            var doctor = await doctorRepository.GetByIdAsync(userId.Value, cancellationToken);
            if (doctor == null)
            {
                return Result<int>.Failure("Doctor not found.", 404);
            }

            doctor.UpdateSpecialties(request.SpecialtyIds);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<int>.Success(doctor.UserId);
        }
    }
}
