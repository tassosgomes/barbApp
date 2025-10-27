using AutoMapper;
using BarbApp.Application.DTOs;
using BarbApp.Domain.Entities;

namespace BarbApp.Application.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Barber, BarbeiroDto>()
            .ForMember(dest => dest.Nome, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Foto, opt => opt.MapFrom(src => (string?)null))
            .ForMember(dest => dest.Especialidades, opt => opt.MapFrom(src => new List<string>()));

        CreateMap<BarbershopService, ServicoDto>()
            .ForMember(dest => dest.Nome, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Descricao, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.DuracaoMinutos, opt => opt.MapFrom(src => src.DurationMinutes))
            .ForMember(dest => dest.Preco, opt => opt.MapFrom(src => src.Price));
    }
}