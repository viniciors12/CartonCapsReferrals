using CartonCapsReferrals.Api.Enums;

namespace CartonCapsReferrals.Api.Models
{
    public class Referral
    {
        public Guid ReferralId { get; set; }

        public int ReferrerUserId { get; set; }

        public int RefereeUserId { get; set; }

        public string RefereeName { get; set; }

        public ReferralStatus Status { get; set; }

        public string ReferralCode { get; set; }

        public DateTime? CreatedDt { get; set; }

        public DateTime? ModifiedDt { get; set; }

        public ReferralLink ReferralLink { get; set; }
    }
}
