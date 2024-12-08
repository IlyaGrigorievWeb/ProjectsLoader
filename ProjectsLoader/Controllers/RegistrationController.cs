using Microsoft.AspNetCore.Mvc;
using ProjectsLoader.Models.Infos;
using ProjectsLoader.Services;

namespace ProjectsLoader.Controllers;

public static class RegistrationControllerRoutes
{
    public const string CreateUser = "";
}

[ApiController]
[Route("[controller]")]
public class RegistrationController: ControllerBase
{
    private readonly RegistrationService _registrationService;

    public RegistrationController(RegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    [HttpPost]
    [Route(RegistrationControllerRoutes.CreateUser)]
    public async Task<bool> CreateUser(UserInfo userCredentials) 
    {
        return await _registrationService.CreateUser(userCredentials);
    }
}