using CartonCapsReferrals.Api.Models;

namespace CartonCapsReferrals.Api.Interfaces
{
    public interface IUserService
    {
        Task<User> GetAuthenticatedUserAsync();
    }
}
