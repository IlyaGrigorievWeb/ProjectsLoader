using AutoMapper;
using Contracts.Entities;
using ProjectsLoader.Models.Infos;

namespace ProjectsLoader.Mappings.UserMapper;

public class UserMapper : Profile
{
    public UserMapper()
    {
        CreateMap<UserInfo, User>().ReverseMap();
    }
}