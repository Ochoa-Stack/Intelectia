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
    private readonly CommerceService _commerceService;
    private readonly Func<MarketplaceViewModel> _marketplaceVmFactory;
    private readonly Func<CartViewModel> _cartVmFactory;

    [ObservableProperty]
    private BookDetailDto? _book;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public BookDetailViewModel(
        MarketplaceService marketplaceService,
        NavigationService navigationService,
        CommerceService commerceService,
        Func<MarketplaceViewModel> marketplaceVmFactory,
        Func<CartViewModel> cartVmFactory)
    {
        _marketplaceService   = marketplaceService;
        _navigationService    = navigationService;
        _commerceService      = commerceService;
        _marketplaceVmFactory = marketplaceVmFactory;
        _cartVmFactory        = cartVmFactory;
        Title = "Detalle del libro";
    }

    // Carga el libro por ID; lo llama el MarketplaceViewModel antes de navegar
    public async Task LoadAsync(Guid bookId)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            Book  = await _marketplaceService.GetBookByIdAsync(bookId);
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

    // Agrega el libro al carrito y navega a CartView
    [RelayCommand]
    private async Task AddToCartAsync()
    {
        if (Book is null) return;

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            await _commerceService.AddToCartAsync(Book.Id);

            var vm = _cartVmFactory();
            await vm.LoadAsync();
            _navigationService.NavigateTo(vm);
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
            OnPropertyChanged(nameof(HasError));
        }
        catch
        {
            ErrorMessage = "No se pudo agregar el libro al carrito.";
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
