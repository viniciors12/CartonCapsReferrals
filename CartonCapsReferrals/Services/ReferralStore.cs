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
        private Dictionary<int, string> referralCodesByUser = new Dictionary<int, string>();

        public ReferralStore()
        {
            var faker = new Faker<Referral>()
            .RuleFor(r => r.ReferralId, _ => Guid.NewGuid())
            .RuleFor(r => r.ReferrerUserId, f => f.Random.Int(1, 10))
            .RuleFor(r => r.Status, f => f.PickRandom(new[]
            {
                ReferralStatus.Pending,
                ReferralStatus.Complete
            }))
            .RuleFor(r => r.RefereeUserId, (f, r) =>
                r.Status == ReferralStatus.Complete
                    ? f.Random.Int(11, 20)
                    : null)
            .RuleFor(r => r.RefereeName, (f, r) =>
                r.Status == ReferralStatus.Complete
                    ? f.Name.FullName()
                    : string.Empty)
            .RuleFor(r => r.ReferralCode, (f, r) =>
                GetReferralCodeForUser(r.ReferrerUserId, f))
            .RuleFor(r => r.CreatedDt, f => f.Date.Recent(30))
            .RuleFor(r => r.ModifiedDt, f => f.Date.Recent(10))
            .RuleFor(r => r.ReferralLink, (f, r) => new ReferralLink
            {
                ReferralLinkId = Guid.NewGuid(),
                Channel = f.PickRandom<Channel>(),
                DeepLinkUrl =
                    $"app://cartoncaps/referralOnboarding?referral_code={r.ReferralCode}",
                ShortLinkUrl =
                    $"https://cartoncaps.short.gy/{f.Random.AlphaNumeric(7)}"
            });

            _referrals = faker.Generate(50);
        }

        public IReadOnlyCollection<Referral> GetAll() => _referrals;

        public Referral? GetById(Guid referralId)
            => _referrals.FirstOrDefault(r => r.ReferralId == referralId);

        public void Update(Referral referral)
        {
            lock (_lock)
            {
                var index = _referrals.FindIndex(r => r.ReferralId == referral.ReferralId);
                if (index >= 0)
                    _referrals[index] = referral;
            }
        }

        public void Add(Referral referral)
        {
            lock (_lock)
            {
                _referrals.Add(referral);
            }
        }

        private string GetReferralCodeForUser(int userId, Faker f)
        {
            if (!referralCodesByUser.TryGetValue(userId, out var code))
            {
                code = f.Random.AlphaNumeric(6).ToUpper();
                referralCodesByUser[userId] = code;
            }

            return code;
        }
    }
}
