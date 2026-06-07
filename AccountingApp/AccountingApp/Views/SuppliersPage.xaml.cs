using AccountingApp.ViewModels;

namespace AccountingApp.Views;

public partial class SuppliersPage : ContentPage
{
    private readonly SuppliersViewModel _viewModel;

    public SuppliersPage(SuppliersViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadSuppliersCommand.ExecuteAsync(null);
    }

    private void OnCreditClicked(object? sender, EventArgs e)
    {
        _viewModel.TransType = "له";
    }

    private void OnDebitClicked(object? sender, EventArgs e)
    {
        _viewModel.TransType = "عليه";
    }
}
