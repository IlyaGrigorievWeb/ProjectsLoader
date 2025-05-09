using Microsoft.AspNetCore.Mvc;
using ProjectsLoader.Models.Infos;
using ProjectsLoader.Services;
using Serilog;

namespace ProjectsLoader.Controllers;

public static class RegistrationControllerRoutes
{
    public const string CreateUser = "";
}

[ApiController]
[Route("[controller]")]
public class RegistrationController : ControllerBase
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
        try
        {
            var success = await _registrationService.CreateUser(userCredentials);

            if (success)
            {
                Log.Information("A new user with login {Login} was successfully created.", userCredentials.Login);
            }
            else
            {
                Log.Warning("Failed to create user with login: {Login}.", userCredentials.Login);
            }
            return success;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred while creating user with login: {Login}.", userCredentials.Login);
            throw new ApplicationException("An error occurred while creating the user.", ex);
        }
    }
}