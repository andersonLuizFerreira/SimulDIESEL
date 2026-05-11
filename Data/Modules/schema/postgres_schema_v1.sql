CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE TABLE IF NOT EXISTS module_profiles (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name TEXT NOT NULL,
    manufacturer TEXT,
    model TEXT,
    category TEXT,
    application TEXT,
    description TEXT,
    status TEXT NOT NULL DEFAULT 'draft' CHECK (status IN ('draft', 'active', 'archived')),
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS module_profile_versions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    module_profile_id UUID NOT NULL REFERENCES module_profiles(id) ON DELETE CASCADE,
    version TEXT NOT NULL,
    status TEXT NOT NULL DEFAULT 'draft' CHECK (status IN ('draft', 'validated', 'released', 'archived')),
    source TEXT,
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    UNIQUE (module_profile_id, version)
);

CREATE TABLE IF NOT EXISTS module_connectors (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    module_profile_version_id UUID NOT NULL REFERENCES module_profile_versions(id) ON DELETE CASCADE,
    name TEXT NOT NULL,
    description TEXT,
    pin_count INTEGER NOT NULL CHECK (pin_count > 0)
);

CREATE TABLE IF NOT EXISTS module_pins (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    connector_id UUID NOT NULL REFERENCES module_connectors(id) ON DELETE CASCADE,
    pin_number TEXT NOT NULL,
    pin_name TEXT,
    function_name TEXT,
    electrical_type TEXT,
    direction TEXT CHECK (direction IS NULL OR direction IN ('input', 'output', 'bidirectional', 'power', 'ground', 'none')),
    notes TEXT,
    UNIQUE (connector_id, pin_number)
);

CREATE TABLE IF NOT EXISTS module_power_requirements (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    module_profile_version_id UUID NOT NULL REFERENCES module_profile_versions(id) ON DELETE CASCADE,
    name TEXT NOT NULL,
    voltage_nominal NUMERIC,
    voltage_min NUMERIC,
    voltage_max NUMERIC,
    current_expected NUMERIC,
    current_max NUMERIC,
    positive_pin_id UUID REFERENCES module_pins(id) ON DELETE SET NULL,
    negative_pin_id UUID REFERENCES module_pins(id) ON DELETE SET NULL,
    enable_sequence_order INTEGER,
    notes TEXT
);

CREATE TABLE IF NOT EXISTS module_can_networks (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    module_profile_version_id UUID NOT NULL REFERENCES module_profile_versions(id) ON DELETE CASCADE,
    name TEXT NOT NULL,
    protocol TEXT NOT NULL CHECK (protocol IN ('CAN', 'J1939')),
    bitrate_kbps INTEGER NOT NULL,
    controller TEXT CHECK (controller IS NULL OR controller IN ('can0', 'can1')),
    expected_source_address INTEGER CHECK (expected_source_address IS NULL OR (expected_source_address >= 0 AND expected_source_address <= 255)),
    can_high_pin_id UUID REFERENCES module_pins(id) ON DELETE SET NULL,
    can_low_pin_id UUID REFERENCES module_pins(id) ON DELETE SET NULL,
    notes TEXT
);

CREATE TABLE IF NOT EXISTS module_j1939_pgns (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    module_profile_version_id UUID NOT NULL REFERENCES module_profile_versions(id) ON DELETE CASCADE,
    source_address INTEGER CHECK (source_address IS NULL OR (source_address >= 0 AND source_address <= 255)),
    destination_address INTEGER CHECK (destination_address IS NULL OR (destination_address >= 0 AND destination_address <= 255)),
    pgn INTEGER NOT NULL CHECK (pgn >= 0 AND pgn <= 262143),
    name TEXT,
    direction TEXT CHECK (direction IS NULL OR direction IN ('rx', 'tx', 'observed', 'bidirectional')),
    period_ms INTEGER CHECK (period_ms IS NULL OR period_ms >= 0),
    proprietary BOOLEAN NOT NULL DEFAULT false,
    data_sample_hex TEXT,
    notes TEXT
);

CREATE TABLE IF NOT EXISTS module_signal_channels (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    module_profile_version_id UUID NOT NULL REFERENCES module_profile_versions(id) ON DELETE CASCADE,
    signal_name TEXT NOT NULL,
    signal_type TEXT,
    direction TEXT CHECK (direction IS NULL OR direction IN ('input', 'output', 'bidirectional')),
    physical_unit TEXT,
    min_value NUMERIC,
    max_value NUMERIC,
    default_value NUMERIC,
    board TEXT CHECK (board IS NULL OR board IN ('BPM', 'UCE', 'GSA')),
    board_channel TEXT,
    related_pin_id UUID REFERENCES module_pins(id) ON DELETE SET NULL,
    notes TEXT
);

