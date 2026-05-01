using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Enums;

namespace Intelectia.Infrastructure.Persistence;

public class DatabaseSeeder
{
    private readonly AppDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(AppDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await _context.Database.MigrateAsync();

        _logger.LogInformation("Iniciando proceso de seed...");

        await SeedCategoriesAsync();
        await SeedSystemVendorAsync();
        await SeedBooksAsync();

        _logger.LogInformation("Seed completado.");
    }

    private async Task SeedCategoriesAsync()
    {
        if (await _context.Categories.AnyAsync())
            return;

        var categories = new List<Category>
        {
            new() { Name = "Ciencias Exactas",           Slug = "ciencias-exactas" },
            new() { Name = "Ingeniería",                  Slug = "ingenieria" },
            new() { Name = "Ciencias de la Computación", Slug = "computacion" },
            new() { Name = "Medicina y Salud",            Slug = "medicina-salud" },
            new() { Name = "Ciencias Sociales",           Slug = "ciencias-sociales" },
            new() { Name = "Humanidades",                 Slug = "humanidades" },
            new() { Name = "Derecho",                     Slug = "derecho" },
            new() { Name = "Administración",              Slug = "administracion" },
        };

        await _context.Categories.AddRangeAsync(categories);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Categorías creadas: {Count}", categories.Count);
    }

    private async Task SeedSystemVendorAsync()
    {
        if (await _context.Users.AnyAsync(u => u.Email == "system@intelectia.dev"))
            return;

        var systemUser = new User
        {
            Email          = "system@intelectia.dev",
            FirstName      = "Intelectia",
            LastName       = "System",
            PasswordHash   = "N/A",
            EmailConfirmed = true,
            VendorProfile  = new VendorProfile
            {
                BusinessName = "Intelectia Editorial",
                Description  = "Catálogo oficial de Intelectia.",
                IsActive     = true,
                ActivatedAt  = DateTime.UtcNow
            }
        };

        await _context.Users.AddAsync(systemUser);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Vendedor del sistema creado.");
    }

