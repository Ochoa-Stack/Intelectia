using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Intelectia.Shared.DTOs.Groups;
using Intelectia.WPF.Core;
using Intelectia.WPF.Services;

namespace Intelectia.WPF.ViewModels;

public partial class GroupsViewModel : BaseViewModel
{
    private readonly GroupsService _groupsService;
    private readonly NavigationService _navigationService;
    private readonly Func<GroupChatViewModel> _chatVmFactory;
    private readonly Func<MarketplaceViewModel> _marketplaceVmFactory;

    public ObservableCollection<GroupDto> MyGroups { get; } = new();
    public ObservableCollection<GroupDto> PublicGroups { get; } = new();

    [ObservableProperty]
    private string _activeTab = "my";

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _showCreateForm;

    [ObservableProperty]
    private string _newGroupName = string.Empty;

    [ObservableProperty]
    private string _newGroupDescription = string.Empty;

    [ObservableProperty]
    private bool _newGroupIsPublic = true;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool IsMyTab => ActiveTab == "my";
    public bool IsExploreTab => ActiveTab == "explore";

    public GroupsViewModel(
        GroupsService groupsService,
        NavigationService navigationService,
        Func<GroupChatViewModel> chatVmFactory,
        Func<MarketplaceViewModel> marketplaceVmFactory)
    {
        _groupsService        = groupsService;
        _navigationService    = navigationService;
        _chatVmFactory        = chatVmFactory;
        _marketplaceVmFactory = marketplaceVmFactory;
        Title = "Grupos de Estudio";
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await LoadMyGroupsAsync();
    }

    [RelayCommand]
    private async Task LoadMyGroupsAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var groups = await _groupsService.GetMyGroupsAsync();
            MyGroups.Clear();
            foreach (var g in groups)
                MyGroups.Add(g);
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
            OnPropertyChanged(nameof(HasError));
        }
        catch
        {
            ErrorMessage = "No se pudieron cargar los grupos.";
            OnPropertyChanged(nameof(HasError));
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadPublicGroupsAsync()
    {
        IsBusy = true;

        try
        {
            var groups = await _groupsService.GetPublicGroupsAsync(SearchText);
            PublicGroups.Clear();
            foreach (var g in groups)
                PublicGroups.Add(g);
        }
        catch (System.Exception ex) { System.Diagnostics.Debug.WriteLine(ex); }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CreateGroupAsync()
    {
        if (string.IsNullOrWhiteSpace(NewGroupName)) return;
        IsBusy = true;

        try
        {
            var group = await _groupsService.CreateGroupAsync(new CreateGroupRequest
            {
                Name        = NewGroupName,
                Description = NewGroupDescription,
                IsPublic    = NewGroupIsPublic
            });

            MyGroups.Insert(0, group);
            NewGroupName        = string.Empty;
            NewGroupDescription = string.Empty;
            ShowCreateForm      = false;

            // Abrimos el chat del grupo reciÃ©n creado
            await OpenGroupChatAsync(group);
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
    private async Task JoinGroupAsync(GroupDto group)
    {
        try
        {
            await _groupsService.JoinGroupAsync(group.Id);
            PublicGroups.Remove(group);
            group.UserRole = "Member";
            MyGroups.Insert(0, group);
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
            OnPropertyChanged(nameof(HasError));
        }
    }

    // Navega al chat del grupo seleccionado
    [RelayCommand]
    private async Task OpenGroupChatAsync(GroupDto group)
    {
        var vm = _chatVmFactory();
        await vm.InitializeAsync(group);
        _navigationService.NavigateTo(vm);
    }

    [RelayCommand]
    private async Task SwitchTabAsync(string tab)
    {
        ActiveTab = tab;
        OnPropertyChanged(nameof(IsMyTab));
        OnPropertyChanged(nameof(IsExploreTab));

        if (tab == "explore")
            await LoadPublicGroupsAsync();
    }

    [RelayCommand]
    private void ToggleCreateForm()
        => ShowCreateForm = !ShowCreateForm;

    [RelayCommand]
    private async Task GoToMarketplaceAsync()
    {
        var vm = _marketplaceVmFactory();
        await vm.InitializeAsync();
        _navigationService.NavigateTo(vm);
    }
}

