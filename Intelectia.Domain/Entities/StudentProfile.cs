using Intelectia.Domain.Common;

namespace Intelectia.Domain.Entities;

public class StudentProfile : BaseEntity
{
    // Usuario al que pertenece este perfil
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // Institución educativa donde estudia
    public string? Institution { get; set; }

    // Carrera o programa académico
    public string? Major { get; set; }

    // Semestre o año académico actual
    public string? AcademicLevel { get; set; }
}
