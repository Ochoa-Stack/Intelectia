using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Intelectia.Domain.Entities;

namespace Intelectia.Infrastructure.Persistence.Configurations;

public class GroupMemberConfiguration : IEntityTypeConfiguration<GroupMember>
{
    public void Configure(EntityTypeBuilder<GroupMember> builder)
    {
        builder.ToTable("GroupMembers");
        builder.HasKey(m => m.Id);

        // Un usuario no puede estar dos veces en el mismo grupo
        builder.HasIndex(m => new { m.GroupId, m.UserId }).IsUnique();

        builder.Property(m => m.Role).HasConversion<int>();

        builder.HasOne(m => m.User)
               .WithMany(u => u.GroupMemberships)
               .HasForeignKey(m => m.UserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
