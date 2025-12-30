using CartonCapsReferrals.Api.Enums;
using CartonCapsReferrals.Api.Interfaces;
using CartonCapsReferrals.Api.Models;
using CartonCapsReferrals.Api.Services;
using CartonCapsReferrals.Api.Tests.Helper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using System.Text;
using static CartonCapsReferrals.Api.Utils.Exceptions.ReferralExceptions;

namespace CartonCapsReferrals.Api.Tests.Services
{
    public class ReferralServiceTests
    {
        private readonly Mock<IUserService> _userService;
        private readonly Mock<IReferralStore> _store;
        private readonly Mock<ILogger<ReferralService>> _logger;
        private readonly ShortIoOptions _options;

        public ReferralServiceTests()
        {
            _userService = new Mock<IUserService>();
            _store = new Mock<IReferralStore>();
            _logger = new Mock<ILogger<ReferralService>>();

            _options = new ShortIoOptions
            {
                ApiKey = "test-key",
                ApiUrl = "https://short.io"
            };
        }

        [Fact]
        public async Task GenerateReferralLink_ShouldCreateNewReferral_NonePending()
        {
            // Arrange
            var user = new User { UserId = 1, ReferralCode = "ABC123" };

            _userService
                .Setup(u => u.GetAuthenticatedUserAsync())
                .ReturnsAsync(user);

            _store
                .Setup(s => s.GetAll())
                .Returns(Array.Empty<Referral>());

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"shortURL\":\"https://short.link/abc\"}",
                    Encoding.UTF8,
                    "application/json")
            };

            var service = CreateService(httpResponse);

            // Act
            var referral = await service.GenerateReferralLink(Channel.Email);

            // Assert
            Assert.NotNull(referral);
            Assert.Equal(user.UserId, referral.ReferrerUserId);
            Assert.Equal(ReferralStatus.Pending, referral.Status);

