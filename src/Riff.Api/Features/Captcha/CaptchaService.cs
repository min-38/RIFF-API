using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Riff.Api.Features.Captcha;

public class CaptchaService : ICaptchaService
{
    // HTTP 클라이언트를 생성하기 위한 팩토리 -> cloudflare API 호출에 사용
    // Turnstile 비밀 키 및 API URL 처리
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _turnstileSecretKey;
    private readonly string _turnstileApiUrl;

    public CaptchaService
    (
        IHttpClientFactory httpClientFactory,
        IOptions<TurnstileOptions> options
    )
    {
        _httpClientFactory = httpClientFactory;
        _turnstileSecretKey = options.Value.SecretKey;
        _turnstileApiUrl = options.Value.ApiUrl;
    }

    // Turnstile 토큰 검증
    public async Task<bool> VerifyTurnstileTokenAsync(string token, string? remoteIp = null)
    {
        // 프로젝트 설정에 토큰 없으면 검증 실패
        if (string.IsNullOrEmpty(token)) return false;

        try
        {
            var httpClient = _httpClientFactory.CreateClient(); // HTTP 클라이언트 생성
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("secret", _turnstileSecretKey), // 비밀 키
                new KeyValuePair<string, string>("response", token), // 사용자 토큰
                new KeyValuePair<string, string>("remoteip", remoteIp ?? "") // 선택적 원격 IP
            });

            var response = await httpClient.PostAsync(_turnstileApiUrl, formData);
            var responseBody = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<TurnstileVerificationResponse>(responseBody);
            if (result?.Success == true)
                return true;

            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private class TurnstileVerificationResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("success")]
        public bool Success { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("error-codes")]
        public string[]? ErrorCodes { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("challenge_ts")]
        public string? ChallengeTs { get; set; } // 인증 시간

        [System.Text.Json.Serialization.JsonPropertyName("hostname")]
        public string? Hostname { get; set; }
    }
}
