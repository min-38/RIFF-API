using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Riff.Api.Features.Auth.Signup;
using Riff.Api.Features.Auth.Signup.Dto;

namespace Riff.Api.Tests.Features.Auth.Signup;

public sealed class SignupEndpointTests
{
    private readonly Mock<ISignupService> _signupServiceMock;

    // SignupEndpoint 테스트에서 공통으로 사용할 서비스 mock 초기화
    public SignupEndpointTests()
    {
        _signupServiceMock = new Mock<ISignupService>();
    }

    // 잘못된 요청이면 VALIDATION_ERROR와 함께 400을 반환해야 한다.
    [Fact]
    public async Task HandleSignupAsync_ReturnsBadRequest_WhenValidationFails()
    {
        var request = new SignupRequest(
            Username: "a",
            Email: "invalid-email",
            Password: "weak",
            PasswordConfirm: "different",
            TurnstileCode: "",
            TermsOfServiceAgreed: false,
            PrivacyCollectionAgreed: false,
            AgeOver14Agreed: false
        );

        var result = await SignupEndpoint.HandleSignupAsync(request, _signupServiceMock.Object);
        var response = await ExecuteAsync(result);

        Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
        Assert.Contains("\"code\":\"VALIDATION_ERROR\"", response.Body);
    }

    // 회원가입이 정상 처리되면 201과 success 응답을 반환해야 한다.
    [Fact]
    public async Task HandleSignupAsync_ReturnsCreated_WhenSignupSucceeds()
    {
        var request = CreateValidRequest();

        var result = await SignupEndpoint.HandleSignupAsync(request, _signupServiceMock.Object);
        var response = await ExecuteAsync(result);

        Assert.Equal(StatusCodes.Status201Created, response.StatusCode);
        Assert.Contains("\"success\":true", response.Body);
        Assert.Contains("\"data\":null", response.Body);
        Assert.Equal("/api/auth/signup", response.Location);
    }

    // 이메일 중복이면 409와 EMAIL_ALREADY_EXISTS 코드를 반환해야 한다.
    [Fact]
    public async Task HandleSignupAsync_ReturnsConflict_WhenEmailAlreadyExists()
    {
        var request = CreateValidRequest();
        _signupServiceMock
            .Setup(x => x.SignupAsync(It.IsAny<SignupRequest>()))
            .ThrowsAsync(new InvalidOperationException("EMAIL_ALREADY_EXISTS"));

        var result = await SignupEndpoint.HandleSignupAsync(request, _signupServiceMock.Object);
        var response = await ExecuteAsync(result);

        Assert.Equal(StatusCodes.Status409Conflict, response.StatusCode);
        Assert.Contains("\"code\":\"EMAIL_ALREADY_EXISTS\"", response.Body);
    }

    // 사용자 생성 중 저장 실패가 나면 500과 USER_CREATION_FAILED를 반환해야 한다.
    [Fact]
    public async Task HandleSignupAsync_ReturnsServerError_WhenUserCreationFails()
    {
        var request = CreateValidRequest();
        _signupServiceMock
            .Setup(x => x.SignupAsync(It.IsAny<SignupRequest>()))
            .ThrowsAsync(new InvalidOperationException("USER_CREATION_FAILED"));

        var result = await SignupEndpoint.HandleSignupAsync(request, _signupServiceMock.Object);
        var response = await ExecuteAsync(result);

        Assert.Equal(StatusCodes.Status500InternalServerError, response.StatusCode);
        Assert.Contains("\"code\":\"USER_CREATION_FAILED\"", response.Body);
    }

    // 정의되지 않은 예외 코드는 endpoint에서 삼키지 않고 다시 던져야 한다.
    [Fact]
    public async Task HandleSignupAsync_RethrowsUnknownInvalidOperationException()
    {
        var request = CreateValidRequest();
        _signupServiceMock
            .Setup(x => x.SignupAsync(It.IsAny<SignupRequest>()))
            .ThrowsAsync(new InvalidOperationException("UNKNOWN_ERROR"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => SignupEndpoint.HandleSignupAsync(request, _signupServiceMock.Object));
    }

    private static SignupRequest CreateValidRequest() =>
        new(
            Username: "validuser",
            Email: "valid@example.com",
            Password: "Password1",
            PasswordConfirm: "Password1",
            TurnstileCode: "turnstile-token",
            TermsOfServiceAgreed: true,
            PrivacyCollectionAgreed: true,
            AgeOver14Agreed: true
        );

    // IResult를 실제 HTTP 응답처럼 실행해서 상태 코드와 바디를 확인한다.
    private static async Task<EndpointResponse> ExecuteAsync(IResult result)
    {
        var httpContext = new DefaultHttpContext();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Options.Create(new JsonOptions()));
        httpContext.RequestServices = services.BuildServiceProvider();
        httpContext.Response.Body = new MemoryStream();

        await result.ExecuteAsync(httpContext);

        httpContext.Response.Body.Position = 0;
        using var reader = new StreamReader(httpContext.Response.Body);
        var body = await reader.ReadToEndAsync();

        return new EndpointResponse(
            StatusCode: httpContext.Response.StatusCode,
            Body: body,
            Location: httpContext.Response.Headers.Location.ToString()
        );
    }

    private sealed record EndpointResponse(int StatusCode, string Body, string Location);
}
