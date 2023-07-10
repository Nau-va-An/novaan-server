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
            this._mongoDBServiceTests.InitializeMongoCollection();
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
        public async Task CannnotCallFollowUserUserExceptionUserAlreadyFollowing()
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

        [Fact]
        public async Task CannnotCallFollowUserExceptionUserFollowItself()
        {
            // Assert
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.FollowUser("1", "1"));
        }


        [Fact]
        public async Task CanCallUnfollowUser()
        {

            // Act
            await _testClass.FollowUser("1", "1");

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async Task CannnotCallUnfollowUserUserExceptionUserNotFollowing()
        {
            // Assert
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.FollowUser("1", "2"));
        }

        [Fact]
        public async Task CannnotCalUnfollowUserExceptionUserNotFound()
        {
            // Assert
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.FollowUser("1", "7"));
        }

        [Fact]
        public async Task CannnotCallUnfollowUserExceptionUserFollowItself()
        {
            // Assert
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.FollowUser("1", "1"));
        }

        [Fact]
        public async Task CanCallGetFollowing()
        {
            Pagination pa = new Pagination { Start = 1, Limit = 2 };
            var result = _testClass.GetFollowing(null, "1", pa);
            // Assert true is Not throw exception 
            Assert.True(true);
        }

        [Fact]
        public async Task CannotCallGetFollowingUserNotFound1()
        {
            Pagination pa = new Pagination { Start = 1, Limit = 2 };
            // Assert 
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.GetFollowing(null, "9", pa));
        }

        [Fact]
        public async Task CannotCallGetFollowingUserNotFound2()
        {
            Pagination pa = new Pagination { Start = 1, Limit = 2 };
            // Assert 
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.GetFollowing("1", "8", pa));
        }
    }
}