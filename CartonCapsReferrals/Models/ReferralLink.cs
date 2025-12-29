using CartonCapsReferrals.Api.Enums;

namespace CartonCapsReferrals.Api.Models
{
    public class ReferralLink
    {
        public Guid ReferralLinkId { get; set; }

        public string DeepLinkUrl { get; set; }

        public string ShortLinkUrl { get; set; }

        public Channel Channel { get; set; }
    }
}
