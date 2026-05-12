# Module Database Schema Dump v1

Source database: `Data/Modules/modules.db`

## Tables

### module_can_networks

| Column | Type | Not Null | Default | PK |
| --- | --- | --- | --- | --- |
| id | TEXT | 0 |  | 1 |
| module_profile_version_id | TEXT | 1 |  | 0 |
| name | TEXT | 1 |  | 0 |
| protocol | TEXT | 1 |  | 0 |
| bitrate_kbps | INTEGER | 1 |  | 0 |
| controller | TEXT | 0 |  | 0 |
| expected_source_address | INTEGER | 0 |  | 0 |
| can_high_pin_id | TEXT | 0 |  | 0 |
| can_low_pin_id | TEXT | 0 |  | 0 |
| notes | TEXT | 0 |  | 0 |

```sql
CREATE TABLE module_can_networks (
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
)
```

### module_capture_events

| Column | Type | Not Null | Default | PK |
| --- | --- | --- | --- | --- |
| id | TEXT | 0 |  | 1 |
| capture_session_id | TEXT | 1 |  | 0 |
| timestamp | TEXT | 1 | CURRENT_TIMESTAMP | 0 |
| event_type | TEXT | 1 |  | 0 |
| board | TEXT | 0 |  | 0 |
| source_address | INTEGER | 0 |  | 0 |
| destination_address | INTEGER | 0 |  | 0 |
| pgn | INTEGER | 0 |  | 0 |
| can_id | INTEGER | 0 |  | 0 |
| data_hex | TEXT | 0 |  | 0 |
| payload_json | TEXT | 0 |  | 0 |
| notes | TEXT | 0 |  | 0 |

```sql
CREATE TABLE module_capture_events (
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
)
```

### module_capture_sessions

| Column | Type | Not Null | Default | PK |
| --- | --- | --- | --- | --- |
| id | TEXT | 0 |  | 1 |
| module_profile_version_id | TEXT | 1 |  | 0 |
| name | TEXT | 1 |  | 0 |
| started_at | TEXT | 1 | CURRENT_TIMESTAMP | 0 |
| ended_at | TEXT | 0 |  | 0 |
| source | TEXT | 0 |  | 0 |
| notes | TEXT | 0 |  | 0 |

```sql
CREATE TABLE module_capture_sessions (
    id TEXT PRIMARY KEY,
    module_profile_version_id TEXT NOT NULL,
    name TEXT NOT NULL,
    started_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ended_at TEXT,
    source TEXT,
    notes TEXT,
    FOREIGN KEY (module_profile_version_id) REFERENCES module_profile_versions(id) ON DELETE CASCADE
)
```

### module_connectors

| Column | Type | Not Null | Default | PK |
| --- | --- | --- | --- | --- |
| id | TEXT | 0 |  | 1 |
| module_profile_version_id | TEXT | 1 |  | 0 |
| name | TEXT | 1 |  | 0 |
| description | TEXT | 0 |  | 0 |
| pin_count | INTEGER | 1 |  | 0 |

```sql
CREATE TABLE module_connectors (
    id TEXT PRIMARY KEY,
    module_profile_version_id TEXT NOT NULL,
    name TEXT NOT NULL,
    description TEXT,
    pin_count INTEGER NOT NULL CHECK (pin_count > 0),
    FOREIGN KEY (module_profile_version_id) REFERENCES module_profile_versions(id) ON DELETE CASCADE
)
```

### module_j1939_pgns

| Column | Type | Not Null | Default | PK |
| --- | --- | --- | --- | --- |
| id | TEXT | 0 |  | 1 |
| module_profile_version_id | TEXT | 1 |  | 0 |
| source_address | INTEGER | 0 |  | 0 |
| destination_address | INTEGER | 0 |  | 0 |
| pgn | INTEGER | 1 |  | 0 |
| name | TEXT | 0 |  | 0 |
| direction | TEXT | 0 |  | 0 |
| period_ms | INTEGER | 0 |  | 0 |
| proprietary | INTEGER | 1 | 0 | 0 |
| data_sample_hex | TEXT | 0 |  | 0 |
| notes | TEXT | 0 |  | 0 |

```sql
CREATE TABLE module_j1939_pgns (
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
)
```

