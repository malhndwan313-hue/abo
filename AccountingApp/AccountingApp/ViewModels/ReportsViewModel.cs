using AccountingApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AccountingApp.ViewModels;

public partial class ReportsViewModel : BaseViewModel
{
    private readonly DatabaseService _db;

    [ObservableProperty]
    private decimal _totalReceivables;

    [ObservableProperty]
    private decimal _totalPayables;

    [ObservableProperty]
    private decimal _totalExpenses;

    [ObservableProperty]
    private decimal _netBalance;

    [ObservableProperty]
    private int _clientCount;

    [ObservableProperty]
    private int _supplierCount;

    [ObservableProperty]
    private string _netBalanceText = string.Empty;

    [ObservableProperty]
    private Color _netBalanceColor = Color.FromArgb("#5f7d9c");

    public ReportsViewModel(DatabaseService db)
    {
        _db = db;
        Title = "التقارير";
    }

    [RelayCommand]
    private async Task LoadReportAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var clients = await _db.GetClientsAsync();
            var suppliers = await _db.GetSuppliersAsync();

            ClientCount = clients.Count;
            SupplierCount = suppliers.Count;

            TotalReceivables = clients.Where(c => c.Balance > 0).Sum(c => c.Balance);
            TotalPayables = suppliers.Where(s => s.Balance < 0).Sum(s => Math.Abs(s.Balance))
                          + clients.Where(c => c.Balance < 0).Sum(c => Math.Abs(c.Balance));
            TotalExpenses = await _db.GetTotalExpensesAsync();

            NetBalance = TotalReceivables - TotalPayables;

            if (NetBalance > 0)
            {
                NetBalanceText = $"صافي لصالحك: {NetBalance:N2}";
                NetBalanceColor = Color.FromArgb("#059669");
            }
            else if (NetBalance < 0)
            {
                NetBalanceText = $"صافي عليك: {Math.Abs(NetBalance):N2}";
                NetBalanceColor = Color.FromArgb("#dc2626");
            }
            else
            {
                NetBalanceText = "الحسابات متزنة";
                NetBalanceColor = Color.FromArgb("#5f7d9c");
            }
        }
        finally { IsBusy = false; }
    }
}
