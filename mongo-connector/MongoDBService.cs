using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoConnector.Models;
using Newtonsoft.Json;
using mongo_connector.Models;

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
            var dietData = await this.Diets.Find(_ => true).ToListAsync();
            if (dietData.Count == 0)
            {
                var dietJson = File.ReadAllText("Seeder/dietPreferences.json");
                var dietList = JsonConvert.DeserializeObject<List<Diet>>(dietJson);
                await this.Diets.InsertManyAsync(dietList);
            }
        }

        // Seed data Cuisine
        public async Task SeedCuisineData()
        {
            var cuisineData = await this.Cuisines.Find(_ => true).ToListAsync();
            if (cuisineData.Count == 0)
            {
                var cuisineJson = File.ReadAllText("Seeder/cuisinePreferences.json");
                var cuisineList = JsonConvert.DeserializeObject<List<Cuisine>>(cuisineJson);
                await this.Cuisines.InsertManyAsync(cuisineList);
            }
        }

        // Seed data mealType
        public async Task SeedMealTypeData()
        {
            var mealTypeData = await this.MealTypes.Find(_ => true).ToListAsync();
            if (mealTypeData.Count == 0)
            {
                var mealTypeJson = File.ReadAllText("Seeder/mealTypePreferences.json");
                var mealTypeList = JsonConvert.DeserializeObject<List<MealType>>(mealTypeJson);
                await this.MealTypes.InsertManyAsync(mealTypeList);
            }
        }

        // Seed data allergen
        public async Task SeedAllergenData()
        {
            var allergenData = await this.Allergens.Find(_ => true).ToListAsync();
            if (allergenData.Count == 0)
            {
                var allergenJson = File.ReadAllText("Seeder/allergenPreferences.json");
                var allergenList = JsonConvert.DeserializeObject<List<Allergen>>(allergenJson);
                await this.Allergens.InsertManyAsync(allergenList);
            }
        }

        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return MongoDatabase.GetCollection<T>(collectionName);
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
        public IMongoCollection<CulinaryTip> CulinaryTips
        {
            get
            {
                return MongoDatabase.GetCollection<CulinaryTip>(MongoCollections.CulinaryTips);
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

        public IMongoCollection<Followership> Followerships
        {
            get
            {
                return MongoDatabase.GetCollection<Followership>(MongoCollections.Followerships);
            }
            // set
            // {
            //     var indexKeysDefinition = Builders<Followership>.IndexKeys.Ascending(f => f.FollowerId).Ascending(f => f.FollowingId); // Create index on follower_id and following_id fields
            //     var indexModel = new CreateIndexModel<Followership>(indexKeysDefinition); // Create index model
            //     value.Indexes.CreateOne(indexModel);    // Create index
            // }
        }

        public IMongoCollection<IngredientToRecipes> IngredientToRecipes
        {
            get
            {
                return MongoDatabase.GetCollection<IngredientToRecipes>(MongoCollections.IngredientToRecipes);
            }
        }

    }
}

