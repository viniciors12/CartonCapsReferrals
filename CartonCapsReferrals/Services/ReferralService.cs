using CartonCapsReferrals.Api.Enums;
using CartonCapsReferrals.Api.Interfaces;
using CartonCapsReferrals.Api.Models;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using static CartonCapsReferrals.Api.Utils.Exceptions.ReferralExceptions;

namespace CartonCapsReferrals.Api.Services
{
    public class ReferralService : IReferralService
    {
        private readonly HttpClient _httpClient;
        private readonly ShortIoOptions _options;
        private readonly IUserService _userService;
        private readonly IReferralStore _store;
        private readonly ILogger<ReferralService> _logger;

        public ReferralService(HttpClient httpClient,
            IOptions<ShortIoOptions> options,
            IUserService userService,
            IReferralStore store,
            ILogger<ReferralService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _httpClient.DefaultRequestHeaders.Add("Authorization", _options.ApiKey);
            _userService = userService;
            _store = store;
            _logger = logger;
        }
        public async Task<Referral> GenerateReferralLink(Channel channel)
        {
            var user = await GetAuthenticatedUserOrThrow();

            _logger.LogInformation(
                "Generating referral link for user {UserId}, channel {Channel}",
                user.UserId,
                channel
            );

            var existingReferral = GetPendingReferral(user.UserId);

            if (existingReferral != null)
            {
                return UpdateExistingReferral(existingReferral, channel);
            }

            return await CreateNewReferralAsync(user, channel);
        }

        public async Task<IEnumerable<Referral>> GetUserReferralsAsync(int userId)
        {
            var referrals = _store.GetAll().Where(r => r.ReferrerUserId == userId); 
            return referrals;
        }

        public async Task<Referral> ResolveReferralAsync(Guid referralId, string refereeName) 
        {
            var referral = _store.GetById(referralId);
            _logger.LogInformation(
                "Attempting to resolve referral {ReferralId}",
                referralId);

            if (referral == null)
                throw new NotFoundException("Referral not found");

            if (referral.Status == ReferralStatus.Complete)
                throw new BadRequestException("Referral already resolved");

            var user = await GetAuthenticatedUserOrThrow();
            if (user == null)
                throw new BadRequestException("User not found");

            if (referral.ReferrerUserId == user.UserId)
            {
                _logger.LogWarning(
                "User attempted to reuse a referral. UserId: {UserId}",
                user.UserId);
                throw new ForbiddenException("Self-referrals are not allowed");
            }

            var alreadyReferred = _store
                .GetAll()
                .Any(r => r.RefereeUserId == user.UserId);

            if (alreadyReferred)
                throw new BadRequestException("User already referred");

            referral.RefereeUserId = user.UserId;
            referral.RefereeName = refereeName;
            referral.Status = ReferralStatus.Complete; 
            referral.ModifiedDt = DateTime.UtcNow;
            _logger.LogInformation(
                "Referral resolved. ReferralId: {ReferralId}, RefereeUserId: {UserId}",
                referral.ReferralId,
                user.UserId
            );
            return referral; 
        }

        private async Task<User> GetAuthenticatedUserOrThrow()
        {
            return await _userService.GetAuthenticatedUserAsync()
                ?? throw new BadRequestException("User not found");
        }

        private Referral? GetPendingReferral(int userId)
        {
            return _store.GetAll()
                .FirstOrDefault(r =>
                    r.ReferrerUserId == userId &&
                    r.Status == ReferralStatus.Pending);
        }

        private Referral UpdateExistingReferral(Referral referral, Channel channel)
        {
            referral.ReferralLink.Channel = channel;
            referral.ModifiedDt = DateTime.UtcNow;

            _store.Update(referral);

            _logger.LogInformation(
                "Reusing existing referral {ReferralId} with channel {Channel}",
                referral.ReferralId,
                channel
            );

            return referral;
        }

        private async Task<Referral> CreateNewReferralAsync(User user, Channel channel)
        {
            var deepLinkUrl = $"app://cartoncaps/referralOnboarding?referral_code={user.ReferralCode}";
            var shortLink = await CreateShortLinkAsync(deepLinkUrl, user.UserId);

            var referralLink = new ReferralLink
            {
                ReferralLinkId = Guid.NewGuid(),
                Channel = channel,
                DeepLinkUrl = deepLinkUrl,
                ShortLinkUrl = shortLink
            };

            var referral = new Referral
            {
                ReferralId = Guid.NewGuid(),
                ReferrerUserId = user.UserId,
                Status = ReferralStatus.Pending,
                ReferralCode = user.ReferralCode,
                CreatedDt = DateTime.UtcNow,
                ModifiedDt = DateTime.UtcNow,
                ReferralLink = referralLink
            };

            _store.Add(referral);

            _logger.LogInformation(
                "New referral created. ReferralId: {ReferralId}, ReferrerUserId: {UserId}",
                referral.ReferralId,
                referral.ReferrerUserId
            );

            return referral;
        }

        private async Task<string> CreateShortLinkAsync(string deepLinkUrl, int userId)
        {
            var request = new
            {
                originalURL = deepLinkUrl,
                domain = "cartoncaps.short.gy"
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_options.ApiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();

                _logger.LogError(
                    "Short.io error for user {UserId}. Status: {StatusCode}. Body: {ErrorBody}",
                    userId,
                    response.StatusCode,
                    errorBody
                );

                throw new BadRequestException(
                    $"Vendor error: {response.StatusCode} - {errorBody}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseBody);

            if (!doc.RootElement.TryGetProperty("shortURL", out var shortUrlElement))
                throw new NotFoundException("Vendor response missing shortURL");

            return shortUrlElement.GetString()!;
        }
    }
}
