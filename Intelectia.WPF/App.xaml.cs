using System.Windows;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
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

        var mainWindow = new MainWindow { DataContext = mainViewModel };
        mainWindow.Show();

        // Llamada fire-and-forget segura a inicialización asíncrona
        _ = InitializeAsync(navigation);
    }

    private async Task InitializeAsync(NavigationService navigation)
    {
        try
        {
            var authService = Services.GetRequiredService<AuthService>();
            var sessionRestored = await authService.TryRestoreSessionAsync();

            if (sessionRestored)
            {
                var marketplaceVm = Services.GetRequiredService<MarketplaceViewModel>();
                await marketplaceVm.InitializeAsync();
                navigation.NavigateTo(marketplaceVm);
            }
            else
            {
                navigation.NavigateTo(Services.GetRequiredService<LoginViewModel>());
            }
        }
        catch (Exception ex)
        {
            // Log error or show message
            System.Diagnostics.Debug.WriteLine($"Error al iniciar: {ex.Message}");
            navigation.NavigateTo(Services.GetRequiredService<LoginViewModel>());
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Leemos la URL base desde configuración para permitir cambio entre dev/prod
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var baseUrl = config["ApiSettings:BaseUrl"] ?? "https://localhost:8080";

        // Servicios de infraestructura
        services.AddSingleton<TokenStore>();
        services.AddSingleton<ToastService>();
        services.AddSingleton<ConnectivityService>();
        services.AddSingleton<CredentialService>();
        services.AddSingleton<GoogleAuthService>();
        services.AddTransient<AuthTokenHandler>();

        // HttpClient con handler de auth
        services.AddHttpClient(nameof(ApiClient), client =>
        {
            client.BaseAddress = new Uri(baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/");
        })
        .AddHttpMessageHandler<AuthTokenHandler>();

        // ApiClient con IServiceProvider para el interceptor
        services.AddSingleton<ApiClient>(sp =>
        {
            var factory    = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = factory.CreateClient(nameof(ApiClient));
            return new ApiClient(httpClient, sp);
        });

        // Servicios de dominio, implementamos Singleton para persistir toda la sesión
        services.AddSingleton<NavigationService>();
        services.AddSingleton<AuthService>();
        services.AddSingleton<MarketplaceService>();
        services.AddSingleton<CommerceService>();
        services.AddSingleton<LibraryService>();
        services.AddSingleton<VendorService>();
        services.AddSingleton<GroupsService>();
        services.AddSingleton<ProfileService>();

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
        services.AddTransient<VendorOnboardingViewModel>();
        services.AddTransient<VendorDashboardViewModel>();
        services.AddTransient<GroupsViewModel>();
        services.AddTransient<GroupChatViewModel>();
        services.AddTransient<ProfileViewModel>();

        // Factories
        services.AddTransient<Func<LoginViewModel>>(sp             => () => sp.GetRequiredService<LoginViewModel>());
        services.AddTransient<Func<RegisterViewModel>>(sp          => () => sp.GetRequiredService<RegisterViewModel>());
        services.AddTransient<Func<ForgotPasswordViewModel>>(sp    => () => sp.GetRequiredService<ForgotPasswordViewModel>());
        services.AddTransient<Func<MarketplaceViewModel>>(sp       => () => sp.GetRequiredService<MarketplaceViewModel>());
        services.AddTransient<Func<BookDetailViewModel>>(sp        => () => sp.GetRequiredService<BookDetailViewModel>());
        services.AddTransient<Func<CartViewModel>>(sp              => () => sp.GetRequiredService<CartViewModel>());
        services.AddTransient<Func<CheckoutViewModel>>(sp          => () => sp.GetRequiredService<CheckoutViewModel>());
        services.AddTransient<Func<OrderHistoryViewModel>>(sp      => () => sp.GetRequiredService<OrderHistoryViewModel>());
        services.AddTransient<Func<LibraryViewModel>>(sp           => () => sp.GetRequiredService<LibraryViewModel>());
        services.AddTransient<Func<VendorOnboardingViewModel>>(sp  => () => sp.GetRequiredService<VendorOnboardingViewModel>());
        services.AddTransient<Func<VendorDashboardViewModel>>(sp   => () => sp.GetRequiredService<VendorDashboardViewModel>());
        services.AddTransient<Func<GroupsViewModel>>(sp            => () => sp.GetRequiredService<GroupsViewModel>());
        services.AddTransient<Func<GroupChatViewModel>>(sp         => () => sp.GetRequiredService<GroupChatViewModel>());
        services.AddTransient<Func<ProfileViewModel>>(sp           => () => sp.GetRequiredService<ProfileViewModel>());
        services.AddTransient<Func<GoogleAuthService>>(sp          => () => sp.GetRequiredService<GoogleAuthService>());
    }
}
