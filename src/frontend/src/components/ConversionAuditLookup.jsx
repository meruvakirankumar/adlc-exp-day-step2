import { useState } from 'react';
import { getConversionById } from '../api/currencyApi.js';

export default function ConversionAuditLookup() {
  const [conversionId, setConversionId] = useState('');
  const [loading, setLoading] = useState(false);
  const [record, setRecord] = useState(null);
  const [error, setError] = useState(null);

  async function handleSubmit(event) {
    event.preventDefault();
    const trimmed = conversionId.trim();
    if (!trimmed) return;

    setLoading(true);
    setError(null);
    setRecord(null);

    try {
      const response = await getConversionById(trimmed);
      setRecord(response);
    } catch (err) {
      setError(err.message || 'Lookup failed.');
    } finally {
      setLoading(false);
    }
  }

  return (
    <section className="card" aria-labelledby="lookup-heading">
      <h2 id="lookup-heading">Look up a past conversion</h2>
      <p className="card-subtitle">Paste a conversion ID to retrieve the persisted audit record — no recalculation.</p>

      <form onSubmit={handleSubmit}>
        <div className="lookup-row">
          <div>
            <label htmlFor="lookup-id">Conversion ID</label>
            <input
              id="lookup-id"
              type="text"
              value={conversionId}
              onChange={(e) => setConversionId(e.target.value)}
              placeholder="e.g. 39ebe205cd5840c8b89ca9d83b33935c"
              required
            />
          </div>
          <button type="submit" disabled={loading}>
            {loading ? 'Loading…' : 'Look up'}
          </button>
        </div>
      </form>

      {error && <div className="error" role="alert">{error}</div>}

      {record && (
        <div className="result" aria-live="polite">
          <div className="result-hero">
            <span className="hero-label">Audit record</span>
            <span className="hero-amount">
              {record.convertedAmount} {record.targetCurrency}
            </span>
            <span className="hero-sub">
              {record.originalAmount} {record.sourceCurrency} @ {record.appliedRate}
            </span>
          </div>
          <dl>
            <dt>Conversion ID</dt>
            <dd className="mono">{record.conversionId}</dd>

            <dt>Provider date marker</dt>
            <dd>{record.providerDateMarker}</dd>

            <dt>Executed at (UTC)</dt>
            <dd className="mono">{record.executedAtUtc}</dd>
          </dl>
        </div>
      )}
    </section>
  );
}
