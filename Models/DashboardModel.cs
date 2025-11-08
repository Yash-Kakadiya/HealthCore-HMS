using System;
using System.Collections.Generic;

namespace HMS.Models
{
    // Helper Classes
    public class DepartmentStat
    {
        public string DepartmentName { get; set; }
        public int DoctorCount { get; set; }
        public int PatientCount { get; set; }
        public decimal DepartmentRevenue { get; set; }
    }

    public class RecentAppointment
    {
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public string AppointmentStatus { get; set; }
        public DateTime AppointmentDate { get; set; }
        public decimal TotalConsultedAmount { get; set; }
        public string DepartmentName { get; set; }
    }

    public class TopDoctor
    {
        public string DoctorName { get; set; }
        public int AppointmentCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public string DepartmentName { get; set; }
        public decimal CompletionRate { get; set; }
    }

    public class RecentActivity
    {
        public string ActivityType { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public DateTime ActivityDate { get; set; }
        public string Status { get; set; }
    }

    public class AgeGroupDistribution
    {
        public string AgeGroup { get; set; }
        public int PatientCount { get; set; }
    }

    public class DailyChartData
    {
        public DateTime ChartDate { get; set; }
        public int DayOfMonth { get; set; }
        public int ScheduledCount { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
        public decimal DailyRevenue { get; set; }
    }

    public class YearlyComparison
    {
        public int MonthNumber { get; set; }
        public string MonthName { get; set; }
        public int AppointmentCount { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int UniquePatients { get; set; }
    }

    public class HourlyDistribution
    {
        public int HourOfDay { get; set; }
        public int AppointmentCount { get; set; }
        public decimal CompletionRate { get; set; }
    }

    public class DepartmentPerformance
    {
        public string DepartmentName { get; set; }
        public int TotalPatients { get; set; }
        public int TotalAppointments { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal SuccessRate { get; set; }
        public int DoctorCount { get; set; }
    }

    // Main Dashboard ViewModel
    public class DashboardModel
    {
        // Core Metrics
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public int TodayAppointments { get; set; }
        public int TotalDepartments { get; set; }

        // Patient Metrics
        public int NewPatientsThisMonth { get; set; }
        public int NewPatientsThisWeek { get; set; }

        // Appointment Metrics
        public int TotalAppointmentsThisMonth { get; set; }
        public int CompletedAppointmentsThisMonth { get; set; }
        public int PendingAppointmentsThisMonth { get; set; }
        public int CancelledAppointmentsThisMonth { get; set; }

        // Revenue Metrics
        public decimal TotalRevenueThisMonth { get; set; }
        public decimal ClearedRevenueThisMonth { get; set; }
        public decimal PendingRevenueThisMonth { get; set; }
        public decimal AverageConsultationFee { get; set; }

        // Performance Metrics
        public decimal CompletionRate { get; set; }
        public decimal CancellationRate { get; set; }
        public decimal OccupancyRate { get; set; }

        // Lists and Collections
        public List<DepartmentStat> DepartmentStats { get; set; } = new List<DepartmentStat>();
        public List<RecentAppointment> RecentAppointments { get; set; } = new List<RecentAppointment>();
        public List<TopDoctor> TopDoctors { get; set; } = new List<TopDoctor>();
        public List<RecentActivity> RecentActivities { get; set; } = new List<RecentActivity>();
        public List<AgeGroupDistribution> AgeGroups { get; set; } = new List<AgeGroupDistribution>();

        // Chart Data
        public List<DailyChartData> DailyChartData { get; set; } = new List<DailyChartData>();
        public List<YearlyComparison> YearlyData { get; set; } = new List<YearlyComparison>();
        public List<HourlyDistribution> HourlyData { get; set; } = new List<HourlyDistribution>();
        public List<DepartmentPerformance> DepartmentPerformance { get; set; } = new List<DepartmentPerformance>();

        // Calculated Properties
        public decimal RevenueGrowthPercentage { get; set; }
        public decimal PatientGrowthPercentage { get; set; }
        public string BusiestHour { get; set; }
        public string TopPerformingDepartment { get; set; }
    }
}