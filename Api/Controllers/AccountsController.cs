using BankingWebApp.Api.Dto.Requests;
using BankingWebApp.Api.Dto.Responses;
using BankingWebApp.Api.Extensions;
using BankingWebApp.Api.Services.Impl;
using BankingWebApp.Auth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingWebApp.Api.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    [Produces("application/json")]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountsService _accountService;
        private readonly ILogger<AccountsController> _logger;

        public AccountsController(IAccountsService accountService, ILogger<AccountsController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        [HttpPost("сreateAccount")]
        public async Task<ActionResult<BaseResponse<AccountDetailsResponse>>> CreateAccountAsync([FromBody] CreateAccountRequest request)
        {
            _logger.LogInformation("Creating account: FirstName={FirstName}, LastName={LastName}, Balance={Balance}",
                request.FirstName, request.LastName, request.Balance);
            
            var result = await _accountService.CreateAccountAsync(request);

            return this.CreatedResult(result);
        }
 

        [HttpGet("getAccount/{iban}")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.User}")]
        public async Task<ActionResult<BaseResponse<AccountDetailsResponse>>> GetAccountByNumerAsync(string iban)
        {
            _logger.LogInformation("Getting account details: Iban={Iban}", iban);

            var claims = HttpContext.User.Claims.ToList();

            var result = await _accountService.GetAccountByIbanAsync(iban, claims);
            
            if (result == null) 
                return this.NotFoundResult<AccountDetailsResponse>($"Account {iban} not found.");
            
            return this.OkResult(result);
        }

        [HttpGet("getAccounts")]
        [Authorize(Policy = Roles.Admin)]
        public async Task<ActionResult<BaseResponse<IEnumerable<AccountDetailsResponse>>>> GetAllAccountsAsync()
        {
            _logger.LogInformation("Getting all accounts");

            return this.OkResult(await _accountService.GetAllAccountsAsync());
        }
    }
}