using Contracts.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectsLoader.Models;
using ProjectsLoader.Services;

namespace ProjectsLoader.Controllers
{
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
        [Route("GetAllUsers")]
        public async Task<List<User>> GetAllUsers()
        {
            return await _userService.GetAllUsersAsync();
        }

        [HttpGet]
        [Route("GetUserById")]
        public async Task<User> GetUserById(Guid id)
        {
            return await _userService.GetUserById(id);
        }

        [HttpGet]
        [Route("GetUserByLogin")]
        public async Task<User> GetUserByLogin(string login)
        {
            return await _userService.GetUserByLogin(login);
        }

        [HttpPost]
        [Route("CreateUser")]
        public async Task<bool> CreateUser(User user)
        {
            return await _userService.CreateUser(user);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("CreateUserWithoutAuth")]
        public async Task<bool> CreateUserWithoutAuth(UserCredentials userCredentials) 
        {
            User user = new() {Id = Guid.NewGuid(), Login = userCredentials.Login, Password = userCredentials.Password };
            return await _userService.CreateUser(user);
        }

        [HttpPut]
        [Route("UpdateUser")]
        public async Task<bool> UpdateUser(User user) 
        {
            return await _userService.UpdateUser(user);
        }

        [HttpDelete]
        [Route("DeleteUser")]
        public async Task<bool> DeleteUser(Guid id) 
        {
            return await _userService.DeleteUser(id);
        }

    }
}
