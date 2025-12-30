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

        [HttpPost]
        [ProducesResponseType(typeof(Referral), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Referral>> CreateReferralLinkAsync(Channel channel)
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
