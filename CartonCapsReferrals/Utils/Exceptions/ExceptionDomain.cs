namespace CartonCapsReferrals.Api.Utils.Exceptions
{
    public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message) { }
    }
}
