using ClosedXML.Excel;
using HMS.Models;
using HMS.Utils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;

//public int AppointmentID { get; set; }
//public int AppointmentID { get; set; }
//public int PatientID { get; set; }
//public DateTime AppointmentDate { get; set; }
//public string AppointmentStatus { get; set; }
//public string Description { get; set; }
//public string SpecialRemarks { get; set; }
//public DateTime Created { get; set; }
//public DateTime Modified { get; set; }
//public int UserID { get; set; }
//public decimal? TotalConsultedAmount { get; set; }

namespace HMS.Controllers
{
    [CheckAccess]
    public class AppointmentController : Controller
    {
        // The AppointmentController class is a controller in an ASP.NET Core application that handles appointment-related actions.


        #region configuration

        private readonly IConfiguration _configuration;

        public AppointmentController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #endregion

        #region AppointmentList

        [HttpGet]
        public IActionResult AppointmentList(string status = null, string dateRange = null, string doctorName = null, decimal? minAmount = null, decimal? maxAmount = null)
        {
            string ConnectionString = this._configuration.GetConnectionString(name: "MyConnectionString");

            using SqlConnection sqlConnection = new SqlConnection(ConnectionString);

            sqlConnection.Open();

            SqlCommand command = sqlConnection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;

            command.CommandText = "PR_Appointment_SelectAll";


            command.Parameters.AddWithValue("@Status", (object)status ?? DBNull.Value);
            command.Parameters.AddWithValue("@DoctorName", (object)doctorName ?? DBNull.Value);
            command.Parameters.AddWithValue("@MinAmount", (object)minAmount ?? DBNull.Value);
            command.Parameters.AddWithValue("@MaxAmount", (object)maxAmount ?? DBNull.Value);


            DateTime? startDate = null;
            DateTime? endDate = null;

            if (!string.IsNullOrEmpty(dateRange))
            {
                try
                {
                    var dates = dateRange.Split(" to ");
                    if (dates.Length == 2)
                    {
                        if (DateTime.TryParse(dates[0].Trim(), out DateTime start))
                            startDate = start;
                        if (DateTime.TryParse(dates[1].Trim(), out DateTime end))
                            endDate = end.Date.AddDays(1).AddTicks(-1);
                    }
                    else if (dates.Length == 1)
                    {
                        if (DateTime.TryParse(dates[0].Trim(), out DateTime singleDate))
                        {
                            startDate = singleDate.Date;
                            endDate = singleDate.Date.AddDays(1).AddTicks(-1);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the error if needed
                }
            }

            command.Parameters.AddWithValue("@StartDate", (object)startDate ?? DBNull.Value);
            command.Parameters.AddWithValue("@EndDate", (object)endDate ?? DBNull.Value);

            using SqlDataReader reader = command.ExecuteReader();

            DataTable table = new DataTable();

            table.Load(reader);

            return View(table);
        }

        #endregion

        #region AppointmentDelete

        [HttpGet]
        public IActionResult AppointmentDelete(string enAppointmentID)
        {
            try
            {
                var decodedId = HttpUtility.UrlDecode(enAppointmentID);
                int AppointmentID = Convert.ToInt32(UrlEncryptDecrypt.Decrypt(decodedId));

                string ConnectionString = this._configuration.GetConnectionString("MyConnectionString");
                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    sqlConnection.Open();
                    SqlCommand command = sqlConnection.CreateCommand();
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "PR_Appointment_Delete";
                    command.Parameters.Add("@AppointmentID", SqlDbType.Int).Value = AppointmentID;
                    command.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Appointment deleted successfully 🗑️.";
                TempData["SuccessType"] = "delete";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Could not cancel the appointment.";
            }

            return RedirectToAction("AppointmentList");
        }
        #endregion

        #region DeleteSelectedAppointments
        [HttpPost]
        public IActionResult DeleteSelectedAppointments([FromForm] List<string> selectedAppointmentIds)
        {
            if (selectedAppointmentIds == null || selectedAppointmentIds.Count == 0)
            {
                TempData["ErrorMessage"] = "No appointments selected for deletion.";
                return RedirectToAction("AppointmentList");
            }

            string connectionString = _configuration.GetConnectionString("MyConnectionString");
            using SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();

            int deletedCount = 0;
            foreach (var enAppointmentID in selectedAppointmentIds)
            {
                try
                {
                    var decodedId = HttpUtility.UrlDecode(enAppointmentID);
                    int AppointmentID = Convert.ToInt32(UrlEncryptDecrypt.Decrypt(decodedId));
                    using SqlCommand command = sqlConnection.CreateCommand();
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "PR_Appointment_Delete";
                    command.Parameters.Add("@AppointmentID", SqlDbType.Int).Value = AppointmentID;
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                        deletedCount++;
                }
                catch
                {
                    // Optionally log or handle individual errors
                }
            }

            if (deletedCount > 0)
            {
                TempData["SuccessMessage"] = $"{deletedCount} appointment(s) canceled successfully 🗑️.";
                TempData["SuccessType"] = "delete";
            }
            else
            {
                TempData["ErrorMessage"] = "No appointments were canceled.";
            }

            return RedirectToAction("AppointmentList");
        }
        #endregion

        #region AppointmentAddEdit (GET)

        [HttpGet]
        public IActionResult AppointmentAddEdit(string? enAppointmentID)
        {

            var decodedId = HttpUtility.UrlDecode(enAppointmentID);
            int? AppointmentID = Convert.ToInt32(UrlEncryptDecrypt.Decrypt(decodedId));


            string connectionString = _configuration.GetConnectionString("MyConnectionString");

            // Retrieve the list of users, patients, doctors from the database to populate a dropdown for selecting a user, patient, doctor associated with the appointment.
            // User List
            List<UserModel> userList = new List<UserModel>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("PR_User_SelectAll", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            userList.Add(new UserModel
                            {
                                UserID = Convert.ToInt32(reader["UserID"]),
                                UserName = reader["UserName"].ToString()
                            });
                        }
                    }
                }
            }
            ViewBag.UserList = userList;

