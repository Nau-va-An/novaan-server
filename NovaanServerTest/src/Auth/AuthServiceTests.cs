namespace NovaanServerTest.Auth
{
    using System;
    using System.Threading.Tasks;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using MongoConnector;
    using NovaanServer.Auth;
    using NovaanServer.Auth.DTOs;
    using NovaanServer.src.Auth.DTOs;
    using NovaanServer.src.ExceptionLayer.CustomExceptions;
    using Xunit;

    public class AuthServiceTests
    {
        private AuthService _testClass;
        private MongoDBService _mongoDBService;
        private MongoDBServiceTests _mongoDBServiceTests = new MongoDBServiceTests();

        public AuthServiceTests()
        {
            this._mongoDBServiceTests.InitializeMongoCollection();
            var mongoDBService = new MongoDBService(_mongoDBServiceTests.mongoClient.Object);
            _testClass = new AuthService(mongoDBService);
        }

        [Fact]
        public async Task CanCallSignInWithCredentialsUser()
        {

            // Act
            SignInDTOs signInDTO = new SignInDTOs { Email = "vanhphamjuly@gmail.com", Password = "meocat1807" };
            var result = await _testClass.SignInWithCredentials(signInDTO);

            // Assert
            Assert.True(result.Equals("1"));
        }

        [Fact]
        public async Task CanCallSignInWithCredentialsAdmin()
        {
            // Act
            SignInDTOs signInDTO = new SignInDTOs { Email = "thanhtra@gmail.com", Password = "meocat1807" };
            var result = await _testClass.SignInWithCredentials(signInDTO);

            // Assert
            Assert.True(result.Equals("1"));
        }
        [Fact]
        public async Task CannotCallSignInWithCredentialsNullPassword()
        {
            // Act
            SignInDTOs signInDTO = new SignInDTOs { Email = "vanhphamjuly@gmail.com", Password = "" };

            // Assert
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.SignInWithCredentials(signInDTO));
        }


        [Fact]
        public async Task CannotCallSignInWithCredentialsWrongPassword()
        {
            // Act
            SignInDTOs signInDTO = new SignInDTOs { Email = "vanhphamjuly@gmail.com", Password = "meooo" };

            // Assert
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.SignInWithCredentials(signInDTO));
        }

        [Fact]
        public async Task CannotCallSignInWithCredentialsBadFormatEmail1()
        {
            // Act
            SignInDTOs signInDTO = new SignInDTOs { Email = "vanhphamjuly#@gmail.com", Password = "" };

            // Assert
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.SignInWithCredentials(signInDTO));
        }

        [Fact]
        public async Task CannotCallSignInWithCredentialsBadFormatEmail2()
        {
            // Act
            SignInDTOs signInDTO = new SignInDTOs { Email = "vanhpham=july=@gmail.com", Password = "" };

            // Assert
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.SignInWithCredentials(signInDTO));
        }
  

        [Fact]
        public async Task CanCallSignUpWithCredentials1()
        {
            // Arrange
            var signUpDTO = new SignUpDTO
            {
                DisplayName = "Mai",
                Password = "MeoCat2006",
                Email = "maingoc2006@gmail.com"
            };

            // Act
            var result = await _testClass.SignUpWithCredentials(signUpDTO);

            // Assert
            Assert.True(result.Equals(true));
        }

        [Fact]
        public async Task CanCallSignUpWithCredentials2()
        {
            // Arrange
            var signUpDTO = new SignUpDTO
            {
                DisplayName = "Ngoc",
                Password = "MeoCat2006",
                Email = "maingoc@gmail.com"
            };

            // Act
            var result = await _testClass.SignUpWithCredentials(signUpDTO);

            // Assert
            Assert.True(result == true);
        }

        [Fact]
        public async Task CannotCallSignUpWithCredentialsWithExistedEmail1()
        {
            // Arrange
            var signUpDTO = new SignUpDTO
            {
                DisplayName = "Ngoc",
                Password = "MeoCat2006",
                Email = "maingoc@gmail.com"
            };

            // Assert
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.SignUpWithCredentials(signUpDTO));
        }

        [Fact]
        public async Task CannotCallSignUpWithCredentialsWithExistedEmail2()
        {
            // Arrange
            var signUpDTO = new SignUpDTO
            {
                DisplayName = "Ngoc",
                Password = "MeoCat2006",
                Email = "anhptvhe@fpt.edu.vn"
            };

            // Assert
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.SignUpWithCredentials(signUpDTO));
        }
    }
}