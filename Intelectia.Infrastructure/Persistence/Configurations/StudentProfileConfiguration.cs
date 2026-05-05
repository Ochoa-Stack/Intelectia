using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Intelectia.Domain.Entities;

namespace Intelectia.Infrastructure.Persistence.Configurations;

public class StudentProfileConfiguration : IEntityTypeConfiguration<StudentProfile>
{
    public void Configure(EntityTypeBuilder<StudentProfile> builder)
    {
        builder.ToTable("StudentProfiles");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Institution).HasMaxLength(200);
        builder.Property(s => s.Major).HasMaxLength(200);
        builder.Property(s => s.AcademicLevel).HasMaxLength(100);
    }
}
