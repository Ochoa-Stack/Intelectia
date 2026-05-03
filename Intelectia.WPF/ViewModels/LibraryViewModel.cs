using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Intelectia.Shared.DTOs.Library;
using Intelectia.WPF.Core;
using Intelectia.WPF.Services;

namespace Intelectia.WPF.ViewModels;

public partial class LibraryViewModel : BaseViewModel
{
    private readonly LibraryService _libraryService;
    private readonly NavigationService _navigationService;
    private readonly Func<MarketplaceViewModel> _marketplaceVmFactory;

    public ObservableCollection<UserBookDto> Books { get; } = new();
    public ObservableCollection<NoteDto>     Notes { get; } = new();

    [ObservableProperty] private UserBookDto? _selectedBook;
    [ObservableProperty] private string _activeTab      = "books";
    [ObservableProperty] private string _errorMessage   = string.Empty;
    [ObservableProperty] private string _newNoteTitle   = string.Empty;
    [ObservableProperty] private string _newNoteContent = string.Empty;
    [ObservableProperty] private bool   _showNoteForm;

    public bool HasError   => !string.IsNullOrEmpty(ErrorMessage);
    public bool HasBooks   => Books.Count > 0;
    public bool HasNotes   => Notes.Count > 0;
    public bool IsBookTab  => ActiveTab == "books";
    public bool IsNotesTab => ActiveTab == "notes";

    public LibraryViewModel(
        LibraryService libraryService,
        NavigationService navigationService,
        Func<MarketplaceViewModel> marketplaceVmFactory)
    {
        _libraryService       = libraryService;
        _navigationService    = navigationService;
        _marketplaceVmFactory = marketplaceVmFactory;
        Title = "Mi Biblioteca";
    }

    // Llamado por NavigationService al aterrizar en la vista
    [RelayCommand]
    public async Task InitializeAsync()
        => await LoadBooksAsync();

    [RelayCommand]
    private async Task LoadBooksAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var books = await _libraryService.GetUserBooksAsync();
            Books.Clear();
            foreach (var book in books)
                Books.Add(book);

            OnPropertyChanged(nameof(HasBooks));
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
            OnPropertyChanged(nameof(HasError));
        }
        catch
        {
            ErrorMessage = "No se pudo cargar la biblioteca.";
            OnPropertyChanged(nameof(HasError));
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Carga notas; del libro seleccionado si hay uno, o todas si no
    [RelayCommand]
    private async Task LoadNotesAsync()
    {
        IsBusy = true;

        try
        {
            var notes = await _libraryService.GetNotesAsync(SelectedBook?.BookId);
            Notes.Clear();
            foreach (var note in notes)
                Notes.Add(note);

            OnPropertyChanged(nameof(HasNotes));
        }
        catch
        {
            // Las notas no bloquean la vista si fallan
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveNoteAsync()
    {
        if (string.IsNullOrWhiteSpace(NewNoteTitle) ||
            string.IsNullOrWhiteSpace(NewNoteContent)) return;

        IsBusy = true;

        try
        {
            var note = await _libraryService.CreateNoteAsync(new CreateNoteRequest
            {
                BookId  = SelectedBook?.BookId,
                Title   = NewNoteTitle,
                Content = NewNoteContent
            });

            Notes.Insert(0, note);
            NewNoteTitle   = string.Empty;
            NewNoteContent = string.Empty;
            ShowNoteForm   = false;

            OnPropertyChanged(nameof(HasNotes));
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
            OnPropertyChanged(nameof(HasError));
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteNoteAsync(NoteDto note)
    {
        try
        {
            await _libraryService.DeleteNoteAsync(note.Id);
            Notes.Remove(note);
            OnPropertyChanged(nameof(HasNotes));
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
            OnPropertyChanged(nameof(HasError));
        }
    }

    // Cambia de tab y carga el contenido correspondiente
    [RelayCommand]
    private async Task SwitchTabAsync(string tab)
    {
        ActiveTab = tab;
        OnPropertyChanged(nameof(IsBookTab));
        OnPropertyChanged(nameof(IsNotesTab));

        if (tab == "notes")
            await LoadNotesAsync();
    }

    [RelayCommand]
    private async Task GoToMarketplaceAsync()
    {
        var vm = _marketplaceVmFactory();
        await vm.InitializeAsync();
        _navigationService.NavigateTo(vm);
    }

    [RelayCommand]
    private void ToggleNoteForm()
        => ShowNoteForm = !ShowNoteForm;
}
