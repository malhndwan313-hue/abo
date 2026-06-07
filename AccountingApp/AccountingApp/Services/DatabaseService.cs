using AccountingApp.Models;
using SQLite;

namespace AccountingApp.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection? _db;
    private const string DbName = "accounting.db3";

    private async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_db != null) return _db;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, DbName);
        _db = new SQLiteAsyncConnection(dbPath);

        await _db.CreateTableAsync<Client>();
        await _db.CreateTableAsync<Supplier>();
        await _db.CreateTableAsync<Transaction>();
        await _db.CreateTableAsync<Expense>();

        return _db;
    }

    // ── Clients ──────────────────────────────────────────────

    public async Task<List<Client>> GetClientsAsync()
    {
        var db = await GetConnectionAsync();
        var clients = await db.Table<Client>().OrderByDescending(c => c.CreatedAt).ToListAsync();

        foreach (var client in clients)
        {
            client.Balance = await CalculateEntityBalanceAsync(client.Id, "client");
        }

        return clients;
    }

    public async Task<Client?> GetClientAsync(int id)
    {
        var db = await GetConnectionAsync();
        var client = await db.FindAsync<Client>(id);
        if (client != null)
            client.Balance = await CalculateEntityBalanceAsync(id, "client");
        return client;
    }

    public async Task<int> SaveClientAsync(Client client)
    {
        var db = await GetConnectionAsync();
        if (client.Id != 0)
            return await db.UpdateAsync(client);
        return await db.InsertAsync(client);
    }

    public async Task<int> DeleteClientAsync(Client client)
    {
        var db = await GetConnectionAsync();
        await db.ExecuteAsync(
            "DELETE FROM \"Transaction\" WHERE EntityType = ? AND EntityId = ?",
            "client", client.Id);
        return await db.DeleteAsync(client);
    }

    // ── Suppliers ────────────────────────────────────────────

    public async Task<List<Supplier>> GetSuppliersAsync()
    {
        var db = await GetConnectionAsync();
        var suppliers = await db.Table<Supplier>().OrderByDescending(s => s.CreatedAt).ToListAsync();

        foreach (var supplier in suppliers)
        {
            supplier.Balance = await CalculateEntityBalanceAsync(supplier.Id, "supplier");
        }

        return suppliers;
    }

    public async Task<Supplier?> GetSupplierAsync(int id)
    {
        var db = await GetConnectionAsync();
        var supplier = await db.FindAsync<Supplier>(id);
        if (supplier != null)
            supplier.Balance = await CalculateEntityBalanceAsync(id, "supplier");
        return supplier;
    }

    public async Task<int> SaveSupplierAsync(Supplier supplier)
    {
        var db = await GetConnectionAsync();
        if (supplier.Id != 0)
            return await db.UpdateAsync(supplier);
        return await db.InsertAsync(supplier);
    }

    public async Task<int> DeleteSupplierAsync(Supplier supplier)
    {
        var db = await GetConnectionAsync();
        await db.ExecuteAsync(
            "DELETE FROM \"Transaction\" WHERE EntityType = ? AND EntityId = ?",
            "supplier", supplier.Id);
        return await db.DeleteAsync(supplier);
    }

    // ── Transactions ─────────────────────────────────────────

    public async Task<List<Transaction>> GetTransactionsAsync(int entityId, string entityType)
    {
        var db = await GetConnectionAsync();
        return await db.Table<Transaction>()
            .Where(t => t.EntityId == entityId && t.EntityType == entityType)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetAllTransactionsAsync()
    {
        var db = await GetConnectionAsync();
        return await db.Table<Transaction>()
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<int> SaveTransactionAsync(Transaction transaction)
    {
        var db = await GetConnectionAsync();
        if (transaction.Id != 0)
            return await db.UpdateAsync(transaction);
        return await db.InsertAsync(transaction);
    }

    public async Task<int> DeleteTransactionAsync(Transaction transaction)
    {
        var db = await GetConnectionAsync();
        return await db.DeleteAsync(transaction);
    }

    // ── Expenses ─────────────────────────────────────────────

    public async Task<List<Expense>> GetExpensesAsync()
    {
        var db = await GetConnectionAsync();
        return await db.Table<Expense>()
            .OrderByDescending(e => e.Date)
            .ToListAsync();
    }

    public async Task<int> SaveExpenseAsync(Expense expense)
    {
        var db = await GetConnectionAsync();
        if (expense.Id != 0)
            return await db.UpdateAsync(expense);
        return await db.InsertAsync(expense);
    }

    public async Task<int> DeleteExpenseAsync(Expense expense)
    {
        var db = await GetConnectionAsync();
        return await db.DeleteAsync(expense);
    }

    // ── Reports / Balances ───────────────────────────────────

    private async Task<decimal> CalculateEntityBalanceAsync(int entityId, string entityType)
    {
        var transactions = await GetTransactionsAsync(entityId, entityType);
        decimal balance = 0;
        foreach (var t in transactions)
        {
            balance += t.Type == "له" ? t.Amount : -t.Amount;
        }
        return balance;
    }

    public async Task<decimal> GetTotalExpensesAsync(DateTime? from = null, DateTime? to = null)
    {
        var expenses = await GetExpensesAsync();
        var filtered = expenses.AsEnumerable();

        if (from.HasValue)
            filtered = filtered.Where(e => e.Date >= from.Value);
        if (to.HasValue)
            filtered = filtered.Where(e => e.Date <= to.Value);

        return filtered.Sum(e => e.Amount);
    }

    public async Task<decimal> GetTotalReceivablesAsync()
    {
        var clients = await GetClientsAsync();
        return clients.Where(c => c.Balance > 0).Sum(c => c.Balance);
    }

    public async Task<decimal> GetTotalPayablesAsync()
    {
        var suppliers = await GetSuppliersAsync();
        return suppliers.Where(s => s.Balance < 0).Sum(s => Math.Abs(s.Balance));
    }
}
