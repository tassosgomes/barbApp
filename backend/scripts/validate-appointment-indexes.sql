-- Script to validate indexes on appointments table
-- Run this with: psql -d barbapp -f validate-appointment-indexes.sql

-- Check if appointments table exists
SELECT EXISTS (
   SELECT FROM information_schema.tables 
   WHERE  table_schema = 'public'
   AND    table_name   = 'appointments'
);

-- List all indexes on appointments table
SELECT
    indexname,
    indexdef
FROM pg_indexes
WHERE tablename = 'appointments'
ORDER BY indexname;

-- Explain query for GetByBarberAndDateAsync
EXPLAIN (ANALYZE, BUFFERS) 
SELECT * 
FROM appointments 
WHERE barber_id = '00000000-0000-0000-0000-000000000001'
  AND start_time >= '2025-10-25 00:00:00+00'
  AND start_time < '2025-10-26 00:00:00+00'
ORDER BY start_time;

-- Explain query with barbearia_id (multi-tenant filter)
EXPLAIN (ANALYZE, BUFFERS)
SELECT *
FROM appointments
WHERE barbearia_id = '00000000-0000-0000-0000-000000000001'
  AND barber_id = '00000000-0000-0000-0000-000000000002'
  AND start_time >= '2025-10-25 00:00:00+00'
  AND start_time < '2025-10-26 00:00:00+00'
ORDER BY start_time;

-- Check column types (ensure TIMESTAMP WITH TIME ZONE)
SELECT 
    column_name,
    data_type,
    is_nullable
FROM information_schema.columns
WHERE table_name = 'appointments'
  AND column_name IN ('start_time', 'end_time', 'created_at', 'updated_at', 'confirmed_at', 'cancelled_at', 'completed_at')
ORDER BY ordinal_position;
