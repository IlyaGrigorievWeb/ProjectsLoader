using System.Security.Claims;
using Contracts.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectsLoader.Models.Infos;
using ProjectsLoader.Services;
using Serilog;

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
        public async Task<IList<User>> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAll();
                return users;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while fetching all users.");
                throw new ApplicationException("An error occurred while fetching users.", ex);
            }
        }

        [HttpGet]
        [Route(UserControllerRoutes.GetById)]
        public async Task<User> GetUserById(Guid id)
        {
            try
            {
                var user = await _userService.GetById(id);
                if (user == null)
                {
                    Log.Warning("User with ID: {UserId} not found.", id);
                }
                return user;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while fetching user with ID: {UserId}", id);
                throw new ApplicationException("An error occurred while fetching the user.", ex);
            }
        }

        [HttpGet]
        [Route(UserControllerRoutes.GetByLogin)]
        public async Task<User> GetUserByLogin(string login)
        {
            try
            {
                var user = await _userService.GetUserByLogin(login);
                if (user == null)
                {
                    Log.Warning("User with login: {Login} not found.", login);
                }
                return user;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while fetching user with login: {Login}", login);
                throw new ApplicationException("An error occurred while fetching the user.", ex);
            }
        }

        [HttpPost]
        [Route(UserControllerRoutes.CreateUser)]
        public async Task<bool> CreateUser(User user)
        {
            try
            {
                var success = await _userService.CreateUser(user);
                var userName = User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault()?.Value ?? "Unknown";
                
                if (success)
                {
                    Log.Information("User with login {Login} created successfully by {UserName}.", user.Login, userName);
                }
                else
                {
                    Log.Warning("Failed to create user with login: {Login} by {UserName}.", user.Login, userName);
                }
                return success;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while creating user with login: {Login}", user.Login);
                throw new ApplicationException("An error occurred while creating the user.", ex);
            }
        }

        [HttpPut]
        [Route(UserControllerRoutes.UpdateUser)]
        public async Task<bool> UpdateUser(User user)
        {
            try
            {
                var success = await _userService.UpdateUser(user);
                var userName = User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault()?.Value ?? "Unknown";
                
                if (success)
                {
                    Log.Information("User with ID {UserId} updated successfully by {UserName}.", user.Id, userName);
                }
                else
                {
                    Log.Warning("Failed to update user with ID: {UserId} by {UserName}.", user.Id, userName);
                }
                return success;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while updating user with ID: {UserId}", user.Id);
                throw new ApplicationException("An error occurred while updating the user.", ex);
            }
        }

        [HttpDelete]
        [Route(UserControllerRoutes.DeleteUser)]
        public async Task<bool> DeleteUser(Guid id)
        {
            try
            {
                var success = await _userService.DeleteUser(id);
                var userName = User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault()?.Value ?? "Unknown";
                
                if (success)
                {
                    Log.Information("User with ID {UserId} deleted successfully by {UserName}.", id, userName);
                }
                else
                {
                    Log.Warning("Failed to delete user with ID: {UserId} by {UserName}.", id, userName);
                }
                return success;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while deleting user with ID: {UserId}", id);
                throw new ApplicationException("An error occurred while deleting the user.", ex);
            }
        }
    }
}