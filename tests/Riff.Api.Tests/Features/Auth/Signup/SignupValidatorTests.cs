using Riff.Api.Features.Auth.Signup;
using Riff.Api.Features.Auth.Signup.Dto;

namespace Riff.Api.Tests.Features.Auth.Signup;

public sealed class SignupValidatorTests
{
    // 정상 요청이면 validation error가 없어야 한다.
    [Fact]
    public void Validate_ReturnsNoErrors_ForValidRequest()
    {
        var request = CreateValidRequest();

        var errors = SignupValidator.Validate(request);

        Assert.Empty(errors);
    }

    // 잘못된 요청이면 필드별 에러 코드가 정확히 반환되어야 한다.
    [Fact]
    public void Validate_ReturnsExpectedErrors_ForInvalidRequest()
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

        var errors = SignupValidator.Validate(request);

        Assert.Equal(["USERNAME_TOO_SHORT"], errors["username"]);
        Assert.Equal(["EMAIL_INVALID"], errors["email"]);
        Assert.Equal(["PASSWORD_INVALID"], errors["password"]);
        Assert.Equal(["PASSWORD_CONFIRM_MISMATCH"], errors["passwordConfirm"]);
        Assert.Equal(["TURNSTILE_REQUIRED"], errors["turnstileCode"]);
        Assert.Equal(["TERMS_OF_SERVICE_NOT_AGREED"], errors["termsOfServiceAgreed"]);
        Assert.Equal(["PRIVACY_COLLECTION_NOT_AGREED"], errors["privacyCollectionAgreed"]);
        Assert.Equal(["AGE_OVER_14_NOT_AGREED"], errors["ageOver14Agreed"]);
    }

    // 사용자명 최대 길이(15자)를 넘기면 USERNAME_TOO_LONG이어야 한다.
    [Fact]
    public void Validate_ReturnsUsernameTooLong_WhenUsernameExceedsMaxLength()
    {
        var request = CreateValidRequest() with
        {
            Username = "abcdefghijklmnop"
        };

        var errors = SignupValidator.Validate(request);

        Assert.Equal(["USERNAME_TOO_LONG"], errors["username"]);
    }

    // 비밀번호 확인값이 비어 있으면 PASSWORD_CONFIRM_REQUIRED여야 한다.
    [Fact]
    public void Validate_ReturnsPasswordConfirmRequired_WhenPasswordConfirmIsMissing()
    {
        var request = CreateValidRequest() with
        {
            PasswordConfirm = ""
        };

        var errors = SignupValidator.Validate(request);

        Assert.Equal(["PASSWORD_CONFIRM_REQUIRED"], errors["passwordConfirm"]);
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
}
