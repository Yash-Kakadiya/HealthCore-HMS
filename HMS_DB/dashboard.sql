-- =============================================
-- PR_Dashboard_GetAllMetrics (hardened + SARGable)
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[PR_Dashboard_GetAllMetrics]
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Today              date = CONVERT(date, GETDATE());
    DECLARE @FirstOfMonth       date = DATEFROMPARTS(YEAR(@Today), MONTH(@Today), 1);
    DECLARE @FirstOfNextMonth   date = DATEADD(month, 1, @FirstOfMonth);

    ;WITH ActivePatients AS (
        SELECT COUNT_BIG(*) AS TotalPatients
        FROM dbo.Patient WITH (NOLOCK)
        WHERE IsActive = 1
    ), ActiveDoctors AS (
        SELECT COUNT_BIG(*) AS TotalDoctors
        FROM dbo.Doctor WITH (NOLOCK)
        WHERE IsActive = 1
    ), ActiveDepartments AS (
        SELECT COUNT_BIG(*) AS TotalDepartments
        FROM dbo.Department WITH (NOLOCK)
        WHERE IsActive = 1
    ), TodayAppts AS (
        SELECT COUNT_BIG(*) AS TodayAppointments
        FROM dbo.Appointment WITH (NOLOCK)
        WHERE CONVERT(date, AppointmentDate) = @Today AND AppointmentStatus <> 'Cancelled'
    ), NewPatients AS (
        SELECT 
            SUM(CASE WHEN Created >= @FirstOfMonth AND Created < @FirstOfNextMonth THEN 1 ELSE 0 END) AS NewPatientsThisMonth,
            SUM(CASE WHEN Created >= DATEADD(day, -7, @Today) THEN 1 ELSE 0 END)                           AS NewPatientsThisWeek
        FROM dbo.Patient WITH (NOLOCK)
    ), ApptsThisMonth AS (
        SELECT 
            COUNT_BIG(*)                                                   AS TotalAppointmentsThisMonth,
            SUM(CASE WHEN AppointmentStatus = 'Confirmed' THEN 1 ELSE 0 END) AS CompletedAppointmentsThisMonth,
            SUM(CASE WHEN AppointmentStatus = 'Pending'   THEN 1 ELSE 0 END) AS PendingAppointmentsThisMonth,
            SUM(CASE WHEN AppointmentStatus = 'Cancelled' THEN 1 ELSE 0 END) AS CancelledAppointmentsThisMonth,
            SUM(TotalConsultedAmount)                                      AS TotalRevenueThisMonth,
            SUM(CASE WHEN AppointmentStatus = 'Confirmed' THEN TotalConsultedAmount ELSE 0 END) AS ClearedRevenueThisMonth,
            AVG(CASE WHEN AppointmentStatus = 'Confirmed' THEN NULLIF(TotalConsultedAmount,0) END) AS AverageConsultationFee
        FROM dbo.Appointment WITH (NOLOCK)
        WHERE AppointmentDate >= @FirstOfMonth AND AppointmentDate < @FirstOfNextMonth
    ), RateCalc AS (
        SELECT 
            CAST(ISNULL(100.0 * NULLIF(CompletedAppointmentsThisMonth,0) / NULLIF(TotalAppointmentsThisMonth,0), 0) AS DECIMAL(5,2)) AS CompletionRate,
            CAST(ISNULL(100.0 * NULLIF(CancelledAppointmentsThisMonth,0) / NULLIF(TotalAppointmentsThisMonth,0), 0) AS DECIMAL(5,2)) AS CancellationRate
        FROM ApptsThisMonth
    ), Occupancy AS (
        SELECT 
            CAST(
                ISNULL(
                    100.0 * 0.5 * 
                    (SELECT COUNT_BIG(*) FROM dbo.Appointment WITH (NOLOCK)
                     WHERE AppointmentDate >= @FirstOfMonth AND AppointmentDate < @FirstOfNextMonth AND AppointmentStatus <> 'Cancelled')
                    / NULLIF((SELECT TotalDoctors FROM ActiveDoctors) * 8.0 * 22.0, 0)
                , 0) AS DECIMAL(5,2)
            ) AS OccupancyRate
    )
    SELECT 
        ap.TotalPatients,
        ad.TotalDoctors,
        ta.TodayAppointments,
        adep.TotalDepartments,
        np.NewPatientsThisMonth,
        np.NewPatientsThisWeek,
        atm.TotalAppointmentsThisMonth,
        atm.CompletedAppointmentsThisMonth,
        atm.PendingAppointmentsThisMonth,
        atm.CancelledAppointmentsThisMonth,
        ISNULL(atm.TotalRevenueThisMonth, 0)  AS TotalRevenueThisMonth,
        ISNULL(atm.ClearedRevenueThisMonth, 0) AS ClearedRevenueThisMonth,
        CAST(ISNULL(atm.AverageConsultationFee, 0) AS DECIMAL(18,2)) AS AverageConsultationFee,
        rc.CompletionRate,
        rc.CancellationRate,
        o.OccupancyRate
    FROM ActivePatients ap
    CROSS JOIN ActiveDoctors ad
    CROSS JOIN ActiveDepartments adep
    CROSS JOIN TodayAppts ta
    CROSS JOIN NewPatients np
    CROSS JOIN ApptsThisMonth atm
    CROSS JOIN RateCalc rc
    CROSS JOIN Occupancy o;

    -- 2. Department Statistics with Patient Count (current month)
    SELECT
        d.DepartmentName,
        COUNT(DISTINCT dd.DoctorID) AS DoctorCount,
        COUNT(DISTINCT a.PatientID) AS PatientCount,
        ISNULL(SUM(a.TotalConsultedAmount), 0) AS DepartmentRevenue
    FROM dbo.Department d WITH (NOLOCK)
    LEFT JOIN dbo.DoctorDepartment dd WITH (NOLOCK) ON d.DepartmentID = dd.DepartmentID
    LEFT JOIN dbo.Appointment a WITH (NOLOCK)
        ON dd.DoctorID = a.DoctorID
       AND a.AppointmentDate >= @FirstOfMonth AND a.AppointmentDate < @FirstOfNextMonth
    WHERE d.IsActive = 1
    GROUP BY d.DepartmentID, d.DepartmentName
    ORDER BY DepartmentRevenue DESC;

    -- 3. Upcoming Appointments (Next 5)
    SELECT TOP (5)
        p.PatientName AS PatientName,
        dr.DoctorName AS DoctorName,
        a.AppointmentStatus,
        a.AppointmentDate,
        a.TotalConsultedAmount,
        dep.DepartmentName
    FROM dbo.Appointment a WITH (NOLOCK)
    INNER JOIN dbo.Patient p  WITH (NOLOCK) ON a.PatientID = p.PatientID
    INNER JOIN dbo.Doctor dr  WITH (NOLOCK) ON a.DoctorID = dr.DoctorID
    LEFT JOIN dbo.DoctorDepartment dd WITH (NOLOCK) ON dr.DoctorID = dd.DoctorID
    LEFT JOIN dbo.Department dep WITH (NOLOCK) ON dd.DepartmentID = dep.DepartmentID
    WHERE a.AppointmentDate >= GETDATE() AND a.AppointmentStatus = 'Pending'
    ORDER BY a.AppointmentDate ASC;

    -- 4. Top Performing Doctors (by appointments this month)
    SELECT TOP (5)
        dr.DoctorName AS DoctorName,
        COUNT(a.AppointmentID) AS AppointmentCount,
        ISNULL(SUM(a.TotalConsultedAmount), 0) AS TotalRevenue,
        dep.DepartmentName,
        CAST(AVG(CASE WHEN a.AppointmentStatus = 'Confirmed' THEN 100.0 ELSE 0 END) AS DECIMAL(5,2)) AS CompletionRate
    FROM dbo.Doctor dr WITH (NOLOCK)
    LEFT JOIN dbo.Appointment a WITH (NOLOCK)
           ON dr.DoctorID = a.DoctorID
          AND a.AppointmentDate >= @FirstOfMonth AND a.AppointmentDate < @FirstOfNextMonth
    LEFT JOIN dbo.DoctorDepartment dd WITH (NOLOCK) ON dr.DoctorID = dd.DoctorID
    LEFT JOIN dbo.Department dep WITH (NOLOCK) ON dd.DepartmentID = dep.DepartmentID
    WHERE dr.IsActive = 1
    GROUP BY dr.DoctorID, dr.DoctorName, dep.DepartmentName
    ORDER BY AppointmentCount DESC;

    -- 5. Recent Activities (Last 10 appointments/updates)
    SELECT TOP (10)
        'Appointment' AS ActivityType,
        CASE 
            WHEN a.AppointmentStatus = 'Confirmed' THEN 'Completed'
            WHEN a.AppointmentStatus = 'Pending'   THEN 'Scheduled'
            WHEN a.AppointmentStatus = 'Cancelled' THEN 'Cancelled'
            ELSE 'Updated'
        END AS Action,
        p.PatientName + ' - ' + dr.DoctorName AS Description,
        a.Modified AS ActivityDate,
        a.AppointmentStatus AS Status
    FROM dbo.Appointment a WITH (NOLOCK)
    INNER JOIN dbo.Patient p WITH (NOLOCK) ON a.PatientID = p.PatientID
    INNER JOIN dbo.Doctor  dr WITH (NOLOCK) ON a.DoctorID  = dr.DoctorID
    WHERE a.Modified IS NOT NULL
    ORDER BY a.Modified DESC;

    -- 6. Age Group Distribution of Patients
    SELECT
        CASE 
            WHEN DATEDIFF(year, DateOfBirth, @Today) < 18 THEN 'Children (0-17)'
            WHEN DATEDIFF(year, DateOfBirth, @Today) BETWEEN 18 AND 30 THEN 'Young Adults (18-30)'
            WHEN DATEDIFF(year, DateOfBirth, @Today) BETWEEN 31 AND 50 THEN 'Adults (31-50)'
            WHEN DATEDIFF(year, DateOfBirth, @Today) BETWEEN 51 AND 65 THEN 'Middle Age (51-65)'
            ELSE 'Seniors (65+)'
        END AS AgeGroup,
        COUNT(*) AS PatientCount,
        MIN(CASE 
            WHEN DATEDIFF(year, DateOfBirth, @Today) < 18 THEN 1
            WHEN DATEDIFF(year, DateOfBirth, @Today) BETWEEN 18 AND 30 THEN 2
            WHEN DATEDIFF(year, DateOfBirth, @Today) BETWEEN 31 AND 50 THEN 3
            WHEN DATEDIFF(year, DateOfBirth, @Today) BETWEEN 51 AND 65 THEN 4
            ELSE 5
        END) AS GroupOrder
    FROM dbo.Patient WITH (NOLOCK)
    WHERE IsActive = 1 AND DateOfBirth IS NOT NULL
    GROUP BY 
        CASE 
            WHEN DATEDIFF(year, DateOfBirth, @Today) < 18 THEN 'Children (0-17)'
            WHEN DATEDIFF(year, DateOfBirth, @Today) BETWEEN 18 AND 30 THEN 'Young Adults (18-30)'
            WHEN DATEDIFF(year, DateOfBirth, @Today) BETWEEN 31 AND 50 THEN 'Adults (31-50)'
            WHEN DATEDIFF(year, DateOfBirth, @Today) BETWEEN 51 AND 65 THEN 'Middle Age (51-65)'
            ELSE 'Seniors (65+)'
        END
    ORDER BY GroupOrder;
