using BankingWebApp.Api.Data.Enums;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BankingWebApp.Api.Data.Models
{
    public class TransactionModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string ExecutionAccount { get; set; }
        public string? ToAccount { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
        public TransactionType Type { get; set; }
    }
}
