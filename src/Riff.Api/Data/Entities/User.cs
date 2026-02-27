// Data/Entities/User.cs
using System.Collections.Generic;

namespace Riff.Api.Data.Entities;

public sealed class User
{
    // PK
    public Guid Id { get; set; }

    // 사용자 이름 (닉네임)
    public string Username { get; set; } = string.Empty;

    // 이메일 주소
    public string Email { get; set; } = string.Empty;

    // 비밀번호 해시
    public string PasswordHash { get; set; } = string.Empty;

    // 인증된 사용자인지 여부
    public bool Verified { get; set; }

    // 사용자 아바타 URL(S3)
    public string? AvatarUrl { get; set; } = null;

    // 이용약관 동의 여부
    public bool TermsOfServiceAgreed { get; set; }

    // 개인정보처리방침 동의 여부
    public bool PrivacyPolicyAgreed { get; set; }

    // 만 14세 이상 동의 여부
    public bool AgeOver14Agreed { get; set; }

    // 생성 시간
    public DateTime CreatedAt { get; set; }

    // 수정 시간
    public DateTime UpdatedAt { get; set; }

    // 삭제 시간
    public DateTime? DeletedAt { get; set; } = null;
}