    private async Task SeedBooksAsync()
    {
        if (await _context.Books.AnyAsync())
            return;

        var cats = await _context.Categories.ToDictionaryAsync(c => c.Slug);
        var vendor = await _context.VendorProfiles
            .FirstAsync(v => v.BusinessName == "Intelectia Editorial");

        Guid Cat(string slug) => cats[slug].Id;

        var books = new List<Book>
        {
            // Recursos de Ciencias Exactas
            new() { Title = "Cálculo: Trascendentes Tempranas", Author = "James Stewart",
                Description = "Texto de referencia para cálculo universitario. Cubre límites, derivadas, integrales y series con enfoque aplicado y ejercicios progresivos.",
                ISBN = "978-0-538-49790-9", PublishedYear = 2015, PageCount = 1368, Language = "es",
                Price = 45.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                CategoryId = Cat("ciencias-exactas"), VendorProfileId = vendor.Id },

            new() { Title = "Álgebra Lineal y sus Aplicaciones", Author = "David C. Lay",
                Description = "Introducción moderna al álgebra lineal con énfasis en la comprensión conceptual y aplicaciones en ciencia e ingeniería.",
                ISBN = "978-0-321-98238-4", PublishedYear = 2016, PageCount = 576, Language = "es",
                Price = 38.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                CategoryId = Cat("ciencias-exactas"), VendorProfileId = vendor.Id },

            new() { Title = "Probabilidad y Estadística para Ingeniería y Ciencias", Author = "Jay L. Devore",
                Description = "Fundamentos de probabilidad y estadística con aplicaciones prácticas en ingeniería. Incluye análisis de regresión y control de calidad.",
                ISBN = "978-0-538-73352-6", PublishedYear = 2016, PageCount = 768, Language = "es",
                Price = 42.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                CategoryId = Cat("ciencias-exactas"), VendorProfileId = vendor.Id },

            // Recursos de Ingeniería
            new() { Title = "Mecánica Vectorial para Ingenieros: Estática", Author = "Beer & Johnston",
                Description = "Texto clásico de mecánica estática. Desarrolla habilidades en análisis de fuerzas, momentos y estructuras con enfoque práctico.",
                ISBN = "978-0-07-352940-0", PublishedYear = 2019, PageCount = 736, Language = "es",
                Price = 50.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                CategoryId = Cat("ingenieria"), VendorProfileId = vendor.Id },

            new() { Title = "Circuitos Eléctricos", Author = "James W. Nilsson",
                Description = "Referencia estándar para el análisis de circuitos eléctricos. Cubre análisis en el dominio del tiempo y la frecuencia con enfoque en aplicaciones.",
                ISBN = "978-0-13-397965-2", PublishedYear = 2014, PageCount = 864, Language = "es",
                Price = 48.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                CategoryId = Cat("ingenieria"), VendorProfileId = vendor.Id },

            new() { Title = "Resistencia de Materiales", Author = "R.C. Hibbeler",
                Description = "Análisis del comportamiento de materiales bajo cargas. Cubre tensión, deformación, flexión y torsión con ejemplos de ingeniería real.",
                ISBN = "978-0-13-411935-9", PublishedYear = 2017, PageCount = 896, Language = "es",
                Price = 46.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                CategoryId = Cat("ingenieria"), VendorProfileId = vendor.Id },

            // Recursos de Computación
            new() { Title = "Introducción a los Algoritmos", Author = "Cormen, Leiserson, Rivest & Stein",
                Description = "La referencia definitiva en algoritmos y estructuras de datos. Cubre desde fundamentos hasta algoritmos avanzados con rigor matemático.",
                ISBN = "978-0-262-03384-8", PublishedYear = 2022, PageCount = 1312, Language = "es",
                Price = 55.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                CategoryId = Cat("computacion"), VendorProfileId = vendor.Id },

            new() { Title = "Clean Code: Manual de Artesanía del Software Ágil", Author = "Robert C. Martin",
                Description = "Guía práctica para escribir código limpio, mantenible y profesional. Incluye principios, patrones y prácticas con ejemplos en Java.",
                ISBN = "978-0-13-235088-4", PublishedYear = 2008, PageCount = 464, Language = "es",
                Price = 35.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                CategoryId = Cat("computacion"), VendorProfileId = vendor.Id },

            new() { Title = "Diseño de Sistemas de Bases de Datos", Author = "Abraham Silberschatz",
                Description = "Fundamentos de sistemas de bases de datos relacionales. Cubre modelado ER, SQL, normalización, transacciones y sistemas distribuidos.",
                ISBN = "978-0-07-352332-3", PublishedYear = 2019, PageCount = 1376, Language = "es",
                Price = 52.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                CategoryId = Cat("computacion"), VendorProfileId = vendor.Id },

            new() { Title = "Redes de Computadoras", Author = "Andrew S. Tanenbaum",
                Description = "Texto completo sobre redes de computadoras. Cubre modelos de referencia, protocolos, seguridad y aplicaciones de red modernas.",
                ISBN = "978-0-13-212695-3", PublishedYear = 2011, PageCount = 960, Language = "es",
                Price = 44.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                CategoryId = Cat("computacion"), VendorProfileId = vendor.Id },

            new() { Title = "Sistemas Operativos Modernos", Author = "Andrew S. Tanenbaum",
                Description = "Diseño e implementación de sistemas operativos modernos. Cubre procesos, memoria, sistemas de archivos, seguridad y virtualización.",
                ISBN = "978-0-13-359162-0", PublishedYear = 2014, PageCount = 1072, Language = "es",
                Price = 49.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                CategoryId = Cat("computacion"), VendorProfileId = vendor.Id },

            // Recursos de Medicina
            new() { Title = "Anatomía Humana", Author = "Frank H. Netter",
                Description = "Atlas de referencia en anatomía humana con ilustraciones detalladas. Cubre todos los sistemas del cuerpo con orientación clínica.",
                ISBN = "978-0-323-39321-1", PublishedYear = 2018, PageCount = 640, Language = "es",
                Price = 65.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                CategoryId = Cat("medicina-salud"), VendorProfileId = vendor.Id },

            new() { Title = "Bioquímica", Author = "Lehninger",
                Description = "Fundamentos de bioquímica con enfoque en procesos metabólicos y moleculares. Texto de referencia para carreras de salud y biología.",
                ISBN = "978-1-4641-2610-9", PublishedYear = 2017, PageCount = 1228, Language = "es",
                Price = 58.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                CategoryId = Cat("medicina-salud"), VendorProfileId = vendor.Id },

            // Recursos de Ciencias Sociales
            new() { Title = "Metodología de la Investigación", Author = "Roberto Hernández Sampieri",
                Description = "Guía completa para el diseño y desarrollo de investigación científica. Cubre enfoques cuantitativo, cualitativo y mixto.",
                ISBN = "978-1-4562-2396-0", PublishedYear = 2014, PageCount = 600, Language = "es",
                Price = 36.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                CategoryId = Cat("ciencias-sociales"), VendorProfileId = vendor.Id },

            new() { Title = "Psicología Social", Author = "Elliot Aronson",
                Description = "Exploración del comportamiento humano en contextos sociales. Cubre influencia, actitudes, prejuicio, grupos y relaciones interpersonales.",
                ISBN = "978-0-13-392892-7", PublishedYear = 2015, PageCount = 560, Language = "es",
                Price = 40.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                CategoryId = Cat("ciencias-sociales"), VendorProfileId = vendor.Id },

            // Recursos de Humanidades
            new() { Title = "Historia Universal", Author = "William McNeill",
                Description = "Visión global de la historia de la humanidad desde las primeras civilizaciones hasta el mundo contemporáneo.",
                ISBN = "978-0-19-521073-0", PublishedYear = 1999, PageCount = 896, Language = "es",
                Price = 32.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                CategoryId = Cat("humanidades"), VendorProfileId = vendor.Id },

            new() { Title = "Filosofía: Una Introducción", Author = "Thomas Nagel",
                Description = "Introducción clara y accesible a los problemas centrales de la filosofía. Ideal para estudiantes universitarios de cualquier carrera.",
                ISBN = "978-0-19-289191-9", PublishedYear = 1987, PageCount = 160, Language = "es",
                Price = 22.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                CategoryId = Cat("humanidades"), VendorProfileId = vendor.Id },

            // Recursos de Derecho
            new() { Title = "Introducción al Derecho", Author = "Eduardo García Máynez",
                Description = "Texto clásico de introducción al derecho en el ámbito hispanohablante. Cubre conceptos fundamentales del orden jurídico.",
                ISBN = "978-970-07-7198-0", PublishedYear = 2002, PageCount = 444, Language = "es",
                Price = 28.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                CategoryId = Cat("derecho"), VendorProfileId = vendor.Id },

            new() { Title = "Derecho Constitucional", Author = "Jorge Carpizo",
                Description = "Análisis del sistema constitucional mexicano. Cubre derechos fundamentales, estructura del Estado y control constitucional.",
                ISBN = "978-970-32-4965-7", PublishedYear = 2007, PageCount = 380, Language = "es",
                Price = 30.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                CategoryId = Cat("derecho"), VendorProfileId = vendor.Id },

            // Recursos de Administración
            new() { Title = "Administración", Author = "Stephen P. Robbins",
                Description = "Fundamentos de administración y gestión organizacional. Cubre planeación, organización, dirección y control con casos reales.",
                ISBN = "978-0-13-292459-3", PublishedYear = 2014, PageCount = 720, Language = "es",
                Price = 42.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                CategoryId = Cat("administracion"), VendorProfileId = vendor.Id },

            new() { Title = "Contabilidad Financiera", Author = "Jan Williams",
                Description = "Principios de contabilidad financiera para la toma de decisiones empresariales. Incluye estados financieros y análisis de información contable.",
                ISBN = "978-0-07-786267-8", PublishedYear = 2015, PageCount = 816, Language = "es",
                Price = 44.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                CategoryId = Cat("administracion"), VendorProfileId = vendor.Id },

            new() { Title = "Marketing", Author = "Philip Kotler",
                Description = "La referencia mundial en marketing. Cubre estrategia, segmentación, comportamiento del consumidor y marketing digital.",
                ISBN = "978-0-13-385646-7", PublishedYear = 2015, PageCount = 800, Language = "es",
                Price = 46.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                CategoryId = Cat("administracion"), VendorProfileId = vendor.Id },
        };

        await _context.Books.AddRangeAsync(books);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Libros del catálogo creados: {Count}", books.Count);
    }
}
