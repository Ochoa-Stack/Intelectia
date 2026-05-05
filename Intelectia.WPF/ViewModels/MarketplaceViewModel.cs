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
    private readonly AuthService _authService;
    private readonly Func<BookDetailViewModel> _bookDetailVmFactory;
    private readonly Func<LibraryViewModel> _libraryVmFactory;
    private readonly Func<VendorDashboardViewModel> _vendorVmFactory;
    private readonly Func<VendorOnboardingViewModel> _vendorOnboardingVmFactory;
    private readonly Func<GroupsViewModel> _groupsVmFactory;
    private readonly Func<ProfileViewModel> _profileVmFactory;

    // Lista de libros que se muestran en el grid
    public ObservableCollection<BookSummaryDto> Books { get; } = new();

    // Lista de categorías para el panel de filtros
    public ObservableCollection<CategoryDto> Categories { get; } = new();

    [ObservableProperty] private string _searchText       = string.Empty;
    [ObservableProperty] private CategoryDto? _selectedCategory;
    [ObservableProperty] private string _selectedSortBy   = "newest";
    [ObservableProperty] private int _currentPage         = 1;
    [ObservableProperty] private int _totalPages          = 1;
    [ObservableProperty] private int _totalCount          = 0;
    [ObservableProperty] private string _errorMessage     = string.Empty;

    public bool HasError        => !string.IsNullOrEmpty(ErrorMessage);
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage     => CurrentPage < TotalPages;

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
        AuthService authService,
        Func<BookDetailViewModel> bookDetailVmFactory,
        Func<LibraryViewModel> libraryVmFactory,
        Func<VendorDashboardViewModel> vendorVmFactory,
        Func<VendorOnboardingViewModel> vendorOnboardingVmFactory,
        Func<GroupsViewModel> groupsVmFactory,
        Func<ProfileViewModel> profileVmFactory)
    {
        _marketplaceService        = marketplaceService;
        _navigationService         = navigationService;
        _authService               = authService;
        _bookDetailVmFactory       = bookDetailVmFactory;
        _libraryVmFactory          = libraryVmFactory;
        _vendorVmFactory           = vendorVmFactory;
        _vendorOnboardingVmFactory = vendorOnboardingVmFactory;
        _groupsVmFactory           = groupsVmFactory;
        _profileVmFactory          = profileVmFactory;
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

    // Navega a la biblioteca personal del usuario
    [RelayCommand]
    private async Task GoToLibraryAsync()
    {
        var vm = _libraryVmFactory();
        await vm.InitializeAsync();
        _navigationService.NavigateTo(vm);
    }

    // Comando actualizado; detecta si tiene perfil antes de navegar
    [RelayCommand]
    private async Task GoToVendorDashboardAsync()
    {
        // Leemos el estado del perfil de vendedor desde la sesión activa
        var isVendor = _authService.CurrentSession?.User.IsVendor ?? false;

        if (!isVendor)
        {
            // Sin perfil -> pantalla de activación
            _navigationService.NavigateTo(_vendorOnboardingVmFactory());
            return;
        }

        // Con perfil -> dashboard directo
        var vm = _vendorVmFactory();
        await vm.InitializeAsync();
        _navigationService.NavigateTo(vm);
    }

    [RelayCommand]
    private async Task GoToGroupsAsync()
    {
        var vm = _groupsVmFactory();
        await vm.InitializeAsync();
        _navigationService.NavigateTo(vm);
    }

    [RelayCommand]
    private async Task GoToProfileAsync()
    {
        var vm = _profileVmFactory();
        await vm.InitializeAsync();
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
