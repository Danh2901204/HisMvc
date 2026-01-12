-- ===============================================
-- S?A L?I "TABLE INVOICES ALREADY EXISTS"
-- ===============================================

-- B??c 1: Ki?m tra table Invoices có t?n t?i không
IF OBJECT_ID('Invoices', 'U') IS NOT NULL
    PRINT '? Table Invoices ?ã t?n t?i'
ELSE
    PRINT '? Table Invoices KHÔNG t?n t?i - Vui lòng ch?y Update-Database'

GO

-- B??c 2: Ki?m tra migration ?ã ???c ghi nh?n ch?a
IF EXISTS (
    SELECT 1 FROM __EFMigrationsHistory 
    WHERE MigrationId = '20260108020251_AddInvoice'
)
    PRINT '? Migration AddInvoice ?ã ???c ghi nh?n'
ELSE
BEGIN
    PRINT '? Migration AddInvoice CH?A ???c ghi nh?n'
    PRINT '?? ?ang thêm vào __EFMigrationsHistory...'
    
    -- Thêm migration vào history
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES ('20260108020251_AddInvoice', '9.0.1')
    
    PRINT '? ?Ã THÊM THÀNH CÔNG!'
END

GO

-- B??c 3: Ki?m tra l?i
PRINT ''
PRINT '========================================='
PRINT 'KI?M TRA K?T QU?:'
PRINT '========================================='

-- Li?t kê t?t c? migrations
SELECT 
    ROW_NUMBER() OVER (ORDER BY MigrationId) AS [#],
    MigrationId AS [Migration],
    ProductVersion AS [Version]
FROM __EFMigrationsHistory
ORDER BY MigrationId

PRINT ''
PRINT '========================================='
PRINT 'K?T QU?: Migration AddInvoice'
PRINT '========================================='

IF EXISTS (
    SELECT 1 FROM __EFMigrationsHistory 
    WHERE MigrationId = '20260108020251_AddInvoice'
)
    PRINT '? ?Ã ???C GHI NH?N - S?A L?I THÀNH CÔNG!'
ELSE
    PRINT '? CH?A ???C GHI NH?N - VUI LÒNG KI?M TRA L?I'

GO

-- ===============================================
-- H??NG D?N S? D?NG:
-- ===============================================
-- 1. D?ng debugger (Shift+F5)
-- 2. M? SQL Server Object Explorer
-- 3. Right-click vào database "HisMvcDb"
-- 4. Ch?n "New Query..."
-- 5. Copy toàn b? script này vào
-- 6. Click "Execute" (ho?c Ctrl+Shift+E)
-- 7. ??c k?t qu? - Ph?i th?y "? ?Ã ???C GHI NH?N"
-- 8. Ch?y l?i app (F5)
-- 9. L?i ?ã bi?n m?t! ??
-- ===============================================
