using AutoMapper;
using Contracts.Entities;
using ProjectsLoader.Models.Infos;

namespace ProjectsLoader.Mappings.UserMapper;

public class UserInfoToUserMapper : Profile
{
    public UserInfoToUserMapper()
    {
        CreateMap<UserInfo, User>().ReverseMap();
    }
}