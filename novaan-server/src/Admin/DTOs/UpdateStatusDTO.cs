﻿using MongoConnector.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NovaanServer.src.Admin.DTOs
{
    public class UpdateStatusDTO
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string PostId { get; set; }

        public Status Status { get; set; }
        public SubmissionType submissionType { get; set; }

        public string AdminComment { get; set; } = string.Empty;
    }
}

