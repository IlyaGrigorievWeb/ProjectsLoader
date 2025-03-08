using AutoMapper;
using Contracts.Entities;
using ProjectsPlanner.Models.Infos;

namespace ProjectsPlanner.Mappings.UserMapper;

public class UserMapper : Profile
{
    public UserMapper()
    {
        CreateMap<UserInfo, User>().ReverseMap();
    }
}