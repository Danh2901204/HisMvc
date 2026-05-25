# ?? H??NG D?N S? D?NG C? S? D? LI?U HIS

## ?? QUICK REFERENCE

### 1. XEM SCHEMA DOCUMENTATION
```powershell
# M? file chi ti?t
code HisMvc/DATABASE_SCHEMA.md
```

### 2. KI?M TRA DATABASE HEALTH
```powershell
# Ch?y SQL script verification
sqlcmd -S localhost -d HisDb -i HisMvc/verify-database-schema.sql -o schema-report.txt
```

### 3. XEM ER DIAGRAM
```powershell
# Generate diagram
cd HisMvc
.\generate-er-diagram.ps1

# Xem diagram
code DATABASE_ER_DIAGRAM.md
# Press Ctrl+Shift+V (c?n extension: Markdown Preview Mermaid Support)
```

---

## ?? TRUY V?N TH??NG DÙNG

### Ki?m Tra S? L??ng Records
```sql
USE HisDb;

SELECT 
    t.name AS TableName,
    SUM(p.rows) AS RowCount
FROM sys.tables t
INNER JOIN sys.partitions p ON t.object_id = p.object_id
WHERE p.index_id IN (0, 1)
GROUP BY t.name
ORDER BY RowCount DESC;
```

### Xem Foreign Key Relationships
```sql
SELECT 
    OBJECT_NAME(f.parent_object_id) AS ChildTable,
    f.name AS ForeignKeyName,
    OBJECT_NAME(f.referenced_object_id) AS ParentTable
FROM sys.foreign_keys f
ORDER BY ChildTable;
```

### Ki?m Tra B?nh Nhân Có BHYT
```sql
SELECT 
    PatientId,
    FullName,
    InsuranceNumber,
    InsuranceType,
    InsuranceExpiry,
    InsuranceCoveragePercent,
    CASE 
        WHEN InsuranceExpiry IS NULL THEN 'Không có BHYT'
        WHEN InsuranceExpiry < GETDATE() THEN 'H?t h?n'
        ELSE 'Còn h?n'
    END AS Status
FROM Patients
WHERE InsuranceNumber IS NOT NULL
ORDER BY InsuranceExpiry DESC;
```

### Th?ng Kê L??t Khám Theo Ngày
```sql
SELECT 
    CAST(CheckInAt AS DATE) AS Date,
    COUNT(*) AS TotalEncounters,
    COUNT(CASE WHEN Status = 8 THEN 1 END) AS Completed,
    COUNT(CASE WHEN Status = 1 THEN 1 END) AS CheckedIn
FROM Encounters
WHERE CheckInAt >= DATEADD(day, -7, GETDATE())
GROUP BY CAST(CheckInAt AS DATE)
ORDER BY Date DESC;
```

### Top 10 Thu?c ???c Kê Nhi?u Nh?t
```sql
SELECT TOP 10
    m.Name,
    m.ActiveIngredient,
    COUNT(*) AS PrescribedCount,
    SUM(pi.Quantity) AS TotalQuantity
FROM PrescriptionItems pi
INNER JOIN Medicines m ON pi.MedicineId = m.MedicineId
GROUP BY m.Name, m.ActiveIngredient
ORDER BY PrescribedCount DESC;
```

### Ki?m Tra Thu?c S?p H?t H?n (< 3 tháng)
```sql
SELECT 
    m.Name,
    mb.BatchNumber,
    mb.ExpiryDate,
    mb.QuantityInStock,
    DATEDIFF(day, GETDATE(), mb.ExpiryDate) AS DaysRemaining
FROM MedicineBatches mb
INNER JOIN Medicines m ON mb.MedicineId = m.MedicineId
WHERE mb.ExpiryDate <= DATEADD(month, 3, GETDATE())
    AND mb.ExpiryDate > GETDATE()
    AND mb.QuantityInStock > 0
    AND mb.IsActive = 1
ORDER BY mb.ExpiryDate;
```

### Gi??ng B?nh Tr?ng
```sql
SELECT 
    w.Name AS WardName,
    b.BedNumber,
    b.Status
FROM Beds b
INNER JOIN Wards w ON b.WardId = w.WardId
WHERE b.Status = 1 -- Empty
    AND b.IsActive = 1
ORDER BY w.Name, b.BedNumber;
```

### B?nh Nhân ?ang N?m Vi?n
```sql
SELECT 
    a.AdmissionCode,
    p.FullName,
    w.Name AS WardName,
    b.BedNumber,
    a.AdmittedAt,
    DATEDIFF(day, a.AdmittedAt, GETDATE()) AS DaysInHospital,
    s.FullName AS AttendingDoctor
FROM Admissions a
INNER JOIN Patients p ON a.PatientId = p.PatientId
INNER JOIN Beds b ON a.BedId = b.BedId
INNER JOIN Wards w ON b.WardId = w.WardId
INNER JOIN Staffs s ON a.AttendingDoctorId = s.StaffId
WHERE a.Status = 1 -- Active
ORDER BY a.AdmittedAt;
```

