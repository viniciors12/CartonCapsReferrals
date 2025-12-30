using CartonCapsReferrals.Api.Enums;
using CartonCapsReferrals.Api.Interfaces;
using CartonCapsReferrals.Api.Models;
using CartonCapsReferrals.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartonCapsReferrals.Api.Tests.Controllers
{
    public class ReferralsControllerTests
    {
        private readonly Mock<IReferralService> _referralServiceMock;
        private readonly ReferralsController _controller;

        public ReferralsControllerTests()
        {
            _referralServiceMock = new Mock<IReferralService>();
            _controller = new ReferralsController(_referralServiceMock.Object);
        }

        [Fact]
        public async Task CreateReferralLinkAsync_ReturnsOk()
        {
            // Arrange
            var referral = new Referral
            {
                ReferralId = Guid.NewGuid(),
                ReferralLink = new ReferralLink 
                {
                    DeepLinkUrl = "deeplink.com"
                }
            };

            _referralServiceMock
                .Setup(s => s.GenerateReferralLink(Channel.Email))
                .ReturnsAsync(referral);

            // Act
            var result = await _controller.CreateReferralLinkAsync(Channel.Email);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(referral, okResult.Value);
        }

        [Fact]
        public async Task GetUserReferralsAsync_ReturnsOk()
        {
            // Arrange
            var referrals = new List<Referral>
            {
                new Referral { ReferralId = Guid.NewGuid() }
            };

            _referralServiceMock
                .Setup(s => s.GetUserReferralsAsync(1))
                .ReturnsAsync(referrals);

            // Act
            var result = await _controller.GetUserReferralsAsync(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(referrals, okResult.Value);
        }

        [Fact]
        public async Task ResolveReferralAsync_ReturnsOk()
        {
            // Arrange
            var referral = new Referral { ReferralId = Guid.NewGuid() };

            _referralServiceMock
                .Setup(s => s.ResolveReferralAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(referral);

            // Act
            var result = await _controller.ResolveReferralAsync(Guid.NewGuid(), "John");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(referral, okResult.Value);
        }
    }
}
