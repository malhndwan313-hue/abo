using SQLite;

namespace AccountingApp.Models;

/// <summary>
/// Represents a customer in the accounting system.
/// </summary>
public class Client
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Net balance: positive = client owes us (له), negative = we owe client (عليه).
    /// Computed from transactions, not stored permanently.
    /// </summary>
    [Ignore]
    public decimal Balance { get; set; }

    [Ignore]
    public string BalanceDisplay =>
        Balance > 0 ? $"له: {Balance:N2}" :
        Balance < 0 ? $"عليه: {Math.Abs(Balance):N2}" :
        "متزن";

    [Ignore]
    public Color BalanceColor =>
        Balance > 0 ? Color.FromArgb("#059669") :
        Balance < 0 ? Color.FromArgb("#dc2626") :
        Color.FromArgb("#5f7d9c");
}
