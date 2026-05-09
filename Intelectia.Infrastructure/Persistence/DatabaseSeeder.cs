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
            if (await _context.Books.CountAsync() >= 47)
                return;

            var cats = await _context.Categories.ToDictionaryAsync(c => c.Slug);
            var vendor = await _context.VendorProfiles
                .FirstAsync(v => v.BusinessName == "Intelectia Editorial");

            Guid Cat(string slug) => cats[slug].Id;

            // No delete existing books to avoid FK conflicts
            var existingBookTitles = await _context.Books.Where(b => b.VendorProfileId == vendor.Id).Select(b => b.Title).ToListAsync();

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

                // Nuevos Libros Añadidos (25)
                new() { Title = "Matemáticas Discretas", Author = "Kenneth H. Rosen",
                    Description = "Fundamentos de matemáticas para la computación. Incluye lógica, teoría de grafos y algoritmos.",
                    ISBN = "978-0-07-288008-3", PublishedYear = 2012, PageCount = 1072, Language = "es",
                    Price = 45.00m, Format = BookFormat.EPUB, Status = BookStatus.Active,
                    CategoryId = Cat("ciencias-exactas"), VendorProfileId = vendor.Id },

                new() { Title = "Ecuaciones Diferenciales", Author = "Dennis G. Zill",
                    Description = "Introducción clásica a ecuaciones diferenciales con modelado de problemas de la vida real.",
                    ISBN = "978-0-534-41887-8", PublishedYear = 2005, PageCount = 688, Language = "es",
                    Price = 38.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                    CategoryId = Cat("ciencias-exactas"), VendorProfileId = vendor.Id },

                new() { Title = "Química Orgánica", Author = "Paula Yurkanis Bruice",
                    Description = "Química orgánica centrada en el razonamiento de mecanismos, síntesis y reactividad.",
                    ISBN = "978-0-321-81139-4", PublishedYear = 2013, PageCount = 1344, Language = "es",
                    Price = 50.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                    CategoryId = Cat("ciencias-exactas"), VendorProfileId = vendor.Id },

                new() { Title = "Mecánica de Fluidos", Author = "Frank M. White",
                    Description = "Estudio integral de fluidos para ingeniería, con ejemplos aplicados a turbomaquinaria y tuberías.",
                    ISBN = "978-0-07-339827-3", PublishedYear = 2010, PageCount = 864, Language = "es",
                    Price = 55.00m, Format = BookFormat.EPUB, Status = BookStatus.Active,
                    CategoryId = Cat("ingenieria"), VendorProfileId = vendor.Id },

                new() { Title = "Termodinámica", Author = "Yunus A. Cengel",
                    Description = "Texto clave para ingenieros que combina principios básicos con aplicaciones del mundo real.",
                    ISBN = "978-0-07-339817-4", PublishedYear = 2014, PageCount = 1008, Language = "es",
                    Price = 60.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                    CategoryId = Cat("ingenieria"), VendorProfileId = vendor.Id },

                new() { Title = "Diseño de Maquinaria", Author = "Robert L. Norton",
                    Description = "Síntesis y análisis de máquinas y mecanismos usando métodos cinemáticos y dinámicos modernos.",
                    ISBN = "978-0-07-339806-8", PublishedYear = 2011, PageCount = 880, Language = "es",
                    Price = 48.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                    CategoryId = Cat("ingenieria"), VendorProfileId = vendor.Id },

                new() { Title = "Ingeniería de Software", Author = "Ian Sommerville",
                    Description = "Procesos, metodologías ágiles y sistemas ciberfísicos para el desarrollo de software actual.",
                    ISBN = "978-0-13-394303-0", PublishedYear = 2015, PageCount = 816, Language = "es",
                    Price = 45.00m, Format = BookFormat.EPUB, Status = BookStatus.Active,
                    CategoryId = Cat("computacion"), VendorProfileId = vendor.Id },

                new() { Title = "Inteligencia Artificial", Author = "Stuart Russell",
                    Description = "Un enfoque moderno sobre IA. Aprendizaje automático, redes neuronales y robótica.",
                    ISBN = "978-0-13-461099-3", PublishedYear = 2020, PageCount = 1166, Language = "es",
                    Price = 65.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                    CategoryId = Cat("computacion"), VendorProfileId = vendor.Id },

                new() { Title = "Arquitectura de Computadoras", Author = "John L. Hennessy",
                    Description = "Análisis cuantitativo de la arquitectura de procesadores, paralelismo y sistemas de memoria.",
                    ISBN = "978-0-12-811905-1", PublishedYear = 2017, PageCount = 936, Language = "es",
                    Price = 52.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                    CategoryId = Cat("computacion"), VendorProfileId = vendor.Id },

                new() { Title = "Patrones de Diseño", Author = "Erich Gamma",
                    Description = "Elementos de software orientado a objetos reutilizable. Catálogo de patrones Gang of Four.",
                    ISBN = "978-0-201-63361-0", PublishedYear = 1994, PageCount = 416, Language = "es",
                    Price = 30.00m, Format = BookFormat.EPUB, Status = BookStatus.Active,
                    CategoryId = Cat("computacion"), VendorProfileId = vendor.Id },

                new() { Title = "Fisiología Médica", Author = "Arthur C. Guyton",
                    Description = "El tratado de fisiología médica más aclamado, enfocado en mecanismos moleculares y celulares.",
                    ISBN = "978-1-4557-7005-2", PublishedYear = 2015, PageCount = 1168, Language = "es",
                    Price = 75.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                    CategoryId = Cat("medicina-salud"), VendorProfileId = vendor.Id },

                new() { Title = "Farmacología Básica", Author = "Bertram G. Katzung",
                    Description = "Principios básicos y clínicos de farmacología para estudiantes de medicina y farmacia.",
                    ISBN = "978-1-259-81340-9", PublishedYear = 2017, PageCount = 1200, Language = "es",
                    Price = 60.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                    CategoryId = Cat("medicina-salud"), VendorProfileId = vendor.Id },

                new() { Title = "Neurología Clínica", Author = "Roger P. Simon",
                    Description = "Diagnóstico y tratamiento de las enfermedades del sistema nervioso para la práctica clínica.",
                    ISBN = "978-1-259-86121-9", PublishedYear = 2018, PageCount = 432, Language = "es",
                    Price = 50.00m, Format = BookFormat.EPUB, Status = BookStatus.Active,
                    CategoryId = Cat("medicina-salud"), VendorProfileId = vendor.Id },

                new() { Title = "Sociología", Author = "Anthony Giddens",
                    Description = "Introducción completa a la sociología moderna, explorando globalización, género y desigualdades.",
                    ISBN = "978-0-7456-9668-3", PublishedYear = 2017, PageCount = 1192, Language = "es",
                    Price = 40.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                    CategoryId = Cat("ciencias-sociales"), VendorProfileId = vendor.Id },

                new() { Title = "Antropología Cultural", Author = "Conrad Phillip Kottak",
                    Description = "Apreciación de la diversidad humana explorando etnicidad, raza, religión y desarrollo.",
                    ISBN = "978-0-07-786153-7", PublishedYear = 2014, PageCount = 608, Language = "es",
                    Price = 38.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                    CategoryId = Cat("ciencias-sociales"), VendorProfileId = vendor.Id },

                new() { Title = "Teoría Política Contemporánea", Author = "Will Kymlicka",
                    Description = "Análisis de las principales corrientes de la filosofía política actual: utilitarismo, liberalismo y feminismo.",
                    ISBN = "978-0-19-878274-2", PublishedYear = 2001, PageCount = 506, Language = "es",
                    Price = 32.00m, Format = BookFormat.EPUB, Status = BookStatus.Active,
                    CategoryId = Cat("ciencias-sociales"), VendorProfileId = vendor.Id },

                new() { Title = "Historia del Arte", Author = "E.H. Gombrich",
                    Description = "Una de las obras más famosas y populares sobre el arte jamás publicadas, abarcando desde las cavernas hasta hoy.",
                    ISBN = "978-0-7148-3247-0", PublishedYear = 1995, PageCount = 688, Language = "es",
                    Price = 45.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                    CategoryId = Cat("humanidades"), VendorProfileId = vendor.Id },

                new() { Title = "Literatura Comparada", Author = "Claudio Guillén",
                    Description = "Estudio introductorio a la literatura desde una perspectiva supranacional, abordando géneros y corrientes.",
                    ISBN = "978-84-8432-683-9", PublishedYear = 2005, PageCount = 576, Language = "es",
                    Price = 28.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                    CategoryId = Cat("humanidades"), VendorProfileId = vendor.Id },

                new() { Title = "Derecho Penal General", Author = "Claus Roxin",
                    Description = "Tratado fundamental para el estudio de la teoría del delito y los fundamentos del derecho penal.",
                    ISBN = "978-84-470-0932-8", PublishedYear = 1997, PageCount = 1072, Language = "es",
                    Price = 65.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                    CategoryId = Cat("derecho"), VendorProfileId = vendor.Id },

                new() { Title = "Derecho Civil: Obligaciones", Author = "René Abeliuk",
                    Description = "Obra clásica que analiza sistemáticamente la teoría de las obligaciones en el derecho civil moderno.",
                    ISBN = "978-956-10-1372-4", PublishedYear = 2001, PageCount = 1200, Language = "es",
                    Price = 58.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                    CategoryId = Cat("derecho"), VendorProfileId = vendor.Id },

                new() { Title = "Finanzas Corporativas", Author = "Stephen A. Ross",
                    Description = "Conceptos fundamentales y aplicaciones modernas de las finanzas en la toma de decisiones empresariales.",
                    ISBN = "978-0-07-786175-9", PublishedYear = 2015, PageCount = 1008, Language = "es",
                    Price = 55.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                    CategoryId = Cat("administracion"), VendorProfileId = vendor.Id },

                new() { Title = "Comportamiento Organizacional", Author = "Stephen P. Robbins",
                    Description = "Estudio exhaustivo del impacto de los individuos, grupos y estructuras sobre el comportamiento en las organizaciones.",
                    ISBN = "978-0-13-410398-3", PublishedYear = 2016, PageCount = 744, Language = "es",
                    Price = 42.00m, Format = BookFormat.EPUB, Status = BookStatus.Active,
                    CategoryId = Cat("administracion"), VendorProfileId = vendor.Id },

                new() { Title = "Dirección Estratégica", Author = "Michael E. Porter",
                    Description = "Técnicas de análisis de industrias y de la competencia para la toma de decisiones gerenciales.",
                    ISBN = "978-0-7432-6088-6", PublishedYear = 1998, PageCount = 432, Language = "es",
                    Price = 35.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                    CategoryId = Cat("administracion"), VendorProfileId = vendor.Id },

                new() { Title = "Macroeconomía", Author = "N. Gregory Mankiw",
                    Description = "Principios de macroeconomía aplicados a fluctuaciones del mercado, crecimiento y políticas económicas.",
                    ISBN = "978-1-4641-8289-1", PublishedYear = 2015, PageCount = 608, Language = "es",
                    Price = 48.00m, Format = BookFormat.PDF, Status = BookStatus.Active,
                    CategoryId = Cat("administracion"), VendorProfileId = vendor.Id },

                new() { Title = "Liderazgo en Tiempos Complejos", Author = "John P. Kotter",
                    Description = "Estrategias para liderar el cambio organizacional y motivar equipos en entornos inciertos.",
                    ISBN = "978-1-63369-106-4", PublishedYear = 2012, PageCount = 208, Language = "es",
                    Price = 25.00m, Format = BookFormat.EPUB, Status = BookStatus.Active,
                    CategoryId = Cat("administracion"), VendorProfileId = vendor.Id }
            };
        var booksToAdd = books.Where(b => !existingBookTitles.Contains(b.Title)).ToList();

        if (booksToAdd.Any())
        {
            await _context.Books.AddRangeAsync(booksToAdd);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Libros del catálogo creados: {Count}", booksToAdd.Count);
        }
    }
}
