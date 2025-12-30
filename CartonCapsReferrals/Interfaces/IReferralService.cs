using CartonCapsReferrals.Api.Enums;
using CartonCapsReferrals.Api.Models;

namespace CartonCapsReferrals.Api.Interfaces
{
    /// <summary>
    /// Provides operations for generating, retrieving, and resolving referrals.
    /// </summary>
    public interface IReferralService
    {
        /// <summary>
        /// Generates a referral link for the authenticated user.
        /// If a pending referral already exists, it is reused.
        /// </summary>
        /// <param name="channel">Channel used to share the referral.</param>
        /// <returns>The active referral.</returns>
        /// <exception cref="BadRequestException">
        /// Bad request (vendor error)
        /// </exception>
        /// <exception cref="NotFoundException">
        /// Vendor response missing required fields or user not authenticated.
        /// </exception>
        Task<Referral> GenerateReferralLink(Channel channel);

        /// <summary>
        /// Retrieves all referrals created by the authenticated user.
        /// </summary>
        Task<IEnumerable<Referral>> GetUserReferralsAsync(int userId);

        /// <summary>
        /// Resolves a referral when a new user completes onboarding.
        /// </summary>
        /// <param name="referralId">Identifier of the referral to resolve.</param>
        /// <param name="refereeName">Name of the referred user.</param>
        /// <returns>The resolved referral.</returns>
        /// <exception cref="BadRequestException">
        /// User already referred or Referral already resolved
        /// </exception>
        /// <exception cref="ForbiddenException">
        /// Self-referrals are not allowed
        /// </exception>
        /// <exception cref="NotFoundException">
        /// Referral to resolve not found
        /// </exception>
        Task<Referral> ResolveReferralAsync(Guid ReferralId, string refereeName);
    }
}