### module_pins

| Column | Type | Not Null | Default | PK |
| --- | --- | --- | --- | --- |
| id | TEXT | 0 |  | 1 |
| connector_id | TEXT | 1 |  | 0 |
| pin_number | TEXT | 1 |  | 0 |
| pin_name | TEXT | 0 |  | 0 |
| function_name | TEXT | 0 |  | 0 |
| electrical_type | TEXT | 0 |  | 0 |
| direction | TEXT | 0 |  | 0 |
| notes | TEXT | 0 |  | 0 |

```sql
CREATE TABLE module_pins (
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
)
```

### module_power_requirements

| Column | Type | Not Null | Default | PK |
| --- | --- | --- | --- | --- |
| id | TEXT | 0 |  | 1 |
| module_profile_version_id | TEXT | 1 |  | 0 |
| name | TEXT | 1 |  | 0 |
| voltage_nominal | REAL | 0 |  | 0 |
| voltage_min | REAL | 0 |  | 0 |
| voltage_max | REAL | 0 |  | 0 |
| current_expected | REAL | 0 |  | 0 |
| current_max | REAL | 0 |  | 0 |
| positive_pin_id | TEXT | 0 |  | 0 |
| negative_pin_id | TEXT | 0 |  | 0 |
| enable_sequence_order | INTEGER | 0 |  | 0 |
| notes | TEXT | 0 |  | 0 |

```sql
CREATE TABLE module_power_requirements (
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
)
```

### module_profile_versions

| Column | Type | Not Null | Default | PK |
| --- | --- | --- | --- | --- |
| id | TEXT | 0 |  | 1 |
| module_profile_id | TEXT | 1 |  | 0 |
| version | TEXT | 1 |  | 0 |
| status | TEXT | 1 | 'draft' | 0 |
| source | TEXT | 0 |  | 0 |
| notes | TEXT | 0 |  | 0 |
| created_at | TEXT | 1 | CURRENT_TIMESTAMP | 0 |
| updated_at | TEXT | 1 | CURRENT_TIMESTAMP | 0 |

```sql
CREATE TABLE module_profile_versions (
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
)
```

### module_profiles

| Column | Type | Not Null | Default | PK |
| --- | --- | --- | --- | --- |
| id | TEXT | 0 |  | 1 |
| name | TEXT | 1 |  | 0 |
| manufacturer | TEXT | 0 |  | 0 |
| model | TEXT | 0 |  | 0 |
| category | TEXT | 0 |  | 0 |
| application | TEXT | 0 |  | 0 |
| description | TEXT | 0 |  | 0 |
| status | TEXT | 1 | 'draft' | 0 |
| created_at | TEXT | 1 | CURRENT_TIMESTAMP | 0 |
| updated_at | TEXT | 1 | CURRENT_TIMESTAMP | 0 |

```sql
CREATE TABLE module_profiles (
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
)
```

### module_sdh_commands

| Column | Type | Not Null | Default | PK |
| --- | --- | --- | --- | --- |
| id | TEXT | 0 |  | 1 |
| module_profile_version_id | TEXT | 1 |  | 0 |
| command_group | TEXT | 0 |  | 0 |
| execution_order | INTEGER | 1 | 0 | 0 |
| target | TEXT | 1 |  | 0 |
| op | TEXT | 1 |  | 0 |
| args_json | TEXT | 1 | '{}' | 0 |
| meta_json | TEXT | 1 | '{}' | 0 |
| enabled | INTEGER | 1 | 1 | 0 |
| notes | TEXT | 0 |  | 0 |

```sql
CREATE TABLE module_sdh_commands (
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
)
```

### module_signal_channels

| Column | Type | Not Null | Default | PK |
| --- | --- | --- | --- | --- |
| id | TEXT | 0 |  | 1 |
| module_profile_version_id | TEXT | 1 |  | 0 |
| signal_name | TEXT | 1 |  | 0 |
| signal_type | TEXT | 0 |  | 0 |
| direction | TEXT | 0 |  | 0 |
| physical_unit | TEXT | 0 |  | 0 |
| min_value | REAL | 0 |  | 0 |
| max_value | REAL | 0 |  | 0 |
| default_value | REAL | 0 |  | 0 |
| board | TEXT | 0 |  | 0 |
| board_channel | TEXT | 0 |  | 0 |
| related_pin_id | TEXT | 0 |  | 0 |
| notes | TEXT | 0 |  | 0 |

