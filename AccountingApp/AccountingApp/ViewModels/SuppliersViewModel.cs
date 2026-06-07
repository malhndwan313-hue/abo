using System.Collections.ObjectModel;
using AccountingApp.Models;
using AccountingApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AccountingApp.ViewModels;

public partial class SuppliersViewModel : BaseViewModel
{
    private readonly DatabaseService _db;

    public ObservableCollection<Supplier> Suppliers { get; } = new();

    [ObservableProperty]
    private string _newSupplierName = string.Empty;

    [ObservableProperty]
    private string _newSupplierPhone = string.Empty;

    [ObservableProperty]
    private string _newSupplierCompany = string.Empty;

    [ObservableProperty]
    private string _newSupplierAddress = string.Empty;

    // Transaction fields
    [ObservableProperty]
    private Supplier? _selectedSupplier;

    [ObservableProperty]
    private string _transDetail = string.Empty;

    [ObservableProperty]
    private decimal _transAmount;

    [ObservableProperty]
    private string _transCurrency = "ريال يمني";

    [ObservableProperty]
    private DateTime _transDate = DateTime.Today;

    [ObservableProperty]
    private string _transType = "عليه";

    [ObservableProperty]
    private bool _isSupplierSelected;

    [ObservableProperty]
    private ObservableCollection<Transaction> _supplierTransactions = new();

    public SuppliersViewModel(DatabaseService db)
    {
        _db = db;
        Title = "الموردين";
    }

    [RelayCommand]
    private async Task LoadSuppliersAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var suppliers = await _db.GetSuppliersAsync();
            Suppliers.Clear();
            foreach (var s in suppliers)
                Suppliers.Add(s);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task AddSupplierAsync()
    {
        if (string.IsNullOrWhiteSpace(NewSupplierName)) return;

        var supplier = new Supplier
        {
            Name = NewSupplierName.Trim(),
            Phone = NewSupplierPhone.Trim(),
            CompanyName = NewSupplierCompany.Trim(),
            Address = NewSupplierAddress.Trim()
        };

        await _db.SaveSupplierAsync(supplier);
        NewSupplierName = string.Empty;
        NewSupplierPhone = string.Empty;
        NewSupplierCompany = string.Empty;
        NewSupplierAddress = string.Empty;
        await LoadSuppliersAsync();
    }

    [RelayCommand]
    private async Task DeleteSupplierAsync(Supplier supplier)
    {
        bool confirm = await Shell.Current.DisplayAlert(
            "تأكيد الحذف",
            $"هل تريد حذف المورد '{supplier.Name}' وجميع معاملاته؟",
            "حذف", "إلغاء");

        if (!confirm) return;

        await _db.DeleteSupplierAsync(supplier);
        await LoadSuppliersAsync();

        if (SelectedSupplier?.Id == supplier.Id)
        {
            SelectedSupplier = null;
            IsSupplierSelected = false;
        }
    }

    [RelayCommand]
    private async Task SelectSupplierAsync(Supplier supplier)
    {
        SelectedSupplier = supplier;
        IsSupplierSelected = true;
        await LoadSupplierTransactionsAsync();
    }

    [RelayCommand]
    private void DeselectSupplier()
    {
        SelectedSupplier = null;
        IsSupplierSelected = false;
        SupplierTransactions.Clear();
    }

    [RelayCommand]
    private async Task LoadSupplierTransactionsAsync()
    {
        if (SelectedSupplier == null) return;
        var transactions = await _db.GetTransactionsAsync(SelectedSupplier.Id, "supplier");
        SupplierTransactions = new ObservableCollection<Transaction>(transactions);
    }

    [RelayCommand]
    private async Task AddTransactionAsync()
    {
        if (SelectedSupplier == null || string.IsNullOrWhiteSpace(TransDetail) || TransAmount <= 0)
            return;

        var transaction = new Transaction
        {
            Details = TransDetail.Trim(),
            Amount = TransAmount,
            Currency = TransCurrency,
            Date = TransDate,
            Type = TransType,
            EntityType = "supplier",
            EntityId = SelectedSupplier.Id
        };

        await _db.SaveTransactionAsync(transaction);
        TransDetail = string.Empty;
        TransAmount = 0;
        TransDate = DateTime.Today;
        TransType = "عليه";
        await LoadSupplierTransactionsAsync();
        await LoadSuppliersAsync();
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
        await LoadSupplierTransactionsAsync();
        await LoadSuppliersAsync();
    }
}
