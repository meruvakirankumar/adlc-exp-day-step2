import CurrencyConversionForm from './components/CurrencyConversionForm.jsx';
import ConversionAuditLookup from './components/ConversionAuditLookup.jsx';

export default function App() {
  return (
    <main className="app">
      <header className="card">
        <h1>Real-Time Currency Conversion &amp; Audit Trail</h1>
        <p>
          Convert amounts using the current provider rate and reconstruct any
          past conversion from its audit record.
        </p>
      </header>

      <CurrencyConversionForm />
      <ConversionAuditLookup />
    </main>
  );
}
