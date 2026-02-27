using Moq;
using Riff.Api.Data.Entities;
using Riff.Api.Data.Repositories;
using Riff.Api.Features.Auth.Signup;
using Riff.Api.Features.Auth.Signup.Dto;
using Riff.Api.Features.Captcha;

namespace Riff.Api.Tests.Features.Auth.Signup;

public sealed class SignupServiceTests
{
    private readonly Mock<ICaptchaService> _captchaServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly SignupService _signupService;

    // SignupService 테스트에서 공통으로 사용할 mock과 서비스 인스턴스를 준비한다.
    public SignupServiceTests()
    {
        _captchaServiceMock = new Mock<ICaptchaService>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _signupService = new SignupService(_captchaServiceMock.Object, _userRepositoryMock.Object);
    }

    // turnstile 인증 실패
    [Fact]
    public async Task SignupAsync_ThrowsCaptchaInvalid_WhenCaptchaVerificationFails()
    {
        _captchaServiceMock
            .Setup(x => x.VerifyTurnstileTokenAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync(false);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _signupService.SignupAsync(CreateValidRequest()));

        Assert.Equal("CAPTCHA_INVALID", ex.Message);
    }

    // 이메일 중복
    [Fact]
    public async Task SignupAsync_ThrowsEmailAlreadyExists_WhenEmailExists()
    {
        _captchaServiceMock
            .Setup(x => x.VerifyTurnstileTokenAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync(true);
        _userRepositoryMock
            .Setup(x => x.ExistsByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _signupService.SignupAsync(CreateValidRequest()));

        Assert.Equal("EMAIL_ALREADY_EXISTS", ex.Message);
    }

    // 사용자명 중복
    [Fact]
    public async Task SignupAsync_ThrowsUsernameAlreadyExists_WhenUsernameExists()
    {
        _captchaServiceMock
            .Setup(x => x.VerifyTurnstileTokenAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync(true);
        _userRepositoryMock
            .Setup(x => x.ExistsByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _userRepositoryMock
            .Setup(x => x.ExistsByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _signupService.SignupAsync(CreateValidRequest()));

        Assert.Equal("USERNAME_ALREADY_EXISTS", ex.Message);
    }

    // 정상적인 회원가입
    [Fact]
    public async Task SignupAsync_CreatesUserAndSaves_WhenRequestIsValid()
    {
        var request = CreateValidRequest();
        User? addedUser = null;

        _captchaServiceMock
            .Setup(x => x.VerifyTurnstileTokenAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync(true);
        _userRepositoryMock
            .Setup(x => x.ExistsByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _userRepositoryMock
            .Setup(x => x.ExistsByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>()))
            .Callback<User>(user => addedUser = user)
            .Returns(Task.CompletedTask);
        _userRepositoryMock
            .Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        await _signupService.SignupAsync(request);

        // 저장소 호출 여부와 생성된 User 객체의 핵심 값들을 함께 검증한다.
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        Assert.NotNull(addedUser);
        Assert.Equal(request.Email, addedUser!.Email);
        Assert.Equal(request.Username, addedUser.Username);
        Assert.Equal(request.TermsOfServiceAgreed, addedUser.TermsOfServiceAgreed);
        Assert.Equal(request.PrivacyCollectionAgreed, addedUser.PrivacyPolicyAgreed);
        Assert.Equal(request.AgeOver14Agreed, addedUser.AgeOver14Agreed);
        Assert.NotEqual(request.Password, addedUser.PasswordHash);
        Assert.False(addedUser.Verified);
        Assert.Null(addedUser.AvatarUrl);
        Assert.True(addedUser.CreatedAt <= DateTime.UtcNow);
    }

    // 사용자 생성 실패 (DB 저장 실패)
    [Fact]
    public async Task SignupAsync_ThrowsUserCreationFailed_WhenSaveChangesThrows()
    {
        _captchaServiceMock
            .Setup(x => x.VerifyTurnstileTokenAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync(true);
        _userRepositoryMock
            .Setup(x => x.ExistsByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _userRepositoryMock
            .Setup(x => x.ExistsByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);
        _userRepositoryMock
            .Setup(x => x.SaveChangesAsync())
            .ThrowsAsync(new Exception("Save failed"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _signupService.SignupAsync(CreateValidRequest()));

        Assert.Equal("USER_CREATION_FAILED", ex.Message);
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
