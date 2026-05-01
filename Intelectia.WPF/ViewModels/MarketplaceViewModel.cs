using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Intelectia.Shared.DTOs.Marketplace;
using Intelectia.WPF.Core;
using Intelectia.WPF.Services;

namespace Intelectia.WPF.ViewModels;

public partial class MarketplaceViewModel : BaseViewModel
{
    private readonly MarketplaceService _marketplaceService;
    private readonly NavigationService _navigationService;
    private readonly Func<BookDetailViewModel> _bookDetailVmFactory;

    // Lista de libros que se muestran en el grid
    public ObservableCollection<BookSummaryDto> Books { get; } = new();

    // Lista de categorías para el panel de filtros
    public ObservableCollection<CategoryDto> Categories { get; } = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private CategoryDto? _selectedCategory;

    [ObservableProperty]
    private string _selectedSortBy = "newest";

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _totalCount = 0;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public bool HasError          => !string.IsNullOrEmpty(ErrorMessage);
    public bool HasPreviousPage   => CurrentPage > 1;
    public bool HasNextPage       => CurrentPage < TotalPages;

    // Opciones de ordenamiento para el ComboBox
    public List<(string Key, string Label)> SortOptions { get; } = new()
    {
        ("newest",     "Más recientes"),
        ("price_asc",  "Precio: menor a mayor"),
        ("price_desc", "Precio: mayor a menor"),
        ("rating",     "Mejor calificados")
    };

    public MarketplaceViewModel(
        MarketplaceService marketplaceService,
        NavigationService navigationService,
        Func<BookDetailViewModel> bookDetailVmFactory)
    {
        _marketplaceService  = marketplaceService;
        _navigationService   = navigationService;
        _bookDetailVmFactory = bookDetailVmFactory;
        Title = "Marketplace";
    }

    // Se llama al navegar a esta vista; carga categorías y primera página
    [RelayCommand]
    public async Task InitializeAsync()
    {
        await LoadCategoriesAsync();
        await LoadBooksAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            var categories = await _marketplaceService.GetCategoriesAsync();
            Categories.Clear();

            // Opción para quitar el filtro de categoría
            Categories.Add(new CategoryDto { Id = Guid.Empty, Name = "Todas las categorías", Slug = "" });
            foreach (var cat in categories)
                Categories.Add(cat);

            SelectedCategory = Categories.First();
        }
        catch
        {
            // Las categorías no son críticas; si fallan no bloqueamos el catálogo
        }
    }

    [RelayCommand]
    public async Task LoadBooksAsync()
    {
        ErrorMessage = string.Empty;
        IsBusy = true;

        try
        {
            var categoryId = SelectedCategory?.Id == Guid.Empty
                ? (Guid?)null
                : SelectedCategory?.Id;

            var result = await _marketplaceService.GetBooksAsync(
                page:       CurrentPage,
                search:     SearchText,
                categoryId: categoryId,
                sortBy:     SelectedSortBy);

            Books.Clear();
            foreach (var book in result.Items)
                Books.Add(book);

            TotalPages  = result.TotalPages;
            TotalCount  = result.TotalCount;
            CurrentPage = result.Page;

            OnPropertyChanged(nameof(HasPreviousPage));
            OnPropertyChanged(nameof(HasNextPage));
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
            OnPropertyChanged(nameof(HasError));
        }
        catch
        {
            ErrorMessage = "No se pudo conectar con el servidor.";
            OnPropertyChanged(nameof(HasError));
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        CurrentPage = 1;
        await LoadBooksAsync();
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (!HasPreviousPage) return;
        CurrentPage--;
        await LoadBooksAsync();
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (!HasNextPage) return;
        CurrentPage++;
        await LoadBooksAsync();
    }

    [RelayCommand]
    private async Task OpenBookAsync(BookSummaryDto book)
    {
        var vm = _bookDetailVmFactory();
        await vm.LoadAsync(book.Id);
        _navigationService.NavigateTo(vm);
    }

    partial void OnSelectedCategoryChanged(CategoryDto? value)
    {
        CurrentPage = 1;
        _ = LoadBooksAsync();
    }

    partial void OnSelectedSortByChanged(string value)
    {
        CurrentPage = 1;
        _ = LoadBooksAsync();
    }
}
