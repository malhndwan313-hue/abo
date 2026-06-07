using AccountingApp.ViewModels;

namespace AccountingApp.Views;

public partial class ClientsPage : ContentPage
{
    private readonly ClientsViewModel _viewModel;

    public ClientsPage(ClientsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadClientsCommand.ExecuteAsync(null);
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
