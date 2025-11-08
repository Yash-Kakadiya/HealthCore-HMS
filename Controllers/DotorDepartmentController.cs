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

//public int DoctorDepartmentID { get; set; }
//public int DoctorID { get; set; }
//public int DepartmentID { get; set; }
//public DateTime Created { get; set; }
//public DateTime Modified { get; set; }
//public int UserID { get; set; }

namespace HMS.Controllers
{
    [CheckAccess]
    public class DoctorDepartmentController : Controller
    {

        #region configuration


        private readonly IConfiguration _configuration;

        public DoctorDepartmentController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #endregion


        #region DoctorDepartmentList


        [HttpGet]
        public IActionResult DoctorDepartmentList()
        {
            string ConnectionString = this._configuration.GetConnectionString(name: "MyConnectionString");


            using SqlConnection sqlConnection = new SqlConnection(ConnectionString);

            sqlConnection.Open();


            SqlCommand command = sqlConnection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;


            command.CommandText = "PR_DoctorDepartment_SelectAll";
            using SqlDataReader reader = command.ExecuteReader();

            DataTable table = new DataTable();

            table.Load(reader);

            return View(table);
        }

        #endregion

        #region DoctorDepartmentDelete

        public IActionResult DoctorDepartmentDelete(string enDoctorDepartmentID)
        {
            try
            {
                var decodedId = HttpUtility.UrlDecode(enDoctorDepartmentID);
                int DoctorDepartmentID = Convert.ToInt32(UrlEncryptDecrypt.Decrypt(decodedId));

                string ConnectionString = this._configuration.GetConnectionString("MyConnectionString");
                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    sqlConnection.Open();
                    SqlCommand command = sqlConnection.CreateCommand();
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "PR_DoctorDepartment_Delete";
                    command.Parameters.Add("@DoctorDepartmentID", SqlDbType.Int).Value = DoctorDepartmentID;
                    command.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "🗑️ Link between doctor and department removed.";
                TempData["SuccessType"] = "delete";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Could not remove the link. It may be associated with other records.";
            }

            return RedirectToAction("DoctorDepartmentList");
        }
        #endregion

        #region DeleteSelectedDoctorDepartments
        [HttpPost]
        public IActionResult DeleteSelectedDoctorDepartments([FromForm] List<string> selectedDoctorDepartmentIds)
        {
            if (selectedDoctorDepartmentIds == null || selectedDoctorDepartmentIds.Count == 0)
            {
                TempData["ErrorMessage"] = "No doctor departments selected for deletion.";
                return RedirectToAction("DoctorDepartmentList");
            }

            string connectionString = _configuration.GetConnectionString("MyConnectionString");
            using SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();

            int deletedCount = 0;
            foreach (var enDoctorDepartmentID in selectedDoctorDepartmentIds)
            {
                try
                {
                    var decodedId = HttpUtility.UrlDecode(enDoctorDepartmentID);
                    int DoctorDepartmentID = Convert.ToInt32(UrlEncryptDecrypt.Decrypt(decodedId));
                    using SqlCommand command = sqlConnection.CreateCommand();
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "PR_DoctorDepartment_Delete";
                    command.Parameters.Add("@DoctorDepartmentID", SqlDbType.Int).Value = DoctorDepartmentID;
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
                TempData["SuccessMessage"] = $" {deletedCount} doctor department(s) removed successfully 🥲.";
                TempData["SuccessType"] = "delete"; // Triggers the delete animation
            }
            else
            {
                TempData["ErrorMessage"] = "No doctor departments were deleted.";
            }

            return RedirectToAction("DoctorDepartmentList");
        }
        #endregion

        #region DoctorDepartmentAddEdit (GET)

        [HttpGet]
        public IActionResult DoctorDepartmentAddEdit(string? enDoctorDepartmentID)
        {
            var decodedId = HttpUtility.UrlDecode(enDoctorDepartmentID);
            int DoctorDepartmentID = Convert.ToInt32(UrlEncryptDecrypt.Decrypt(decodedId));

            string connectionString = _configuration.GetConnectionString("MyConnectionString");

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

            // Department List
            List<DepartmentModel> patientList = new List<DepartmentModel>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("PR_Department_SelectAll", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            patientList.Add(new DepartmentModel
                            {
                                DepartmentID = Convert.ToInt32(reader["DepartmentID"]),
                                DepartmentName = reader["DepartmentName"].ToString()
                            });
                        }
                    }
                }
            }
            ViewBag.DepartmentList = patientList;

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

