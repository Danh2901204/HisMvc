-- ============================================
-- HIS DATABASE SCHEMA VERIFICATION SCRIPT
-- ============================================
-- Purpose: Ki?m tra c?u trúc database, indexes, constraints
-- Date: 2026-01-28
-- ============================================

USE master;
GO

-- 1. KI?M TRA DATABASE T?N T?I
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'HisDb')
BEGIN
    PRINT '? Database HisDb exists'
END
ELSE
BEGIN
    PRINT '? Database HisDb does NOT exist'
END
GO

USE HisDb;
GO

-- 2. TH?NG KÊ TABLES
PRINT CHAR(13) + CHAR(10) + '========== TABLE STATISTICS =========='
SELECT 
    SCHEMA_NAME(schema_id) AS SchemaName,
    name AS TableName,
    create_date AS CreatedDate,
    modify_date AS ModifiedDate
FROM sys.tables
ORDER BY name;

PRINT CHAR(13) + CHAR(10) + 'Total Tables: ' + CAST((SELECT COUNT(*) FROM sys.tables) AS VARCHAR(10))
GO

-- 3. TH?NG KÊ COLUMNS
PRINT CHAR(13) + CHAR(10) + '========== TOP TABLES BY COLUMN COUNT =========='
SELECT TOP 10
    t.name AS TableName,
    COUNT(c.column_id) AS ColumnCount
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
GROUP BY t.name
ORDER BY ColumnCount DESC;
GO

-- 4. KI?M TRA PRIMARY KEYS
PRINT CHAR(13) + CHAR(10) + '========== PRIMARY KEYS =========='
SELECT 
    t.name AS TableName,
    i.name AS PrimaryKeyName,
    COL_NAME(ic.object_id, ic.column_id) AS ColumnName
FROM sys.tables t
INNER JOIN sys.indexes i ON t.object_id = i.object_id AND i.is_primary_key = 1
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
ORDER BY t.name;
GO

-- 5. KI?M TRA FOREIGN KEYS
PRINT CHAR(13) + CHAR(10) + '========== FOREIGN KEY RELATIONSHIPS =========='
SELECT 
    OBJECT_NAME(f.parent_object_id) AS TableName,
    f.name AS ForeignKeyName,
    OBJECT_NAME(f.referenced_object_id) AS ReferencedTable,
    COL_NAME(fc.parent_object_id, fc.parent_column_id) AS ForeignKeyColumn,
    COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS ReferencedColumn,
    CASE f.delete_referential_action
        WHEN 0 THEN 'NO ACTION'
        WHEN 1 THEN 'CASCADE'
        WHEN 2 THEN 'SET NULL'
        WHEN 3 THEN 'SET DEFAULT'
    END AS DeleteAction
FROM sys.foreign_keys f
INNER JOIN sys.foreign_key_columns fc ON f.object_id = fc.constraint_object_id
ORDER BY TableName, ForeignKeyName;

PRINT CHAR(13) + CHAR(10) + 'Total Foreign Keys: ' + CAST((SELECT COUNT(*) FROM sys.foreign_keys) AS VARCHAR(10))
GO

-- 6. KI?M TRA UNIQUE CONSTRAINTS & INDEXES
PRINT CHAR(13) + CHAR(10) + '========== UNIQUE CONSTRAINTS & INDEXES =========='
SELECT 
    t.name AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType,
    COL_NAME(ic.object_id, ic.column_id) AS ColumnName
FROM sys.tables t
INNER JOIN sys.indexes i ON t.object_id = i.object_id
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
WHERE i.is_unique = 1 AND i.is_primary_key = 0
ORDER BY t.name, i.name;
GO

-- 7. DATA VOLUME (ROW COUNTS)
PRINT CHAR(13) + CHAR(10) + '========== TABLE ROW COUNTS =========='
SELECT 
    t.name AS TableName,
    SUM(p.rows) AS RowCount
FROM sys.tables t
INNER JOIN sys.partitions p ON t.object_id = p.object_id
WHERE p.index_id IN (0, 1) -- Heap or Clustered Index
GROUP BY t.name
HAVING SUM(p.rows) > 0
ORDER BY RowCount DESC;
GO

-- 8. CHECK CONSTRAINTS
PRINT CHAR(13) + CHAR(10) + '========== CHECK CONSTRAINTS =========='
SELECT 
    OBJECT_NAME(parent_object_id) AS TableName,
    name AS ConstraintName,
    definition AS CheckDefinition
FROM sys.check_constraints
ORDER BY TableName;
GO

