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

//public int DepartmentID { get; set; }
//public string DepartmentName { get; set; }
//public string Description { get; set; }
//public bool IsActive { get; set; }
//public DateTime Created { get; set; }
//public DateTime Modified { get; set; }
//public int UserID { get; set; }

namespace HMS.Controllers
{
    [CheckAccess]
    public class DepartmentController : Controller
    {

        #region configuration


        private readonly IConfiguration _configuration;

        public DepartmentController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #endregion

        #region DepartmentList


        [HttpGet]
        public IActionResult DepartmentList()
        {
            string ConnectionString = this._configuration.GetConnectionString(name: "MyConnectionString");


            using SqlConnection sqlConnection = new SqlConnection(ConnectionString);

            sqlConnection.Open();


            SqlCommand command = sqlConnection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;


            command.CommandText = "PR_Department_SelectAll";
            using SqlDataReader reader = command.ExecuteReader();

            DataTable table = new DataTable();

            table.Load(reader);

            return View(table);
        }

        #endregion

        #region DepartmentDelete

        public IActionResult DepartmentDelete(string enDepartmentID)
        {
            try
            {
                var decodedId = HttpUtility.UrlDecode(enDepartmentID);
                int DepartmentID = Convert.ToInt32(UrlEncryptDecrypt.Decrypt(decodedId));

                string ConnectionString = this._configuration.GetConnectionString("MyConnectionString");
                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    sqlConnection.Open();
                    SqlCommand command = sqlConnection.CreateCommand();
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "PR_Department_Delete";
                    command.Parameters.Add("@DepartmentID", SqlDbType.Int).Value = DepartmentID;
                    command.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Department deleted successfully 🥲.";
                TempData["SuccessType"] = "delete";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Could not delete department. It may be linked to other records.";
            }

            return RedirectToAction("DepartmentList");
        }
        #endregion

        #region DeleteSelectedDepartments
        [HttpPost]
        public IActionResult DeleteSelectedDepartments([FromForm] List<string> selectedDepartmentIds)
        {
            if (selectedDepartmentIds == null || selectedDepartmentIds.Count == 0)
            {
                TempData["ErrorMessage"] = "No departments selected for deletion.";
                return RedirectToAction("DepartmentList");
            }

            string connectionString = _configuration.GetConnectionString("MyConnectionString");
            using SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();

            int deletedCount = 0;
            foreach (var enDepartmentID in selectedDepartmentIds)
            {
                try
                {
                    var decodedId = HttpUtility.UrlDecode(enDepartmentID);
                    int DepartmentID = Convert.ToInt32(UrlEncryptDecrypt.Decrypt(decodedId));
                    using SqlCommand command = sqlConnection.CreateCommand();
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "PR_Department_Delete";
                    command.Parameters.Add("@DepartmentID", SqlDbType.Int).Value = DepartmentID;
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
                TempData["SuccessMessage"] = $"{deletedCount} department(s) deleted successfully 🥲🥲.";
                TempData["SuccessType"] = "delete";
            }
            else
            {
                TempData["ErrorMessage"] = "No departments were deleted 😶‍🌫️.";
            }

            return RedirectToAction("DepartmentList");
        }
        #endregion

        #region DepartmentAddEdit (GET)

        [HttpGet]
        public IActionResult DepartmentAddEdit(string? enDepartmentID)
        {
            var decodedId = HttpUtility.UrlDecode(enDepartmentID);
            int DepartmentID = Convert.ToInt32(UrlEncryptDecrypt.Decrypt(decodedId));

            string ConnectionString = this._configuration.GetConnectionString(name: "MyConnectionString");
            using SqlConnection sqlConnectionForUser = new SqlConnection(ConnectionString);
            sqlConnectionForUser.Open();
            using SqlCommand commandForUser = sqlConnectionForUser.CreateCommand();
            commandForUser.CommandType = CommandType.StoredProcedure;
            commandForUser.CommandText = "PR_User_SelectAll";
            using SqlDataReader readerForUser = commandForUser.ExecuteReader();

            List<UserModel> userList = new List<UserModel>();
            while (readerForUser.Read())
            {
                userList.Add(
                    new UserModel()
                    {
                        UserID = Convert.ToInt32(readerForUser["UserID"]),
                        UserName = readerForUser["UserName"].ToString(),
                    });
            }
            ViewBag.UserList = userList; // Populate the ViewBag with a SelectList of users for dropdown selection in the view.

            DepartmentModel departmentModel = new DepartmentModel();

            if (DepartmentID != null)
            {
                string connectionString = _configuration.GetConnectionString("MyConnectionString");
                using SqlConnection sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();
                using SqlCommand command = sqlConnection.CreateCommand();

                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "PR_Department_SelectByID";
                command.Parameters.AddWithValue("@DepartmentID", DepartmentID);
                using SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    Console.WriteLine(departmentModel.UserID);
                    departmentModel.DepartmentID = Convert.ToInt32(reader["DepartmentID"]);
                    departmentModel.DepartmentName = reader["DepartmentName"].ToString();
                    departmentModel.Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : string.Empty;
                    departmentModel.IsActive = Convert.ToBoolean(reader["IsActive"]);
                    departmentModel.Created = Convert.ToDateTime(reader["Created"]);
                    departmentModel.Modified = Convert.ToDateTime(reader["Modified"]);
                    departmentModel.UserID = Convert.ToInt32(reader["UserID"]);

                }
            }

            return View(departmentModel);
        }
        #endregion

