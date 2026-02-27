using Riff.Api.Features.Auth.Signup.Dto;

namespace Riff.Api.Features.Auth.Signup;

public static class SignupValidator
{
    public static Dictionary<string, string[]> Validate(SignupRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Username) || request.Username.Length < 2)
        {
            errors["username"] = ["USERNAME_TOO_SHORT"];
        }
        else if (request.Username.Length > 15)
        {
            errors["username"] = ["USERNAME_TOO_LONG"];
        }

        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
        {
            errors["email"] = ["EMAIL_INVALID"];
        }

        if (string.IsNullOrWhiteSpace(request.Password) ||
            request.Password.Length < 8 ||
            !HasRequiredPasswordComplexity(request.Password))
        {
            errors["password"] = ["PASSWORD_INVALID"];
        }

        if (string.IsNullOrWhiteSpace(request.PasswordConfirm))
        {
            errors["passwordConfirm"] = ["PASSWORD_CONFIRM_REQUIRED"];
        }
        else if (request.Password != request.PasswordConfirm)
        {
            errors["passwordConfirm"] = ["PASSWORD_CONFIRM_MISMATCH"];
        }

        if (string.IsNullOrWhiteSpace(request.TurnstileCode))
        {
            errors["turnstileCode"] = ["TURNSTILE_REQUIRED"];
        }

        if (!request.TermsOfServiceAgreed)
        {
            errors["termsOfServiceAgreed"] = ["TERMS_OF_SERVICE_NOT_AGREED"];
        }

        if (!request.PrivacyCollectionAgreed)
        {
            errors["privacyCollectionAgreed"] = ["PRIVACY_COLLECTION_NOT_AGREED"];
        }

        if (!request.AgeOver14Agreed)
        {
            errors["ageOver14Agreed"] = ["AGE_OVER_14_NOT_AGREED"];
        }

        return errors;
    }

    private static bool HasRequiredPasswordComplexity(string password) =>
        !string.IsNullOrWhiteSpace(password) &&
        password.Any(char.IsUpper) &&
        password.Any(char.IsLower) &&
        password.Any(char.IsDigit);
}
