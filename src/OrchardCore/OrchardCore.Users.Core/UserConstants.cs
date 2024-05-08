namespace OrchardCore.Users;

public class UserConstants
{
    public const string TwoFactorAuthenticationClaimType = "TwoFacAuth";

    public class Features
    {
        public const string Users = "OrchardCore.Users";

        public const string TwoFactorAuthentication = "OrchardCore.Users.2FA";

        public const string AuthenticatorApp = "OrchardCore.Users.2FA.AuthenticatorApp";

        public const string EmailAuthenticator = "OrchardCore.Users.2FA.Email";

        public const string SmsAuthenticator = "OrchardCore.Users.2FA.Sms";

        public const string UserEmailConfirmation = "OrchardCore.Users.EmailConfirmation";

        public const string UserRegistration = "OrchardCore.Users.Registration";

        public const string ResetPassword = "OrchardCore.Users.ResetPassword";

    }
}
