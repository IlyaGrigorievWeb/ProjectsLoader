using Contracts.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ProjectsLoader.Services;


namespace ProjectsLoader.Controllers
{
    public static class IndentityControllerRoutes
    {
        public const string GetToken = "{login}/{password}";
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
    }
}