            // Patient List
            List<PatientModel> patientList = new List<PatientModel>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("PR_Patient_SelectAll", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            patientList.Add(new PatientModel
                            {
                                PatientID = Convert.ToInt32(reader["PatientID"]),
                                PatientName = reader["PatientName"].ToString()
                            });
                        }
                    }
                }
            }
            ViewBag.PatientList = patientList;

            // Doctor List
            List<DoctorModel> doctorList = new List<DoctorModel>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("PR_Doctor_SelectAll", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            doctorList.Add(new DoctorModel
                            {
                                DoctorID = Convert.ToInt32(reader["DoctorID"]),
                                DoctorName = reader["DoctorName"].ToString()
                            });
                        }
                    }
                }
            }
            ViewBag.DoctorList = doctorList;
            AppointmentModel appointmentModel = new AppointmentModel();

            if (AppointmentID != null)
            {
                using SqlConnection sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();
                using SqlCommand command = sqlConnection.CreateCommand();

                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "PR_Appointment_SelectByID";
                command.Parameters.AddWithValue("@AppointmentID", AppointmentID);
                using SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    appointmentModel.AppointmentID = Convert.ToInt32(reader["AppointmentID"]);
                    appointmentModel.DoctorID = Convert.ToInt32(reader["DoctorID"]);
                    appointmentModel.PatientID = Convert.ToInt32(reader["PatientID"]);
                    appointmentModel.AppointmentDate = Convert.ToDateTime(reader["AppointmentDate"]);
                    appointmentModel.AppointmentStatus = reader["AppointmentStatus"].ToString();

                    appointmentModel.Description = reader["Description"].ToString();
                    appointmentModel.SpecialRemarks = reader["SpecialRemarks"].ToString();
                    appointmentModel.TotalConsultedAmount = (decimal)(reader["TotalConsultedAmount"] != DBNull.Value ? (decimal?)Convert.ToDecimal(reader["TotalConsultedAmount"]) : null);
                    appointmentModel.Created = Convert.ToDateTime(reader["Created"]);
                    appointmentModel.Modified = Convert.ToDateTime(reader["Modified"]);
                    appointmentModel.UserID = Convert.ToInt32(reader["UserID"]);

                }
            }


            return View(appointmentModel);
        }
        #endregion

        #region AppointmentAddEdit (POST)

        [HttpPost]
        public IActionResult AppointmentAddEdit(AppointmentModel appointmentModel)
        {
            string connectionString = _configuration.GetConnectionString("MyConnectionString");
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                try
                {
                    sqlConnection.Open();
                    using (SqlCommand command = sqlConnection.CreateCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        string successMessage;
                        string successType;

                        if (appointmentModel.AppointmentID > 0)
                        {
                            command.CommandText = "PR_Appointment_Update";
                            command.Parameters.Add("@AppointmentID", SqlDbType.Int).Value = appointmentModel.AppointmentID;
                            successMessage = "Appointment details updated successfully ✅ .";
                            successType = "update";
                        }
                        else
                        {
                            command.CommandText = "PR_Appointment_Insert";
                            successMessage = "New appointment booked successfully 🗓️ .";
                            successType = "add";
                        }

                        command.Parameters.Add("@DoctorID", SqlDbType.Int).Value = appointmentModel.DoctorID;
                        command.Parameters.Add("@PatientID", SqlDbType.Int).Value = appointmentModel.PatientID;
                        command.Parameters.Add("@AppointmentDate", SqlDbType.DateTime).Value = appointmentModel.AppointmentDate;
                        command.Parameters.Add("@AppointmentStatus", SqlDbType.NVarChar, 20).Value = appointmentModel.AppointmentStatus;
                        command.Parameters.Add("@Description", SqlDbType.NVarChar, 250).Value = (object)appointmentModel.Description ?? DBNull.Value;
                        command.Parameters.Add("@SpecialRemarks", SqlDbType.NVarChar, 100).Value = (object)appointmentModel.SpecialRemarks ?? DBNull.Value;

                        var param = command.Parameters.Add("@TotalConsultedAmount", SqlDbType.Decimal);
                        param.Precision = 10;
                        param.Scale = 2;
                        param.Value = (object)appointmentModel.TotalConsultedAmount ?? DBNull.Value;

                        command.Parameters.Add("@UserID", SqlDbType.Int).Value = appointmentModel.UserID;
                        command.ExecuteNonQuery();

                        TempData["SuccessMessage"] = successMessage;
                        TempData["SuccessType"] = successType;
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while saving the appointment.";
                }
            }
            return RedirectToAction("AppointmentList");
        }
        #endregion

        #region AppointmentDetail

        [HttpGet]
        public IActionResult AppointmentDetail(string enAppointmentID)
        {
            var decodedId = HttpUtility.UrlDecode(enAppointmentID);
            int AppointmentID = Convert.ToInt32(UrlEncryptDecrypt.Decrypt(decodedId));

            string connectionString = _configuration.GetConnectionString("MyConnectionString");
            using SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            using SqlCommand command = sqlConnection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_Appointment_SelectByID";
            command.Parameters.AddWithValue("@AppointmentID", AppointmentID);
            using SqlDataReader reader = command.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(reader);

            DataRow row = dt.Rows.Count > 0 ? dt.Rows[0] : null;

            return View(row);
        }
        #endregion

        #region Export
        [HttpGet]
        public IActionResult ExportAppointments(string exportType)
        {
            // Step 1: Fetch data same as DoctorList
            string ConnectionString = _configuration.GetConnectionString("MyConnectionString");
            using SqlConnection sqlConnection = new SqlConnection(ConnectionString);
            sqlConnection.Open();

            SqlCommand command = sqlConnection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_Appointment_SelectAll";
            using SqlDataReader reader = command.ExecuteReader();

            DataTable table = new DataTable();
            table.Load(reader);

            // Step 2: Export based on type
            switch (exportType?.ToLower())
            {
                case "excel":
                    return ExportToExcel(table);
                case "csv":
                    return ExportToCsv(table);
                case "pdf":
                    return ExportToPdf(table);
                default:
                    return RedirectToAction("AppointmentList");
            }
        }

        private FileResult ExportToExcel(DataTable table)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Appointments");

            // Insert DataTable as an Excel table (structured table)
            var xlTable = worksheet.Cell(1, 1).InsertTable(table, "AppointmentsTable", true);
            // Apply built-in Excel Table Style (Ice Blue - Medium 23)
            xlTable.Theme = XLTableTheme.TableStyleMedium23;

            // Autofit columns
            worksheet.Columns().AdjustToContents();

            // Apply borders and styling
            var rngTable = worksheet.RangeUsed();
            rngTable.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            rngTable.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Header styling
            var headerRow = rngTable.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "Appointments.xlsx");
        }


        private FileResult ExportToCsv(DataTable table)
        {
            var sb = new StringBuilder();

            // Headers
            IEnumerable<string> columnNames = table.Columns.Cast<DataColumn>().Select(col => col.ColumnName);
            sb.AppendLine(string.Join(",", columnNames));

            // Rows
            foreach (DataRow row in table.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(",", fields));
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "Appointments.csv");
        }
        private FileResult ExportToPdf(DataTable table)
        {
            using (var ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 20f, 20f, 20f, 20f);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // Title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, new BaseColor(91, 155, 213));
                Paragraph title = new Paragraph("Appointments List", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                doc.Add(title);
                doc.Add(new Paragraph("\n"));

                // Table
                PdfPTable pdfTable = new PdfPTable(table.Columns.Count);
                pdfTable.WidthPercentage = 100;
                pdfTable.SpacingBefore = 10f;
                pdfTable.SpacingAfter = 10f;

                // Header row
                foreach (DataColumn column in table.Columns)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(
                        column.ColumnName,
                        FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, new BaseColor(255, 255, 255))
                    ));
                    cell.BackgroundColor = new BaseColor(91, 155, 213); // Ice Blue
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.Padding = 6;

                    // Full border
                    cell.BorderWidth = 1;
                    cell.BorderColor = new BaseColor(0, 0, 0);

                    pdfTable.AddCell(cell);
                }

                // Data rows with alternating colors
                bool isAlternate = false;
                foreach (DataRow row in table.Rows)
                {
                    foreach (var item in row.ItemArray)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(
                            item?.ToString() ?? "",
                            FontFactory.GetFont(FontFactory.HELVETICA, 11, BaseColor.BLACK)
                        ));
                        cell.Padding = 5;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;

                        // Background zebra striping
                        cell.BackgroundColor = isAlternate
                            ? new BaseColor(221, 235, 247) // Light Blue
                            : BaseColor.WHITE;

                        // Full border
                        cell.BorderWidth = 1;
                        cell.BorderColor = new BaseColor(0, 0, 0);

                        pdfTable.AddCell(cell);
                    }
                    isAlternate = !isAlternate;
                }

                doc.Add(pdfTable);
                doc.Close();

                return File(ms.ToArray(), "application/pdf", "Appointments.pdf");
            }
        }



        #endregion

    }
}
