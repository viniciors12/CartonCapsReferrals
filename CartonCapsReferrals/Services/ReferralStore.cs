using Bogus;
using CartonCapsReferrals.Api.Enums;
using CartonCapsReferrals.Api.Interfaces;
using CartonCapsReferrals.Api.Models;

namespace CartonCapsReferrals.Api.Services
{
    public class ReferralStore : IReferralStore
    {
        private readonly List<Referral> _referrals;
        private readonly object _lock = new();

        public ReferralStore()
        {
            var faker = new Faker<Referral>()
                .RuleFor(r => r.ReferralId, _ => Guid.NewGuid())
                .RuleFor(r => r.ReferrerUserId, f => f.Random.Int(1, 10))
                .RuleFor(r => r.RefereeUserId, f => f.Random.Int(1, 10))
                .RuleFor(r => r.RefereeName, _ => string.Empty)
                .RuleFor(r => r.Status, f => f.PickRandom<ReferralStatus>())
                .RuleFor(r => r.ReferralCode, f => f.Random.AlphaNumeric(6).ToUpper())
                .RuleFor(r => r.CreatedDt, f => f.Date.Recent(30))
                .RuleFor(r => r.ModifiedDt, f => f.Date.Recent(10));

            _referrals = faker.Generate(50);
        }

        public IReadOnlyCollection<Referral> GetAll() => _referrals;

        public Referral? GetById(Guid referralId)
            => _referrals.FirstOrDefault(r => r.ReferralId == referralId);

        public void Add(Referral referral)
        {
            lock (_lock)
            {
                _referrals.Add(referral);
            }
        }
    }
}
