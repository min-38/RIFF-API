using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Riff.Api.Data;

// EF Core CLI가 migration 작업 시 AppDbContext를 생성할 때 사용한다.
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        // 실행 위치에 따라 프로젝트 루트 또는 API 프로젝트 경로를 맞춘다.
        var projectPath = File.Exists(Path.Combine(basePath, "appsettings.json"))
            ? basePath
            : Path.Combine(basePath, "src", "Riff.Api");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(projectPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets<Program>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("Default"));

        return new AppDbContext(optionsBuilder.Options);
    }
}
