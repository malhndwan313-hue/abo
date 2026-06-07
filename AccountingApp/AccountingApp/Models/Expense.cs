using SQLite;

namespace AccountingApp.Models;

/// <summary>
/// General business expense entry.
/// </summary>
public class Expense
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    [MaxLength(50)]
    public string Currency { get; set; } = "ريال يمني";

    [MaxLength(100)]
    public string Category { get; set; } = "عام";

    public DateTime Date { get; set; } = DateTime.Today;

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Ignore]
    public string AmountDisplay => $"{Amount:N2} {Currency}";

    [Ignore]
    public string DateDisplay => Date.ToString("yyyy-MM-dd");
}
