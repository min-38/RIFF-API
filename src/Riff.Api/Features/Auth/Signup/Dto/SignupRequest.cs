namespace Riff.Api.Features.Auth.Signup.Dto;

public sealed record SignupRequest(
    string Username,
    string Email,
    string Password,
    string PasswordConfirm,
    string TurnstileCode,
    bool TermsOfServiceAgreed,
    bool PrivacyCollectionAgreed,
    bool AgeOver14Agreed
);