-- 9. DEFAULT CONSTRAINTS
PRINT CHAR(13) + CHAR(10) + '========== DEFAULT CONSTRAINTS =========='
SELECT 
    OBJECT_NAME(parent_object_id) AS TableName,
    name AS ConstraintName,
    COL_NAME(parent_object_id, parent_column_id) AS ColumnName,
    definition AS DefaultValue
FROM sys.default_constraints
ORDER BY TableName;
GO

-- 10. KI?M TRA IDENTITY COLUMNS
PRINT CHAR(13) + CHAR(10) + '========== IDENTITY COLUMNS =========='
SELECT 
    OBJECT_NAME(c.object_id) AS TableName,
    c.name AS ColumnName,
    IDENT_SEED(OBJECT_NAME(c.object_id)) AS Seed,
    IDENT_INCR(OBJECT_NAME(c.object_id)) AS Increment,
    IDENT_CURRENT(OBJECT_NAME(c.object_id)) AS CurrentValue
FROM sys.columns c
WHERE c.is_identity = 1
ORDER BY TableName;
GO

-- 11. NULLABLE COLUMNS
PRINT CHAR(13) + CHAR(10) + '========== NULLABLE COLUMNS BY TABLE =========='
SELECT 
    t.name AS TableName,
    COUNT(CASE WHEN c.is_nullable = 1 THEN 1 END) AS NullableColumns,
    COUNT(CASE WHEN c.is_nullable = 0 THEN 1 END) AS NotNullColumns
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
GROUP BY t.name
ORDER BY NullableColumns DESC;
GO

-- 12. DECIMAL/NUMERIC PRECISION
PRINT CHAR(13) + CHAR(10) + '========== DECIMAL/NUMERIC COLUMNS =========='
SELECT 
    OBJECT_NAME(c.object_id) AS TableName,
    c.name AS ColumnName,
    TYPE_NAME(c.user_type_id) AS DataType,
    c.precision AS Precision,
    c.scale AS Scale
FROM sys.columns c
WHERE TYPE_NAME(c.user_type_id) IN ('decimal', 'numeric')
ORDER BY TableName, ColumnName;
GO

-- 13. KI?M TRA INDEXES (ALL)
PRINT CHAR(13) + CHAR(10) + '========== ALL INDEXES =========='
SELECT 
    OBJECT_NAME(i.object_id) AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType,
    i.is_unique AS IsUnique,
    i.is_primary_key AS IsPrimaryKey,
    STRING_AGG(COL_NAME(ic.object_id, ic.column_id), ', ') AS IndexedColumns
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
WHERE i.type > 0 -- Exclude heaps
GROUP BY i.object_id, i.name, i.type_desc, i.is_unique, i.is_primary_key
ORDER BY TableName, IndexName;
GO

-- 14. DATABASE SIZE
PRINT CHAR(13) + CHAR(10) + '========== DATABASE SIZE =========='
SELECT 
    name AS DatabaseName,
    size * 8 / 1024 AS SizeMB,
    max_size * 8 / 1024 AS MaxSizeMB
FROM sys.database_files;
GO

-- 15. KI?M TRA EF MIGRATIONS
PRINT CHAR(13) + CHAR(10) + '========== EF MIGRATIONS HISTORY =========='
IF EXISTS (SELECT * FROM sys.tables WHERE name = '__EFMigrationsHistory')
BEGIN
    SELECT 
        MigrationId,
        ProductVersion,
        ROW_NUMBER() OVER (ORDER BY MigrationId) AS MigrationOrder
    FROM __EFMigrationsHistory
    ORDER BY MigrationId;
    
    PRINT CHAR(13) + CHAR(10) + 'Total Migrations Applied: ' + 
        CAST((SELECT COUNT(*) FROM __EFMigrationsHistory) AS VARCHAR(10))
END
ELSE
BEGIN
    PRINT '? __EFMigrationsHistory table not found'
END
GO

