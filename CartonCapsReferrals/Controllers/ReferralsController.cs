using CartonCapsReferrals.Api.Enums;
using CartonCapsReferrals.Api.Interfaces;
using CartonCapsReferrals.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace CartonCapsReferrals.Controllers
{
    [ApiController]
    [Route("Referrals/")]
    public class ReferralsController : ControllerBase
    {
        private readonly IReferralService _referralService;

        public ReferralsController(IReferralService referralService)
        {
            _referralService = referralService;
        }

        /// <summary>
        ///  Generates a referral link for the authenticated user using their existing referral code. 
        ///  If the user already has an active pending referral, the same referral is reused.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(Referral), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Referral>> CreateReferralLinkAsync(Channel channel)
        {
            var ReferralLink = await _referralService.GenerateReferralLink(channel);

            return Ok(ReferralLink);
        }

        /// <summary>
        /// Returns all referrals created by the one user.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Referral>>> GetUserReferralsAsync(int userId)
        {
            var referrals = await _referralService.GetUserReferralsAsync(userId);

            return Ok(referrals);
        }

        /// <summary>
        /// Resolves a referral when a new user completes onboarding via a referral link. Abuse prevention rules apply.
        /// </summary>
        [HttpPost("Resolve")]
        [ProducesResponseType(typeof(Referral), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Referral>>> ResolveReferralAsync(Guid referralId, string refereeName)
        {
            var referrals = await _referralService.ResolveReferralAsync(referralId, refereeName);

            return Ok(referrals);
        }
    }
}
