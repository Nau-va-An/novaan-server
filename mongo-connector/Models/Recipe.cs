﻿using MongoConnector.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoConnector.Models
{
    public class Recipe
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Title { get; set; }

        public string Video { get; set; }

        public Difficulty Difficulty { get; set; } = Difficulty.Easy;

        public int PortionQuantity { get; set; }

        public PortionType PortionType { get; set; } = PortionType.Servings;

        public TimeSpan PrepTime { get; set; }

        public TimeSpan CookTime { get; set; }

        public List<Instruction> Instructions { get; set; }


        [BsonRepresentation(BsonType.ObjectId)]
        public string CreatorId { get; set; }

        public List<Ingredient> Ingredients { get; set; }

        public Status Status { get; set; } = Status.Pending;
    }
}
