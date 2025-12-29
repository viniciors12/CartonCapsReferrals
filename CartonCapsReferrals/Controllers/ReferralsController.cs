using CartonCapsReferrals.Api.Enums;
using CartonCapsReferrals.Api.Interfaces;
using CartonCapsReferrals.Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Transactions;

namespace CartonCapsReferrals.Controllers
{
    [ApiController]
    [Route("Referrals/")]
    public class ReferralsController : ControllerBase
    {
        private readonly ILogger<ReferralsController> _logger;
        private IReferralService _referralService;

        public ReferralsController(ILogger<ReferralsController> logger, IReferralService referralService)
        {
            _logger = logger;
            _referralService = referralService;
        }

        [HttpPost]
        public async Task<ActionResult<ReferralLink>> CreateReferralLinkAsync(Channel channel)
        {
            var ReferralLink = await _referralService.GenerateReferralLink(channel);

            return Ok(ReferralLink);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Referral>>> GetUserReferralsAsync(int userId)
        {
            var referrals = await _referralService.GetUserReferralsAsync(userId);

            return Ok(referrals);
        }

        [HttpPost("Resolve")]
        public async Task<ActionResult<IEnumerable<Referral>>> ResolveReferralAsync(Guid referralId, string refereeName)
        {
            var referrals = await _referralService.ResolveReferralAsync(referralId, refereeName);

            return Ok(referrals);
        }
    }
}
