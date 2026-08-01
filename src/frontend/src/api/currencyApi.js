// Runtime API client for the currency conversion feature.
// The API base URL is resolved from `window.__RUNTIME_CONFIG__.VITE_API_URL`,
// which is populated at container start-up by the entrypoint script that
// replaces the `__VITE_API_URL__` placeholder inside the built index.html.
// An empty string means "same origin" (Nginx proxies /api/* to the backend).

const PLACEHOLDER = '__VITE_API_URL__';

export function getApiBaseUrl() {
  if (typeof window === 'undefined') return '';
  const cfg = window.__RUNTIME_CONFIG__ || {};
  const raw = cfg.VITE_API_URL;

  if (raw === undefined || raw === null) return '';
  // If the entrypoint didn't run (e.g., during local dev), avoid using the token literally.
  if (raw === PLACEHOLDER) return '';
  return String(raw).replace(/\/+$/, '');
}

function buildUrl(path) {
  const base = getApiBaseUrl();
  return `${base}${path}`;
}

async function readProblemSafe(response) {
  try {
    return await response.json();
  } catch {
    return null;
  }
}

export async function postConversion({ amount, sourceCurrency, targetCurrency }) {
  const response = await fetch(buildUrl('/api/conversions'), {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
    body: JSON.stringify({ amount, sourceCurrency, targetCurrency }),
  });

  if (!response.ok) {
    const problem = await readProblemSafe(response);
    const err = new Error(problem?.title || `Conversion request failed (HTTP ${response.status})`);
    err.status = response.status;
    err.problem = problem;
    throw err;
  }

  return response.json();
}

export async function getConversionById(conversionId) {
  const response = await fetch(buildUrl(`/api/conversions/${encodeURIComponent(conversionId)}`), {
    headers: { Accept: 'application/json' },
  });

  if (!response.ok) {
    const problem = await readProblemSafe(response);
    const err = new Error(problem?.title || `Lookup failed (HTTP ${response.status})`);
    err.status = response.status;
    err.problem = problem;
    throw err;
  }

  return response.json();
}
