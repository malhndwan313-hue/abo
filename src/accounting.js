/**
 * Core business logic for the accounting application.
 * Extracted from index.html for testability.
 */

function getTodayDateString() {
  return new Date().toISOString().split('T')[0];
}

function getCurrentMonthPrefix() {
  return getTodayDateString().slice(0, 7);
}

function filterTransactionsByRange(transactions, range) {
  const today = getTodayDateString();
  const monthPrefix = getCurrentMonthPrefix();

  let filtered = [...transactions];
  if (range === 'day') {
    filtered = filtered.filter(t => t.date === today);
  } else if (range === 'month') {
    filtered = filtered.filter(t => t.date.startsWith(monthPrefix));
  }
  return filtered.sort((a, b) => new Date(a.date) - new Date(b.date));
}

function escapeHtml(str) {
  if (!str) return '';
  return str.replace(/[&<>]/g, function (m) {
    if (m === '&') return '&amp;';
    if (m === '<') return '&lt;';
    if (m === '>') return '&gt;';
    return m;
  });
}

function calculateBalances(transactions) {
  const sums = {};
  transactions.forEach(t => {
    if (!sums[t.currency]) sums[t.currency] = { credit: 0, debit: 0 };
    if (t.type === '\u0644\u0647') {
      sums[t.currency].credit += t.amount;
    } else {
      sums[t.currency].debit += t.amount;
    }
  });
  const result = {};
  for (const curr in sums) {
    result[curr] = {
      credit: sums[curr].credit,
      debit: sums[curr].debit,
      net: sums[curr].credit - sums[curr].debit,
    };
  }
  return result;
}

function createClient(name, phone) {
  if (!name || !name.trim()) return null;
  return {
    id: Date.now().toString(),
    name: name.trim(),
    phone: phone ? phone.trim() : '',
    transactions: [],
  };
}

function createTransaction(details, amount, currency, date, type) {
  if (!details || !details.trim()) return null;
  const parsedAmount = parseFloat(amount);
  if (isNaN(parsedAmount)) return null;
  if (type !== '\u0644\u0647' && type !== '\u0639\u0644\u064a\u0647') return null;
  return {
    id: Date.now().toString(),
    details: details.trim(),
    amount: parsedAmount,
    currency: currency || '\u0631\u064a\u0627\u0644 \u064a\u0645\u0646\u064a',
    date: date || getTodayDateString(),
    type,
  };
}

function formatBalanceText(net, currency) {
  if (net > 0) return `\u0644\u0647 \u0628\u0645\u0628\u0644\u063a: ${net.toFixed(2)}`;
  if (net < 0) return `\u0639\u0644\u064a\u0647 \u0628\u0645\u0628\u0644\u063a: ${Math.abs(net).toFixed(2)}`;
  return '\u0645\u062a\u0632\u0646 \u0648\u0635\u0641\u0631';
}

function generateWhatsAppMessage(client, transactions, range) {
  if (!client || !transactions || transactions.length === 0) return null;

  const rangeLabel =
    range === 'day' ? '\u0627\u0644\u064a\u0648\u0645' : range === 'month' ? '\u0627\u0644\u0634\u0647\u0631 \u0627\u0644\u062d\u0627\u0644\u064a' : '\u0643\u0627\u0645\u0644';
  let msg = `*\u{1f4cb} \u0643\u0634\u0641 \u062d\u0633\u0627\u0628 \u0645\u0639\u062a\u0645\u062f - ${client.name}*\n`;
  msg += `\u0627\u0644\u0646\u0637\u0627\u0642: ${rangeLabel}\n`;
  msg += `------------------------------------\n`;

  transactions.forEach(t => {
    const sign = t.type === '\u0644\u0647' ? '+' : '-';
    msg += `\u{1f4cc} *\u0627\u0644\u0628\u064a\u0627\u0646:* ${t.details}\n\u{1f4b0} *\u0627\u0644\u0645\u0628\u0644\u063a:* ${sign} ${t.amount.toFixed(2)} ${t.currency}\n\u{1f4c5} *\u0627\u0644\u062a\u0627\u0631\u064a\u062e:* ${t.date} | ${t.type}\n------------------------------------\n`;
  });

  const summary = {};
  transactions.forEach(t => {
    if (!summary[t.currency]) summary[t.currency] = { credit: 0, debit: 0 };
    if (t.type === '\u0644\u0647') {
      summary[t.currency].credit += t.amount;
    } else {
      summary[t.currency].debit += t.amount;
    }
  });

  msg += `\n\u{1f9ee} *\u0635\u0627\u0641\u064a \u0627\u0644\u0631\u0635\u064a\u062f:*\n`;
  for (const c in summary) {
    const bal = summary[c].credit - summary[c].debit;
    msg += `\u2022 ${c}: ${bal > 0 ? `\u0644\u0647 ${bal.toFixed(2)}` : bal < 0 ? `\u0639\u0644\u064a\u0647 ${Math.abs(bal).toFixed(2)}` : '\u0645\u062a\u0632\u0646 \u0648\u0635\u0641\u0631'} ${c}\n`;
  }
  return msg;
}

function sanitizePhone(phone) {
  if (!phone) return '';
  return phone.replace(/[^0-9]/g, '');
}

module.exports = {
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
};
