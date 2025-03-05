using System.Security.Claims;
using Contracts.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectsPlanner.Services;
using Serilog;

namespace ProjectsPlanner.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private const string GetAllUsersRoute = "";
        private const string GetByIdRoute = "{id:guid}";
        private const string GetByLoginRoute = "{login}";
        private const string CreateUserRoute = "";
        private const string UpdateUserRoute = "";
        private const string DeleteUserRoute = "{id:guid}";

        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Route(GetAllUsersRoute)]
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
        [Route(GetByIdRoute)]
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
        [Route(GetByLoginRoute)]
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
        [Route(CreateUserRoute)]
        public async Task<bool> CreateUser(User user)
        {
            try
            {
                var success = await _userService.CreateUser(user);
                var userName = User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault()?.Value ??
                               "Unknown";

                if (success)
                {
                    Log.Information("User with login {Login} created successfully by {UserName}.", user.Login,
                        userName);
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
        [Route(UpdateUserRoute)]
        public async Task<bool> UpdateUser(User user)
        {
            try
            {
                var success = await _userService.UpdateUser(user);
                var userName = User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault()?.Value ??
                               "Unknown";

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
        [Route(DeleteUserRoute)]
        public async Task<bool> DeleteUser(Guid id)
        {
            try
            {
                var success = await _userService.DeleteUser(id);
                var userName = User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault()?.Value ??
                               "Unknown";

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