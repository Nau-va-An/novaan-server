namespace NovaanServerTest
{
    using System;
    using System.ComponentModel;
    using System.Security.AccessControl;
    using System.Threading.Tasks;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using MongoConnector;
    using MongoConnector.Models;
    using MongoDB.Driver;
    using Moq;
    using NovaanServer.Auth;
    using NovaanServer.Auth.DTOs;
    using Utils.Hash;
    using Xunit;
    using T = System.String;

    public class MongoDBServiceTests
    {
        public Mock<IMongoClient> mongoClient { get; }
        public Mock<IMongoDatabase> mongodb;
        public Mock<IMongoCollection<Account>> Accounts;
        private Mock<IMongoCollection<Recipe>> Recipes;
        private Mock<IMongoCollection<CulinaryTip>> CulinaryTips;
        private Mock<IMongoCollection<Diet>> Diets;
        private Mock<IMongoCollection<Cuisine>> Cuisines;
        private Mock<IMongoCollection<MealType>> MealTypes;
        private Mock<IMongoCollection<RefreshToken>> RefreshTokens;
        private Mock<IMongoCollection<User>> Users;
        private Mock<IMongoCollection<Allergen>> Allergens;
        private Mock<IMongoCollection<Followership>> Followerships;

        private Mock<IAsyncCursor<Account>> AccountCursor;
        private Mock<IAsyncCursor<User>> UserCursor;
        private Mock<IAsyncCursor<Followership>> FollowershipCursor;
        private Mock<IAsyncCursor<MealType>> MealTypeCursor;
        private Mock<IAsyncCursor<Diet>> DietCursor;
        private Mock<IAsyncCursor<Cuisine>> CuisineCursor;
        private Mock<IAsyncCursor<Allergen>> AllergenCursor;
        private Mock<IAsyncCursor<Recipe>> RecipeCursor;
        private Mock<IAsyncCursor<CulinaryTip>> CulinaryTipCursor;


        public List<Account> accountList;
        public List<User> userList;
        public List<Followership> followershipList;
        public List<MealType> mealTypeList;
        public List<Diet> dietList;
        public List<Cuisine> cuisineList;
        public List<Allergen> allergenList;
        public List<Recipe> recipeList;
        public List<CulinaryTip> culinaryTipList;


        public MongoDBServiceTests() {
            this.mongodb = new Mock<IMongoDatabase>();
            this.mongoClient = new Mock<IMongoClient>();

            this.Accounts = new Mock<IMongoCollection<Account>>();
            this.AccountCursor = new Mock<IAsyncCursor<Account>>();
            this.accountList = new List<Account>() {
                new Account { Id = "1", UserId = "1", Email = "vanhphamjuly@gmail.com", Password =CustomHash.GetHashString("meocat1807"), Role = Role.User},
                new Account { Id = "2", UserId = "2", Email = "anhptvhe150038@fpt.edu.vn", Password = CustomHash.GetHashString("meocat1807"), Role = Role.User},
                new Account { Id = "3", UserId = "3", Email = "duaxaothit1807@gmail.com", Password =CustomHash.GetHashString("meocat1807"), Role = Role.User},
                new Account { Id = "4", UserId = "4", Email = "anhptvhe@fpt.edu.vn", Password = CustomHash.GetHashString("meocat1807"), Role = Role.User},
                new Account { Id = "5", UserId = "5", Email = "ngocmai@gmail.com", Password =CustomHash.GetHashString("meocat1807"), Role = Role.User},
                new Account { Id = "6",  Email = "thanhtra@gmail.com", Password =CustomHash.GetHashString("meocat1807"), Role = Role.Admin},
                new Account { Id = "7",  Email = "maingoc@gmail.com", Password =CustomHash.GetHashString("meocat1807"), Role = Role.Admin}

            };

            this.Users = new Mock<IMongoCollection<User>>();
            this.UserCursor = new Mock<IAsyncCursor<User>>();
            this.userList = new List<User> {
                new User { Id = "1", DisplayName = "vanh pham", FollowerCount = 0, FollowingCount = 0},
                new User { Id = "2", DisplayName = "Van Anh", FollowerCount = 0, FollowingCount = 0 },
                new User { Id = "3", DisplayName = "Pham Thi Dua", FollowerCount = 0, FollowingCount = 0},
                new User { Id = "4", DisplayName = "Pham Thi Van Anh", FollowerCount = 0, FollowingCount = 0},
                new User { Id = "5", DisplayName = "mai ngoc", FollowerCount = 0, FollowingCount = 0}

            };

            this.Followerships = new Mock<IMongoCollection<Followership>>();
            this.FollowershipCursor = new Mock<IAsyncCursor<Followership>>();
            this.followershipList = new List<Followership>()
            {
                new Followership {Id = "1", FollowerId = "1", FollowingId = "2"},
                new Followership {Id = "2", FollowerId = "2", FollowingId = "1"},
                new Followership {Id = "3", FollowerId = "3", FollowingId = "1"},
                new Followership {Id = "4", FollowerId = "3", FollowingId = "5"}
            };

            this.MealTypes = new Mock<IMongoCollection<MealType>>();
            this.MealTypeCursor = new Mock<IAsyncCursor<MealType>>();
            this.mealTypeList = new List<MealType>()
            {
                new MealType {Id = "1", Title = "Bữa sáng"},
                new MealType {Id = "2", Title = "Bữa trưa"},
                new MealType {Id = "3", Title = "Bữa tối"}
            };

            this.Cuisines = new Mock<IMongoCollection<Cuisine>>();
            this.CuisineCursor = new Mock<IAsyncCursor<Cuisine>>();
            this.cuisineList = new List<Cuisine>()
            {
                new Cuisine {Id = "1", Title = "Việt Nam", Description = "Món ăn Việt Nam thường đa dạng hương vị, đặc trưng bởi các món ăn nhiệt đới"},
                new Cuisine {Id = "2", Title = "Trung Quốc"},
                new Cuisine {Id = "3",Title = "Nhật Bản"},
                new Cuisine {Id = "4", Title = "Hàn Quốc"}
            };

            this.Diets = new Mock<IMongoCollection<Diet>>();
            this.DietCursor = new Mock<IAsyncCursor<Diet>>();
            this.dietList = new List<Diet>()
            {
                new Diet {Id = "1", Title = "Ăn chay"},
                new Diet {Id = "2", Title = "Ăn không chất đạm"},
                new Diet {Id = "3", Title = "Ăn không gluten"}
            };

            this.Allergens = new Mock<IMongoCollection<Allergen>>();
            this.AllergenCursor = new Mock<IAsyncCursor<Allergen>>();
            this.allergenList = new List<Allergen>()
            {
                new Allergen {Id = "1", Title = "Đậu nành"},
                new Allergen {Id = "2", Title = "Sữa" }
            };

            this.Recipes = new Mock<IMongoCollection<Recipe>>();
            this.RecipeCursor = new Mock<IAsyncCursor<Recipe>>();
            this.recipeList = new List<Recipe>()
            {
                new Recipe {Id = "1", Title = "Gà xào sả ớt"},
                new Recipe {Id = "2", Title = "Mật ong chanh đào"}
            };

            this.CulinaryTips = new Mock<IMongoCollection<CulinaryTip>>();
            this.CulinaryTipCursor = new Mock<IAsyncCursor<CulinaryTip>>();
            this.culinaryTipList = new List<CulinaryTip>()
            {
                new CulinaryTip {Id = "1", Title = "Gà xào sả ớt"},
                new CulinaryTip {Id = "2", Title = "Mật ong chanh đào"}
            };
        }

        public void InitializeMongoDb()
        {
            this.mongodb.Setup(x => x.GetCollection<Account>(MongoCollections.Accounts,
                default)).Returns(this.Accounts.Object);
            this.mongodb.Setup(x => x.GetCollection<User>(MongoCollections.Users,
                default)).Returns(this.Users.Object);
            this.mongodb.Setup(x => x.GetCollection<Followership>(MongoCollections.Followerships,
                default)).Returns(this.Followerships.Object);
            this.mongodb.Setup(x => x.GetCollection<Diet>(MongoCollections.Diets,
                 default)).Returns(this.Diets.Object);
            this.mongodb.Setup(x => x.GetCollection<Cuisine>(MongoCollections.Cuisines,
                default)).Returns(this.Cuisines.Object);
            this.mongodb.Setup(x => x.GetCollection<MealType>(MongoCollections.MealTypes,
                default)).Returns(this.MealTypes.Object);
            this.mongodb.Setup(x => x.GetCollection<Allergen>(MongoCollections.Allergens,
                default)).Returns(this.Allergens.Object);
            this.mongodb.Setup(x => x.GetCollection<Recipe>(MongoCollections.Recipes,
                default)).Returns(this.Recipes.Object);
            this.mongodb.Setup(x => x.GetCollection<CulinaryTip>(MongoCollections.CulinaryTips,
                default)).Returns(this.CulinaryTips.Object);

            this.mongoClient.Setup(x => x.GetDatabase(It.IsAny<string>(),
                default)).Returns(this.mongodb.Object);
        }
        public void InitializeMongoCollection()
        {
            //Set up the mocks behavior
            var mockUpdateResult = new Mock<UpdateResult>();
            //Set up the mocks behavior
            mockUpdateResult.Setup(_ => _.IsAcknowledged).Returns(true);
            mockUpdateResult.Setup(_ => _.ModifiedCount).Returns(1);

            this.AccountCursor.Setup( ac=> ac.Current).Returns(this.accountList);
            this.AccountCursor.SetupSequence( ac=> ac.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            this.AccountCursor.SetupSequence( ac=> ac.MoveNextAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(true)).Returns(Task.FromResult(false));
            this.Accounts.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<Account>>(), It.IsAny<FindOptions<Account, Account>>(), 
                default(CancellationToken))).ReturnsAsync(this.AccountCursor.Object);
            this.Accounts.Setup(x => x.InsertOneAsync(It.IsAny<Account>(), It.IsAny<InsertOneOptions>(), default(CancellationToken)))
            .Returns(Task.CompletedTask);

            this.UserCursor.Setup(ac => ac.Current).Returns(this.userList);
            this.UserCursor.SetupSequence(ac => ac.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            this.UserCursor.SetupSequence(ac => ac.MoveNextAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(true)).Returns(Task.FromResult(false));
            this.Users.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<User>>(), It.IsAny<FindOptions<User, User>>(),
                default(CancellationToken))).ReturnsAsync(this.UserCursor.Object);
            this.Users.Setup(x => x.InsertOneAsync(It.IsAny<User>(), It.IsAny<InsertOneOptions>(), default(CancellationToken)))
            .Returns(Task.CompletedTask);
            this.Users.Setup(x => x.UpdateOneAsync(It.IsAny<FilterDefinition<User>>(), It.IsAny<UpdateDefinition<User>>(), It.IsAny<UpdateOptions>(),
                It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(mockUpdateResult.Object);

            this.FollowershipCursor.Setup(ac => ac.Current).Returns(this.followershipList);
            this.FollowershipCursor.SetupSequence(ac => ac.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            this.FollowershipCursor.SetupSequence(ac => ac.MoveNextAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(true)).Returns(Task.FromResult(false));
            this.Followerships.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<Followership>>(), It.IsAny<FindOptions<Followership, Followership>>(),
                default(CancellationToken))).ReturnsAsync(this.FollowershipCursor.Object);
            this.Followerships.Setup(x => x.InsertOneAsync(It.IsAny<Followership>(), It.IsAny<InsertOneOptions>(), default(CancellationToken)))
            .Returns(Task.CompletedTask);
            this.Followerships
                .Setup(x => x.UpdateOneAsync(It.IsAny<FilterDefinition<Followership>>(), It.IsAny<UpdateDefinition<Followership>>(), It.IsAny<UpdateOptions>(), 
                It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(mockUpdateResult.Object);


            this.DietCursor.Setup(acc => acc.Current).Returns(this.dietList);
            this.DietCursor.SetupSequence(ac => ac.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            this.DietCursor.SetupSequence(ac => ac.MoveNextAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(true)).Returns(Task.FromResult(false));
            this.Diets.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<Diet>>(), It.IsAny<FindOptions<Diet, Diet>>(),
                default(CancellationToken))).ReturnsAsync(this.DietCursor.Object);

            this.MealTypeCursor.Setup(acc => acc.Current).Returns(this.mealTypeList);
            this.MealTypeCursor.SetupSequence(ac => ac.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            this.MealTypeCursor.SetupSequence(ac => ac.MoveNextAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(true)).Returns(Task.FromResult(false));
            this.MealTypes.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<MealType>>(), It.IsAny<FindOptions<MealType, MealType>>(),
                default(CancellationToken))).ReturnsAsync(this.MealTypeCursor.Object);

            this.CuisineCursor.Setup(acc => acc.Current).Returns(this.cuisineList);
            this.CuisineCursor.SetupSequence(ac => ac.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            this.CuisineCursor.SetupSequence(ac => ac.MoveNextAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(true)).Returns(Task.FromResult(false));
            this.Cuisines.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<Cuisine>>(), It.IsAny<FindOptions<Cuisine, Cuisine>>(),
                default(CancellationToken))).ReturnsAsync(this.CuisineCursor.Object);

            this.AllergenCursor.Setup(acc => acc.Current).Returns(this.allergenList);
            this.AllergenCursor.SetupSequence(ac => ac.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            this.AllergenCursor.SetupSequence(ac => ac.MoveNextAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(true)).Returns(Task.FromResult(false));
            this.Allergens.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<Allergen>>(), It.IsAny<FindOptions<Allergen, Allergen>>(),
                default(CancellationToken))).ReturnsAsync(this.AllergenCursor.Object);

            this.RecipeCursor.Setup(acc => acc.Current).Returns(this.recipeList);
            this.RecipeCursor.SetupSequence(ac => ac.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            this.RecipeCursor.SetupSequence(ac => ac.MoveNextAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(true)).Returns(Task.FromResult(false));
            this.Recipes.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<Recipe>>(), It.IsAny<FindOptions<Recipe, Recipe>>(),
                default(CancellationToken))).ReturnsAsync(this.RecipeCursor.Object);

            this.CulinaryTipCursor.Setup(acc => acc.Current).Returns(this.culinaryTipList);
            this.CulinaryTipCursor.SetupSequence(ac => ac.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            this.CulinaryTipCursor.SetupSequence(ac => ac.MoveNextAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(true)).Returns(Task.FromResult(false));
            this.CulinaryTips.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<CulinaryTip>>(), It.IsAny<FindOptions<CulinaryTip, CulinaryTip>>(),
                default(CancellationToken))).ReturnsAsync(this.CulinaryTipCursor.Object);

            this.InitializeMongoDb();

        }

    }
}