using CartonCapsReferrals.Api.Enums;
using CartonCapsReferrals.Api.Models;

namespace CartonCapsReferrals.Api.Interfaces
{
    public interface IReferralService
    {
        Task<ReferralLink> GenerateReferralLink(Channel channel);
        Task<IEnumerable<Referral>> GetUserReferralsAsync(int userId);
        Task<Referral> ResolveReferralAsync(Guid ReferralId, string refereeName);
    }
}
