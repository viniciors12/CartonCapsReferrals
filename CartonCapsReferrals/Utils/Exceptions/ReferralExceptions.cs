namespace CartonCapsReferrals.Api.Utils.Exceptions
{
    public class ReferralExceptions
    {
        public class NotFoundException : DomainException
        {
            public NotFoundException(string message) : base(message) { }
        }

        public class BusinessRuleException : DomainException
        {
            public BusinessRuleException(string message) : base(message) { }
        }

        public class ForbiddenException : DomainException
        {
            public ForbiddenException(string message) : base(message) { }
        }

        public class BadRequestException : DomainException
        {
            public BadRequestException(string message) : base(message) { }
        }
    }
}
