-- Adds J1939/81 reference catalogs for CAN network identity interpretation.
-- This migration creates structure only and intentionally does not seed catalog data.

CREATE TABLE IF NOT EXISTS j1939_industry_groups (
    id TEXT PRIMARY KEY,
    code INTEGER NOT NULL UNIQUE CHECK (code >= 0 AND code <= 7),
    name TEXT NOT NULL,
    description TEXT,
    source TEXT,
    notes TEXT,
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_j1939_industry_groups_code
    ON j1939_industry_groups(code);

CREATE TABLE IF NOT EXISTS j1939_manufacturers (
    id TEXT PRIMARY KEY,
    code INTEGER NOT NULL UNIQUE CHECK (code >= 0),
    name TEXT NOT NULL,
    country TEXT,
    source TEXT,
    notes TEXT,
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_j1939_manufacturers_code
    ON j1939_manufacturers(code);

CREATE INDEX IF NOT EXISTS idx_j1939_manufacturers_name
    ON j1939_manufacturers(name);

CREATE TABLE IF NOT EXISTS j1939_functions (
    id TEXT PRIMARY KEY,
    code INTEGER NOT NULL UNIQUE CHECK (code >= 0 AND code <= 255),
    name TEXT NOT NULL,
    description TEXT,
    source TEXT,
    notes TEXT,
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_j1939_functions_code
    ON j1939_functions(code);

CREATE INDEX IF NOT EXISTS idx_j1939_functions_name
    ON j1939_functions(name);

CREATE TABLE IF NOT EXISTS j1939_vehicle_systems (
    id TEXT PRIMARY KEY,
    code INTEGER NOT NULL CHECK (code >= 0 AND code <= 127),
    name TEXT NOT NULL,
    description TEXT,
    industry_group_code INTEGER CHECK (industry_group_code IS NULL OR (industry_group_code >= 0 AND industry_group_code <= 7)),
    source TEXT,
    notes TEXT,
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_j1939_vehicle_systems_code
    ON j1939_vehicle_systems(code);

CREATE INDEX IF NOT EXISTS idx_j1939_vehicle_systems_industry_group_code
    ON j1939_vehicle_systems(industry_group_code);

CREATE UNIQUE INDEX IF NOT EXISTS idx_j1939_vehicle_systems_code_industry_group_code
    ON j1939_vehicle_systems(code, industry_group_code);

CREATE TABLE IF NOT EXISTS j1939_preferred_addresses (
    id TEXT PRIMARY KEY,
    address INTEGER NOT NULL CHECK (address >= 0 AND address <= 255),
    name TEXT NOT NULL,
    description TEXT,
    function_code INTEGER CHECK (function_code IS NULL OR (function_code >= 0 AND function_code <= 255)),
    industry_group_code INTEGER CHECK (industry_group_code IS NULL OR (industry_group_code >= 0 AND industry_group_code <= 7)),
    source TEXT,
    notes TEXT,
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_j1939_preferred_addresses_address
    ON j1939_preferred_addresses(address);

CREATE INDEX IF NOT EXISTS idx_j1939_preferred_addresses_function_code
    ON j1939_preferred_addresses(function_code);

CREATE INDEX IF NOT EXISTS idx_j1939_preferred_addresses_industry_group_code
    ON j1939_preferred_addresses(industry_group_code);

CREATE UNIQUE INDEX IF NOT EXISTS idx_j1939_preferred_addresses_address_industry_group_code
    ON j1939_preferred_addresses(address, industry_group_code);

CREATE TABLE IF NOT EXISTS j1939_name_field_definitions (
    id TEXT PRIMARY KEY,
    field_name TEXT NOT NULL UNIQUE,
    bit_start INTEGER CHECK (bit_start IS NULL OR bit_start >= 0),
    bit_length INTEGER CHECK (bit_length IS NULL OR bit_length > 0),
    description TEXT,
    source TEXT,
    notes TEXT,
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_j1939_name_field_definitions_field_name
    ON j1939_name_field_definitions(field_name);