CREATE TABLE IF NOT EXISTS module_sdh_commands (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    module_profile_version_id UUID NOT NULL REFERENCES module_profile_versions(id) ON DELETE CASCADE,
    command_group TEXT,
    execution_order INTEGER NOT NULL DEFAULT 0,
    target TEXT NOT NULL,
    op TEXT NOT NULL,
    args_json JSONB NOT NULL DEFAULT '{}'::jsonb,
    meta_json JSONB NOT NULL DEFAULT '{}'::jsonb,
    enabled BOOLEAN NOT NULL DEFAULT true,
    notes TEXT
);

CREATE TABLE IF NOT EXISTS module_test_sequences (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    module_profile_version_id UUID NOT NULL REFERENCES module_profile_versions(id) ON DELETE CASCADE,
    name TEXT NOT NULL,
    description TEXT,
    enabled BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS module_test_steps (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    test_sequence_id UUID NOT NULL REFERENCES module_test_sequences(id) ON DELETE CASCADE,
    step_order INTEGER NOT NULL,
    step_type TEXT NOT NULL CHECK (step_type IN ('sdh_command', 'wait', 'expect_event', 'note')),
    sdh_command_json JSONB,
    expected_response_json JSONB,
    expected_event_json JSONB,
    timeout_ms INTEGER CHECK (timeout_ms IS NULL OR timeout_ms >= 0),
    retry_count INTEGER NOT NULL DEFAULT 0 CHECK (retry_count >= 0),
    notes TEXT,
    UNIQUE (test_sequence_id, step_order)
);

CREATE TABLE IF NOT EXISTS module_capture_sessions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    module_profile_version_id UUID NOT NULL REFERENCES module_profile_versions(id) ON DELETE CASCADE,
    name TEXT NOT NULL,
    started_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    ended_at TIMESTAMPTZ,
    source TEXT,
    notes TEXT
);

CREATE TABLE IF NOT EXISTS module_capture_events (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    capture_session_id UUID NOT NULL REFERENCES module_capture_sessions(id) ON DELETE CASCADE,
    timestamp TIMESTAMPTZ NOT NULL DEFAULT now(),
    event_type TEXT NOT NULL,
    board TEXT CHECK (board IS NULL OR board IN ('BPM', 'UCE', 'GSA')),
    source_address INTEGER CHECK (source_address IS NULL OR (source_address >= 0 AND source_address <= 255)),
    destination_address INTEGER CHECK (destination_address IS NULL OR (destination_address >= 0 AND destination_address <= 255)),
    pgn INTEGER CHECK (pgn IS NULL OR (pgn >= 0 AND pgn <= 262143)),
    can_id BIGINT,
    data_hex TEXT,
    payload_json JSONB,
    notes TEXT
);

CREATE INDEX IF NOT EXISTS idx_module_profile_versions_profile ON module_profile_versions(module_profile_id);
CREATE INDEX IF NOT EXISTS idx_module_connectors_version ON module_connectors(module_profile_version_id);
CREATE INDEX IF NOT EXISTS idx_module_pins_connector ON module_pins(connector_id);
CREATE INDEX IF NOT EXISTS idx_module_power_version ON module_power_requirements(module_profile_version_id);
CREATE INDEX IF NOT EXISTS idx_module_can_networks_version ON module_can_networks(module_profile_version_id);
CREATE INDEX IF NOT EXISTS idx_module_j1939_pgns_version_pgn ON module_j1939_pgns(module_profile_version_id, pgn);
CREATE INDEX IF NOT EXISTS idx_module_signal_channels_version ON module_signal_channels(module_profile_version_id);
CREATE INDEX IF NOT EXISTS idx_module_sdh_commands_version_order ON module_sdh_commands(module_profile_version_id, command_group, execution_order);
CREATE INDEX IF NOT EXISTS idx_module_test_sequences_version ON module_test_sequences(module_profile_version_id);
CREATE INDEX IF NOT EXISTS idx_module_test_steps_sequence_order ON module_test_steps(test_sequence_id, step_order);
CREATE INDEX IF NOT EXISTS idx_module_capture_sessions_version ON module_capture_sessions(module_profile_version_id);
CREATE INDEX IF NOT EXISTS idx_module_capture_events_session_time ON module_capture_events(capture_session_id, timestamp);
CREATE INDEX IF NOT EXISTS idx_module_capture_events_j1939 ON module_capture_events(pgn, source_address, destination_address);