        #region DepartmentAddEdit (POST)

        [HttpPost]
        public IActionResult DepartmentAddEdit(DepartmentModel departmentModel)
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

                        if (departmentModel.DepartmentID > 0)
                        {
                            command.CommandText = "PR_Department_Update";
                            command.Parameters.Add("@DepartmentID", SqlDbType.Int).Value = departmentModel.DepartmentID;
                            successMessage = "Department details updated successfully 📂.";
                            successType = "update";
                        }
                        else
                        {
                            command.CommandText = "PR_Department_Insert";
                            successMessage = "New department added successfully 🏢.";
                            successType = "add";
                        }

                        command.Parameters.Add("@DepartmentName", SqlDbType.NVarChar).Value = departmentModel.DepartmentName;
                        command.Parameters.Add("@Description", SqlDbType.NVarChar).Value = string.IsNullOrEmpty(departmentModel.Description) ? (object)DBNull.Value : departmentModel.Description;
                        command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = departmentModel.IsActive;
                        command.Parameters.Add("@UserID", SqlDbType.Int).Value = departmentModel.UserID;

                        command.ExecuteNonQuery();

                        TempData["SuccessMessage"] = successMessage;
                        TempData["SuccessType"] = successType;
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception 'ex' in a real-world application
                    TempData["ErrorMessage"] = "An error occurred while saving the department.";
                }
            }
            return RedirectToAction("DepartmentList");
        }
        #endregion

        #region DepartmentDetail

        [HttpGet]
        public IActionResult DepartmentDetail(string enDepartmentID)
        {
            var decodedId = HttpUtility.UrlDecode(enDepartmentID);
            int DepartmentID = Convert.ToInt32(UrlEncryptDecrypt.Decrypt(decodedId));

            string connectionString = _configuration.GetConnectionString("MyConnectionString");
            using SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            using SqlCommand command = sqlConnection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_Department_SelectByID";
            command.Parameters.AddWithValue("@DepartmentID", DepartmentID);
            using SqlDataReader reader = command.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(reader);

            DataRow row = dt.Rows.Count > 0 ? dt.Rows[0] : null;

            return View(row);
        }
        #endregion

        #region Export
        [HttpGet]
        public IActionResult ExportDepartments(string exportType)
        {
            // Step 1: Fetch data same as DepartmentList
            string ConnectionString = _configuration.GetConnectionString("MyConnectionString");
            using SqlConnection sqlConnection = new SqlConnection(ConnectionString);
            sqlConnection.Open();

            SqlCommand command = sqlConnection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_Department_SelectAll";
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
                    return RedirectToAction("DepartmentList");
            }
        }

        private FileResult ExportToExcel(DataTable table)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Departments");

            // Insert DataTable as an Excel table (structured table)
            var xlTable = worksheet.Cell(1, 1).InsertTable(table, "DepartmentsTable", true);
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
                        "Departments.xlsx");
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

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "Departments.csv");
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
                Paragraph title = new Paragraph("Departments List", titleFont);
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

                return File(ms.ToArray(), "application/pdf", "Departments.pdf");
            }
        }



        #endregion

    }
}
