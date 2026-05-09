using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
using Intelectia.Shared.DTOs.Groups;
using Intelectia.WPF.Core;
using Intelectia.WPF.Services;

namespace Intelectia.WPF.ViewModels;

public partial class GroupChatViewModel : BaseViewModel
{
    private readonly GroupsService _groupsService;
    private readonly NavigationService _navigationService;
    private readonly TokenStore _tokenStore;
    private readonly Func<GroupsViewModel> _groupsVmFactory;

    private HubConnection? _hubConnection;

    public ObservableCollection<GroupMessageDto> Messages { get; } = new();

    [ObservableProperty]
    private GroupDto? _group;

    [ObservableProperty]
    private string _messageText = string.Empty;

    [ObservableProperty]
    private string _typingIndicator = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    private CancellationTokenSource? _typingCts;

    public GroupChatViewModel(
        GroupsService groupsService,
        NavigationService navigationService,
        TokenStore tokenStore,
        Func<GroupsViewModel> groupsVmFactory)
    {
        _groupsService    = groupsService;
        _navigationService = navigationService;
        _tokenStore       = tokenStore;
        _groupsVmFactory  = groupsVmFactory;
    }

    // Inicializa el chat; carga historial y conecta al Hub
    public async Task InitializeAsync(GroupDto group)
    {
        Group = group;
        Title = group.Name;

        await LoadHistoryAsync();
        await ConnectToHubAsync();
    }

    // Carga los mensajes previos del grupo
    private async Task LoadHistoryAsync()
    {
        try
        {
            var result = await _groupsService.GetMessagesAsync(Group!.Id);

            // Los mensajes vienen del mÃ¡s reciente al mÃ¡s antiguo; invertimos para mostrar cronolÃ³gicamente
            Messages.Clear();
            foreach (var msg in result.Items.Reverse())
                Messages.Add(msg);
        }
        catch (System.Exception ex) { System.Diagnostics.Debug.WriteLine(ex); }
    }

    // Establece la conexiÃ³n con el Hub de SignalR
    private async Task ConnectToHubAsync()
    {
        try
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5028/hubs/chat", options =>
                {
                    // Inyectamos el token JWT en la conexiÃ³n de SignalR
                    options.AccessTokenProvider = () =>
                        Task.FromResult(_tokenStore.AccessToken);
                })
                .WithAutomaticReconnect()
                .Build();

            // Escuchamos los mensajes nuevos del grupo
            _hubConnection.On<GroupMessageDto>("ReceiveMessage", msg =>
            {
                // Actualizamos la UI en el hilo principal
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Add(msg);
                    TypingIndicator = string.Empty;
                });
            });

            // Escuchamos el indicador de escritura
            _hubConnection.On<string>("UserTyping", userId =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    TypingIndicator = "Alguien estÃ¡ escribiendo...";
                });

                // Ocultamos el indicador despuÃ©s de 2 segundos
                _ = Task.Delay(2000).ContinueWith(_ =>
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        TypingIndicator = string.Empty));
            });

            await _hubConnection.StartAsync();

            // Entramos al canal del grupo
            await _hubConnection.InvokeAsync("JoinGroup", Group!.Id.ToString());
        }
        catch (Exception ex)
        {
            ErrorMessage = $"No se pudo conectar al chat: {ex.Message}";
            OnPropertyChanged(nameof(HasError));
        }
    }

    // EnvÃ­a el mensaje al Hub
    [RelayCommand]
    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(MessageText) || _hubConnection is null)
            return;

        var content = MessageText.Trim();
        MessageText = string.Empty;

        try
        {
            await _hubConnection.InvokeAsync(
                "SendMessage", Group!.Id.ToString(), content);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"No se pudo enviar el mensaje: {ex.Message}";
            OnPropertyChanged(nameof(HasError));
        }
    }

    // Notifica al Hub que el usuario estÃ¡ escribiendo
    [RelayCommand]
    private async Task NotifyTypingAsync()
    {
        if (_hubConnection?.State != HubConnectionState.Connected) return;

        _typingCts?.Cancel();
        _typingCts = new CancellationTokenSource();

        try
        {
            await _hubConnection.InvokeAsync("SendTyping", Group!.Id.ToString());
        }
        catch (System.Exception ex) { System.Diagnostics.Debug.WriteLine(ex); }
    }

    // Vuelve a la lista de grupos y desconecta el Hub
    [RelayCommand]
    private async Task GoBackAsync()
    {
        await DisconnectAsync();
        var vm = _groupsVmFactory();
        await vm.InitializeAsync();
        _navigationService.NavigateTo(vm);
    }

    // Desconecta el Hub al salir
    private async Task DisconnectAsync()
    {
        if (_hubConnection is not null)
        {
            try
            {
                await _hubConnection.InvokeAsync("LeaveGroup", Group!.Id.ToString());
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
            }
            catch (System.Exception ex) { System.Diagnostics.Debug.WriteLine(ex); }
            finally
            {
                _hubConnection = null;
            }
        }
    }
}