-- 16. SAMPLE DATA CHECK
PRINT CHAR(13) + CHAR(10) + '========== SAMPLE DATA CHECK =========='
PRINT 'Patients: ' + CAST((SELECT COUNT(*) FROM Patients) AS VARCHAR(10))
PRINT 'Departments: ' + CAST((SELECT COUNT(*) FROM Departments) AS VARCHAR(10))
PRINT 'Staffs: ' + CAST((SELECT COUNT(*) FROM Staffs) AS VARCHAR(10))
PRINT 'TimeSlots: ' + CAST((SELECT COUNT(*) FROM TimeSlots) AS VARCHAR(10))
PRINT 'Services: ' + CAST((SELECT COUNT(*) FROM Services) AS VARCHAR(10))
PRINT 'Medicines: ' + CAST((SELECT COUNT(*) FROM Medicines) AS VARCHAR(10))
PRINT 'MedicineBatches: ' + CAST((SELECT COUNT(*) FROM MedicineBatches) AS VARCHAR(10))
PRINT 'Wards: ' + CAST((SELECT COUNT(*) FROM Wards) AS VARCHAR(10))
PRINT 'Beds: ' + CAST((SELECT COUNT(*) FROM Beds) AS VARCHAR(10))
GO

-- 17. ORPHAN RECORDS CHECK (Records with invalid FKs)
PRINT CHAR(13) + CHAR(10) + '========== ORPHAN RECORDS CHECK =========='

-- Check Appointments with invalid DoctorId
IF EXISTS (SELECT 1 FROM Appointments a LEFT JOIN Staffs s ON a.DoctorId = s.StaffId WHERE s.StaffId IS NULL)
    PRINT '? Found orphan Appointments (invalid DoctorId)'
ELSE
    PRINT '? No orphan Appointments'

-- Check Encounters with invalid PatientId
IF EXISTS (SELECT 1 FROM Encounters e LEFT JOIN Patients p ON e.PatientId = p.PatientId WHERE p.PatientId IS NULL)
    PRINT '? Found orphan Encounters (invalid PatientId)'
ELSE
    PRINT '? No orphan Encounters'

-- Check Prescriptions with invalid EncounterId
IF EXISTS (SELECT 1 FROM Prescriptions pr LEFT JOIN Encounters e ON pr.EncounterId = e.EncounterId WHERE e.EncounterId IS NULL)
    PRINT '? Found orphan Prescriptions (invalid EncounterId)'
ELSE
    PRINT '? No orphan Prescriptions'

GO

-- 18. INDEX FRAGMENTATION
PRINT CHAR(13) + CHAR(10) + '========== INDEX FRAGMENTATION (> 10%) =========='
SELECT 
    OBJECT_NAME(ips.object_id) AS TableName,
    i.name AS IndexName,
    ips.index_type_desc AS IndexType,
    ips.avg_fragmentation_in_percent AS FragmentationPercent,
    ips.page_count AS PageCount
FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ips
INNER JOIN sys.indexes i ON ips.object_id = i.object_id AND ips.index_id = i.index_id
WHERE ips.avg_fragmentation_in_percent > 10 
    AND ips.page_count > 100
    AND i.name IS NOT NULL
ORDER BY ips.avg_fragmentation_in_percent DESC;
GO

-- 19. MISSING INDEXES RECOMMENDATIONS
PRINT CHAR(13) + CHAR(10) + '========== MISSING INDEX RECOMMENDATIONS (TOP 10) =========='
SELECT TOP 10
    OBJECT_NAME(mid.object_id) AS TableName,
    mid.equality_columns AS EqualityColumns,
    mid.inequality_columns AS InequalityColumns,
    mid.included_columns AS IncludedColumns,
    migs.avg_user_impact AS AvgUserImpact,
    migs.user_seeks AS UserSeeks,
    migs.user_scans AS UserScans
FROM sys.dm_db_missing_index_details mid
INNER JOIN sys.dm_db_missing_index_groups mig ON mid.index_handle = mig.index_handle
INNER JOIN sys.dm_db_missing_index_group_stats migs ON mig.index_group_handle = migs.group_handle
WHERE mid.database_id = DB_ID()
ORDER BY migs.avg_user_impact * (migs.user_seeks + migs.user_scans) DESC;
GO

-- 20. SUMMARY
PRINT CHAR(13) + CHAR(10) + '========== DATABASE HEALTH SUMMARY =========='
PRINT '? Schema verification complete!'
PRINT CHAR(13) + CHAR(10) + 'Total Tables: ' + CAST((SELECT COUNT(*) FROM sys.tables) AS VARCHAR(10))
PRINT 'Total Foreign Keys: ' + CAST((SELECT COUNT(*) FROM sys.foreign_keys) AS VARCHAR(10))
PRINT 'Total Indexes: ' + CAST((SELECT COUNT(*) FROM sys.indexes WHERE type > 0) AS VARCHAR(10))
PRINT 'Total Unique Constraints: ' + CAST((SELECT COUNT(*) FROM sys.indexes WHERE is_unique = 1 AND is_primary_key = 0) AS VARCHAR(10))

GO
