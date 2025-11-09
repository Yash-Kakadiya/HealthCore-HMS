using HMS.Models;
using HMS.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace HMS.Controllers
{
    [CheckAccess]
    public class DashboardController : Controller
    {
        private readonly IConfiguration _configuration;

        public DashboardController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var viewModel = new DashboardModel();
            string connectionString = _configuration.GetConnectionString("MyConnectionString");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Get all metrics from main procedure
                using (SqlCommand cmd = new SqlCommand("PR_Dashboard_GetAllMetrics", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        // Result 1: Core Metrics
                        if (reader.Read())
                        {
                            viewModel.TotalPatients = Convert.ToInt32(reader["TotalPatients"]);
                            viewModel.TotalDoctors = Convert.ToInt32(reader["TotalDoctors"]);
                            viewModel.TodayAppointments = Convert.ToInt32(reader["TodayAppointments"]);
                            viewModel.TotalDepartments = Convert.ToInt32(reader["TotalDepartments"]);

                            viewModel.NewPatientsThisMonth = Convert.ToInt32(reader["NewPatientsThisMonth"]);
                            viewModel.NewPatientsThisWeek = Convert.ToInt32(reader["NewPatientsThisWeek"]);

                            viewModel.TotalAppointmentsThisMonth = Convert.ToInt32(reader["TotalAppointmentsThisMonth"]);
                            viewModel.CompletedAppointmentsThisMonth = reader["CompletedAppointmentsThisMonth"] != DBNull.Value
    ? Convert.ToInt32(reader["CompletedAppointmentsThisMonth"])
    : 0;
                            viewModel.PendingAppointmentsThisMonth = reader["PendingAppointmentsThisMonth"] != DBNull.Value
    ? Convert.ToInt32(reader["PendingAppointmentsThisMonth"])
    : 0;
                            viewModel.CancelledAppointmentsThisMonth = reader["CancelledAppointmentsThisMonth"] != DBNull.Value
    ? Convert.ToInt32(reader["CancelledAppointmentsThisMonth"])
    : 0;

                            viewModel.TotalRevenueThisMonth = Convert.ToDecimal(reader["TotalRevenueThisMonth"]);
                            viewModel.ClearedRevenueThisMonth = Convert.ToDecimal(reader["ClearedRevenueThisMonth"]);
                            viewModel.PendingRevenueThisMonth = viewModel.TotalRevenueThisMonth - viewModel.ClearedRevenueThisMonth;
                            viewModel.AverageConsultationFee = Convert.ToDecimal(reader["AverageConsultationFee"]);

                            viewModel.CompletionRate = Convert.ToDecimal(reader["CompletionRate"]);
                            viewModel.CancellationRate = Convert.ToDecimal(reader["CancellationRate"]);
                            viewModel.OccupancyRate = Convert.ToDecimal(reader["OccupancyRate"]);
                        }

                        // Result 2: Department Stats
                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                viewModel.DepartmentStats.Add(new DepartmentStat
                                {
                                    DepartmentName = reader["DepartmentName"].ToString(),
                                    DoctorCount = Convert.ToInt32(reader["DoctorCount"]),
                                    PatientCount = Convert.ToInt32(reader["PatientCount"]),
                                    DepartmentRevenue = Convert.ToDecimal(reader["DepartmentRevenue"])
                                });
                            }
                        }

                        // Result 3: Recent Appointments
                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                viewModel.RecentAppointments.Add(new RecentAppointment
                                {
                                    PatientName = reader["PatientName"].ToString(),
                                    DoctorName = reader["DoctorName"].ToString(),
                                    AppointmentStatus = reader["AppointmentStatus"].ToString(),
                                    AppointmentDate = Convert.ToDateTime(reader["AppointmentDate"]),
                                    TotalConsultedAmount = Convert.ToDecimal(reader["TotalConsultedAmount"]),
                                    DepartmentName = reader["DepartmentName"]?.ToString() ?? "General"
                                });
                            }
                        }

                        // Result 4: Top Doctors
                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                viewModel.TopDoctors.Add(new TopDoctor
                                {
                                    DoctorName = reader["DoctorName"].ToString(),
                                    AppointmentCount = Convert.ToInt32(reader["AppointmentCount"]),
                                    TotalRevenue = Convert.ToDecimal(reader["TotalRevenue"]),
                                    DepartmentName = reader["DepartmentName"]?.ToString() ?? "General",
                                    CompletionRate = Convert.ToDecimal(reader["CompletionRate"])
                                });
                            }
                        }

                        // Result 5: Recent Activities
                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                viewModel.RecentActivities.Add(new RecentActivity
                                {
                                    ActivityType = reader["ActivityType"].ToString(),
                                    Action = reader["Action"].ToString(),
                                    Description = reader["Description"].ToString(),
                                    ActivityDate = Convert.ToDateTime(reader["ActivityDate"]),
                                    Status = reader["Status"].ToString()
                                });
                            }
                        }

                        // Result 6: Age Group Distribution
                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                viewModel.AgeGroups.Add(new AgeGroupDistribution
                                {
                                    AgeGroup = reader["AgeGroup"].ToString(),
                                    PatientCount = Convert.ToInt32(reader["PatientCount"])
                                });
                            }
                        }
                    }
                }

                // Calculate additional metrics
                viewModel.TopPerformingDepartment = viewModel.DepartmentStats.FirstOrDefault()?.DepartmentName ?? "N/A";

                // Calculate growth percentages (mock data for now)
                viewModel.RevenueGrowthPercentage = 12.5m; // Would need previous month data
                viewModel.PatientGrowthPercentage = 8.3m; // Would need previous month data
            }

            return View(viewModel);
        }

        [HttpGet]
        public JsonResult GetAppointmentChartData()
        {
            var chartData = new
            {
                scheduled = new List<int>(),
                completed = new List<int>(),
                cancelled = new List<int>(),
                revenue = new List<decimal>(),
                days = new List<int>()
            };

            string connectionString = _configuration.GetConnectionString("MyConnectionString");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("PR_Dashboard_GetMonthlyChartData", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            chartData.days.Add(Convert.ToInt32(reader["DayOfMonth"]));
                            chartData.scheduled.Add(Convert.ToInt32(reader["ScheduledCount"]));
                            chartData.completed.Add(Convert.ToInt32(reader["CompletedCount"]));
                            chartData.cancelled.Add(Convert.ToInt32(reader["CancelledCount"]));
                            chartData.revenue.Add(Convert.ToDecimal(reader["DailyRevenue"]));
                        }
                    }
                }
            }
            return Json(chartData);
        }

        [HttpGet]
        public JsonResult GetYearlyComparison()
        {
            var yearlyData = new
            {
                months = new List<string>(),
                appointments = new List<int>(),
                revenue = new List<decimal>(),
                patients = new List<int>()
            };

            string connectionString = _configuration.GetConnectionString("MyConnectionString");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("PR_Dashboard_GetYearlyComparison", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            yearlyData.months.Add(reader["MonthName"].ToString());
                            yearlyData.appointments.Add(Convert.ToInt32(reader["AppointmentCount"]));
                            yearlyData.revenue.Add(Convert.ToDecimal(reader["MonthlyRevenue"]));
                            yearlyData.patients.Add(Convert.ToInt32(reader["UniquePatients"]));
                        }
                    }
                }
            }
            return Json(yearlyData);
        }

        [HttpGet]
        public JsonResult GetHourlyDistribution()
        {
            var hourlyData = new
            {
                hours = new List<int>(),
                appointments = new List<int>(),
                completionRate = new List<decimal>()
            };

            string connectionString = _configuration.GetConnectionString("MyConnectionString");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("PR_Dashboard_GetHourlyDistribution", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            hourlyData.hours.Add(Convert.ToInt32(reader["HourOfDay"]));
                            hourlyData.appointments.Add(Convert.ToInt32(reader["AppointmentCount"]));
                            hourlyData.completionRate.Add(Convert.ToDecimal(reader["CompletionRate"]));
                        }
                    }
                }
            }
            return Json(hourlyData);
        }

        [HttpGet]
        public JsonResult GetDepartmentPerformance()
        {
            var performanceData = new List<object>();

            string connectionString = _configuration.GetConnectionString("MyConnectionString");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("PR_Dashboard_GetDepartmentPerformance", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            performanceData.Add(new
                            {
                                department = reader["DepartmentName"].ToString(),
                                patients = Convert.ToInt32(reader["TotalPatients"]),
                                appointments = Convert.ToInt32(reader["TotalAppointments"]),
                                revenue = Convert.ToDecimal(reader["TotalRevenue"]),
                                successRate = Convert.ToDecimal(reader["SuccessRate"]),
                                doctors = Convert.ToInt32(reader["DoctorCount"])
                            });
                        }
                    }
                }
            }
            return Json(performanceData);
        }
    }
}