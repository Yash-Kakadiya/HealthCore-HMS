USE HMS_DB;
GO

-- Disable all constraints
EXEC sp_msforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all";

-- Delete data in child → parent order
DELETE FROM Appointment;
DELETE FROM DoctorDepartment;
DELETE FROM Patient;
DELETE FROM Doctor;
DELETE FROM Department;
DELETE FROM [User];

-- Reset identity columns manually
DBCC CHECKIDENT ('Appointment', RESEED, 0);
DBCC CHECKIDENT ('DoctorDepartment', RESEED, 0);
DBCC CHECKIDENT ('Patient', RESEED, 0);
DBCC CHECKIDENT ('Doctor', RESEED, 0);
DBCC CHECKIDENT ('Department', RESEED, 0);
DBCC CHECKIDENT ('[User]', RESEED, 0);

-- Re-enable constraints
EXEC sp_msforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all";