END
GO

-- =============================================
-- PR_Dashboard_GetMonthlyChartData (SARGable)
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[PR_Dashboard_GetMonthlyChartData]
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Today            date = CONVERT(date, GETDATE());
    DECLARE @StartDate        date = DATEFROMPARTS(YEAR(@Today), MONTH(@Today), 1);
    DECLARE @EndDateInclusive date = EOMONTH(@Today);

    ;WITH DateSeries AS (
        SELECT @StartDate AS ChartDate
        UNION ALL
        SELECT DATEADD(day, 1, ChartDate)
        FROM DateSeries
        WHERE ChartDate < @EndDateInclusive
    )
    SELECT
        ds.ChartDate,
        DAY(ds.ChartDate) AS DayOfMonth,
        SUM(CASE WHEN a.AppointmentStatus = 'Pending'   THEN 1 ELSE 0 END) AS ScheduledCount,
        SUM(CASE WHEN a.AppointmentStatus = 'Confirmed' THEN 1 ELSE 0 END) AS CompletedCount,
        SUM(CASE WHEN a.AppointmentStatus = 'Cancelled' THEN 1 ELSE 0 END) AS CancelledCount,
        ISNULL(SUM(a.TotalConsultedAmount), 0) AS DailyRevenue
    FROM DateSeries ds
    LEFT JOIN dbo.Appointment a WITH (NOLOCK)
           ON CONVERT(date, a.AppointmentDate) = ds.ChartDate
    GROUP BY ds.ChartDate
    ORDER BY ds.ChartDate ASC
    OPTION (MAXRECURSION 0);
