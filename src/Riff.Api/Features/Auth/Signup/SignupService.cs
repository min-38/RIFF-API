using Microsoft.EntityFrameworkCore;
using Riff.Api.Features.Auth.Signup.Dto;
using Riff.Api.Features.Captcha;
using Riff.Api.Data.Repositories;
using Riff.Api.Data.Entities;

namespace Riff.Api.Features.Auth.Signup;

public sealed class SignupService : ISignupService
{
    private readonly ICaptchaService _captchaService;
    private readonly IUserRepository _userRepository;

    public SignupService
    (
        ICaptchaService captchaService,
        IUserRepository userRepository
    )
    {
        _captchaService = captchaService;
        _userRepository = userRepository;
    }

    public async Task SignupAsync(SignupRequest request)
    {
        // 1. turnstile 검증
        string turnstileCode = request.TurnstileCode;
        bool captchaVertifed = await _captchaService.VerifyTurnstileTokenAsync(turnstileCode);
        if (!captchaVertifed) throw new InvalidOperationException("CAPTCHA_INVALID");

        // 2. email 존재 여부 체크
        bool existEmail = await _userRepository.ExistsByEmailAsync(request.Email);
        if (existEmail) throw new InvalidOperationException("EMAIL_ALREADY_EXISTS");

        // 3. username 존재 여부 체크
        bool existUsername = await _userRepository.ExistsByUsernameAsync(request.Username);
        if (existUsername) throw new InvalidOperationException("USERNAME_ALREADY_EXISTS");

        /* ----- 중복 user 없음 ----- */

        // 4. 패스워드 해싱
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // 5. 계정 생성
        User newUser = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = hashedPassword,
            Verified = false, // TODO: 이메일 인증 기능 추가 시 true로 변경
            AvatarUrl = null, // TODO: S3 연동 후 null이 아닌 기본 아바타 URL로 변경
            TermsOfServiceAgreed = request.TermsOfServiceAgreed,
            PrivacyPolicyAgreed = request.PrivacyCollectionAgreed,
            AgeOver14Agreed = request.AgeOver14Agreed,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        try
        {
            await _userRepository.AddAsync(newUser);
            await _userRepository.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("USER_CREATION_FAILED");
        }
        catch (Exception)
        {
            throw new InvalidOperationException("USER_CREATION_FAILED");
        }
    }
}
