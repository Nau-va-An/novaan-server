namespace NovaanServerTest.src.Content
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using MongoConnector;
    using MongoConnector.Enums;
    using MongoConnector.Models;
    using NovaanServer.src.Content;
    using NovaanServer.src.Content.DTOs;
    using NovaanServer.src.ExceptionLayer.CustomExceptions;
    using S3Connector;
    using Xunit;
    using T = System.String;

    public class ContentServiceTests
    {
        private ContentService _testClass;
        private MongoDBService _mongoDBService;
        private S3Service _s3Service;

        public ContentServiceTests()
        {
            _mongoDBService = new MongoDBService();
            _s3Service = new S3Service();
            _testClass = new ContentService(_mongoDBService, _s3Service);
        }


        [Fact]
        public void CanCallValidateFileMetadataVideo()
        {
            // Arrange
            var fileMetadataDTO = new FileInformationDTO
            {
                FileSize = 518400L,
                Width = 960,
                Height = 540,
                FileExtension = ".mp4",
                VideoDuration = TimeSpan.FromSeconds(90)
            };

            // Act
            var result = _testClass.ValidateFileMetadata(fileMetadataDTO);

            // Assert
            Assert.True(result == true);
        }

        [Fact]
        public void CanCallValidateFileMetadataImage()
        {
            // Arrange
            var fileMetadataDTO = new FileInformationDTO
            {
                FileSize = 518400L,
                Width = 960,
                Height = 540,
                FileExtension = ".jpg"
            };

            // Act
            var result = _testClass.ValidateFileMetadata(fileMetadataDTO);

            // Assert
            Assert.True(result == true);
        }
        [Fact]
        public void CannotCallValidateFileMetadataInvalidFileExtension()
        {
            // Arrange
            var fileMetadataDTO = new FileInformationDTO
            {
                FileSize = 518400L,
                Width = 960,
                Height = 540,
                FileExtension = ".txt"
            };

            // Assert
            Assert.Throws<NovaanException>(() => _testClass.ValidateFileMetadata(fileMetadataDTO));
        }

        [Fact]
        public void CannotCallValidateFileMetadataInvalidResoImage()
        {
            // Arrange
            var fileMetadataDTO = new FileInformationDTO
            {
                FileSize = 518400L,
                Width = 640,
                Height = 470,
                FileExtension = ".jpg"
            };


            // Assert
            Assert.Throws<NovaanException>(() => _testClass.ValidateFileMetadata(fileMetadataDTO));
        }

        [Fact]
        public void CannotCallValidateFileMetadataInvalidSizeImage()
        {
            // Arrange
            var fileMetadataDTO = new FileInformationDTO
            {
                FileSize = 6L*1024L*1024L,
                Width = 1024,
                Height = 960,
                FileExtension = ".jpg"
            };

            // Assert
            Assert.Throws<NovaanException>(() => _testClass.ValidateFileMetadata(fileMetadataDTO));
        }

        [Fact]
        public void CannotCallValidateFileMetadataInvalidResoVideo()
        {
            // Arrange
            var fileMetadataDTO = new FileInformationDTO
            {
                FileSize = 518400L,
                Width = 720,
                Height = 720,
                VideoDuration = TimeSpan.FromSeconds(90),
                FileExtension = ".mp4"
            };


            // Assert
            Assert.Throws<NovaanException>(() => _testClass.ValidateFileMetadata(fileMetadataDTO));
        }
        [Fact]
        public void CannotCallValidateFileMetadataInvalidLenVideo()
        {
            // Arrange
            var fileMetadataDTO = new FileInformationDTO
            {
                FileSize = 518400L,
                Width = 720,
                Height = 720,
                FileExtension = ".mp4",
                VideoDuration = TimeSpan.FromSeconds(180)
            };


            // Assert
            Assert.Throws<NovaanException>(() => _testClass.ValidateFileMetadata(fileMetadataDTO));
        }

        [Fact]
        public void CannotCallValidateFileMetadataInvalidSizeVideo()
        {
            // Arrange
            var fileMetadataDTO = new FileInformationDTO
            {
                FileSize = 21L*1024L*1024L,
                Width = 720,
                Height = 720,
                FileExtension = ".mp4",
                VideoDuration = TimeSpan.FromSeconds(120)
            };

            // Assert
            Assert.Throws<NovaanException>(() => _testClass.ValidateFileMetadata(fileMetadataDTO));
        }

        [Fact]
        public async Task CanCallUploadRecipe()
        {
            // Arrange
            var recipe = new Recipe
            {
                Id = "1",
                CreatorId = "1",
                Title = "Gà xào sả ớt",
                Description = "Đây là món gà xào sả ớt cực ngon miệng, cả nhà đều thích. Chúc bạn thành công.",
                Video = "TestValue1430172362",
                Difficulty = Difficulty.Easy,
                PortionQuantity = 2,
                PortionType = PortionType.Pieces,
                PrepTime = TimeSpan.FromSeconds(1800),
                CookTime = TimeSpan.FromSeconds(1200),
                Instructions = new List<Instruction>(),
                Ingredients = new List<Ingredient>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                AdminComment = new List<AdminComment>()
            };
            var creatorId = "1";

            // Act
            await _testClass.UploadRecipe(recipe, creatorId);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async Task CannotCallUploadRecipeWithInvalidDescription()
        {
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.UploadRecipe(new Recipe
            {
                Id = "1",
                CreatorId = "2",
                Title = "Ga xao sa ot",
                Description = null,
                Video = "TestValue504537900",
                Difficulty = Difficulty.Easy,
                PortionQuantity = 2,
                PortionType = PortionType.Pieces,
                PrepTime = TimeSpan.FromSeconds(1800),
                CookTime = TimeSpan.FromSeconds(1200),
                Instructions = new List<Instruction>(),
                Ingredients = new List<Ingredient>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            }, "1"));
        }

        [Fact]
        public async Task CannotCallUploadRecipeWithInvalidPortionQuantity()
        {
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.UploadRecipe(new Recipe
            {
                Id = "1",
                CreatorId = "2",
                Title = "Ga xao sa ot",
                Description = "Đây là món gà xào sả ớt cực ngon miệng, cả nhà đều thích. Chúc bạn thành công.",
                Video = "TestValue504537900",
                Difficulty = Difficulty.Easy,
                PortionQuantity = 2222222,
                PortionType = PortionType.Pieces,
                PrepTime = TimeSpan.FromSeconds(1800),
                CookTime = TimeSpan.FromSeconds(1200),
                Instructions = new List<Instruction>(),
                Ingredients = new List<Ingredient>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            }, "1"));
        }

        [Fact]
        public async Task CannotCallUploadRecipeWithInvalidPrepTime()
        {
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.UploadRecipe(new Recipe
            {
                Id = "1",
                CreatorId = "2",
                Title = "Ga xao sa ot",
                Description = "Đây là món gà xào sả ớt cực ngon miệng, cả nhà đều thích. Chúc bạn thành công.",
                Video = "TestValue504537900",
                Difficulty = Difficulty.Easy,
                PortionQuantity = 2,
                PortionType = PortionType.Pieces,
                PrepTime = TimeSpan.FromDays(3),
                CookTime = TimeSpan.FromSeconds(1200),
                Instructions = new List<Instruction>(),
                Ingredients = new List<Ingredient>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            }, "1"));
        }
        [Fact]
        public async Task CannotCallUploadRecipeWithInvalidCookTime()
        {
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.UploadRecipe(new Recipe
            {
                Id = "1",
                CreatorId = "2",
                Title = "Ga xao sa ot",
                Description = "Đây là món gà xào sả ớt cực ngon miệng, cả nhà đều thích. Chúc bạn thành công.",
                Video = "TestValue504537900",
                Difficulty = Difficulty.Easy,
                PortionQuantity = 2,
                PortionType = PortionType.Pieces,
                PrepTime = TimeSpan.FromSeconds(1800),
                CookTime = TimeSpan.FromDays(3),
                Instructions = new List<Instruction>(),
                Ingredients = new List<Ingredient>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            }, "1"));
        }
        [Fact]
        public async Task CanCallUploadTips()
        {
            // Arrange
            var culinaryTips = new CulinaryTip
            {
                Id = "1",
                CreatorId = "2",
                Title = "Cách chặt gà sao cho đẹp",
                Description = "CHuẩn bị một con dao thật sắc và một chiếc đĩa phẳng.",
                Video = "TestValue1860071024",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            var creatorId = "1";

            // Act
            await _testClass.UploadTips(culinaryTips, creatorId);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async Task CannotCallUploadTipsWithNullVideo()
        {
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.UploadTips( new CulinaryTip
            {
                Id = "1",
                Title = "Cách chặt gà sao cho đẹp",
                Description = "CHuẩn bị một con dao thật sắc và một chiếc đĩa phẳng.",
                Video = "bbbb",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            }, "1"));
        }

        [Fact]
        public async Task CannotCallUploadTipsWithEmptyTitle()
        {
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.UploadTips(new CulinaryTip
            {
                Id = "1",
                Title = "",
                Description = "CHuẩn bị một con dao thật sắc và một chiếc đĩa phẳng.",
                Video = "bbbb",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            }, "1"));
        }


        [Fact]
        public async Task CannotCallUploadTipsWithInvalidDescription()
        {
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.UploadTips(new CulinaryTip
            {
                Id = "1",
                Title = "Cách chặt gà sao cho đẹp",
                Description = "Khum có",
                Video = "bbbb",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            }, "1"));
        }

        [Fact]
        public async Task CannotCallUploadTipsWithInvalidCreatorId()
        {
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.UploadTips(new CulinaryTip
            {
                Id = "TestValue794269415",
                CreatorId = "TestValue250223879",
                Title = "TestValue1886051818",
                Description = "CHuẩn bị một con dao thật sắc và một chiếc đĩa phẳng.",
                Video = "TestValue1866478995",
                Status = Status.Rejected,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                AdminComment = new List<AdminComment>()
            }, null));
        }

        [Fact]
        public async Task CanCallGetPersonalReel()
        {
            // Arrange
            var userId = "1";

            // Act
            var result = await _testClass.GetPersonalReel(userId);

            // Assert
            Assert.True(true);
        }

    }
}