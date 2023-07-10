namespace NovaanServerTest.src.Admin
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using MongoConnector;
    using MongoConnector.Enums;
    using NovaanServer.src.Admin;
    using NovaanServer.src.Admin.DTOs;
    using NovaanServer.src.ExceptionLayer.CustomExceptions;
    using Xunit;
    using T = System.String;

    public class AdminServiceTests
    {
        private AdminService _testClass;
        private MongoDBService _mongoService;

        public AdminServiceTests()
        {
            _mongoService = new MongoDBService();
            _testClass = new AdminService(_mongoService);
        }

        [Fact]
        public async Task CanCallGetSubmissionsApprovedAndRejectedStatus()
        {
            // Arrange
            var status = new List<Status>() { Status.Approved, Status.Rejected};

            // Act
            await _testClass.GetSubmissions(status);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async Task CanCallGetSubmissionsWithNullStatus()
        {
            var status = new List<Status>();

            // Act
            await _testClass.GetSubmissions(status);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async Task CanCallGetSubmissionsPending()
        {
            var status = new List<Status>() { Status.Pending};

            // Act
            await _testClass.GetSubmissions(status);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async Task CanCallUpdateStatusRecipe()
        {
            // Arrange
            var statusDTO = new UpdateStatusDTO
            {
                PostId = "1",
                Status = Status.Rejected,
                AdminComment = "Noi dung khong phu hop. Xin hay chinh sua lai de duoc dang len cong dong"
            };
            var collectionName = "Recipe";

            // Act
            await _testClass.UpdateStatus<T>(statusDTO, collectionName);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async Task CanCallUpdateStatusCulinaryTip()
        {
            // Arrange
            var statusDTO = new UpdateStatusDTO
            {
                PostId = "1",
                Status = Status.Rejected,
                AdminComment = "Noi dung khong phu hop. Xin hay chinh sua lai de duoc dang len cong dong"
            };
            var collectionName = "CulinaryTip";

            // Act
            await _testClass.UpdateStatus<T>(statusDTO, collectionName);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async Task CannotCallUpdateStatusWithInvalidCollectionName()
        {
            var collectionName = "CulinaryTips";

            await Assert.ThrowsAsync<NovaanException>(() => _testClass.UpdateStatus<T>(new UpdateStatusDTO
            {
                PostId = "TestValue358502583",
                Status = Status.Approved,
                AdminComment = "TestValue1574970207"
            }, collectionName));
        }
    }
}