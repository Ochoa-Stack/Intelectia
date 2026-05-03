using Intelectia.Shared.DTOs.Library;

namespace Intelectia.WPF.Services;

public class LibraryService
{
    private readonly ApiClient _apiClient;

    public LibraryService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    // Trae los libros adquiridos por el usuario autenticado
    public Task<List<UserBookDto>> GetUserBooksAsync(CancellationToken cancellationToken = default)
        => _apiClient.GetAsync<List<UserBookDto>>("api/library/books", cancellationToken);

    // Trae las notas del usuario; bookId filtra por libro si está presente
    public Task<List<NoteDto>> GetNotesAsync(
        Guid? bookId = null, CancellationToken cancellationToken = default)
    {
        var url = bookId.HasValue ? $"api/notes?bookId={bookId}" : "api/notes";
        return _apiClient.GetAsync<List<NoteDto>>(url, cancellationToken);
    }

    // Crea una nota nueva y devuelve el DTO con Id asignado
    public Task<NoteDto> CreateNoteAsync(
        CreateNoteRequest request, CancellationToken cancellationToken = default)
        => _apiClient.PostAsync<NoteDto>("api/notes", request, cancellationToken);

    // Elimina una nota por ID; la API devuelve 204 No Content
    public Task DeleteNoteAsync(Guid noteId, CancellationToken cancellationToken = default)
        => _apiClient.DeleteAsync($"api/notes/{noteId}", cancellationToken);

    // Genera y persiste una cita bibliográfica
    public Task<CitationDto> GenerateCitationAsync(
        GenerateCitationRequest request, CancellationToken cancellationToken = default)
        => _apiClient.PostAsync<CitationDto>("api/citations", request, cancellationToken);

    // Traduce un texto vía DeepL a través del backend
    public Task<TranslationDto> TranslateAsync(
        TranslateRequest request, CancellationToken cancellationToken = default)
        => _apiClient.PostAsync<TranslationDto>("api/translation/translate", request, cancellationToken);
}
