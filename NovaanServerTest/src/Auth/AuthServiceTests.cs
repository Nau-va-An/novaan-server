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
            this._mongoDBServiceTests.InitializeMongoAccountCollection();
            var mongoDBService = new MongoDBService(_mongoDBServiceTests.mongoClient.Object);
            _testClass = new AuthService(mongoDBService);
        }



        [Fact]
        public async Task CanCallSignInWithCredentialsUserRole()
        {

            // Act
            SignInDTOs signInDTO = new SignInDTOs { UsernameOrEmail = "vanhphamjuly@gmail.com", Password = "meocat1807" };
            var result = await _testClass.SignInWithCredentials(signInDTO);

            // Assert
            Assert.True(result.Equals("1"));
        }
        [Fact]
        public async Task CanCallSignInWithCredentialsAdminRole()
        {
            // Act
            SignInDTOs signInDTO = new SignInDTOs { UsernameOrEmail = "thanhtra@gmail.com", Password = "meocat1807" };
            var result = await _testClass.SignInWithCredentials(signInDTO);

            // Assert
            Assert.True(result.Equals("1"));
        }
        [Fact]
        public async Task CannotCallSignInWithCredentialsNullPassword()
        {
            // Act
            SignInDTOs signInDTO = new SignInDTOs { UsernameOrEmail = "vanhphamjuly@gmail.com", Password = "" };

            // Assert
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.SignInWithCredentials(signInDTO));
        }


        [Fact]
        public async Task CannotCallSignInWithCredentialsWrongPassword()
        {
            // Act
            SignInDTOs signInDTO = new SignInDTOs { UsernameOrEmail = "vanhphamjuly@gmail.com", Password = "" };

            // Assert
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.SignInWithCredentials(signInDTO));
        }

        [Fact]
        public async Task CannotCallSignInWithCredentialsBadFormatEmail()
        {
            // Act
            SignInDTOs signInDTO = new SignInDTOs { UsernameOrEmail = "vanhphamjuly#@gmail.com", Password = "" };

            // Assert
            await Assert.ThrowsAsync<NovaanException>(() => _testClass.SignInWithCredentials(signInDTO));
        }
        /*
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
            _mongoDBServiceTests.InitializeMongoAccountCollection();
            var mongoDBService = new MongoDBService(_mongoDBServiceTests.mongoClient.Object);
            AuthService _testClass = new AuthService(mongoDBService);

            // Act
            var result = await _testClass.SignUpWithCredentials(signUpDTO);

            // Assert
            Assert.True(result == true);
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
        */
    }
}