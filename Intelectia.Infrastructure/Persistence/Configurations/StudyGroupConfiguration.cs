using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Intelectia.Domain.Entities;

namespace Intelectia.Infrastructure.Persistence.Configurations;

public class StudyGroupConfiguration : IEntityTypeConfiguration<StudyGroup>
{
    public void Configure(EntityTypeBuilder<StudyGroup> builder)
    {
        builder.ToTable("StudyGroups");
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Name).IsRequired().HasMaxLength(200);
        builder.Property(g => g.Description).HasMaxLength(1000);

        // MemberCount es calculado desde la colección; no se persiste
        builder.Ignore(g => g.MemberCount);

        builder.HasOne(g => g.CreatedByUser)
               .WithMany(u => u.CreatedGroups)
               .HasForeignKey(g => g.CreatedByUserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(g => g.Members)
               .WithOne(m => m.Group)
               .HasForeignKey(m => m.GroupId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(g => g.Messages)
               .WithOne(m => m.Group)
               .HasForeignKey(m => m.GroupId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
