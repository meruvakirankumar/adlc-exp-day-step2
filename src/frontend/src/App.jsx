import CurrencyConversionForm from './components/CurrencyConversionForm.jsx';
import ConversionAuditLookup from './components/ConversionAuditLookup.jsx';

export default function App() {
  return (
    <main className="app">
      <header className="app-header">
        <span className="eyebrow">
          <span className="eyebrow-dot" aria-hidden="true" />
          Treasury Operations
        </span>
        <h1>Real-Time Currency Conversion &amp; Audit Trail</h1>
        <p>
          Convert amounts using the current provider rate and reconstruct any
          past conversion on demand — every quote is signed, timestamped, and
          replay-ready for audit.
        </p>
      </header>

      <CurrencyConversionForm />
      <ConversionAuditLookup />

      <p className="footnote">
        Rates provided by an external rate feed. Audit records include the
        applied rate, provider date marker, and backend UTC timestamp.
      </p>
    </main>
  );
}