### Doanh Thu Theo Ngày (7 ngày g?n nh?t)
```sql
SELECT 
    CAST(CreatedAt AS DATE) AS Date,
    COUNT(*) AS TotalInvoices,
    SUM(TotalAmount) AS TotalRevenue,
    SUM(InsuranceAmount) AS InsurancePaid,
    SUM(PatientAmount) AS PatientPaid,
    COUNT(CASE WHEN Status = 2 THEN 1 END) AS PaidInvoices,
    COUNT(CASE WHEN Status = 1 THEN 1 END) AS UnpaidInvoices
FROM Invoices
WHERE CreatedAt >= DATEADD(day, -7, GETDATE())
GROUP BY CAST(CreatedAt AS DATE)
ORDER BY Date DESC;
```

---

## ??? MAINTENANCE TASKS

### 1. Rebuild Fragmented Indexes
```sql
-- Ki?m tra fragmentation
SELECT 
    OBJECT_NAME(ips.object_id) AS TableName,
    i.name AS IndexName,
    ips.avg_fragmentation_in_percent
FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ips
INNER JOIN sys.indexes i ON ips.object_id = i.object_id AND ips.index_id = i.index_id
WHERE ips.avg_fragmentation_in_percent > 30
    AND i.name IS NOT NULL
ORDER BY ips.avg_fragmentation_in_percent DESC;

-- Rebuild indexes
ALTER INDEX ALL ON Encounters REBUILD;
ALTER INDEX ALL ON Orders REBUILD;
ALTER INDEX ALL ON Prescriptions REBUILD;
```

### 2. Update Statistics
```sql
-- Update statistics cho performance
UPDATE STATISTICS Encounters;
UPDATE STATISTICS Orders;
UPDATE STATISTICS Prescriptions;
UPDATE STATISTICS Invoices;
UPDATE STATISTICS Admissions;
```

### 3. Backup Database
```powershell
# Full backup
sqlcmd -S localhost -Q "BACKUP DATABASE HisDb TO DISK = 'C:\Backups\HisDb_Full.bak' WITH INIT"

# Differential backup
sqlcmd -S localhost -Q "BACKUP DATABASE HisDb TO DISK = 'C:\Backups\HisDb_Diff.bak' WITH DIFFERENTIAL, INIT"
```

### 4. Shrink Transaction Log (sau khi backup)
```sql
USE HisDb;
DBCC SHRINKFILE (HisDb_log, 1);
```

---

## ?? PERFORMANCE MONITORING

### Xem Top Slow Queries
```sql
SELECT TOP 10
    qs.execution_count,
    qs.total_elapsed_time / 1000000.0 AS TotalSeconds,
    qs.total_elapsed_time / qs.execution_count / 1000.0 AS AvgMs,
    SUBSTRING(qt.text, (qs.statement_start_offset/2)+1,
        ((CASE qs.statement_end_offset
            WHEN -1 THEN DATALENGTH(qt.text)
            ELSE qs.statement_end_offset
        END - qs.statement_start_offset)/2)+1) AS QueryText
FROM sys.dm_exec_query_stats qs
CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) qt
WHERE qt.dbid = DB_ID()
ORDER BY qs.total_elapsed_time DESC;
```

### Ki?m Tra Locks
```sql
SELECT 
    request_session_id AS SessionID,
    resource_type AS ResourceType,
    resource_database_id AS DatabaseID,
    resource_description AS Description,
    request_mode AS Mode,
    request_status AS Status
FROM sys.dm_tran_locks
WHERE resource_database_id = DB_ID();
```

---

## ?? SECURITY

### T?o Read-Only User
```sql
USE HisDb;
CREATE USER HisReader WITHOUT LOGIN;
ALTER ROLE db_datareader ADD MEMBER HisReader;
DENY INSERT, UPDATE, DELETE ON SCHEMA::dbo TO HisReader;
```

### Audit Trail Query
```sql
-- Xem ai ?ã làm gì (n?u có audit enabled)
SELECT 
    session_server_principal_name AS UserName,
    action_id,
    succeeded,
    statement,
    event_time
FROM sys.fn_get_audit_file('C:\SQLAudit\*', DEFAULT, DEFAULT)
WHERE database_name = 'HisDb'
ORDER BY event_time DESC;
```

---

## ?? SUPPORT

**Database Issues:**
- Check connection string in `appsettings.json`
- Verify SQL Server service is running
- Check firewall settings

**Migration Issues:**
```powershell
# Xem migrations hi?n t?i
cd HisMvc
dotnet ef migrations list

# Remove last migration n?u có l?i
dotnet ef migrations remove

# Apply migrations
dotnet ef database update
```

**Performance Issues:**
- Run index rebuild script
- Update statistics
- Check for long-running queries
- Monitor database size

---

## ?? REFERENCES

- **Schema Documentation:** `DATABASE_SCHEMA.md`
- **ER Diagram:** `DATABASE_ER_DIAGRAM.md`
- **Verification Script:** `verify-database-schema.sql`
- **Code Review:** `CODE_REVIEW_REPORT.md`

---

**Last Updated:** 2026-01-28  
**Database Version:** 1.0  
**Maintained By:** HIS Development Team
