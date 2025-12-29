using CartonCapsReferrals.Api.Models;

namespace CartonCapsReferrals.Api.Interfaces
{
    public interface IReferralStore
    {
        IReadOnlyCollection<Referral> GetAll();
        void Add(Referral referral);
        void Update(Referral referral);
        Referral? GetById(Guid referralId);
    }
}
