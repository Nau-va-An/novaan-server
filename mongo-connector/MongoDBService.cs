﻿using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoConnector.Models;
using Newtonsoft.Json;

namespace MongoConnector
{
    public class MongoDBService
    {
        private MongoClient MongoClient { get; }

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
            mongoDbSettings.LinqProvider = MongoDB.Driver.Linq.LinqProvider.V3;

            MongoClient = new MongoClient(mongoDbSettings);
            MongoDatabase = MongoClient.GetDatabase(databaseName);
        }

        public bool PingDatabase()
        {
            var result = MongoDatabase.RunCommand<BsonDocument>(new BsonDocument("ping", 1));
            return true;
        }

        // Seed data Diet
        public async Task SeedDietData()
        {
            var dietCollection = MongoDatabase.GetCollection<Diet>(MongoCollections.Diets);
            var dietData = await dietCollection.Find(_ => true).ToListAsync();
            if (dietData.Count == 0)
            {
                var dietJson = File.ReadAllText("dietPreferences.json");
                var dietList = JsonConvert.DeserializeObject<List<Diet>>(dietJson);
                await dietCollection.InsertManyAsync(dietList);
            }
        }

        public IMongoCollection<Account> Accounts
        {
            get
            {
                return MongoDatabase.GetCollection<Account>(MongoCollections.Accounts);
            }
        }
        public IMongoCollection<Recipe> Recipes
        {
            get
            {
                return MongoDatabase.GetCollection<Recipe>(MongoCollections.Recipes);
            }
        }
        public IMongoCollection<CulinaryTips> CulinaryTips
        {
            get
            {
                return MongoDatabase.GetCollection<CulinaryTips>(MongoCollections.CulinaryTips);
            }
        }
        public IMongoCollection<RefreshToken> RefreshTokens
        {
            get
            {
                return MongoDatabase.GetCollection<RefreshToken>(MongoCollections.RefreshTokens);
            }
        }
    }
}

