using AccountingApp.Services;
using AccountingApp.ViewModels;
using AccountingApp.Views;
using Microsoft.Extensions.Logging;

namespace AccountingApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Services
        builder.Services.AddSingleton<DatabaseService>();

        // ViewModels
        builder.Services.AddTransient<ClientsViewModel>();
        builder.Services.AddTransient<SuppliersViewModel>();
        builder.Services.AddTransient<TransactionsViewModel>();
        builder.Services.AddTransient<ExpensesViewModel>();
        builder.Services.AddTransient<ReportsViewModel>();

        // Views
        builder.Services.AddTransient<ClientsPage>();
        builder.Services.AddTransient<SuppliersPage>();
        builder.Services.AddTransient<TransactionsPage>();
        builder.Services.AddTransient<ExpensesPage>();
        builder.Services.AddTransient<ReportsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
