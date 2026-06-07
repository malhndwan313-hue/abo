using System.Collections.ObjectModel;
using AccountingApp.Models;
using AccountingApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AccountingApp.ViewModels;

public partial class ClientsViewModel : BaseViewModel
{
    private readonly DatabaseService _db;

    public ObservableCollection<Client> Clients { get; } = new();

    [ObservableProperty]
    private string _newClientName = string.Empty;

    [ObservableProperty]
    private string _newClientPhone = string.Empty;

    [ObservableProperty]
    private string _newClientAddress = string.Empty;

    // Transaction fields
    [ObservableProperty]
    private Client? _selectedClient;

    [ObservableProperty]
    private string _transDetail = string.Empty;

    [ObservableProperty]
    private decimal _transAmount;

    [ObservableProperty]
    private string _transCurrency = "ريال يمني";

    [ObservableProperty]
    private DateTime _transDate = DateTime.Today;

    [ObservableProperty]
    private string _transType = "له";

    [ObservableProperty]
    private bool _isClientSelected;

    [ObservableProperty]
    private ObservableCollection<Transaction> _clientTransactions = new();

    public ClientsViewModel(DatabaseService db)
    {
        _db = db;
        Title = "العملاء";
    }

    [RelayCommand]
    private async Task LoadClientsAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var clients = await _db.GetClientsAsync();
            Clients.Clear();
            foreach (var c in clients)
                Clients.Add(c);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task AddClientAsync()
    {
        if (string.IsNullOrWhiteSpace(NewClientName)) return;

        var client = new Client
        {
            Name = NewClientName.Trim(),
            Phone = NewClientPhone.Trim(),
            Address = NewClientAddress.Trim()
        };

        await _db.SaveClientAsync(client);
        NewClientName = string.Empty;
        NewClientPhone = string.Empty;
        NewClientAddress = string.Empty;
        await LoadClientsAsync();
    }

    [RelayCommand]
    private async Task DeleteClientAsync(Client client)
    {
        bool confirm = await Shell.Current.DisplayAlert(
            "تأكيد الحذف",
            $"هل تريد حذف العميل '{client.Name}' وجميع معاملاته؟",
            "حذف", "إلغاء");

        if (!confirm) return;

        await _db.DeleteClientAsync(client);
        await LoadClientsAsync();

        if (SelectedClient?.Id == client.Id)
        {
            SelectedClient = null;
            IsClientSelected = false;
        }
    }

    [RelayCommand]
    private async Task SelectClientAsync(Client client)
    {
        SelectedClient = client;
        IsClientSelected = true;
        await LoadClientTransactionsAsync();
    }

    [RelayCommand]
    private void DeselectClient()
    {
        SelectedClient = null;
        IsClientSelected = false;
        ClientTransactions.Clear();
    }

    [RelayCommand]
    private async Task LoadClientTransactionsAsync()
    {
        if (SelectedClient == null) return;
        var transactions = await _db.GetTransactionsAsync(SelectedClient.Id, "client");
        ClientTransactions = new ObservableCollection<Transaction>(transactions);
    }

    [RelayCommand]
    private async Task AddTransactionAsync()
    {
        if (SelectedClient == null || string.IsNullOrWhiteSpace(TransDetail) || TransAmount <= 0)
            return;

        var transaction = new Transaction
        {
            Details = TransDetail.Trim(),
            Amount = TransAmount,
            Currency = TransCurrency,
            Date = TransDate,
            Type = TransType,
            EntityType = "client",
            EntityId = SelectedClient.Id
        };

        await _db.SaveTransactionAsync(transaction);
        TransDetail = string.Empty;
        TransAmount = 0;
        TransDate = DateTime.Today;
        TransType = "له";
        await LoadClientTransactionsAsync();
        await LoadClientsAsync();
    }

    [RelayCommand]
    private async Task DeleteTransactionAsync(Transaction transaction)
    {
        bool confirm = await Shell.Current.DisplayAlert(
            "حذف المعاملة",
            $"هل تريد حذف '{transaction.Details}'؟",
            "حذف", "إلغاء");

        if (!confirm) return;

        await _db.DeleteTransactionAsync(transaction);
        await LoadClientTransactionsAsync();
        await LoadClientsAsync();
    }
}
