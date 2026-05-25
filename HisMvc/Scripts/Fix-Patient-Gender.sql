-- =============================================================================
-- Fix Patient Gender — HIS_MVC_DB
-- Mapping (HisMvc.Entities.Gender):
--   0 = Unknown (Chua ro)
--   1 = Male      (Nam)
--   2 = Female    (Nu)
--   3 = Other     (Khac)
--
-- Chay:
--   sqlcmd -S localhost -d HIS_MVC_DB -I -i Fix-Patient-Gender.sql
--   (flag -I bat buoc vi bang Patients co filtered index)
--
-- Mac dinh: dry-run (ROLLBACK). Doi COMMIT TRANSACTION de luu that.
-- =============================================================================

USE HIS_MVC_DB;
GO

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

-- Ky tu "i" co dau (i) — dung cho pattern "Thi" / "Thị" trong ho ten Viet
DECLARE @iDot NCHAR(1) = NCHAR(7883);

PRINT '========== THONG KE TRUOC KHI SUA ==========';
SELECT Gender, COUNT(*) AS SoLuong
FROM Patients
GROUP BY Gender
ORDER BY Gender;

PRINT '';
PRINT '--- Se sua: co "Thi" trong ten, chua phai Nu (2) ---';
SELECT PatientId, FullName, Gender
FROM Patients
WHERE Gender <> 2
  AND FullName LIKE N'%Th' + @iDot + N'%'
ORDER BY PatientId;

BEGIN TRANSACTION;

DECLARE @FixedFemale INT = 0;

-- Quy tac chinh: "Thi" (ten dem nu) -> Nu (2)
-- Khong match "Trinh" (Tr+i) vi pattern la "Th"+i
UPDATE Patients
SET Gender = 2
WHERE Gender <> 2
  AND FullName LIKE N'%Th' + @iDot + N'%';

SET @FixedFemale = @@ROWCOUNT;

PRINT '';
PRINT '========== KET QUA SUA ==========';
PRINT 'Da cap nhat sang Nu (2): ' + CAST(@FixedFemale AS VARCHAR(10));

PRINT '';
PRINT '--- Thong ke sau khi sua (chua COMMIT) ---';
SELECT Gender, COUNT(*) AS SoLuong
FROM Patients
GROUP BY Gender
ORDER BY Gender;

PRINT '';
PRINT '--- Con lai: Ten co Thi nhung chua phai Nu ---';
SELECT PatientId, FullName, Gender
FROM Patients
WHERE Gender <> 2
  AND FullName LIKE N'%Th' + @iDot + N'%';

-- =============================================================================
-- XAC NHAN: Neu dung -> bo comment dong COMMIT, comment dong ROLLBACK
-- =============================================================================
-- COMMIT TRANSACTION;
ROLLBACK TRANSACTION;

PRINT '';
PRINT 'Da ROLLBACK (dry-run). Sua file: COMMIT TRANSACTION de luu that.';
GO

-- =============================================================================
-- (TUY CHON) Ten nu pho bien khong co "Thi" — xem truoc, chinh tay tung dong
-- Vi du: Mai Hong, Hồng, Lan... — KHONG tu dong vi de nham (Linh, Hoa...)
-- =============================================================================
/*
SET QUOTED_IDENTIFIER ON;
USE HIS_MVC_DB;

SELECT PatientId, FullName, Gender
FROM Patients
WHERE Gender = 1
  AND PatientId BETWEEN 21 AND 220
  AND FullName NOT LIKE N'%Th' + NCHAR(7883) + N'%'
ORDER BY PatientId;
*/

-- =============================================================================
-- (TUY CHON) Sua thu cong mot ban ghi
-- =============================================================================
/*
UPDATE Patients SET Gender = 2 WHERE PatientId = 22;  -- Tran Mai Hong
UPDATE Patients SET Gender = 2 WHERE PatientId = 23; -- ...
*/

-- =============================================================================
-- Kiem tra hien thi
-- =============================================================================
/*
SELECT PatientId, FullName, Gender,
       CASE Gender
           WHEN 0 THEN N'Chua ro'
           WHEN 1 THEN N'Nam'
           WHEN 2 THEN N'Nu'
           WHEN 3 THEN N'Khac'
       END AS GioiTinhText
FROM Patients
WHERE PatientId IN (4, 22, 27, 104, 222);
*/
