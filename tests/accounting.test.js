const {
  getTodayDateString,
  getCurrentMonthPrefix,
  filterTransactionsByRange,
  escapeHtml,
  calculateBalances,
  createClient,
  createTransaction,
  formatBalanceText,
  generateWhatsAppMessage,
  sanitizePhone,
} = require('../src/accounting');

// ---------------------------------------------------------------------------
// getTodayDateString
// ---------------------------------------------------------------------------
describe('getTodayDateString', () => {
  it('should return a string in YYYY-MM-DD format', () => {
    const result = getTodayDateString();
    expect(result).toMatch(/^\d{4}-\d{2}-\d{2}$/);
  });

  it('should match the current date', () => {
    const now = new Date();
    const expected = now.toISOString().split('T')[0];
    expect(getTodayDateString()).toBe(expected);
  });
});

// ---------------------------------------------------------------------------
// getCurrentMonthPrefix
// ---------------------------------------------------------------------------
describe('getCurrentMonthPrefix', () => {
  it('should return a string in YYYY-MM format', () => {
    const result = getCurrentMonthPrefix();
    expect(result).toMatch(/^\d{4}-\d{2}$/);
  });

  it('should be the first 7 characters of today\'s date', () => {
    expect(getCurrentMonthPrefix()).toBe(getTodayDateString().slice(0, 7));
  });
});

// ---------------------------------------------------------------------------
// escapeHtml
// ---------------------------------------------------------------------------
describe('escapeHtml', () => {
  it('should escape & to &amp;', () => {
    expect(escapeHtml('a&b')).toBe('a&amp;b');
  });

  it('should escape < to &lt;', () => {
    expect(escapeHtml('a<b')).toBe('a&lt;b');
  });

  it('should escape > to &gt;', () => {
    expect(escapeHtml('a>b')).toBe('a&gt;b');
  });

  it('should escape multiple special characters', () => {
    expect(escapeHtml('<script>alert("xss")&</script>')).toBe(
      '&lt;script&gt;alert("xss")&amp;&lt;/script&gt;'
    );
  });

  it('should return empty string for null', () => {
    expect(escapeHtml(null)).toBe('');
  });

  it('should return empty string for undefined', () => {
    expect(escapeHtml(undefined)).toBe('');
  });

  it('should return empty string for empty string', () => {
    expect(escapeHtml('')).toBe('');
  });

  it('should return unchanged string when no special chars', () => {
    expect(escapeHtml('hello world')).toBe('hello world');
  });

  it('should handle Arabic text without changes', () => {
    const arabic = 'فاتورة مبيعات';
    expect(escapeHtml(arabic)).toBe(arabic);
  });
});

// ---------------------------------------------------------------------------
// filterTransactionsByRange
// ---------------------------------------------------------------------------
describe('filterTransactionsByRange', () => {
  const today = new Date().toISOString().split('T')[0];
  const monthPrefix = today.slice(0, 7);

  const transactions = [
    { id: '1', date: today, details: 'Today tx', amount: 100, currency: 'ريال يمني', type: 'له' },
    { id: '2', date: `${monthPrefix}-01`, details: 'This month tx', amount: 200, currency: 'ريال يمني', type: 'عليه' },
    { id: '3', date: '2020-01-15', details: 'Old tx', amount: 300, currency: 'دولار أمريكي', type: 'له' },
    { id: '4', date: today, details: 'Another today tx', amount: 50, currency: 'ريال سعودي', type: 'عليه' },
  ];

  it('should return all transactions for "all" range, sorted by date', () => {
    const result = filterTransactionsByRange(transactions, 'all');
    expect(result).toHaveLength(4);
    // oldest first
    expect(result[0].id).toBe('3');
  });

  it('should filter only today\'s transactions for "day" range', () => {
    const result = filterTransactionsByRange(transactions, 'day');
    result.forEach(t => expect(t.date).toBe(today));
    expect(result.length).toBeGreaterThanOrEqual(2);
  });

  it('should filter by current month for "month" range', () => {
    const result = filterTransactionsByRange(transactions, 'month');
    result.forEach(t => expect(t.date.startsWith(monthPrefix)).toBe(true));
  });

  it('should return empty array when no transactions match the range', () => {
    const oldOnly = [{ id: '1', date: '2019-06-01', details: 'old', amount: 10, currency: 'ريال يمني', type: 'له' }];
    const result = filterTransactionsByRange(oldOnly, 'day');
    expect(result).toHaveLength(0);
  });

  it('should not mutate the original array', () => {
    const copy = [...transactions];
    filterTransactionsByRange(transactions, 'all');
    expect(transactions).toEqual(copy);
  });

  it('should sort results by date ascending', () => {
    const unsorted = [
      { id: '1', date: '2024-12-31', details: 'late', amount: 1, currency: 'ريال يمني', type: 'له' },
      { id: '2', date: '2024-01-01', details: 'early', amount: 1, currency: 'ريال يمني', type: 'له' },
    ];
    const result = filterTransactionsByRange(unsorted, 'all');
    expect(new Date(result[0].date) <= new Date(result[1].date)).toBe(true);
  });
});

