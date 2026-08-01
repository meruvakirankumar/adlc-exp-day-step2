#!/bin/sh
# Generic runtime placeholder replacement.
# Replaces the __VITE_API_URL__ token inside the built index.html with the
# value of the runtime VITE_API_URL environment variable, then hands control
# back to the base nginx image's docker-entrypoint (which then execs nginx).
# Placed under /docker-entrypoint.d/ so the official nginx image picks it up.
set -eu

TARGET_FILE="/usr/share/nginx/html/index.html"
PLACEHOLDER="__VITE_API_URL__"
RUNTIME_VALUE="${VITE_API_URL:-}"

if [ ! -f "$TARGET_FILE" ]; then
  echo "[inject-runtime-env] Skipped: $TARGET_FILE not found" >&2
  exit 0
fi

# Escape characters that are special to sed's replacement string so arbitrary
# URL values (including '/', '&', '|') do not break the substitution.
ESCAPED_VALUE=$(printf '%s' "$RUNTIME_VALUE" | sed -e 's/[\/&|]/\\&/g')

sed -i "s|${PLACEHOLDER}|${ESCAPED_VALUE}|g" "$TARGET_FILE"

echo "[inject-runtime-env] Replaced ${PLACEHOLDER} in ${TARGET_FILE}"
