using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace MongoConnector.Models
{
    public class Recipe
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string Title { get; set; }
        public string Video { get; set; }
        public List<string> Images { get; set; }
        public string Instructions { get; set; }
        public Difficulty Difficulty { get; set; } = Difficulty.Easy;
        public string PortionType { get; set; }
        public TimeSpan PrepTime { get; set; }
        public TimeSpan CookTime { get; set; }
        public string CreatorId { get; set; }
        public RecipeIngredient RecipeIngredient { get; set; }
        public Status Status { get; set; } = Status.Draft;
    }
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }
    public enum Status
    {
        Draft,
        Pending,
        Approved,
        Rejected
    }
}
