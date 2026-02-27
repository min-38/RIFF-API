using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using MailKit.Net.Smtp;
using Riff.Api.Data;
using Riff.Api.Data.Repositories;
using Riff.Api.Features.Auth;
using Riff.Api.Features.Auth.Signup;
using Riff.Api.Features.Captcha;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services
    .AddOptions<TurnstileOptions>()
    .Bind(builder.Configuration.GetSection(TurnstileOptions.SectionName));
builder.Services.AddHttpClient();
builder.Services.AddScoped<ICaptchaService, CaptchaService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISignupService, SignupService>();

var hc = builder.Services.AddHealthChecks();

// DB (Postgres)
hc.AddNpgSql(
    connectionString: builder.Configuration.GetConnectionString("Default")!,
    name: "db",
    timeout: TimeSpan.FromSeconds(3)
);

// SMTP (비동기 체크)
hc.AddAsyncCheck(
    name: "smtp",
    check: async () =>
    {
        var host = builder.Configuration["Smtp:Host"];
        var portStr = builder.Configuration["Smtp:Port"];
        var user = builder.Configuration["Smtp:Username"];
        var pass = builder.Configuration["Smtp:Password"];

        if (string.IsNullOrWhiteSpace(host) ||
            string.IsNullOrWhiteSpace(portStr) ||
            string.IsNullOrWhiteSpace(user) ||
            string.IsNullOrWhiteSpace(pass))
        {
            return HealthCheckResult.Unhealthy("SMTP config missing");
        }

        if (!int.TryParse(portStr, out var port))
            return HealthCheckResult.Unhealthy("SMTP port invalid");

        try
        {
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.Auto);
            await smtp.AuthenticateAsync(user, pass);
            await smtp.DisconnectAsync(true);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message);
        }
    },
    timeout: TimeSpan.FromSeconds(5)
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapAuthEndpoints();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        static string Label(string name) => name switch
        {
            "db" => "DB",
            "smtp" => "SMTP",
            _ => name.ToUpperInvariant()
        };

        static string Word(HealthStatus s) => s switch
        {
            HealthStatus.Healthy => "OK",
            HealthStatus.Degraded => "DEGRADED",
            HealthStatus.Unhealthy => "DOWN",
            _ => s.ToString().ToUpperInvariant()
        };

        var lines = report.Entries.Select(kv =>
        {
            var name = kv.Key;
            var entry = kv.Value;

            var line = $"{Label(name)} {Word(entry.Status)}";

            // Add reason only when not OK
            if (entry.Status != HealthStatus.Healthy)
            {
                if (entry.Exception is not null)
                    line += $" - {entry.Exception.GetType().Name}: {entry.Exception.Message}";
                else if (!string.IsNullOrWhiteSpace(entry.Description))
                    line += $" - {entry.Description}";
            }

            return line;
        });

        var overall = $"OVERALL {Word(report.Status)}";
        var body = overall + "\n" + string.Join("\n", lines);

        // log
        var logger = context.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Health");

        logger.LogInformation("HealthCheck: {Body}", body.Replace("\n", " | "));

        context.Response.ContentType = "text/plain; charset=utf-8";
        await context.Response.WriteAsync(body);
    }
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
