﻿using System;
using Utils.Json;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDB.Bson;

namespace MongoConnector
{
    public class MongoDBService
    {
        public MongoClient MongoClient { get; }

        public IMongoDatabase MongoDatabase { get; }

        public MongoDBService()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var config = builder.Build();

            var databaseURI = config.GetSection("MongoDB:ConnectionURI").Value;
            var databaseName = config.GetSection("MongoDB:DatabaseName").Value;
            if (databaseURI == null || databaseName == null)
            {
                throw new Exception("Cannot read MongoDB settings in appsettings");
            }

            var mongoDbSettings = MongoClientSettings.FromConnectionString(databaseURI);
            mongoDbSettings.ServerApi = new ServerApi(ServerApiVersion.V1);

            MongoClient = new MongoClient(mongoDbSettings);
            MongoDatabase = MongoClient.GetDatabase(databaseName);
        }

        public bool PingDatabase()
        {
            var result = MongoDatabase.RunCommand<BsonDocument>(new BsonDocument("ping", 1));
            return true;
        }
    }
}

