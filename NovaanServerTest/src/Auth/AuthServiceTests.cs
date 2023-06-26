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
    using Xunit;

    public class AuthServiceTests
    {
        private AuthService _testClass;
        private MongoDBService _mongoDBService;

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new AuthService(_mongoDBService);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CannotConstructWithNullMongoDBService()
        {
            Assert.Throws<ArgumentNullException>(() => new AuthService(default(MongoDBService)));
        }

        [Fact]
        public async Task CanCallSignInWithCredentials()
        {
            // Arrange
            var signInDTO = new SignInDTOs
            {
                UsernameOrEmail = "TestValue1194246412",
                Password = "TestValue1415588841"
            };

            // Act
            var result = await _testClass.SignInWithCredentials(signInDTO);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public async Task CannotCallSignInWithCredentialsWithNullSignInDTO()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _testClass.SignInWithCredentials(default(SignInDTOs)));
        }

        [Fact]
        public async Task CanCallSignUpWithCredentials()
        {
            // Arrange
            var signUpDTO = new SignUpDTO
            {
                DisplayName = "TestValue940777782",
                Password = "TestValue1253465065",
                Email = "TestValue473189641"
            };

            // Act
            var result = await _testClass.SignUpWithCredentials(signUpDTO);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public async Task CannotCallSignUpWithCredentialsWithNullSignUpDTO()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _testClass.SignUpWithCredentials(default(SignUpDTO)));
        }

        [Fact]
        public async Task CanCallGoogleAuthentication()
        {
            // Arrange
            var googleOAuthDTO = new GoogleOAuthDTO { Token = "TestValue1499889" };

            // Act
            var result = await _testClass.GoogleAuthentication(googleOAuthDTO);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public async Task CannotCallGoogleAuthenticationWithNullGoogleOAuthDTO()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _testClass.GoogleAuthentication(default(GoogleOAuthDTO)));
        }
        
    }
}