using Bogus;
using CartonCapsReferrals.Api.Interfaces;
using CartonCapsReferrals.Api.Models;

public class UserService : IUserService
{
    private readonly Faker _faker = new();

    public Task<User> GetAuthenticatedUserAsync()
    {
        var user = new User
        {
            UserId = _faker.Random.Int(1, 10),
            Name = _faker.Name.FullName(),
            Email = _faker.Internet.Email(),
            ReferralCode = _faker.Random.AlphaNumeric(6).ToUpper()
        };

        return Task.FromResult(user);
    }
}
