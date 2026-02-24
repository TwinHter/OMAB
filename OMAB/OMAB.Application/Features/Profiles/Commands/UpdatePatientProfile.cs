using System;
using FluentValidation;
using MediatR;
using OMAB.Application.Cores;
using OMAB.Application.Features.Profiles.DTOs;
using OMAB.Application.Interfaces;
using OMAB.Domain.Entities;

namespace OMAB.Application.Features.Profiles.Commands;

public class UpdatePatientProfile
{
    public record Command(UpdatePatientDto UpdateDto) : IRequest<Result<int>>;
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.UpdateDto.FullName)
                .NotEmpty()
                .When(x => x.UpdateDto.FullName != null);

            RuleFor(x => x.UpdateDto.PhoneNumber)
                .Matches(@"^\+?[1-9]\d{1,14}$")
                .When(x => !string.IsNullOrWhiteSpace(x.UpdateDto.PhoneNumber));

            RuleFor(x => x.UpdateDto.DateOfBirth)
                .LessThan(DateTime.Now)
                .When(x => x.UpdateDto.DateOfBirth != null);
        }
    }

    public class Handler(IUserAccessor userAccessor, IPatientRepository patientRepository, IUnitOfWork unitOfWork) : IRequestHandler<Command, Result<int>>
    {
        public async Task<Result<int>> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = userAccessor.GetCurrentUserId();
            if (userId == null)
                return Result<int>.Failure("User not authenticated.", 401);
            var patient = await patientRepository.GetPatientByIdAsync(userId.Value, cancellationToken);
            if (patient == null)
            {
                return Result<int>.Failure("Patient profile not found.", 404);
            }

            patient.User.UpdatePersonalInfo(fullName: request.UpdateDto.FullName,
                dateOfBirth: request.UpdateDto.DateOfBirth,
                phoneNumber: request.UpdateDto.PhoneNumber);
            patient.UpdateMedicalInfo(bloodType: request.UpdateDto.BloodType,
                diseaseHistory: request.UpdateDto.DiseaseHistory,
                relativePhoneNumber: request.UpdateDto.RelativePhoneNumber);

            patientRepository.Update(patient);
            var updateResult = await unitOfWork.SaveChangesAsync(cancellationToken);
            if (updateResult <= 0)
            {
                return Result<int>.Failure("Failed to update patient profile.", 500);
            }
            return Result<int>.Success(patient.UserId);
        }
    }
}
