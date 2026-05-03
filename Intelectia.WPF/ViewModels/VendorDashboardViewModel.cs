using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Intelectia.Shared.DTOs.Marketplace;
using Intelectia.Shared.DTOs.Vendor;
using Intelectia.WPF.Core;
using Intelectia.WPF.Services;

namespace Intelectia.WPF.ViewModels;

public partial class VendorDashboardViewModel : BaseViewModel
{
    private readonly VendorService _vendorService;
    private readonly MarketplaceService _marketplaceService;
    private readonly NavigationService _navigationService;
    private readonly Func<MarketplaceViewModel> _marketplaceVmFactory;

    public ObservableCollection<VendorBookDto> Books { get; } = new();
    public ObservableCollection<CategoryDto> Categories { get; } = new();

    [ObservableProperty]
    private VendorStatsDto? _stats;

    [ObservableProperty]
    private string _activeTab = "inventory";

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _successMessage = string.Empty;

    // Campos del formulario de publicación
    [ObservableProperty]
    private string _bookTitle = string.Empty;

    [ObservableProperty]
    private string _bookAuthor = string.Empty;

    [ObservableProperty]
    private string _bookDescription = string.Empty;

    [ObservableProperty]
    private string _bookIsbn = string.Empty;

    [ObservableProperty]
    private int _bookPublishedYear = DateTime.Now.Year;

    [ObservableProperty]
    private int _bookPageCount;

    [ObservableProperty]
    private decimal _bookPrice;

    [ObservableProperty]
    private CategoryDto? _selectedCategory;

    [ObservableProperty]
    private string _selectedFormat = "PDF";

    [ObservableProperty]
    private bool _showPublishForm;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool HasSuccess => !string.IsNullOrEmpty(SuccessMessage);
    public bool HasBooks => Books.Count > 0;
    public bool IsInventoryTab => ActiveTab == "inventory";
    public bool IsStatsTab => ActiveTab == "stats";
    public bool IsPublishTab => ActiveTab == "publish";

    public List<string> Formats { get; } = new() { "PDF", "EPUB", "Physical" };

    public VendorDashboardViewModel(
        VendorService vendorService,
        MarketplaceService marketplaceService,
        NavigationService navigationService,
        Func<MarketplaceViewModel> marketplaceVmFactory)
    {
        _vendorService        = vendorService;
        _marketplaceService   = marketplaceService;
        _navigationService    = navigationService;
        _marketplaceVmFactory = marketplaceVmFactory;
        Title = "Panel de Vendedor";
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await LoadCategoriesAsync();
        await LoadBooksAsync();
        await LoadStatsAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            var categories = await _marketplaceService.GetCategoriesAsync();
            Categories.Clear();
            foreach (var cat in categories)
                Categories.Add(cat);

            SelectedCategory = Categories.FirstOrDefault();
        }
        catch { }
    }

    [RelayCommand]
    private async Task LoadBooksAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var books = await _vendorService.GetMyBooksAsync();
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
            ErrorMessage = "No se pudo cargar el inventario.";
            OnPropertyChanged(nameof(HasError));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadStatsAsync()
    {
        try
        {
            Stats = await _vendorService.GetStatsAsync();
        }
        catch { }
    }

    // Publica el libro con los datos del formulario
    [RelayCommand]
    private async Task PublishBookAsync()
    {
        ErrorMessage  = string.Empty;
        SuccessMessage = string.Empty;
        IsBusy = true;

        try
        {
            var request = new PublishBookRequest
            {
                Title         = BookTitle,
                Author        = BookAuthor,
                Description   = BookDescription,
                ISBN          = BookIsbn,
                PublishedYear = BookPublishedYear,
                PageCount     = BookPageCount,
                Price         = BookPrice,
                Format        = SelectedFormat,
                CategoryId    = SelectedCategory?.Id ?? Guid.Empty
            };

            var book = await _vendorService.PublishBookAsync(request);
            Books.Insert(0, book);

            SuccessMessage = $"'{book.Title}' publicado exitosamente en el catálogo.";
            OnPropertyChanged(nameof(HasSuccess));
            OnPropertyChanged(nameof(HasBooks));

            // Limpiamos el formulario
            BookTitle         = string.Empty;
            BookAuthor        = string.Empty;
            BookDescription   = string.Empty;
            BookIsbn          = string.Empty;
            BookPublishedYear = DateTime.Now.Year;
            BookPageCount     = 0;
            BookPrice         = 0;
            ShowPublishForm   = false;

            // Actualizamos las estadísticas
            await LoadStatsAsync();
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
            OnPropertyChanged(nameof(HasError));
        }
        catch
        {
            ErrorMessage = "No se pudo publicar el libro.";
            OnPropertyChanged(nameof(HasError));
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SwitchTabAsync(string tab)
    {
        ActiveTab = tab;
        OnPropertyChanged(nameof(IsInventoryTab));
        OnPropertyChanged(nameof(IsStatsTab));
        OnPropertyChanged(nameof(IsPublishTab));

        if (tab == "stats")
            await LoadStatsAsync();
    }

    [RelayCommand]
    private void TogglePublishForm()
        => ShowPublishForm = !ShowPublishForm;

    [RelayCommand]
    private async Task GoToMarketplaceAsync()
    {
        var vm = _marketplaceVmFactory();
        await vm.InitializeAsync();
        _navigationService.NavigateTo(vm);
    }
}
