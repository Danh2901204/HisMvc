SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;

DECLARE @kb INT = (SELECT TOP 1 DepartmentId FROM Departments WHERE Code = 'KB' ORDER BY CASE WHEN Name LIKE N'Khoa %' THEN 0 ELSE 1 END, LEN(Name) DESC, DepartmentId);
DECLARE @kb2 INT = (SELECT TOP 1 DepartmentId FROM Departments WHERE Code = 'KB' AND DepartmentId <> @kb ORDER BY DepartmentId DESC);
IF @kb IS NOT NULL AND @kb2 IS NOT NULL
BEGIN
    UPDATE Staffs SET DepartmentId = @kb WHERE DepartmentId = @kb2;
    UPDATE Appointments SET DepartmentId = @kb WHERE DepartmentId = @kb2;
    IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Encounters')
        UPDATE Encounters SET DepartmentId = @kb WHERE DepartmentId = @kb2;
    DELETE FROM Departments WHERE DepartmentId = @kb2;
END

DECLARE @noi INT = (SELECT TOP 1 DepartmentId FROM Departments WHERE Code = 'NOI' ORDER BY CASE WHEN Name LIKE N'Khoa %' THEN 0 ELSE 1 END, LEN(Name) DESC, DepartmentId);
DECLARE @noi2 INT = (SELECT TOP 1 DepartmentId FROM Departments WHERE Code = 'NOI' AND DepartmentId <> @noi ORDER BY DepartmentId DESC);
IF @noi IS NOT NULL AND @noi2 IS NOT NULL
BEGIN
    UPDATE Staffs SET DepartmentId = @noi WHERE DepartmentId = @noi2;
    UPDATE Appointments SET DepartmentId = @noi WHERE DepartmentId = @noi2;
    IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Encounters')
        UPDATE Encounters SET DepartmentId = @noi WHERE DepartmentId = @noi2;
    DELETE FROM Departments WHERE DepartmentId = @noi2;
END

DECLARE @tmh INT = (SELECT TOP 1 DepartmentId FROM Departments WHERE Code = 'TMH' ORDER BY CASE WHEN Name LIKE N'Khoa %' THEN 0 ELSE 1 END, LEN(Name) DESC, DepartmentId);
DECLARE @tmh2 INT = (SELECT TOP 1 DepartmentId FROM Departments WHERE Code = 'TMH' AND DepartmentId <> @tmh ORDER BY DepartmentId DESC);
IF @tmh IS NOT NULL AND @tmh2 IS NOT NULL
BEGIN
    UPDATE Staffs SET DepartmentId = @tmh WHERE DepartmentId = @tmh2;
    UPDATE Appointments SET DepartmentId = @tmh WHERE DepartmentId = @tmh2;
    IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Encounters')
        UPDATE Encounters SET DepartmentId = @tmh WHERE DepartmentId = @tmh2;
    DELETE FROM Departments WHERE DepartmentId = @tmh2;
END

UPDATE Departments SET Kind = 1
WHERE Code IN ('KB','NOI','NGOAI','SAN','NHI','TIM','TMH','MAT','RHM','DA','YHCT','NOITIET','TIEUHOA','HH','TK');

UPDATE Departments SET Kind = 3 WHERE Code IN ('XN','CDHA','HS','LAB');
UPDATE Departments SET Kind = 4 WHERE Code IN ('HSCC','TTHS','CC');
UPDATE Departments SET Kind = 2 WHERE Code IN ('CNTT','HANHCHINH','TCKT','DUOC');
