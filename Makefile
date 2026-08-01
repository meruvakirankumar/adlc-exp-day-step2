.PHONY: install run dev run

run: export NODE_ENV=development
run: export OPENCODE_SPAN_ATTRIBUTES=participant=$(ENROLLMENT_ID)
run: export OPENCODE_ENABLE_TELEMETRY=1
run: export OPENCODE_OTLP_ENDPOINT=https://monitoring.pronative.ai
run: export OPENCODE_OTLP_PROTOCOL=http/json
run:
	opencode

