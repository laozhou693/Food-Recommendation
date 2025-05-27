using System.ComponentModel.DataAnnotations;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Food.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("username")]
        [Required]
        public string Username { get; set; }=string.Empty;

        [BsonElement("passwordHash")]
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [BsonElement("favorites")]
        public List<string> Favorites { get; set; } = new List<string>(); // 收藏的商家ID列表

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("lastLoginAt")]
        public DateTime? LastLoginAt { get; set; }
    }
}
