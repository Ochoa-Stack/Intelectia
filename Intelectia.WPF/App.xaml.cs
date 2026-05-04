using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Intelectia.WPF.Services;
using Intelectia.WPF.ViewModels;
using Intelectia.WPF.ViewModels.Auth;

namespace Intelectia.WPF;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        var mainViewModel = Services.GetRequiredService<MainViewModel>();
        var navigation    = Services.GetRequiredService<NavigationService>();
        navigation.Initialize(mainViewModel);
        navigation.NavigateTo(Services.GetRequiredService<LoginViewModel>());

        var mainWindow = new MainWindow { DataContext = mainViewModel };
        mainWindow.Show();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // TokenStore Singleton; almacén compartido del JWT activo entre todos los servicios
        services.AddSingleton<TokenStore>();

        // Handler que inyecta el token en cada petición HTTP saliente
        services.AddTransient<AuthTokenHandler>();

        // HttpClient con el handler de autenticación; resuelve el problema de instancias múltiples
        services.AddHttpClient<ApiClient>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:5028/");
        })
        .AddHttpMessageHandler<AuthTokenHandler>();

        // Servicios Singleton; persisten toda la sesión
        services.AddSingleton<NavigationService>();
        services.AddSingleton<AuthService>();
        services.AddSingleton<MarketplaceService>();
        services.AddSingleton<CommerceService>();
        services.AddSingleton<LibraryService>();
        services.AddSingleton<VendorService>();
        services.AddSingleton<GroupsService>();

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<ForgotPasswordViewModel>();
        services.AddTransient<MarketplaceViewModel>();
        services.AddTransient<BookDetailViewModel>();
        services.AddTransient<CartViewModel>();
        services.AddTransient<CheckoutViewModel>();
        services.AddTransient<OrderHistoryViewModel>();
        services.AddTransient<LibraryViewModel>();
        services.AddTransient<VendorDashboardViewModel>();
        services.AddTransient<VendorOnboardingViewModel>();
        services.AddTransient<GroupsViewModel>();
        services.AddTransient<GroupChatViewModel>();

        // Factories para navegación sin dependencias circulares
        services.AddTransient<Func<LoginViewModel>>(sp           => () => sp.GetRequiredService<LoginViewModel>());
        services.AddTransient<Func<RegisterViewModel>>(sp        => () => sp.GetRequiredService<RegisterViewModel>());
        services.AddTransient<Func<ForgotPasswordViewModel>>(sp  => () => sp.GetRequiredService<ForgotPasswordViewModel>());
        services.AddTransient<Func<MarketplaceViewModel>>(sp     => () => sp.GetRequiredService<MarketplaceViewModel>());
        services.AddTransient<Func<BookDetailViewModel>>(sp      => () => sp.GetRequiredService<BookDetailViewModel>());
        services.AddTransient<Func<CartViewModel>>(sp            => () => sp.GetRequiredService<CartViewModel>());
        services.AddTransient<Func<CheckoutViewModel>>(sp        => () => sp.GetRequiredService<CheckoutViewModel>());
        services.AddTransient<Func<OrderHistoryViewModel>>(sp    => () => sp.GetRequiredService<OrderHistoryViewModel>());
        services.AddTransient<Func<LibraryViewModel>>(sp         => () => sp.GetRequiredService<LibraryViewModel>());
        services.AddTransient<Func<VendorDashboardViewModel>>(sp => () => sp.GetRequiredService<VendorDashboardViewModel>());
        services.AddTransient<Func<VendorOnboardingViewModel>>(sp => () => sp.GetRequiredService<VendorOnboardingViewModel>());
        services.AddTransient<Func<GroupsViewModel>>(sp  => () => sp.GetRequiredService<GroupsViewModel>());
        services.AddTransient<Func<GroupChatViewModel>>(sp => () => sp.GetRequiredService<GroupChatViewModel>());
    }
}
