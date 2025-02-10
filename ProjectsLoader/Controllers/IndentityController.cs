using System.Security.Claims;
using Contracts.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ProjectsLoader.Services;
using Serilog;

namespace ProjectsLoader.Controllers
{
    public static class IndentityControllerRoutes
    {
        public const string GetToken = "{login}/{password}";
        public const string LogOut = "";
        public const string GetAllActiveUser = "";
        public const string GetAllActiveUserRedis = "active-users-redis";
    }

    [ApiController]
    [Route("[controller]")]
    public class IndentityController : ControllerBase
    {
        private readonly IndentityService _indentityService;

        public IndentityController(IndentityService indentityService)
        {
            _indentityService = indentityService;
        }

        [HttpGet]
        [Route(IndentityControllerRoutes.GetToken)]
        public async Task<string> GetToken(string login, string password)
        {
            try
            {
                Log.Information("Attempting to authenticate user with login: {Login}", login);
                
                var token = await _indentityService.Authenticate(login, password);
                
                Log.Information("User {Login} authenticated successfully.", login);
                
                return token;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred during authentication for user {Login}", login);
                throw new ApplicationException("Authentication failed.", ex);
            }
        }

        [HttpPost]
        [Route(IndentityControllerRoutes.LogOut)]
        public async Task LogOut()
        {
            try
            {
                var name = User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault()?.Value;
                if (name != null)
                {
                    await _indentityService.Logout(name);
                    Log.Information("User {UserName} logged out successfully.", name);
                }
                else
                {
                    Log.Warning("Logout attempt failed: User claims did not contain a NameIdentifier.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred during logout for user {UserName}", User.Identity?.Name);
                throw new ApplicationException(ex.Message);
            }
        }
        
        [HttpGet]
        [Route(IndentityControllerRoutes.GetAllActiveUser)]
        public List<string> GetAllActiveUser()
        {
            try
            {
                var activeUsers = _indentityService.GetAllActiveUser();
                return activeUsers;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while fetching active users.");
                throw new ApplicationException("An error occurred while fetching active users.", ex);
            }
        }
        
        [HttpGet]
        [Route(IndentityControllerRoutes.GetAllActiveUserRedis)]
        public async Task<List<string>> GetAllActiveUserRedis()
        {
            try
            {
                return await _indentityService.GetActiveUsersAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while fetching active users from redis.");
                throw new ApplicationException("An error occurred while fetching active users from redis.", ex);
            }
        }
        
    }
}