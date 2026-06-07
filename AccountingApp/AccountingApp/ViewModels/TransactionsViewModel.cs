using System.Collections.ObjectModel;
using AccountingApp.Models;
using AccountingApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AccountingApp.ViewModels;

public partial class TransactionsViewModel : BaseViewModel
{
    private readonly DatabaseService _db;

    public ObservableCollection<Transaction> Transactions { get; } = new();

    [ObservableProperty]
    private string _filterType = "الكل";

    public TransactionsViewModel(DatabaseService db)
    {
        _db = db;
        Title = "جميع المعاملات";
    }

    [RelayCommand]
    private async Task LoadTransactionsAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var all = await _db.GetAllTransactionsAsync();

            if (FilterType == "له")
                all = all.Where(t => t.Type == "له").ToList();
            else if (FilterType == "عليه")
                all = all.Where(t => t.Type == "عليه").ToList();

            Transactions.Clear();
            foreach (var t in all)
                Transactions.Add(t);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task FilterByTypeAsync(string type)
    {
        FilterType = type;
        await LoadTransactionsAsync();
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
        await LoadTransactionsAsync();
    }
}
