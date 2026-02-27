using Riff.Api.Contracts;
using Riff.Api.Features.Auth.Signup.Dto;

namespace Riff.Api.Features.Auth.Signup;

public static class SignupEndpoint
{
    public static RouteGroupBuilder MapSignupEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost("/signup", HandleSignupAsync)
        .WithName("Signup");

        return group;
    }

    public static async Task<IResult> HandleSignupAsync(SignupRequest request, ISignupService signupService)
    {
        var errors = SignupValidator.Validate(request);
        if (errors.Count > 0)
        {
            return Results.BadRequest(ApiResponses.Error("VALIDATION_ERROR", errors));
        }

        try
        {
            await signupService.SignupAsync(request);
            var apiResponse = ApiResponses.Success();

            return Results.Created("/api/auth/signup", apiResponse);
        }
        catch (InvalidOperationException ex)
        {
            int? statusCode = ex.Message switch
            {
                "EMAIL_ALREADY_EXISTS" => StatusCodes.Status409Conflict,
                "USERNAME_ALREADY_EXISTS" => StatusCodes.Status409Conflict,
                "CAPTCHA_INVALID" => StatusCodes.Status400BadRequest,
                "USER_CREATION_FAILED" => StatusCodes.Status500InternalServerError,
                _ => null
            };

            if (statusCode is null)
            {
                throw;
            }

            return Results.Json(
                ApiResponses.Error(ex.Message),
                statusCode: statusCode.Value
            );
        }
    }
}
