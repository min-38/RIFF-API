namespace Riff.Api.Features.Captcha;

public interface ICaptchaService
{
    Task<bool> VerifyTurnstileTokenAsync(string token, string? remoteIp = null);
}