```sql
CREATE TABLE module_signal_channels (
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
)
```

### module_test_sequences

| Column | Type | Not Null | Default | PK |
| --- | --- | --- | --- | --- |
| id | TEXT | 0 |  | 1 |
| module_profile_version_id | TEXT | 1 |  | 0 |
| name | TEXT | 1 |  | 0 |
| description | TEXT | 0 |  | 0 |
| enabled | INTEGER | 1 | 1 | 0 |
| created_at | TEXT | 1 | CURRENT_TIMESTAMP | 0 |
| updated_at | TEXT | 1 | CURRENT_TIMESTAMP | 0 |

```sql
CREATE TABLE module_test_sequences (
    id TEXT PRIMARY KEY,
    module_profile_version_id TEXT NOT NULL,
    name TEXT NOT NULL,
    description TEXT,
    enabled INTEGER NOT NULL DEFAULT 1 CHECK (enabled IN (0, 1)),
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (module_profile_version_id) REFERENCES module_profile_versions(id) ON DELETE CASCADE
)
```

### module_test_steps

| Column | Type | Not Null | Default | PK |
| --- | --- | --- | --- | --- |
| id | TEXT | 0 |  | 1 |
| test_sequence_id | TEXT | 1 |  | 0 |
| step_order | INTEGER | 1 |  | 0 |
| step_type | TEXT | 1 |  | 0 |
| sdh_command_json | TEXT | 0 |  | 0 |
| expected_response_json | TEXT | 0 |  | 0 |
| expected_event_json | TEXT | 0 |  | 0 |
| timeout_ms | INTEGER | 0 |  | 0 |
| retry_count | INTEGER | 1 | 0 | 0 |
| notes | TEXT | 0 |  | 0 |

```sql
CREATE TABLE module_test_steps (
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
)
```

## Indexes

| Index | Table | Definition |
| --- | --- | --- |
| idx_module_can_networks_version | module_can_networks | CREATE INDEX idx_module_can_networks_version ON module_can_networks(module_profile_version_id) |
| idx_module_capture_events_j1939 | module_capture_events | CREATE INDEX idx_module_capture_events_j1939 ON module_capture_events(pgn, source_address, destination_address) |
| idx_module_capture_events_session_time | module_capture_events | CREATE INDEX idx_module_capture_events_session_time ON module_capture_events(capture_session_id, timestamp) |
| idx_module_capture_sessions_version | module_capture_sessions | CREATE INDEX idx_module_capture_sessions_version ON module_capture_sessions(module_profile_version_id) |
| idx_module_connectors_version | module_connectors | CREATE INDEX idx_module_connectors_version ON module_connectors(module_profile_version_id) |
| idx_module_j1939_pgns_version_pgn | module_j1939_pgns | CREATE INDEX idx_module_j1939_pgns_version_pgn ON module_j1939_pgns(module_profile_version_id, pgn) |
| idx_module_pins_connector | module_pins | CREATE INDEX idx_module_pins_connector ON module_pins(connector_id) |
| idx_module_power_version | module_power_requirements | CREATE INDEX idx_module_power_version ON module_power_requirements(module_profile_version_id) |
| idx_module_profile_versions_profile | module_profile_versions | CREATE INDEX idx_module_profile_versions_profile ON module_profile_versions(module_profile_id) |
| idx_module_sdh_commands_version_order | module_sdh_commands | CREATE INDEX idx_module_sdh_commands_version_order ON module_sdh_commands(module_profile_version_id, command_group, execution_order) |
| idx_module_signal_channels_version | module_signal_channels | CREATE INDEX idx_module_signal_channels_version ON module_signal_channels(module_profile_version_id) |
| idx_module_test_sequences_version | module_test_sequences | CREATE INDEX idx_module_test_sequences_version ON module_test_sequences(module_profile_version_id) |
| idx_module_test_steps_sequence_order | module_test_steps | CREATE INDEX idx_module_test_steps_sequence_order ON module_test_steps(test_sequence_id, step_order) |
