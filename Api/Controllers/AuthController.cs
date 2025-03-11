using BankingWebApp.Auth.Models;
using BankingWebApp.Auth.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankingWebApp.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;

        public AuthController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpGet("adminToken")]
        public IActionResult GetAdminToken()
        {
            var token = _tokenService.GenerateToken( Roles.Admin);

            return Ok(new { token });
        }

        [HttpGet("userToken/{iban}")]
        public IActionResult GetUserToken(string iban)
        {
            var token = _tokenService.GenerateToken(Roles.User, iban);

            return Ok(new { token });
        }
    }
}