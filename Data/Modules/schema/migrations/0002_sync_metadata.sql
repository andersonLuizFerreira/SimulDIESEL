-- Adds provider-neutral metadata for future PostgreSQL/Supabase synchronization.
-- The custom directives are executed by SqliteMigrationRunner only when the column is missing.

-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_profiles sync_status TEXT NOT NULL DEFAULT 'local' CHECK (sync_status IN ('local', 'synced', 'pending', 'conflict', 'deleted'))
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_profiles cloud_id TEXT
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_profiles deleted_at TEXT

-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_profile_versions sync_status TEXT NOT NULL DEFAULT 'local' CHECK (sync_status IN ('local', 'synced', 'pending', 'conflict', 'deleted'))
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_profile_versions cloud_id TEXT
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_profile_versions deleted_at TEXT

-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_connectors created_at TEXT
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_connectors updated_at TEXT
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_connectors sync_status TEXT NOT NULL DEFAULT 'local' CHECK (sync_status IN ('local', 'synced', 'pending', 'conflict', 'deleted'))
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_connectors cloud_id TEXT
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_connectors deleted_at TEXT

-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_pins created_at TEXT
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_pins updated_at TEXT
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_pins sync_status TEXT NOT NULL DEFAULT 'local' CHECK (sync_status IN ('local', 'synced', 'pending', 'conflict', 'deleted'))
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_pins cloud_id TEXT
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_pins deleted_at TEXT

-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_power_requirements created_at TEXT
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_power_requirements updated_at TEXT
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_power_requirements sync_status TEXT NOT NULL DEFAULT 'local' CHECK (sync_status IN ('local', 'synced', 'pending', 'conflict', 'deleted'))
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_power_requirements cloud_id TEXT
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_power_requirements deleted_at TEXT

-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_can_networks created_at TEXT
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_can_networks updated_at TEXT
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_can_networks sync_status TEXT NOT NULL DEFAULT 'local' CHECK (sync_status IN ('local', 'synced', 'pending', 'conflict', 'deleted'))
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_can_networks cloud_id TEXT
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_can_networks deleted_at TEXT

-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_j1939_pgns created_at TEXT
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_j1939_pgns updated_at TEXT
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_j1939_pgns sync_status TEXT NOT NULL DEFAULT 'local' CHECK (sync_status IN ('local', 'synced', 'pending', 'conflict', 'deleted'))
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_j1939_pgns cloud_id TEXT
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_j1939_pgns deleted_at TEXT

-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_signal_channels created_at TEXT
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_signal_channels updated_at TEXT
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_signal_channels sync_status TEXT NOT NULL DEFAULT 'local' CHECK (sync_status IN ('local', 'synced', 'pending', 'conflict', 'deleted'))
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_signal_channels cloud_id TEXT
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_signal_channels deleted_at TEXT

-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_sdh_commands created_at TEXT
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_sdh_commands updated_at TEXT
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_sdh_commands sync_status TEXT NOT NULL DEFAULT 'local' CHECK (sync_status IN ('local', 'synced', 'pending', 'conflict', 'deleted'))
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_sdh_commands cloud_id TEXT
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_sdh_commands deleted_at TEXT

-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_test_sequences sync_status TEXT NOT NULL DEFAULT 'local' CHECK (sync_status IN ('local', 'synced', 'pending', 'conflict', 'deleted'))
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_test_sequences cloud_id TEXT
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_test_sequences deleted_at TEXT

-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_test_steps created_at TEXT
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_test_steps updated_at TEXT
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_test_steps sync_status TEXT NOT NULL DEFAULT 'local' CHECK (sync_status IN ('local', 'synced', 'pending', 'conflict', 'deleted'))
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_test_steps cloud_id TEXT
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_test_steps deleted_at TEXT

-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_capture_sessions updated_at TEXT
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_capture_sessions sync_status TEXT NOT NULL DEFAULT 'local' CHECK (sync_status IN ('local', 'synced', 'pending', 'conflict', 'deleted'))
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_capture_sessions cloud_id TEXT
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_capture_sessions deleted_at TEXT

-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_capture_events updated_at TEXT
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_capture_events sync_status TEXT NOT NULL DEFAULT 'local' CHECK (sync_status IN ('local', 'synced', 'pending', 'conflict', 'deleted'))
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_capture_events cloud_id TEXT
-- SIMULDIESEL_ADD_COLUMN_IF_MISSING module_capture_events deleted_at TEXT
