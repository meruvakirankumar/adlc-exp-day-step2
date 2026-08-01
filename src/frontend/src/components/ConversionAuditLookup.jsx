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

      <form onSubmit={handleSubmit}>
        <label htmlFor="lookup-id">Conversion ID</label>
        <input
          id="lookup-id"
          type="text"
          value={conversionId}
          onChange={(e) => setConversionId(e.target.value)}
          placeholder="Paste a conversionId returned from a previous conversion"
          required
        />

        <div style={{ marginTop: '1rem' }}>
          <button type="submit" disabled={loading}>
            {loading ? 'Loading…' : 'Look up'}
          </button>
        </div>
      </form>

      {error && <div className="error" role="alert">{error}</div>}

      {record && (
        <div className="result" aria-live="polite">
          <dl>
            <dt>Conversion ID</dt>
            <dd className="mono">{record.conversionId}</dd>

            <dt>Original amount</dt>
            <dd>{record.originalAmount} {record.sourceCurrency}</dd>

            <dt>Applied rate</dt>
            <dd>{record.appliedRate}</dd>

            <dt>Converted amount</dt>
            <dd>{record.convertedAmount} {record.targetCurrency}</dd>

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
