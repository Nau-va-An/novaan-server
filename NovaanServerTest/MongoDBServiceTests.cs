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
        private Mock<IMongoClient> mongoClient;
        private Mock<IMongoDatabase> mongodb;
        private Mock<IMongoCollection<Account>> Accounts;
        private Mock<IMongoCollection<Recipe>> Recipes;
        private Mock<IMongoCollection<CulinaryTip>> CulinaryTips;
        private Mock<IMongoCollection<Diet>> Diets;
        private Mock<IMongoCollection<Cuisine>> Cuisines;
        private Mock<IMongoCollection<MealType>> MealTypes;
        private Mock<IMongoCollection<RefreshToken>> RefreshTokens;
        private Mock<IMongoCollection<User>> Users;
        private Mock<IMongoCollection<Allergen>> Allergens;
        private Mock<IMongoCollection<Followership>> FollowerShip;
        private Mock<IAsyncCursor<Account>> AccountCursor;
        private Mock<IFindFluent<Account, Account>> AccountFluent;
        private List<Account> accountList;


        public MongoDBServiceTests() {
            this.mongodb = new Mock<IMongoDatabase>();
            this.mongoClient = new Mock<IMongoClient>();
            this.Accounts = new Mock<IMongoCollection<Account>>();
            this.AccountCursor = new Mock<IAsyncCursor<Account>>();
            this.accountList = new List<Account>() {
                new Account { Id = "1", Username = "vanhpham", Email = "vanhphamjuly@gmail.com", Password =CustomHash.GetHashString("meocat1807"), Role = Role.User},
                new Account { Id = "2", Username = "van anh", Email = "anhptvhe150038@fpt.edu.vn", Password = CustomHash.GetHashString("meocat1807"), Role = Role.User},
                new Account { Id = "3", Username = "pham thi van anh", Email = "duaxaothit1807@gmail.com", Password =CustomHash.GetHashString("meocat1807"), Role = Role.User}

        };

        }

        private void InitializeMongoDb()
        {
            this.mongodb.Setup(x => x.GetCollection<Account>(MongoCollections.Accounts,
                default)).Returns(this.Accounts.Object);
            this.mongoClient.Setup(x => x.GetDatabase(It.IsAny<string>(),
                default)).Returns(this.mongodb.Object);
        }
        private void InitializeMongoAccountCollection()
        {
            this.AccountCursor.Setup( ac=> ac.Current).Returns(this.accountList);
            this.AccountCursor.SetupSequence( ac=> ac.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            this.AccountCursor.SetupSequence( ac=> ac.MoveNextAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(true)).Returns(Task.FromResult(false));
            this.Accounts.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<Account>>(), It.IsAny<FindOptions<Account, Account>>(), 
                default(CancellationToken))).ReturnsAsync(this.AccountCursor.Object);
            this.Accounts.Setup(x => x.InsertOneAsync(It.IsAny<Account>(), It.IsAny<InsertOneOptions>(), default(CancellationToken)))
            .Returns(Task.CompletedTask);
            this.Accounts.Setup(c => c.Find(
            It.IsAny<FilterDefinition<Account>>(),
            It.IsAny<FindOptions>())).Returns(this.AccountFluent.Object);

            this.InitializeMongoDb();

        }

        [Fact]
        public async Task CanCallSignInWithCredentials()
        {
            // Arrange
            this.InitializeMongoAccountCollection();
            var mongoDBService = new MongoDBService(this.mongoClient.Object);
            AuthService _testClass = new AuthService(mongoDBService);

            // Act
            SignInDTOs signInDTO = new SignInDTOs { UsernameOrEmail = "vanhphamjuly@gmail.com", Password= "meocat1807" };
            var result = await _testClass.SignInWithCredentials(signInDTO);

            // Assert
            Assert.True(result.Equals("1"));
        }

       
        [Fact]
        public async Task CanCallSignUpWithCredentials()
        {
            // Arrange
            var signUpDTO = new SignUpDTO
            {
                DisplayName = "Vanh Pham",
                Password = "meocat1807",
                Email = "vanh1807@gmail.com"
            };

            this.InitializeMongoAccountCollection();
            var mongoDBService = new MongoDBService(this.mongoClient.Object);
            AuthService _testClass = new AuthService(mongoDBService);
            // Act
            var result = await _testClass.SignUpWithCredentials(signUpDTO);

            // Assert
            Assert.True(result == true);
        }

    }
}