// ---------------------------------------------------------------------------
// calculateBalances
// ---------------------------------------------------------------------------
describe('calculateBalances', () => {
  it('should calculate credit and debit totals per currency', () => {
    const transactions = [
      { amount: 100, currency: 'ريال يمني', type: 'له' },
      { amount: 50, currency: 'ريال يمني', type: 'عليه' },
      { amount: 200, currency: 'ريال يمني', type: 'له' },
    ];
    const result = calculateBalances(transactions);
    expect(result['ريال يمني'].credit).toBe(300);
    expect(result['ريال يمني'].debit).toBe(50);
    expect(result['ريال يمني'].net).toBe(250);
  });

  it('should handle multiple currencies independently', () => {
    const transactions = [
      { amount: 100, currency: 'ريال يمني', type: 'له' },
      { amount: 50, currency: 'دولار أمريكي', type: 'عليه' },
      { amount: 30, currency: 'دولار أمريكي', type: 'له' },
    ];
    const result = calculateBalances(transactions);
    expect(result['ريال يمني'].net).toBe(100);
    expect(result['دولار أمريكي'].net).toBe(-20);
  });

  it('should return empty object for empty transactions', () => {
    expect(calculateBalances([])).toEqual({});
  });

  it('should return zero net when credits equal debits', () => {
    const transactions = [
      { amount: 100, currency: 'ريال يمني', type: 'له' },
      { amount: 100, currency: 'ريال يمني', type: 'عليه' },
    ];
    const result = calculateBalances(transactions);
    expect(result['ريال يمني'].net).toBe(0);
  });

  it('should return negative net when debits exceed credits', () => {
    const transactions = [
      { amount: 50, currency: 'ريال يمني', type: 'له' },
      { amount: 200, currency: 'ريال يمني', type: 'عليه' },
    ];
    const result = calculateBalances(transactions);
    expect(result['ريال يمني'].net).toBe(-150);
  });
});

// ---------------------------------------------------------------------------
// createClient
// ---------------------------------------------------------------------------
describe('createClient', () => {
  it('should create a client with name, phone, and empty transactions', () => {
    const client = createClient('أحمد', '967777123456');
    expect(client).not.toBeNull();
    expect(client.name).toBe('أحمد');
    expect(client.phone).toBe('967777123456');
    expect(client.transactions).toEqual([]);
    expect(client.id).toBeDefined();
  });

  it('should trim whitespace from name and phone', () => {
    const client = createClient('  علي  ', '  967123  ');
    expect(client.name).toBe('علي');
    expect(client.phone).toBe('967123');
  });

  it('should return null for empty name', () => {
    expect(createClient('', '967123')).toBeNull();
  });

  it('should return null for whitespace-only name', () => {
    expect(createClient('   ', '967123')).toBeNull();
  });

  it('should return null for null name', () => {
    expect(createClient(null, '967123')).toBeNull();
  });

  it('should handle missing phone gracefully', () => {
    const client = createClient('محمد', null);
    expect(client.phone).toBe('');
  });

  it('should generate a string id', () => {
    const client = createClient('test', '123');
    expect(typeof client.id).toBe('string');
  });
});

// ---------------------------------------------------------------------------
// createTransaction
// ---------------------------------------------------------------------------
describe('createTransaction', () => {
  it('should create a credit transaction', () => {
    const tx = createTransaction('فاتورة', 500, 'ريال يمني', '2024-01-15', 'له');
    expect(tx).not.toBeNull();
    expect(tx.details).toBe('فاتورة');
    expect(tx.amount).toBe(500);
    expect(tx.currency).toBe('ريال يمني');
    expect(tx.date).toBe('2024-01-15');
    expect(tx.type).toBe('له');
  });

  it('should create a debit transaction', () => {
    const tx = createTransaction('سداد', 300, 'دولار أمريكي', '2024-06-01', 'عليه');
    expect(tx.type).toBe('عليه');
    expect(tx.amount).toBe(300);
  });

  it('should return null for empty details', () => {
    expect(createTransaction('', 100, 'ريال يمني', '2024-01-01', 'له')).toBeNull();
  });

  it('should return null for NaN amount', () => {
    expect(createTransaction('test', 'abc', 'ريال يمني', '2024-01-01', 'له')).toBeNull();
  });

  it('should return null for invalid type', () => {
    expect(createTransaction('test', 100, 'ريال يمني', '2024-01-01', 'invalid')).toBeNull();
  });

  it('should trim whitespace from details', () => {
    const tx = createTransaction('  فاتورة  ', 100, 'ريال يمني', '2024-01-01', 'له');
    expect(tx.details).toBe('فاتورة');
  });

  it('should default currency to ريال يمني when not provided', () => {
    const tx = createTransaction('test', 100, null, '2024-01-01', 'له');
    expect(tx.currency).toBe('ريال يمني');
  });

  it('should parse string amounts', () => {
    const tx = createTransaction('test', '123.45', 'ريال يمني', '2024-01-01', 'له');
    expect(tx.amount).toBe(123.45);
  });

  it('should handle zero amount', () => {
    const tx = createTransaction('test', 0, 'ريال يمني', '2024-01-01', 'له');
    expect(tx).not.toBeNull();
    expect(tx.amount).toBe(0);
  });
});

