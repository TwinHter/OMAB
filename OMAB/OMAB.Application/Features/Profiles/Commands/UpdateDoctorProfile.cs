using System;
using FluentValidation;
using MediatR;
using OMAB.Application.Cores;
using OMAB.Application.Features.Profiles.DTOs;
using OMAB.Application.Interfaces;
using OMAB.Domain.Entities;

namespace OMAB.Application.Features.Profiles.Commands;

public class UpdateDoctorProfile
{
    public record Command(UpdateDoctorDto UpdateDto) : IRequest<Result<int>>;
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.UpdateDto.FullName)
    .NotEmpty()
    .When(x => x.UpdateDto.FullName != null);

            RuleFor(x => x.UpdateDto.ConsultationFee)
                .GreaterThanOrEqualTo(0)
                .When(x => x.UpdateDto != null && x.UpdateDto.ConsultationFee.HasValue);

            RuleFor(x => x.UpdateDto.ExperienceYears)
                .GreaterThanOrEqualTo(0)
                .When(x => x.UpdateDto != null && x.UpdateDto.ExperienceYears.HasValue);

            RuleFor(x => x.UpdateDto.PhoneNumber)
                .Matches(@"^\+?[1-9]\d{1,14}$")
                .When(x => !string.IsNullOrWhiteSpace(x.UpdateDto.PhoneNumber));

            RuleFor(x => x.UpdateDto.DateOfBirth)
                .LessThan(DateTime.Now)
                .When(x => x.UpdateDto.DateOfBirth.HasValue);
        }
    }

    public class Handler(IUserAccessor userAccessor, IDoctorRepository doctorRepository, IUnitOfWork unitOfWork) : IRequestHandler<Command, Result<int>>
    {
        public async Task<Result<int>> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = userAccessor.GetCurrentUserId();
            var doctor = await doctorRepository.GetDoctorWithSpecialtyAsync(userId);
            if (doctor == null)
            {
                return Result<int>.Failure("Doctor profile not found.", 404);
            }


            // if (request.UpdateDto.SpecialtyIds != null)
            // {
            //     doctor.UpdateSpecialties(request.UpdateDto.SpecialtyIds);
            // }

            doctor.UpdateInfo(experienceYears: request.UpdateDto.ExperienceYears,
                consultationFee: request.UpdateDto.ConsultationFee,
                isActive: request.UpdateDto.IsActive);
            doctor.User.UpdatePersonalInfo(fullName: request.UpdateDto.FullName,
                dateOfBirth: request.UpdateDto.DateOfBirth,
                phoneNumber: request.UpdateDto.PhoneNumber);

            doctorRepository.Update(doctor);
            var updateResult = await unitOfWork.SaveChangesAsync();
            if (updateResult <= 0)
            {
                return Result<int>.Failure("Failed to update doctor profile.", 500);
            }
            return Result<int>.Success(userId);
        }
    }
}
