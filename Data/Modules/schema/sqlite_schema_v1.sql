PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS module_profiles (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    manufacturer TEXT,
    model TEXT,
    category TEXT,
    application TEXT,
    description TEXT,
    status TEXT NOT NULL DEFAULT 'draft' CHECK (status IN ('draft', 'active', 'archived')),
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS module_profile_versions (
    id TEXT PRIMARY KEY,
    module_profile_id TEXT NOT NULL,
    version TEXT NOT NULL,
    status TEXT NOT NULL DEFAULT 'draft' CHECK (status IN ('draft', 'validated', 'released', 'archived')),
    source TEXT,
    notes TEXT,
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (module_profile_id) REFERENCES module_profiles(id) ON DELETE CASCADE,
    UNIQUE (module_profile_id, version)
);

CREATE TABLE IF NOT EXISTS module_connectors (
    id TEXT PRIMARY KEY,
    module_profile_version_id TEXT NOT NULL,
    name TEXT NOT NULL,
    description TEXT,
    pin_count INTEGER NOT NULL CHECK (pin_count > 0),
    FOREIGN KEY (module_profile_version_id) REFERENCES module_profile_versions(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS module_pins (
    id TEXT PRIMARY KEY,
    connector_id TEXT NOT NULL,
    pin_number TEXT NOT NULL,
    pin_name TEXT,
    function_name TEXT,
    electrical_type TEXT,
    direction TEXT CHECK (direction IS NULL OR direction IN ('input', 'output', 'bidirectional', 'power', 'ground', 'none')),
    notes TEXT,
    FOREIGN KEY (connector_id) REFERENCES module_connectors(id) ON DELETE CASCADE,
    UNIQUE (connector_id, pin_number)
);

CREATE TABLE IF NOT EXISTS module_power_requirements (
    id TEXT PRIMARY KEY,
    module_profile_version_id TEXT NOT NULL,
    name TEXT NOT NULL,
    voltage_nominal REAL,
    voltage_min REAL,
    voltage_max REAL,
    current_expected REAL,
    current_max REAL,
    positive_pin_id TEXT,
    negative_pin_id TEXT,
    enable_sequence_order INTEGER,
    notes TEXT,
    FOREIGN KEY (module_profile_version_id) REFERENCES module_profile_versions(id) ON DELETE CASCADE,
    FOREIGN KEY (positive_pin_id) REFERENCES module_pins(id) ON DELETE SET NULL,
    FOREIGN KEY (negative_pin_id) REFERENCES module_pins(id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS module_can_networks (
    id TEXT PRIMARY KEY,
    module_profile_version_id TEXT NOT NULL,
    name TEXT NOT NULL,
    protocol TEXT NOT NULL CHECK (protocol IN ('CAN', 'J1939')),
    bitrate_kbps INTEGER NOT NULL,
    controller TEXT CHECK (controller IS NULL OR controller IN ('can0', 'can1')),
    expected_source_address INTEGER CHECK (expected_source_address IS NULL OR (expected_source_address >= 0 AND expected_source_address <= 255)),
    can_high_pin_id TEXT,
    can_low_pin_id TEXT,
    notes TEXT,
    FOREIGN KEY (module_profile_version_id) REFERENCES module_profile_versions(id) ON DELETE CASCADE,
    FOREIGN KEY (can_high_pin_id) REFERENCES module_pins(id) ON DELETE SET NULL,
    FOREIGN KEY (can_low_pin_id) REFERENCES module_pins(id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS module_j1939_pgns (
    id TEXT PRIMARY KEY,
    module_profile_version_id TEXT NOT NULL,
    source_address INTEGER CHECK (source_address IS NULL OR (source_address >= 0 AND source_address <= 255)),
    destination_address INTEGER CHECK (destination_address IS NULL OR (destination_address >= 0 AND destination_address <= 255)),
    pgn INTEGER NOT NULL CHECK (pgn >= 0 AND pgn <= 262143),
    name TEXT,
    direction TEXT CHECK (direction IS NULL OR direction IN ('rx', 'tx', 'observed', 'bidirectional')),
    period_ms INTEGER CHECK (period_ms IS NULL OR period_ms >= 0),
    proprietary INTEGER NOT NULL DEFAULT 0 CHECK (proprietary IN (0, 1)),
    data_sample_hex TEXT,
    notes TEXT,
    FOREIGN KEY (module_profile_version_id) REFERENCES module_profile_versions(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS module_signal_channels (
    id TEXT PRIMARY KEY,
    module_profile_version_id TEXT NOT NULL,
    signal_name TEXT NOT NULL,
    signal_type TEXT,
    direction TEXT CHECK (direction IS NULL OR direction IN ('input', 'output', 'bidirectional')),
    physical_unit TEXT,
    min_value REAL,
    max_value REAL,
    default_value REAL,
    board TEXT CHECK (board IS NULL OR board IN ('BPM', 'UCE', 'GSA')),
    board_channel TEXT,
    related_pin_id TEXT,
    notes TEXT,
    FOREIGN KEY (module_profile_version_id) REFERENCES module_profile_versions(id) ON DELETE CASCADE,
    FOREIGN KEY (related_pin_id) REFERENCES module_pins(id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS module_sdh_commands (
    id TEXT PRIMARY KEY,
    module_profile_version_id TEXT NOT NULL,
    command_group TEXT,
    execution_order INTEGER NOT NULL DEFAULT 0,
    target TEXT NOT NULL,
    op TEXT NOT NULL,
    args_json TEXT NOT NULL DEFAULT '{}' CHECK (json_valid(args_json)),
    meta_json TEXT NOT NULL DEFAULT '{}' CHECK (json_valid(meta_json)),
    enabled INTEGER NOT NULL DEFAULT 1 CHECK (enabled IN (0, 1)),
    notes TEXT,
    FOREIGN KEY (module_profile_version_id) REFERENCES module_profile_versions(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS module_test_sequences (
    id TEXT PRIMARY KEY,
    module_profile_version_id TEXT NOT NULL,
    name TEXT NOT NULL,
    description TEXT,
    enabled INTEGER NOT NULL DEFAULT 1 CHECK (enabled IN (0, 1)),
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (module_profile_version_id) REFERENCES module_profile_versions(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS module_test_steps (
    id TEXT PRIMARY KEY,
    test_sequence_id TEXT NOT NULL,
    step_order INTEGER NOT NULL,
    step_type TEXT NOT NULL CHECK (step_type IN ('sdh_command', 'wait', 'expect_event', 'note')),
    sdh_command_json TEXT CHECK (sdh_command_json IS NULL OR json_valid(sdh_command_json)),
    expected_response_json TEXT CHECK (expected_response_json IS NULL OR json_valid(expected_response_json)),
    expected_event_json TEXT CHECK (expected_event_json IS NULL OR json_valid(expected_event_json)),
    timeout_ms INTEGER CHECK (timeout_ms IS NULL OR timeout_ms >= 0),
    retry_count INTEGER NOT NULL DEFAULT 0 CHECK (retry_count >= 0),
    notes TEXT,
    FOREIGN KEY (test_sequence_id) REFERENCES module_test_sequences(id) ON DELETE CASCADE,
    UNIQUE (test_sequence_id, step_order)
);

CREATE TABLE IF NOT EXISTS module_capture_sessions (
    id TEXT PRIMARY KEY,
    module_profile_version_id TEXT NOT NULL,
    name TEXT NOT NULL,
    started_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ended_at TEXT,
    source TEXT,
    notes TEXT,
    FOREIGN KEY (module_profile_version_id) REFERENCES module_profile_versions(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS module_capture_events (
    id TEXT PRIMARY KEY,
    capture_session_id TEXT NOT NULL,
    timestamp TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    event_type TEXT NOT NULL,
    board TEXT CHECK (board IS NULL OR board IN ('BPM', 'UCE', 'GSA')),
    source_address INTEGER CHECK (source_address IS NULL OR (source_address >= 0 AND source_address <= 255)),
    destination_address INTEGER CHECK (destination_address IS NULL OR (destination_address >= 0 AND destination_address <= 255)),
    pgn INTEGER CHECK (pgn IS NULL OR (pgn >= 0 AND pgn <= 262143)),
    can_id INTEGER,
    data_hex TEXT,
    payload_json TEXT CHECK (payload_json IS NULL OR json_valid(payload_json)),
    notes TEXT,
    FOREIGN KEY (capture_session_id) REFERENCES module_capture_sessions(id) ON DELETE CASCADE
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