END
GO

-- =============================================
-- PR_Dashboard_GetYearlyComparison
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[PR_Dashboard_GetYearlyComparison]
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StartOfYear date = DATEFROMPARTS(YEAR(GETDATE()), 1, 1);
    DECLARE @StartNext   date = DATEADD(year, 1, @StartOfYear);

    WITH MonthlyData AS (
        SELECT 
            MONTH(AppointmentDate) AS MonthNumber,
            DATENAME(MONTH, AppointmentDate) AS MonthName,
            COUNT_BIG(*) AS AppointmentCount,
            ISNULL(SUM(TotalConsultedAmount), 0) AS MonthlyRevenue,
            COUNT(DISTINCT PatientID) AS UniquePatients
        FROM dbo.Appointment WITH (NOLOCK)
        WHERE AppointmentDate >= @StartOfYear AND AppointmentDate < @StartNext
        GROUP BY MONTH(AppointmentDate), DATENAME(MONTH, AppointmentDate)
    )
    SELECT 
        MonthNumber,
        MonthName,
        AppointmentCount,
        MonthlyRevenue,
        UniquePatients
    FROM MonthlyData
    ORDER BY MonthNumber;
END
GO

-- =============================================
-- PR_Dashboard_GetHourlyDistribution
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[PR_Dashboard_GetHourlyDistribution]
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Today        date = CONVERT(date, GETDATE());
    DECLARE @FirstOfMonth date = DATEFROMPARTS(YEAR(@Today), MONTH(@Today), 1);
    DECLARE @FirstNext    date = DATEADD(month, 1, @FirstOfMonth);

    SELECT 
        DATEPART(hour, AppointmentDate) AS HourOfDay,
        COUNT(*) AS AppointmentCount,
        AVG(CAST(CASE WHEN AppointmentStatus = 'Confirmed' THEN 1 ELSE 0 END AS float)) * 100 AS CompletionRate
    FROM dbo.Appointment WITH (NOLOCK)
    WHERE AppointmentDate >= @FirstOfMonth AND AppointmentDate < @FirstNext
    GROUP BY DATEPART(hour, AppointmentDate)
    ORDER BY HourOfDay;
