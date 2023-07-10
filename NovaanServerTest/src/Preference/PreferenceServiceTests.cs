namespace NovaanServerTest.src.Preference
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using MongoConnector;
    using NovaanServer.src.ExceptionLayer.CustomExceptions;
    using NovaanServer.src.Followerships;
    using NovaanServer.src.Preference;
    using NovaanServer.src.Preference.DTOs;
    using Xunit;

    public class PreferenceServiceTests
    {

        private PreferenceService _testClass;
        private MongoDBServiceTests _mongoDBServiceTests = new MongoDBServiceTests();


        public PreferenceServiceTests()
        {

            this._mongoDBServiceTests.InitializeMongoCollection();
            var mongoDBService = new MongoDBService(_mongoDBServiceTests.mongoClient.Object);
            _testClass = new PreferenceService(mongoDBService);
        }

        [Fact]
        public async Task CanCallGetAllPreferences()
        {
            // Act
            await _testClass.GetAllPreferences();

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async Task CanCallGetUserPreferences()
        {
            // Arrange
            var userId = "64a54e3140729cd305564b54";

            // Act
            await _testClass.GetUserPreferences(userId);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async Task CannotCallGetUserPreferencesWithInvalidUserId()
        {
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.GetUserPreferences(null));
        }

        [Fact]
        public async Task CannotCallGetUserPreferencesUserNotFound()
        {
            var userId = "123";
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.GetUserPreferences(userId));
        }

        [Fact]
        public async Task CanCallUpdateUserPreferences()
        {
            // Arrange
            var userId = "64a54e3140729cd305564b54";
            var userPreferenceDTO = new UserPreferenceDTO
            {
                Diets = new HashSet<string>(),
                Cuisines = new HashSet<string>(),
                Allergens = new HashSet<string>()
            };

            // Act
            await _testClass.UpdateUserPreferences(userId, userPreferenceDTO);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async Task CannotCallUpdateUserPreferencesWithInvalidUserId()
        {

            await Assert.ThrowsAsync<NovaanException>(() => _testClass.UpdateUserPreferences(null, new UserPreferenceDTO
            {
                Diets = new HashSet<string>(),
                Cuisines = new HashSet<string>(),
                Allergens = new HashSet<string>()
            }));
        }

        [Fact]
        public async Task CannotCallUpdateUserPreferencesUserNotFound()
        {
            var userId = "123";
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.UpdateUserPreferences(userId, new UserPreferenceDTO
            {
                Diets = new HashSet<string>(),
                Cuisines = new HashSet<string>(),
                Allergens = new HashSet<string>()
            }));
        }
    }
}