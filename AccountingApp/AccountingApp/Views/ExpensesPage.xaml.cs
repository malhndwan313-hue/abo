using AccountingApp.ViewModels;

namespace AccountingApp.Views;

public partial class ExpensesPage : ContentPage
{
    private readonly ExpensesViewModel _viewModel;

    public ExpensesPage(ExpensesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadExpensesCommand.ExecuteAsync(null);
    }
}
