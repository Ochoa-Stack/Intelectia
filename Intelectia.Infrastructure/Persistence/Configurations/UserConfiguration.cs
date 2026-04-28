using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Intelectia.Domain.Entities;

namespace Intelectia.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        // No puede haber dos usuarios con el mismo correo (Email único)
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);

        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(100);

        builder.Property(u => u.ProfilePictureUrl).HasMaxLength(1000);
        builder.Property(u => u.PasswordHash).HasMaxLength(500);
        builder.Property(u => u.GoogleId).HasMaxLength(100);
        builder.Property(u => u.PasswordResetToken).HasMaxLength(500);

        // Guardamos el enum como número entero en la base de datos
        builder.Property(u => u.AuthProvider).HasConversion<int>();

        // Un usuario tiene un solo perfil de estudiante
        builder.HasOne(u => u.StudentProfile)
               .WithOne(s => s.User)
               .HasForeignKey<StudentProfile>(s => s.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        // Un usuario tiene un solo perfil de vendedor
        builder.HasOne(u => u.VendorProfile)
               .WithOne(v => v.User)
               .HasForeignKey<VendorProfile>(v => v.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        // Un usuario puede tener varios refresh tokens activos
        builder.HasMany(u => u.RefreshTokens)
               .WithOne(r => r.User)
               .HasForeignKey(r => r.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
