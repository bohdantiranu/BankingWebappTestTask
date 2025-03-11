using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BankingWebApp.Api.Data.Models
{
    public class AccountModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Iban { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal Balance { get; set; }
    }
}
