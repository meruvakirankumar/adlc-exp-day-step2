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

  function handleSwap() {
    setSourceCurrency(targetCurrency);
    setTargetCurrency(sourceCurrency);
  }

  return (
    <section className="card" aria-labelledby="convert-heading">
      <h2 id="convert-heading">Convert currency</h2>
      <p className="card-subtitle">Enter an amount, pick the currencies, and submit for an audit-logged conversion.</p>

      <form onSubmit={handleSubmit}>
        <div className="convert-grid">
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
            <label htmlFor="source">From</label>
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
          <button
            type="button"
            className="swap-button"
            onClick={handleSwap}
            aria-label="Swap source and target currencies"
            title="Swap currencies"
            disabled={submitting}
          >
            <svg viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
              <path d="M7 4L3 8L7 12" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
              <path d="M3 8H17" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
              <path d="M17 20L21 16L17 12" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
              <path d="M21 16H7" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
            </svg>
          </button>
          <div>
            <label htmlFor="target">To</label>
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

        <div className="actions">
          <button type="submit" disabled={submitting}>
            {submitting ? 'Converting…' : 'Convert'}
          </button>
        </div>
      </form>

      {error && <div className="error" role="alert">{error}</div>}

      {result && (
        <div className="result" aria-live="polite">
          <div className="result-hero">
            <span className="hero-label">Converted amount</span>
            <span className="hero-amount">
              {result.convertedAmount} {result.targetCurrency}
            </span>
            <span className="hero-sub">
              {result.originalAmount} {result.sourceCurrency} @ {result.appliedRate}
            </span>
          </div>
          <dl>
            <dt>Conversion ID</dt>
            <dd className="mono">{result.conversionId}</dd>

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