// ---------------------------------------------------------------------------
// formatBalanceText
// ---------------------------------------------------------------------------
describe('formatBalanceText', () => {
  it('should format positive balance as credit (له)', () => {
    expect(formatBalanceText(250, 'ريال يمني')).toBe('له بمبلغ: 250.00');
  });

  it('should format negative balance as debit (عليه)', () => {
    expect(formatBalanceText(-150, 'ريال يمني')).toBe('عليه بمبلغ: 150.00');
  });

  it('should format zero balance as balanced', () => {
    expect(formatBalanceText(0, 'ريال يمني')).toBe('متزن وصفر');
  });

  it('should format small decimal amounts correctly', () => {
    expect(formatBalanceText(0.5, 'ريال يمني')).toBe('له بمبلغ: 0.50');
  });

  it('should format large amounts correctly', () => {
    expect(formatBalanceText(1000000, 'ريال يمني')).toBe('له بمبلغ: 1000000.00');
  });
});

// ---------------------------------------------------------------------------
// generateWhatsAppMessage
// ---------------------------------------------------------------------------
describe('generateWhatsAppMessage', () => {
  const client = { name: 'أحمد', phone: '967777123456' };
  const transactions = [
    { details: 'فاتورة', amount: 100, currency: 'ريال يمني', date: '2024-01-15', type: 'له' },
    { details: 'سداد', amount: 50, currency: 'ريال يمني', date: '2024-01-16', type: 'عليه' },
  ];

  it('should return null for null client', () => {
    expect(generateWhatsAppMessage(null, transactions, 'all')).toBeNull();
  });

  it('should return null for empty transactions', () => {
    expect(generateWhatsAppMessage(client, [], 'all')).toBeNull();
  });

  it('should contain the client name', () => {
    const msg = generateWhatsAppMessage(client, transactions, 'all');
    expect(msg).toContain('أحمد');
  });

  it('should contain transaction details', () => {
    const msg = generateWhatsAppMessage(client, transactions, 'all');
    expect(msg).toContain('فاتورة');
    expect(msg).toContain('سداد');
  });

  it('should show + for credit and - for debit', () => {
    const msg = generateWhatsAppMessage(client, transactions, 'all');
    expect(msg).toContain('+ 100.00');
    expect(msg).toContain('- 50.00');
  });

  it('should include range label for day', () => {
    const msg = generateWhatsAppMessage(client, transactions, 'day');
    expect(msg).toContain('اليوم');
  });

  it('should include range label for month', () => {
    const msg = generateWhatsAppMessage(client, transactions, 'month');
    expect(msg).toContain('الشهر الحالي');
  });

  it('should include balance summary', () => {
    const msg = generateWhatsAppMessage(client, transactions, 'all');
    expect(msg).toContain('صافي الرصيد');
    expect(msg).toContain('له 50.00');
  });

  it('should handle multi-currency transactions', () => {
    const multiCurr = [
      { details: 'a', amount: 100, currency: 'ريال يمني', date: '2024-01-01', type: 'له' },
      { details: 'b', amount: 200, currency: 'دولار أمريكي', date: '2024-01-02', type: 'عليه' },
    ];
    const msg = generateWhatsAppMessage(client, multiCurr, 'all');
    expect(msg).toContain('ريال يمني');
    expect(msg).toContain('دولار أمريكي');
  });
});

// ---------------------------------------------------------------------------
// sanitizePhone
// ---------------------------------------------------------------------------
describe('sanitizePhone', () => {
  it('should remove non-numeric characters', () => {
    expect(sanitizePhone('+967-777-123-456')).toBe('967777123456');
  });

  it('should return empty string for null', () => {
    expect(sanitizePhone(null)).toBe('');
  });

  it('should return empty string for undefined', () => {
    expect(sanitizePhone(undefined)).toBe('');
  });

  it('should keep pure numeric strings unchanged', () => {
    expect(sanitizePhone('967777123456')).toBe('967777123456');
  });

  it('should handle strings with spaces', () => {
    expect(sanitizePhone('967 777 123')).toBe('967777123');
  });

  it('should handle empty string', () => {
    expect(sanitizePhone('')).toBe('');
  });
});
