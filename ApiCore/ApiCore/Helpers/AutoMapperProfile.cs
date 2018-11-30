using ApiCore.DTO;
using ApiCore.Models;
using AutoMapper;

namespace ApiCore.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<UserDTO, Users>();
            CreateMap<Users, UserDTO>();

        }
    }
}
