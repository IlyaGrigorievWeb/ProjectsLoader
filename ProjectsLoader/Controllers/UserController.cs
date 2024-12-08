using Contracts.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectsLoader.Models.Infos;
using ProjectsLoader.Services;

namespace ProjectsLoader.Controllers
{
    
    public static class UserControllerRoutes
    {
        public const string BasePrefix = "User";
        public const string GetAllUsers = "";
        public const string GetById = "{id:guid}";
        public const string GetByLogin = "{login}";
        public const string CreateUserWithoutAuth = "CreateUserWithoutAuth";
        public const string CreateUser = "";
        public const string UpdateUser = "";
        public const string DeleteUser = "{id:guid}";
    }
    
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Route(UserControllerRoutes.GetAllUsers)]
        public Task<IQueryable<User>> GetAllUsers()
        {
            return Task.FromResult(_userService.Get());
        }

        [HttpGet]
        [Route(UserControllerRoutes.GetById)]
        public Task<IQueryable<User>> GetUserById(Guid id)
        {
            return Task.FromResult(_userService.Get().Where(x => x.Id == id));
        }

        [HttpGet]
        [Route(UserControllerRoutes.GetByLogin)]
        public Task<IQueryable<User>> GetUserByLogin(string login)
        {
            return Task.FromResult(_userService.Get().Where(x => x.Login == login));
        }
        
        [HttpPost]
        [Route(UserControllerRoutes.CreateUser)]
        public async Task<bool> CreateUser(User user)
        {
            return await _userService.CreateUser(user);
        }


        [HttpPut]
        [Route(UserControllerRoutes.UpdateUser)]
        public async Task<bool> UpdateUser(User user) 
        {
            return await _userService.UpdateUser(user);
        }

        [HttpDelete]
        [Route(UserControllerRoutes.DeleteUser)]
        public async Task<bool> DeleteUser(Guid id) 
        {
            return await _userService.DeleteUser(id);
        }

    }
}
