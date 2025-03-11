using BankingWebApp.Api.Dto.Requests;
using BankingWebApp.Api.Dto.Responses;
using BankingWebApp.Api.Extensions;
using BankingWebApp.Api.Services.Impl;
using BankingWebApp.Auth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankingWebApp.Api.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    [Produces("application/json")]
    [Authorize(Policy = Roles.User)]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(ITransactionService transactionService, ILogger<TransactionsController> logger)
        {
            _transactionService = transactionService;
            _logger = logger;
        }

        [HttpPost("deposit")]
        public async Task<ActionResult<BaseResponse<TransactionResponse>>> DepositAsync([FromBody] TransactionRequest request)
        {
            _logger.LogInformation("Depositing funds: Iban={Iban}, Amount={Amount}", request.Iban, request.Amount);
            
            var transactionResponse = await _transactionService.DepositAsync(request, GetClaims().ToList());
            
            return this.OkResult(transactionResponse);
        }

        [HttpPost("withdraw")]
        public async Task<ActionResult<BaseResponse<TransactionResponse>>> WithdrawFundsAsync([FromBody] TransactionRequest request)
        {
            _logger.LogInformation("Withdrawing funds: Iban={Iban}, Amount={Amount}",
                request.Iban, request.Amount);
            
            var transactionResponse = await _transactionService.WithdrawAsync(request, GetClaims().ToList());
            
            return this.OkResult(transactionResponse);
        }

        [HttpPost("transfer")]
        public async Task<ActionResult<BaseResponse<TransactionResponse>>> TransferFundsAsync([FromBody] TransferTransactionRequest request)
        {
            _logger.LogInformation("Transferring funds: FromAccount={FromAccountIban}, ToAccount={ToAccountIban}, Amount={Amount}", 
                request.FromAccount, request.ToAccount, request.Amount);
            
            var transactionResponse = await _transactionService.TransferAsync(request, GetClaims().ToList());
            
            return this.OkResult(transactionResponse);
        }

        private IEnumerable<Claim> GetClaims() => HttpContext.User.Claims; 
    }
}