            _store.Verify(s => s.Add(It.IsAny<Referral>()), Times.Once);
        }

        [Fact]
        public async Task GenerateReferralLink_ShouldReuseExistingPendingReferral()
        {
            // Arrange
            var user = new User { UserId = 1, ReferralCode = "ABC123" };

            var existingReferral = new Referral
            {
                ReferralId = Guid.NewGuid(),
                ReferrerUserId = 1,
                Status = ReferralStatus.Pending,
                ReferralLink = new ReferralLink()
            };

            _userService
                .Setup(u => u.GetAuthenticatedUserAsync())
                .ReturnsAsync(user);

            _store
                .Setup(s => s.GetAll())
                .Returns([existingReferral]);

            var service = CreateService(new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            var referral = await service.GenerateReferralLink(Channel.Email);

            // Assert
            Assert.Equal(existingReferral.ReferralId, referral.ReferralId);
            _store.Verify(s => s.Update(existingReferral), Times.Once);
        }

        [Fact]
        public async Task GenerateReferralLink_ShouldThrow_UserNotAuthenticated()
        {
            // Arrange
            _userService
                .Setup(u => u.GetAuthenticatedUserAsync())
                .ReturnsAsync((User?)null);

            var service = CreateService(new HttpResponseMessage(HttpStatusCode.OK));

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() =>
                service.GenerateReferralLink(Channel.Email));
        }

        [Fact]
        public async Task GenerateReferralLink_ShouldThrow_VendorReturnsError()
        {
            // Arrange
            var user = new User { UserId = 1, ReferralCode = "ABC123" };

            _userService
                .Setup(u => u.GetAuthenticatedUserAsync())
                .ReturnsAsync(user);

            _store
                .Setup(s => s.GetAll())
                .Returns(Array.Empty<Referral>());

            var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Vendor failure")
            };

            var service = CreateService(httpResponse);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() =>
                service.GenerateReferralLink(Channel.Email));
        }

        [Fact]
        public async Task GenerateReferralLink_ShouldThrow_VendorMissingShortUrl()
        {
            // Arrange
            var user = new User { UserId = 1, ReferralCode = "ABC123" };

            _userService
                .Setup(u => u.GetAuthenticatedUserAsync())
                .ReturnsAsync(user);

            _store
                .Setup(s => s.GetAll())
                .Returns(Array.Empty<Referral>());

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"invalid\":\"value\"}")
            };

            var service = CreateService(httpResponse);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.GenerateReferralLink(Channel.Email));
        }

        [Fact]
        public async Task GetUserReferralsAsync_ShouldReturnOnlyReferralsForAuthenticatedUser()
        {
            // Arrange
            var user = new User
            {
                UserId = 1,
                ReferralCode = "ABC123"
            };

            _userService
                .Setup(u => u.GetAuthenticatedUserAsync())
                .ReturnsAsync(user);

            var referrals = new List<Referral>
            {
                new Referral { ReferrerUserId = 1 },
                new Referral { ReferrerUserId = 1 },
                new Referral { ReferrerUserId = 2 }
            };

            _store
                .Setup(s => s.GetAll())
                .Returns(referrals);

            var service = CreateService(new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            var result = await service.GetUserReferralsAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, r => Assert.Equal(1, r.ReferrerUserId));
        }

        [Fact]
        public async Task ResolveReferralAsync_ShouldResolveReferralSuccessfully()
        {
            // Arrange
            var referralId = Guid.NewGuid();

            var referral = new Referral
            {
                ReferralId = referralId,
                ReferrerUserId = 1,
                Status = ReferralStatus.Pending
            };

            var referrals = new List<Referral>
            {
                new Referral 
                {
                    ReferralId = Guid.NewGuid(),
                    ReferrerUserId = 1,
                    Status = ReferralStatus.Pending
                },
                new Referral
                {
                    ReferralId = Guid.NewGuid(),
                    ReferrerUserId = 2,
                    Status = ReferralStatus.Pending
                }
            };

            _store
                .Setup(s => s.GetById(referralId))
                .Returns(referral);

            _store
                .Setup(s => s.GetAll())
                .Returns(referrals);

            _userService
                .Setup(u => u.GetAuthenticatedUserAsync())
                .ReturnsAsync(new User { UserId = 2 });

            var service = CreateService(new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            var result = await service.ResolveReferralAsync(referralId, "John Doe");

            // Assert
            Assert.Equal(ReferralStatus.Complete, result.Status);
            Assert.Equal(2, result.RefereeUserId);
            Assert.Equal("John Doe", result.RefereeName);
            Assert.NotNull(result.ModifiedDt);
        }

        [Fact]
        public async Task ResolveReferralAsync_ShouldThrow_WhenReferralNotFound()
        {
            // Arrange
            var referralId = Guid.NewGuid();

            _store
                .Setup(s => s.GetById(referralId))
                .Returns((Referral?)null);

            var service = CreateService(new HttpResponseMessage(HttpStatusCode.OK));

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.ResolveReferralAsync(referralId, "John"));
        }

        [Fact]
        public async Task ResolveReferralAsync_ShouldThrow_WhenSelfReferral()
        {
            // Arrange
            var referralId = Guid.NewGuid();

            var referral = new Referral
            {
                ReferralId = referralId,
                ReferrerUserId = 1,
                Status = ReferralStatus.Pending
            };

            _store
                .Setup(s => s.GetById(referralId))
                .Returns(referral);

            _userService
                .Setup(u => u.GetAuthenticatedUserAsync())
                .ReturnsAsync(new User { UserId = 1 });

            var service = CreateService(new HttpResponseMessage(HttpStatusCode.OK));

            // Act & Assert
            await Assert.ThrowsAsync<ForbiddenException>(() =>
                service.ResolveReferralAsync(referralId, "John"));
        }

        [Fact]
        public async Task ResolveReferralAsync_ShouldThrow_WhenReferralAlreadyResolved()
        {
            // Arrange
            var referralId = Guid.NewGuid();

            var referral = new Referral
            {
                ReferralId = referralId,
                ReferrerUserId = 1,
                Status = ReferralStatus.Complete
            };

            _store
                .Setup(s => s.GetById(referralId))
                .Returns(referral);

            var service = CreateService(new HttpResponseMessage(HttpStatusCode.OK));

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() =>
                service.ResolveReferralAsync(referralId, "John"));
        }

        [Fact]
        public async Task ResolveReferralAsync_ShouldThrow_WhenUserAlreadyReferred()
        {
            // Arrange
            var referralId = Guid.NewGuid();

            var referralToResolve = new Referral
            {
                ReferralId = referralId,
                ReferrerUserId = 1,
                Status = ReferralStatus.Pending
            };

            var existingReferral = new Referral
            {
                ReferralId = Guid.NewGuid(),
                RefereeUserId = 2,
                Status = ReferralStatus.Complete
            };

            _store
                .Setup(s => s.GetById(referralId))
                .Returns(referralToResolve);

            _store
                .Setup(s => s.GetAll())
                .Returns(new[] { existingReferral });

            _userService
                .Setup(u => u.GetAuthenticatedUserAsync())
                .ReturnsAsync(new User { UserId = 2 });

            var service = CreateService(new HttpResponseMessage(HttpStatusCode.OK));

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() =>
                service.ResolveReferralAsync(referralId, "John"));
        }

        private ReferralService CreateService(HttpResponseMessage httpResponse)
        {
            var handler = new FakeHttpMessageHandler(httpResponse);
            var httpClient = new HttpClient(handler);

            var options = Options.Create(_options);

            return new ReferralService(
                httpClient,
                options,
                _userService.Object,
                _store.Object,
                _logger.Object);
        }
    }
}
