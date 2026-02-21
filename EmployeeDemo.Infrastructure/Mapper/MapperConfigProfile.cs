using AutoMapper;

using EmployeeDemo.Application.ViewModels.MeetingViewModels;
using EmployeeDemo.Domain.Entities;

namespace EmployeeDemo.Infrastructure.Mapper
{
    public class MapperConfigProfile : Profile
    {
        public MapperConfigProfile()
        {
            // Product mapping removed

            // Meeting mappings
            CreateMap<MeetingCreateDTO, Meeting>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.HostId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreationDate, opt => opt.Ignore());

            CreateMap<Meeting, MeetingResponseDTO>();
        }
    }
}
