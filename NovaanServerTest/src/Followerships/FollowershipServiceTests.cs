namespace NovaanServerTest.src.Followerships
{
    using System;
    using System.Threading.Tasks;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using MongoConnector;
    using MongoDB.Driver.Core.Misc;
    using NovaanServer.Auth;
    using NovaanServer.src.Common.DTOs;
    using NovaanServer.src.ExceptionLayer.CustomExceptions;
    using NovaanServer.src.Followerships;
    using Xunit;

    public class FollowershipServiceTests
    {
        private FollowershipService _testClass;
        private MongoDBService _mongoDBService;
        private MongoDBServiceTests _mongoDBServiceTests = new MongoDBServiceTests();

        public FollowershipServiceTests()
        {
            this._mongoDBServiceTests.InitializeMongoAccountCollection();
            var mongoDBService = new MongoDBService(_mongoDBServiceTests.mongoClient.Object);
            _testClass = new FollowershipService(mongoDBService);
        }


        [Fact]
        public async Task CanCallFollowUser()
        {

            // Act
            await _testClass.FollowUser("1", "1");

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async Task CannnotCallFollowUserUserExceptionUserFollowed()
        {
            // Assert
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.FollowUser("1", "2"));
        }

        [Fact]
        public async Task CannnotCallFollowUserExceptionUserNotFound()
        {
            // Assert
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.FollowUser("1", "7"));
        }
        /*
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallFollowUserWithInvalidCurrentUserID(string value)
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            await Assert.ThrowsAsync<ArgumentNullException>(() => _testClass.FollowUser(value, fixture.Create<string>()));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallFollowUserWithInvalidFollowingUserId(string value)
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            await Assert.ThrowsAsync<ArgumentNullException>(() => _testClass.FollowUser(fixture.Create<string>(), value));
        }

        [Fact]
        public async Task CanCallUnfollowUser()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var currentUserID = fixture.Create<string>();
            var followingUserId = fixture.Create<string>();

            // Act
            await _testClass.UnfollowUser(currentUserID, followingUserId);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallUnfollowUserWithInvalidCurrentUserID(string value)
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            await Assert.ThrowsAsync<ArgumentNullException>(() => _testClass.UnfollowUser(value, fixture.Create<string>()));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallUnfollowUserWithInvalidFollowingUserId(string value)
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            await Assert.ThrowsAsync<ArgumentNullException>(() => _testClass.UnfollowUser(fixture.Create<string>(), value));
        }

        [Fact]
        public async Task CanCallGetFollowers()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var currentUserID = fixture.Create<string>();
            var userId = fixture.Create<string>();
            var pagination = fixture.Create<Pagination>();

            // Act
            var result = await _testClass.GetFollowers(currentUserID, userId, pagination);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public async Task CannotCallGetFollowersWithNullPagination()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            await Assert.ThrowsAsync<ArgumentNullException>(() => _testClass.GetFollowers(fixture.Create<string>(), fixture.Create<string>(), default(Pagination)));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetFollowersWithInvalidCurrentUserID(string value)
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            await Assert.ThrowsAsync<ArgumentNullException>(() => _testClass.GetFollowers(value, fixture.Create<string>(), fixture.Create<Pagination>()));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetFollowersWithInvalidUserId(string value)
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            await Assert.ThrowsAsync<ArgumentNullException>(() => _testClass.GetFollowers(fixture.Create<string>(), value, fixture.Create<Pagination>()));
        }

        [Fact]
        public async Task CanCallGetFollowing()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var currentUserID = fixture.Create<string>();
            var userId = fixture.Create<string>();
            var pagination = fixture.Create<Pagination>();

            // Act
            var result = await _testClass.GetFollowing(currentUserID, userId, pagination);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public async Task CannotCallGetFollowingWithNullPagination()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            await Assert.ThrowsAsync<ArgumentNullException>(() => _testClass.GetFollowing(fixture.Create<string>(), fixture.Create<string>(), default(Pagination)));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetFollowingWithInvalidCurrentUserID(string value)
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            await Assert.ThrowsAsync<ArgumentNullException>(() => _testClass.GetFollowing(value, fixture.Create<string>(), fixture.Create<Pagination>()));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallGetFollowingWithInvalidUserId(string value)
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            await Assert.ThrowsAsync<ArgumentNullException>(() => _testClass.GetFollowing(fixture.Create<string>(), value, fixture.Create<Pagination>()));
        }

    */
    }
}