END
GO

-- =============================================
-- PR_Dashboard_GetDepartmentPerformance
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[PR_Dashboard_GetDepartmentPerformance]
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Today        date = CONVERT(date, GETDATE());
    DECLARE @FirstOfMonth date = DATEFROMPARTS(YEAR(@Today), MONTH(@Today), 1);
    DECLARE @FirstNext    date = DATEADD(month, 1, @FirstOfMonth);

    SELECT 
        d.DepartmentName,
        COUNT(DISTINCT a.PatientID) AS TotalPatients,
        COUNT(a.AppointmentID)      AS TotalAppointments,
        ISNULL(SUM(a.TotalConsultedAmount), 0) AS TotalRevenue,
        CAST(AVG(CASE WHEN a.AppointmentStatus = 'Confirmed' THEN 100.0 ELSE 0 END) AS DECIMAL(5,2)) AS SuccessRate,
        COUNT(DISTINCT dd.DoctorID) AS DoctorCount
    FROM dbo.Department d WITH (NOLOCK)
    LEFT JOIN dbo.DoctorDepartment dd WITH (NOLOCK) ON d.DepartmentID = dd.DepartmentID
    LEFT JOIN dbo.Appointment a WITH (NOLOCK)
           ON dd.DoctorID = a.DoctorID
          AND a.AppointmentDate >= @FirstOfMonth AND a.AppointmentDate < @FirstNext
    WHERE d.IsActive = 1
    GROUP BY d.DepartmentID, d.DepartmentName
    ORDER BY TotalRevenue DESC;
END
GO

-- Suggested supporting indexes (create as needed)
-- CREATE INDEX IX_Appointment_Date_Status ON dbo.Appointment (AppointmentDate, AppointmentStatus) INCLUDE (TotalConsultedAmount, PatientID, DoctorID, Modified);
-- CREATE INDEX IX_Patient_CreatedDate ON dbo.Patient (Created) INCLUDE (IsActive, DateOfBirth);
-- CREATE INDEX IX_Doctor_IsActive ON dbo.Doctor (IsActive);
-- CREATE INDEX IX_Department_IsActive ON dbo.Department (IsActive);
