using Microsoft.Extensions.Configuration;
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
                var dietJson = File.ReadAllText("Seeder/dietPreferences.json");
                var dietList = JsonConvert.DeserializeObject<List<Diet>>(dietJson);
                await dietCollection.InsertManyAsync(dietList);
            }
        }

        // Seed data Cuisine
        public async Task SeedCuisineData()
        {
            var cuisineCollection = MongoDatabase.GetCollection<Cuisine>(MongoCollections.Cuisines);
            var cuisineData = await cuisineCollection.Find(_ => true).ToListAsync();
            if (cuisineData.Count == 0)
            {
                var cuisineJson = File.ReadAllText("Seeder/cuisinePreferences.json");
                var cuisineList = JsonConvert.DeserializeObject<List<Cuisine>>(cuisineJson);
                await cuisineCollection.InsertManyAsync(cuisineList);
            }
        }

        // Seed data mealType
        public async Task SeedMealTypeData()
        {
            var mealTypeCollection = MongoDatabase.GetCollection<MealType>(MongoCollections.MealTypes);
            var mealTypeData = await mealTypeCollection.Find(_ => true).ToListAsync();
            if (mealTypeData.Count == 0)
            {
                var mealTypeJson = File.ReadAllText("Seeder/mealTypePreferences.json");
                var mealTypeList = JsonConvert.DeserializeObject<List<MealType>>(mealTypeJson);
                await mealTypeCollection.InsertManyAsync(mealTypeList);
            }
        }

        // Seed data allergen
        public async Task SeedAllergenData()
        {
            var allergenCollection = MongoDatabase.GetCollection<Allergen>(MongoCollections.Allergens);
            var allergenData = await allergenCollection.Find(_ => true).ToListAsync();
            if (allergenData.Count == 0)
            {
                var allergenJson = File.ReadAllText("Seeder/allergenPreferences.json");
                var allergenList = JsonConvert.DeserializeObject<List<Allergen>>(allergenJson);
                await allergenCollection.InsertManyAsync(allergenList);
            }
        }

        // // Seed data user
        // public async Task SeedUserData()
        // {
        //     var userCollection = MongoDatabase.GetCollection<User>(MongoCollections.Users);
        //     var userData = await userCollection.Find(_ => true).ToListAsync();
        //     if (userData.Count == 0)
        //     {
        //         // Seed one user with id from account collection, and some preferences (diet, cuisine, allergen) and display name is from account collection username
        //         var accountCollection = MongoDatabase.GetCollection<Account>(MongoCollections.Accounts);
        //         var user = new User
        //         {
        //             Id = accountCollection.Find(_ => true).FirstOrDefault().Id,
        //             DisplayName = accountCollection.Find(_ => true).FirstOrDefault().Username,
        //             DietID = new List<string> { "1", "2" },
        //             CuisineID = new List<string> { "1", "2" },
        //             AllergenID = new List<string> { "1", "2" }
        //         };

        //     }
        // }

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

        public IMongoCollection<Diet> Diets
        {
            get
            {
                return MongoDatabase.GetCollection<Diet>(MongoCollections.Diets);
            }
        }

        public IMongoCollection<Cuisine> Cuisines
        {
            get
            {
                return MongoDatabase.GetCollection<Cuisine>(MongoCollections.Cuisines);
            }
        }

        public IMongoCollection<MealType> MealTypes
        {
            get
            {
                return MongoDatabase.GetCollection<MealType>(MongoCollections.MealTypes);
            }
        }

        public IMongoCollection<RefreshToken> RefreshTokens
        {
            get
            {
                return MongoDatabase.GetCollection<RefreshToken>(MongoCollections.RefreshTokens);
            }
        }

        public IMongoCollection<User> Users
        {
            get
            {
                return MongoDatabase.GetCollection<User>(MongoCollections.Users);
            }
        }

        public IMongoCollection<Allergen> Allergens
        {
            get
            {
                return MongoDatabase.GetCollection<Allergen>(MongoCollections.Allergens);
            }
        }
    }
}

