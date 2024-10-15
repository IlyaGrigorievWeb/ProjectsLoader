using Contracts.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ProjectsLoader.Services;


namespace ProjectsLoader.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IndentityController : ControllerBase
    {
        private readonly IndentityService _indentityService;

        public IndentityController(IndentityService indentityService)
        {
            _indentityService = indentityService;
        }

        [HttpPost]
        [Route("GetToken")]
        public async Task<string> GetToken(string login, string password) 
        {
            return await _indentityService.Authenticate(login, password);
        }
    }
}
