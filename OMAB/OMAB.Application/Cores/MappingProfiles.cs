using System;
using AutoMapper;
using OMAB.Application.Features.Appointments.DTOs;
using OMAB.Application.Features.Profiles.DTOs;
using OMAB.Domain.Entities;

namespace OMAB.Application.Cores;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<User, UserProfileDto>()
            .ForMember(dest => dest.Doctor, opt => opt.MapFrom(src => src.Doctor))
            .ForMember(dest => dest.Patient, opt => opt.MapFrom(src => src.Patient));
        CreateMap<Doctor, UserProfileDto.DoctorInfo>()
            .ForMember(
                dest => dest.Specializations,
                opt => opt.MapFrom(src =>
                    src.DoctorSpecialties.Select(ds => ds.Specialty.Name)
                )
            );
        CreateMap<Patient, UserProfileDto.PatientInfo>();

        CreateMap<Doctor, DoctorSummaryDto>()
            .ForMember(
                dest => dest.Specializations,
                opt => opt.MapFrom(src =>
                    src.DoctorSpecialties.Select(ds => ds.Specialty.Name).ToList()
                )
            )
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName));

        CreateMap<DoctorFilterDto, DoctorFilter>();

        CreateMap<Appointment, AppointmentDetailDto>()
            .ForMember(
                dest => dest.Patient,
                opt => opt.MapFrom(src =>
                    new PersonDto(src.Patient.UserId, src.Patient.User.FullName))
            )
            .ForMember(
                dest => dest.Doctor,
                opt => opt.MapFrom(src =>
                    new PersonDto(src.Doctor.UserId, src.Doctor.User.FullName))
            )
            .ForMember(
                dest => dest.Disease,
                opt => opt.MapFrom(src =>
                    src.Disease == null
                        ? null
                        : new DiseaseDto(src.Disease.Id, src.Disease.Name))
            )
            .ForMember(
                dest => dest.Review,
                opt => opt.MapFrom(src => src.Review == null
                    ? null
                    : new ReviewDto(src.Review.Id, src.Review.Rating, src.Review.Comment, src.Patient.User.FullName, src.Review.CreatedAt))
            );

        CreateMap<Appointment, AppointmentItemDto>()
            .ForMember(dest => dest.Patient, opt => opt.MapFrom(src =>
                new PersonDto(src.Patient.UserId, src.Patient.User.FullName)))
            .ForMember(dest => dest.Doctor, opt => opt.MapFrom(src =>
                new PersonDto(src.Doctor.UserId, src.Doctor.User.FullName)));
    }
}
