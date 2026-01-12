INSERT INTO Patients (FullName, Phone, Gender)
VALUES (N'Nguy?n V?n A', '0999999999', 1)

DECLARE @pid INT = SCOPE_IDENTITY()

INSERT INTO Appointments
(Code, PatientId, DepartmentId, DoctorId, Date, TimeSlotId, Status, CreatedAt)
VALUES
('TEST-001', @pid, 1, 1, CAST(GETDATE() AS DATE), 1, 1, GETDATE())
