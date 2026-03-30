using AutoMapper;
using RestaurantApp.API.Modules.Auth.DTOs;
using RestaurantApp.API.Modules.Auth.Models;

namespace RestaurantApp.API.Common
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>();
            // Add more mappings here for other modules as we progress
        }
    }
}
