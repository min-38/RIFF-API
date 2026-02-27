using Microsoft.EntityFrameworkCore;
using Riff.Api.Data.Entities;

namespace Riff.Api.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    // 엔티티가 DB의 테이블, 컬럼, 인덱스와 어떻게 매핑되는지 설정한다.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            // PK 설정
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id)
                .HasColumnName("id");

            entity.Property(u => u.Username)
                .HasColumnName("username")
                .IsRequired()
                .HasMaxLength(15);

            entity.Property(u => u.Email)
                .HasColumnName("email")
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(u => u.PasswordHash)
                .HasColumnName("password_hash")
                .IsRequired();

            entity.Property(u => u.Verified)
                .HasColumnName("verified");

            entity.Property(u => u.AvatarUrl)
                .HasColumnName("avatar_url");

            entity.Property(u => u.TermsOfServiceAgreed)
                .HasColumnName("terms_of_service_agreed");

            entity.Property(u => u.PrivacyPolicyAgreed)
                .HasColumnName("privacy_policy_agreed");

            entity.Property(u => u.AgeOver14Agreed)
                .HasColumnName("age_over_14_agreed");

            entity.Property(u => u.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(u => u.UpdatedAt)
                .HasColumnName("updated_at");

            entity.Property(u => u.DeletedAt)
                .HasColumnName("deleted_at");

            // Unique 인덱스 설정
            entity.HasIndex(u => u.Username)
                .IsUnique();

            entity.HasIndex(u => u.Email)
                .IsUnique();

            // 삭제 여부 기반 조회를 위한 인덱스 설정
            entity.HasIndex(u => u.DeletedAt);
        });
    }
}
