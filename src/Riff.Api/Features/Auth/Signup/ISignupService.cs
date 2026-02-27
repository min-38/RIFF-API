using Riff.Api.Features.Auth.Signup.Dto;

namespace Riff.Api.Features.Auth.Signup;

public interface ISignupService
{
    Task SignupAsync(SignupRequest request);
}
