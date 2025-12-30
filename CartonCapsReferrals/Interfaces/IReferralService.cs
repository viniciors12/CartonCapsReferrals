using CartonCapsReferrals.Api.Enums;
using CartonCapsReferrals.Api.Models;

namespace CartonCapsReferrals.Api.Interfaces
{
    public interface IReferralService
    {
        Task<Referral> GenerateReferralLink(Channel channel);
        Task<IEnumerable<Referral>> GetUserReferralsAsync();
        Task<Referral> ResolveReferralAsync(Guid ReferralId, string refereeName);
    }
}
