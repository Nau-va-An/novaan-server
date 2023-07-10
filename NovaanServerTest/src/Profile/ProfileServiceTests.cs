namespace NovaanServerTest.src.Profile
{
    using System;
    using System.Threading.Tasks;
    using MongoConnector;
    using NovaanServer.src.Common.DTOs;
    using NovaanServer.src.ExceptionLayer.CustomExceptions;
    using NovaanServer.src.Preference;
    using NovaanServer.src.Profile;
    using Xunit;

    public class ProfileServiceTests
    {
        private ProfileService _testClass;
        private MongoDBServiceTests _mongoDBServiceTests = new MongoDBServiceTests();
        public ProfileServiceTests()
        {
            this._mongoDBServiceTests.InitializeMongoCollection();
            var mongoDBService = new MongoDBService(_mongoDBServiceTests.mongoClient.Object);
            _testClass = new ProfileService(mongoDBService);
        }

        [Fact]
        public async Task CanCallGetProfile()
        {
            // Arrange
            var currentUserId = "1";
            var targetUserId = "2";

            // Act
            var result = await _testClass.GetProfile(currentUserId, targetUserId);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async Task CannotCallGetProfileWithInvalidCurrentUserId()
        {
            var targetUserId = "3";
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.GetProfile(null, targetUserId));
        }

        [Fact]
        public async Task CannotCallGetProfileWithInvalidTargetUserId()
        {
            var currentUserId = "1";
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.GetProfile(currentUserId, null));
        }

        [Fact]
        public async Task CanCallGetRecipesWithPagination()
        {
            // Arrange
            var currentUserId = "1";
            var targetUserId = "1";
            var pagination = new Pagination
            {
                Start = 1,
                Limit = 2
            };

            // Act
            var result = await _testClass.GetRecipes(currentUserId, targetUserId, pagination);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async Task CanCallGetRecipesWithoutPagination()
        {
            // Arrange
            var currentUserId = "1";
            var targetUserId = "1";

            var pagination = new Pagination();
            // Act
            var result = await _testClass.GetRecipes(currentUserId, targetUserId, pagination);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async Task CannotCallGetRecipesWithInvalidCurrentUserId()
        {
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.GetRecipes(null, "1", new Pagination
            {
                Start = 1,
                Limit = 2
            }));
        }

        [Fact]
        public async Task CannotCallGetRecipesWithInvalidTargetUserId()
        {
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.GetRecipes("1", null, new Pagination
            {
                Start = 1,
                Limit = 2
            }));
        }

        [Fact]
        public async Task CanCallGetSavedPosts()
        {
            // Arrange
            var currentUserId = "1";
            var targetUserId = "1";
            var pagination = new Pagination
            {
                Start = 1,
                Limit = 2
            };

            // Act
            var result = await _testClass.GetSavedPosts(currentUserId, targetUserId, pagination);

            // Assert
            Assert.True(true);
        }



        [Fact]
        public async Task CannotCallGetSavedPostsForbidenContent()
        {
            var currentUserId = "2";
            var targetUserId = "1";
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.GetSavedPosts(currentUserId, targetUserId, new Pagination
            {
                Start = 1,
                Limit = 2
            }));
        }

        [Fact]
        public async Task CannotCallGetSavedPostsWithInvalidTargetUserId()
        {
            var currentUserId = "1";
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.GetSavedPosts(currentUserId, null, new Pagination
            {
                Start = 1,
                Limit = 2
            }));
        }

        [Fact]
        public async Task CannotCallGetSavedPostsWithInvalidCurrentUserId()
        {
            var targetUserId = "2";
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.GetSavedPosts(null, targetUserId, new Pagination
            {
                Start = 1,
                Limit = 2
            }));
        }
        [Fact]
        public async Task CanCallGetTips()
        {
            // Arrange
            var currentUserId = "1";
            var targetUserId = "2";
            var pagination = new Pagination
            {
                Start = 1,
                Limit = 2
            };

            // Act
            var result = await _testClass.GetTips(currentUserId, targetUserId, pagination);

            // Assert
            Assert.True(true);
        }


        [Fact]
        public async Task CannotCallGetTipsWithInvalidCurrentUserId()
        {
            var targetUserId = "2";
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.GetTips(null, targetUserId, new Pagination
            {
                Start = 421612853,
                Limit = 1910405784
            }));
        }

        [Fact]
        public async Task CannotCallGetTipsWithInvalidTargetUserId()
        {
            var currentUserId = "1";
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.GetTips(currentUserId, null, new Pagination
            {
                Start = 1,
                Limit = 2
            }));
        }
    }
}