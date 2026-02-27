# DbContext란?

`DbContext`는 EF Core에서 데이터베이스와 상호작용하는 중심 클래스다.

쉽게 말하면 C# 코드로 DB를 조회하고 저장할 때 쓰는 진입점이다.

이 프로젝트에서는 `AppDbContext`가 그 역할을 맡는다.

예시:

```csharp
public sealed class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}
```

## AppDbContextFactory란?

`AppDbContextFactory`는 `dotnet ef migrations add`, `dotnet ef database update` 같은 EF Core 명령어가 실행될 때 `AppDbContext`를 생성해주는 도구용 클래스다.

즉:
- `AppDbContext`는 런타임에서 DB 작업할 때 쓰고
- `AppDbContextFactory`는 migration 같은 EF CLI 작업할 때 쓴다.
