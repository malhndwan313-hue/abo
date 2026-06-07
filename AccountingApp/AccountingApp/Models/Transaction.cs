using SQLite;

namespace AccountingApp.Models;

/// <summary>
/// Financial transaction (debt/payment) linked to a client or supplier.
/// </summary>
public class Transaction
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(500)]
    public string Details { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    [MaxLength(50)]
    public string Currency { get; set; } = "ريال يمني";

    public DateTime Date { get; set; } = DateTime.Today;

    /// <summary>
    /// "له" (credit/receivable) or "عليه" (debit/payable).
    /// </summary>
    [MaxLength(20)]
    public string Type { get; set; } = "له";

    /// <summary>
    /// "client" or "supplier" — which entity this transaction belongs to.
    /// </summary>
    [MaxLength(20)]
    public string EntityType { get; set; } = "client";

    /// <summary>
    /// Foreign key to Client.Id or Supplier.Id.
    /// </summary>
    public int EntityId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Display helpers
    [Ignore]
    public string AmountDisplay =>
        Type == "له" ? $"+{Amount:N2}" : $"-{Amount:N2}";

    [Ignore]
    public Color AmountColor =>
        Type == "له" ? Color.FromArgb("#059669") : Color.FromArgb("#dc2626");

    [Ignore]
    public string TypeBadge =>
        Type == "له" ? "له (دائن)" : "عليه (مدين)";

    [Ignore]
    public string DateDisplay => Date.ToString("yyyy-MM-dd");
}
