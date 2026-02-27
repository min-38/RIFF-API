using Riff.Api.Features.Auth.Signup;

namespace Riff.Api.Features.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapSignupEndpoint();

        return app;
    }
}
