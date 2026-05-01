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

        // Inicializamos el NavigationService con el MainViewModel
        var navigation = Services.GetRequiredService<NavigationService>();
        navigation.Initialize(mainViewModel);

        // Navegamos a login como primera pantalla
        navigation.NavigateTo(Services.GetRequiredService<LoginViewModel>());

        var mainWindow = new MainWindow { DataContext = mainViewModel };
        mainWindow.Show();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // HttpClient apuntando a la API
        services.AddHttpClient<ApiClient>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:5028/");
        });

        // Servicios de la aplicación; Singleton para que persistan toda la sesión
        services.AddSingleton<NavigationService>();
        services.AddSingleton<AuthService>();
        services.AddSingleton<MarketplaceService>();

        // MainViewModel es Singleton; es la raíz de la ventana, debe ser la misma instancia
        services.AddSingleton<MainViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<ForgotPasswordViewModel>();
        services.AddTransient<MarketplaceViewModel>();
        services.AddTransient<BookDetailViewModel>();

        // Factories para navegación entre ViewModels sin dependencias circulares
        services.AddTransient<Func<LoginViewModel>>(sp           => () => sp.GetRequiredService<LoginViewModel>());
        services.AddTransient<Func<RegisterViewModel>>(sp        => () => sp.GetRequiredService<RegisterViewModel>());
        services.AddTransient<Func<ForgotPasswordViewModel>>(sp  => () => sp.GetRequiredService<ForgotPasswordViewModel>());
        services.AddTransient<Func<MarketplaceViewModel>>(sp     => () => sp.GetRequiredService<MarketplaceViewModel>());
        services.AddTransient<Func<BookDetailViewModel>>(sp      => () => sp.GetRequiredService<BookDetailViewModel>());
    }
}
