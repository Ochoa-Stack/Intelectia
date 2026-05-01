using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Intelectia.Shared.DTOs.Marketplace;
using Intelectia.WPF.Core;
using Intelectia.WPF.Services;

namespace Intelectia.WPF.ViewModels;

public partial class BookDetailViewModel : BaseViewModel
{
    private readonly MarketplaceService _marketplaceService;
    private readonly NavigationService _navigationService;
    private readonly Func<MarketplaceViewModel> _marketplaceVmFactory;

    [ObservableProperty]
    private BookDetailDto? _book;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public BookDetailViewModel(
        MarketplaceService marketplaceService,
        NavigationService navigationService,
        Func<MarketplaceViewModel> marketplaceVmFactory)
    {
        _marketplaceService   = marketplaceService;
        _navigationService    = navigationService;
        _marketplaceVmFactory = marketplaceVmFactory;
        Title = "Detalle del libro";
    }

    // Carga el libro por ID; lo llama el MarketplaceViewModel antes de navegar
    public async Task LoadAsync(Guid bookId)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            Book = await _marketplaceService.GetBookByIdAsync(bookId);
            Title = Book.Title;
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
            OnPropertyChanged(nameof(HasError));
        }
        catch
        {
            ErrorMessage = "No se pudo cargar el libro.";
            OnPropertyChanged(nameof(HasError));
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Vuelve al catálogo inicializando el ViewModel de marketplace
    [RelayCommand]
    private async Task GoBackAsync()
    {
        var vm = _marketplaceVmFactory();
        await vm.InitializeAsync();
        _navigationService.NavigateTo(vm);
    }
}
