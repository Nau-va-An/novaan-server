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
        private Mock<IMongoCollection<Followership>> FollowerShips;

        private Mock<IAsyncCursor<Account>> AccountCursor;
        private Mock<IAsyncCursor<User>> UserCursor;
        private Mock<IAsyncCursor<Followership>> FollowershipCursor;


        public List<Account> accountList;
        public List<User> userList;
        public List<Followership>followershipList;


        public MongoDBServiceTests() {
            this.mongodb = new Mock<IMongoDatabase>();
            this.mongoClient = new Mock<IMongoClient>();

            this.Accounts = new Mock<IMongoCollection<Account>>();
            this.AccountCursor = new Mock<IAsyncCursor<Account>>();
            this.accountList = new List<Account>() {
                new Account { Id = "1", Username = "vanhphamjuly", Email = "vanhphamjuly@gmail.com", Password =CustomHash.GetHashString("meocat1807"), Role = Role.User},
                new Account { Id = "2", Username = "anhptvhe150038", Email = "anhptvhe150038@fpt.edu.vn", Password = CustomHash.GetHashString("meocat1807"), Role = Role.User},
                new Account { Id = "3", Username = "duaxaothit1807", Email = "duaxaothit1807@gmail.com", Password =CustomHash.GetHashString("meocat1807"), Role = Role.User},
                new Account { Id = "4", Username = "thanhtra", Email = "thanhtra@gmail.com", Password =CustomHash.GetHashString("meocat1807"), Role = Role.Admin},
                new Account { Id = "5", Username = "Ngoc Mai", Email = "maingoc@gmail.com", Password =CustomHash.GetHashString("meocat1807"), Role = Role.Admin}

            };

            this.Users = new Mock<IMongoCollection<User>>();
            this.UserCursor = new Mock<IAsyncCursor<User>>();
            this.userList = new List<User> {
                new User { Id = "1", AccountID = "1", DisplayName = "vanh pham", FollowerCount = 0, FollowingCount = 0},
                new User { Id = "2", AccountID = "2", DisplayName = "Van Anh", FollowerCount = 0, FollowingCount = 0 },
                new User { Id = "3", AccountID = "3", DisplayName = "Pham Thi Dua", FollowerCount = 0, FollowingCount = 0},
                new User { Id = "4", AccountID = "4", DisplayName = "Pham Thi Van Anh", FollowerCount = 0, FollowingCount = 0},
                new User { Id = "5", AccountID = "5", DisplayName = "mai ngoc", FollowerCount = 0, FollowingCount = 0}

            };

            this.FollowerShips = new Mock<IMongoCollection<Followership>>();
            this.FollowershipCursor = new Mock<IAsyncCursor<Followership>>();
            this.followershipList = new List<Followership>()
            {
                new Followership {Id = "1", FollowerId = "1", FollowingId = "2"},
                new Followership {Id = "2", FollowerId = "2", FollowingId = "1"},
                new Followership {Id = "3", FollowerId = "3", FollowingId = "1"},
                new Followership {Id = "4", FollowerId = "3", FollowingId = "5"}
            };
        }

        public void InitializeMongoDb()
        {
            this.mongodb.Setup(x => x.GetCollection<Account>(MongoCollections.Accounts,
                default)).Returns(this.Accounts.Object);
            this.mongodb.Setup(x => x.GetCollection<User>(MongoCollections.Users,
                default)).Returns(this.Users.Object);
            this.mongoClient.Setup(x => x.GetDatabase(It.IsAny<string>(),
                default)).Returns(this.mongodb.Object);
        }
        public void InitializeMongoAccountCollection()
        {
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
            this.Users.Setup(x => x.FindAsync(u => u.Id == "", It.IsAny<FindOptions<User, User>>(),
                default(CancellationToken))).ReturnsAsync(this.UserCursor.Object);

            this.Users.Setup(x => x.InsertOneAsync(It.IsAny<User>(), It.IsAny<InsertOneOptions>(), default(CancellationToken)))
            .Returns(Task.CompletedTask);

            this.FollowershipCursor.Setup(ac => ac.Current).Returns(this.followershipList);
            this.FollowershipCursor.SetupSequence(ac => ac.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            this.FollowershipCursor.SetupSequence(ac => ac.MoveNextAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(true)).Returns(Task.FromResult(false));
            this.FollowerShips.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<Followership>>(), It.IsAny<FindOptions<Followership, Followership>>(),
                default(CancellationToken))).ReturnsAsync(this.FollowershipCursor.Object);
            this.FollowerShips.Setup(x => x.InsertOneAsync(It.IsAny<Followership>(), It.IsAny<InsertOneOptions>(), default(CancellationToken)))
            .Returns(Task.CompletedTask);


            //Set up the mocks behavior
            var mockUpdateResult = new Mock<UpdateResult>();
            //Set up the mocks behavior
            mockUpdateResult.Setup(_ => _.IsAcknowledged).Returns(true);
            mockUpdateResult.Setup(_ => _.ModifiedCount).Returns(1);
            this.FollowerShips
                .Setup(x => x.UpdateOneAsync(It.IsAny<FilterDefinition<Followership>>(), It.IsAny<UpdateDefinition<Followership>>(), It.IsAny<UpdateOptions>(), 
                It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(mockUpdateResult.Object);



            this.InitializeMongoDb();

        }

    }
}