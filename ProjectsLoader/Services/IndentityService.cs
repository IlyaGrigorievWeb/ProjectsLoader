using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Contracts.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ProjectsLoader.Services
{
    public class IndentityService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly UserService _userService;
        private readonly IPasswordHasher _passwordHasher;
        public IndentityService(IOptions<JwtSettings> jwtSettings, UserService userService, IPasswordHasher passwordHasher)
        {
            _jwtSettings = jwtSettings.Value;
            _userService = userService;
            _passwordHasher = passwordHasher;
        }

        public async Task<string> Authenticate(string login, string password)
        {
            var user = await _userService.GetUserByLogin(login);

            var result = _passwordHasher.Verify(user.Password, password);

            if (!result) 
            {
                throw new Exception("Login or password is not correct.");
            }

            return GenerateJwtToken(login, "User");
        }

        private string GenerateJwtToken(string login, string role)
        {
            if (string.IsNullOrEmpty(_jwtSettings.Key))
            {
                throw new ArgumentNullException(nameof(_jwtSettings.Key), "JWT Key cannot be null or empty");
            }

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, login),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}


