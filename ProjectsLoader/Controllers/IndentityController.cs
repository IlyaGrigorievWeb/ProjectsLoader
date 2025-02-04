using System.Security.Claims;
using Contracts.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ProjectsLoader.Services;


namespace ProjectsLoader.Controllers
{
    public static class IndentityControllerRoutes
    {
        public const string GetToken = "{login}/{password}";
        public const string LogOut = "";
        public const string GetAllActiveUser = "";
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
            return await _indentityService.Authenticate(login, password);
        }

        [HttpPost]
        [Route(IndentityControllerRoutes.LogOut)]
        public async void LogOut()
        {
            try
            {
                var name = User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
                await _indentityService.Logout(name);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }
        
        [HttpGet]
        [Route(IndentityControllerRoutes.GetAllActiveUser)]
        public List<string> GetAllActiveUser()
        {
            return _indentityService.GetAllActiveUser();
        }
    }
}