            DoctorDepartmentModel dotorDepartmentModel = new DoctorDepartmentModel();

            if (DoctorDepartmentID != null)
            {
                using SqlConnection sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();
                using SqlCommand command = sqlConnection.CreateCommand();

                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "PR_DoctorDepartment_SelectByID";
                command.Parameters.AddWithValue("@DoctorDepartmentID", DoctorDepartmentID);
                using SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    dotorDepartmentModel.DoctorDepartmentID = Convert.ToInt32(reader["DoctorDepartmentID"]);
                    dotorDepartmentModel.DoctorID = Convert.ToInt32(reader["DoctorID"]);
                    dotorDepartmentModel.DepartmentID = Convert.ToInt32(reader["DepartmentID"]);

                    dotorDepartmentModel.Created = Convert.ToDateTime(reader["Created"]);
                    dotorDepartmentModel.Modified = Convert.ToDateTime(reader["Modified"]);
                    dotorDepartmentModel.UserID = Convert.ToInt32(reader["UserID"]);

                }
            }


            return View(dotorDepartmentModel);
        }
        #endregion

        #region DoctorDepartmentAddEdit (POST)

        [HttpPost]
        public IActionResult DoctorDepartmentAddEdit(DoctorDepartmentModel dotorDepartmentModel)
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

                        if (dotorDepartmentModel.DoctorDepartmentID > 0)
                        {
                            command.CommandText = "PR_DoctorDepartment_Update";
                            command.Parameters.Add("@DoctorDepartmentID", SqlDbType.Int).Value = dotorDepartmentModel.DoctorDepartmentID;
                            successMessage = "Link updated successfully ✅ .";
                            successType = "update";
                        }
                        else
                        {
                            command.CommandText = "PR_DoctorDepartment_Insert";
                            successMessage = "Doctor linked to department successfully 🔗.";
                            successType = "add";
                        }

                        command.Parameters.Add("@DoctorID", SqlDbType.Int).Value = dotorDepartmentModel.DoctorID;
                        command.Parameters.Add("@DepartmentID", SqlDbType.Int).Value = dotorDepartmentModel.DepartmentID;
                        command.Parameters.Add("@UserID", SqlDbType.Int).Value = dotorDepartmentModel.UserID;

                        command.ExecuteNonQuery();

                        TempData["SuccessMessage"] = successMessage;
                        TempData["SuccessType"] = successType;
                    }
                }
                catch (Exception ex)
                {
                    // For a unique constraint violation, this error will be shown.
                    TempData["ErrorMessage"] = "An error occurred. This doctor may already be linked to this department.";
                }
            }
            return RedirectToAction("DoctorDepartmentList");
        }
        #endregion

        #region DoctorDepartmentDetail

        [HttpGet]
        public IActionResult DoctorDepartmentDetail(string? enDoctorDepartmentID)
        {
            var decodedId = HttpUtility.UrlDecode(enDoctorDepartmentID);
            int DoctorDepartmentID = Convert.ToInt32(UrlEncryptDecrypt.Decrypt(decodedId));


            string connectionString = _configuration.GetConnectionString("MyConnectionString");
            using SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            using SqlCommand command = sqlConnection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_DoctorDepartment_SelectByID";
            command.Parameters.AddWithValue("@DoctorDepartmentID", DoctorDepartmentID);
            using SqlDataReader reader = command.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(reader);

            DataRow row = dt.Rows.Count > 0 ? dt.Rows[0] : null;

            return View(row);
        }
        #endregion

        #region Export
        [HttpGet]
        public IActionResult ExportDoctorDepartments(string exportType)
        {
            // Step 1: Fetch data same as DoctorList
            string ConnectionString = _configuration.GetConnectionString("MyConnectionString");
            using SqlConnection sqlConnection = new SqlConnection(ConnectionString);
            sqlConnection.Open();

            SqlCommand command = sqlConnection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_DoctorDepartment_SelectAll";
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
                    return RedirectToAction("DoctorDepartmentList");
            }
        }

        private FileResult ExportToExcel(DataTable table)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("DoctorDepartments");

            // Insert DataTable as an Excel table (structured table)
            var xlTable = worksheet.Cell(1, 1).InsertTable(table, "DoctorDepartmentsTable", true);
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
                        "DoctorDepartments.xlsx");
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

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "DoctorDepartments.csv");
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
                Paragraph title = new Paragraph("DoctorDepartments List", titleFont);
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

                return File(ms.ToArray(), "application/pdf", "DoctorDepartments.pdf");
            }
        }

        #endregion

    }
}
