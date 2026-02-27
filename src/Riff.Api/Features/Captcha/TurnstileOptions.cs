namespace Riff.Api.Features.Captcha;

public sealed class TurnstileOptions
{
    public const string SectionName = "Turnstile";

    public string ApiUrl { get; init; } = string.Empty;
    public string SiteKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
}
