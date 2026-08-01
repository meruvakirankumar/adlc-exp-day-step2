import { useState } from 'react';
import { postConversion } from '../api/currencyApi.js';

// ISO 4217 codes supported by the Frankfurter provider; label shows country/name for the operator.
const CURRENCIES = [
  { code: 'USD', label: 'US Dollar' },
  { code: 'EUR', label: 'Euro' },
  { code: 'GBP', label: 'British Pound' },
  { code: 'JPY', label: 'Japanese Yen' },
  { code: 'AUD', label: 'Australian Dollar' },
  { code: 'CAD', label: 'Canadian Dollar' },
  { code: 'CHF', label: 'Swiss Franc' },
  { code: 'CNY', label: 'Chinese Yuan' },
  { code: 'HKD', label: 'Hong Kong Dollar' },
  { code: 'NZD', label: 'New Zealand Dollar' },
  { code: 'SEK', label: 'Swedish Krona' },
  { code: 'KRW', label: 'South Korean Won' },
  { code: 'SGD', label: 'Singapore Dollar' },
  { code: 'NOK', label: 'Norwegian Krone' },
  { code: 'MXN', label: 'Mexican Peso' },
  { code: 'INR', label: 'Indian Rupee' },
  { code: 'BRL', label: 'Brazilian Real' },
  { code: 'ZAR', label: 'South African Rand' },
  { code: 'TRY', label: 'Turkish Lira' },
  { code: 'PLN', label: 'Polish Zloty' },
  { code: 'THB', label: 'Thai Baht' },
  { code: 'DKK', label: 'Danish Krone' },
  { code: 'CZK', label: 'Czech Koruna' },
  { code: 'HUF', label: 'Hungarian Forint' },
];

export default function CurrencyConversionForm() {
  const [amount, setAmount] = useState('100.00');
  const [sourceCurrency, setSourceCurrency] = useState('USD');
  const [targetCurrency, setTargetCurrency] = useState('EUR');

  const [submitting, setSubmitting] = useState(false);
  const [result, setResult] = useState(null);
  const [error, setError] = useState(null);

  async function handleSubmit(event) {
    event.preventDefault();

    if (sourceCurrency === targetCurrency) {
      setError('Source and target currencies must be different.');
      setResult(null);
      return;
    }

    setSubmitting(true);
    setError(null);
    setResult(null);

    try {
      const payload = {
        amount: Number(amount),
        sourceCurrency,
        targetCurrency,
      };
      const response = await postConversion(payload);
      setResult(response);
    } catch (err) {
      setError(err.message || 'Conversion failed.');
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <section className="card" aria-labelledby="convert-heading">
      <h2 id="convert-heading">Convert currency</h2>

      <form onSubmit={handleSubmit}>
        <div className="form-grid">
          <div>
            <label htmlFor="amount">Amount</label>
            <input
              id="amount"
              type="number"
              min="0"
              step="0.01"
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
              required
            />
          </div>
          <div>
            <label htmlFor="source">Source currency</label>
            <select
              id="source"
              value={sourceCurrency}
              onChange={(e) => setSourceCurrency(e.target.value)}
              required
            >
              {CURRENCIES.map((c) => (
                <option key={c.code} value={c.code}>
                  {c.code} — {c.label}
                </option>
              ))}
            </select>
          </div>
          <div>
            <label htmlFor="target">Target currency</label>
            <select
              id="target"
              value={targetCurrency}
              onChange={(e) => setTargetCurrency(e.target.value)}
              required
            >
              {CURRENCIES.map((c) => (
                <option key={c.code} value={c.code}>
                  {c.code} — {c.label}
                </option>
              ))}
            </select>
          </div>
        </div>

        <div style={{ marginTop: '1rem' }}>
          <button type="submit" disabled={submitting}>
            {submitting ? 'Converting…' : 'Convert'}
          </button>
        </div>
      </form>

      {error && <div className="error" role="alert">{error}</div>}

      {result && (
        <div className="result" aria-live="polite">
          <dl>
            <dt>Conversion ID</dt>
            <dd className="mono">{result.conversionId}</dd>

            <dt>Original amount</dt>
            <dd>{result.originalAmount} {result.sourceCurrency}</dd>

            <dt>Applied rate</dt>
            <dd>{result.appliedRate}</dd>

            <dt>Converted amount</dt>
            <dd>{result.convertedAmount} {result.targetCurrency}</dd>

            <dt>Provider date marker</dt>
            <dd>{result.providerDateMarker}</dd>

            <dt>Executed at (UTC)</dt>
            <dd className="mono">{result.executedAtUtc}</dd>
          </dl>
        </div>
      )}
    </section>
  );
}
