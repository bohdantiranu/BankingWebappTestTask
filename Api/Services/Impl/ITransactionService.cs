using BankingWebApp.Api.Dto.Requests;
using BankingWebApp.Api.Dto.Responses;
using System.Security.Claims;

namespace BankingWebApp.Api.Services.Impl
{
    public interface ITransactionService
    {
        public Task<TransactionResponse> DepositAsync(TransactionRequest request, List<Claim> claims);
        public Task<TransactionResponse> WithdrawAsync(TransactionRequest request, List<Claim> claims);
        public Task<TransactionResponse> TransferAsync(TransferTransactionRequest request, List<Claim> claims);
    }
}