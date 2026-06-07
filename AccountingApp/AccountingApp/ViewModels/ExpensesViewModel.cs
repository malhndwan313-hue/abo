using System.Collections.ObjectModel;
using AccountingApp.Models;
using AccountingApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AccountingApp.ViewModels;

public partial class ExpensesViewModel : BaseViewModel
{
    private readonly DatabaseService _db;

    public ObservableCollection<Expense> Expenses { get; } = new();

    [ObservableProperty]
    private string _newDescription = string.Empty;

    [ObservableProperty]
    private decimal _newAmount;

    [ObservableProperty]
    private string _newCurrency = "ريال يمني";

    [ObservableProperty]
    private string _newCategory = "عام";

    [ObservableProperty]
    private DateTime _newDate = DateTime.Today;

    [ObservableProperty]
    private string _newNotes = string.Empty;

    [ObservableProperty]
    private decimal _totalExpenses;

    public ExpensesViewModel(DatabaseService db)
    {
        _db = db;
        Title = "المصاريف";
    }

    [RelayCommand]
    private async Task LoadExpensesAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var expenses = await _db.GetExpensesAsync();
            Expenses.Clear();
            foreach (var e in expenses)
                Expenses.Add(e);

            TotalExpenses = expenses.Sum(e => e.Amount);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task AddExpenseAsync()
    {
        if (string.IsNullOrWhiteSpace(NewDescription) || NewAmount <= 0) return;

        var expense = new Expense
        {
            Description = NewDescription.Trim(),
            Amount = NewAmount,
            Currency = NewCurrency,
            Category = NewCategory.Trim(),
            Date = NewDate,
            Notes = NewNotes.Trim()
        };

        await _db.SaveExpenseAsync(expense);
        NewDescription = string.Empty;
        NewAmount = 0;
        NewCategory = "عام";
        NewDate = DateTime.Today;
        NewNotes = string.Empty;
        await LoadExpensesAsync();
    }

    [RelayCommand]
    private async Task DeleteExpenseAsync(Expense expense)
    {
        bool confirm = await Shell.Current.DisplayAlert(
            "حذف المصروف",
            $"هل تريد حذف '{expense.Description}'؟",
            "حذف", "إلغاء");

        if (!confirm) return;

        await _db.DeleteExpenseAsync(expense);
        await LoadExpensesAsync();
    }
}
