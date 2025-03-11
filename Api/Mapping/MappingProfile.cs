using AutoMapper;
using BankingWebApp.Api.Data.Models;
using BankingWebApp.Api.Dto.Requests;
using BankingWebApp.Api.Dto.Responses;

namespace BankingWebApp.Api.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateAccountRequest, AccountModel>();
            CreateMap<AccountModel, AccountDetailsResponse>();
            CreateMap<TransactionModel, TransactionResponse>()
                .ForMember(dest => dest.TransactionType, e => e.MapFrom(src => src.Type.ToString()));
        }
    }
}