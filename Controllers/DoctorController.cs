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

//public int DoctorID { get; set; }
//public string Name { get; set; }
//public string Phone { get; set; }
//public string Email { get; set; }
//public string Qualification { get; set; }
//public string Specialization { get; set; }
//public bool IsActive { get; set; }
//public DateTime Created { get; set; }
//public DateTime Modified { get; set; }
//public int UserID { get; set; }

namespace HMS.Controllers
{
    [CheckAccess]
    public class DoctorController : Controller
    {

        #region configuration


        private readonly IConfiguration _configuration;

        public DoctorController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #endregion

        #region DoctorList


        [HttpGet]
        public IActionResult DoctorList()
        {
            string ConnectionString = this._configuration.GetConnectionString(name: "MyConnectionString");


            using SqlConnection sqlConnection = new SqlConnection(ConnectionString);

            sqlConnection.Open();


            SqlCommand command = sqlConnection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;


            command.CommandText = "PR_Doctor_SelectAll";
            using SqlDataReader reader = command.ExecuteReader();

            DataTable table = new DataTable();

            table.Load(reader);

            return View(table);
        }

        #endregion

        #region DoctorDelete

        public IActionResult DoctorDelete(string enDoctorID)
        {
            try
            {
                var decodedId = HttpUtility.UrlDecode(enDoctorID);
                int DoctorID = Convert.ToInt32(UrlEncryptDecrypt.Decrypt(decodedId));

                string ConnectionString = this._configuration.GetConnectionString("MyConnectionString");
                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    sqlConnection.Open();
                    SqlCommand command = sqlConnection.CreateCommand();
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "PR_Doctor_Delete";
                    command.Parameters.Add("@DoctorID", SqlDbType.Int).Value = DoctorID;
                    command.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Doctor removed successfully 🥲.";
                TempData["SuccessType"] = "delete";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Could not delete doctor. They may be linked to other records (e.g., appointments).";
            }

            return RedirectToAction("DoctorList");
        }
        #endregion

        #region DeleteSelectedDoctors
        [HttpPost]
        public IActionResult DeleteSelectedDoctors([FromForm] List<string> selectedDoctorIds)
        {
            if (selectedDoctorIds == null || selectedDoctorIds.Count == 0)
            {
                TempData["ErrorMessage"] = "No doctors selected for deletion.";
                return RedirectToAction("DoctorList");
            }

            string connectionString = _configuration.GetConnectionString("MyConnectionString");
            using SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();

            int deletedCount = 0;
            foreach (var enDoctorID in selectedDoctorIds)
            {
                try
                {
                    var decodedId = HttpUtility.UrlDecode(enDoctorID);
                    int DoctorID = Convert.ToInt32(UrlEncryptDecrypt.Decrypt(decodedId));
                    using SqlCommand command = sqlConnection.CreateCommand();
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "PR_Doctor_Delete";
                    command.Parameters.Add("@DoctorID", SqlDbType.Int).Value = DoctorID;
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
                TempData["SuccessMessage"] = $" {deletedCount} doctor(s) removed successfully 🥲.";
                TempData["SuccessType"] = "delete";
            }
            else
            {
                TempData["ErrorMessage"] = "No doctors were deleted.";
            }

            return RedirectToAction("DoctorList");
        }
        #endregion

        #region DoctorAddEdit (GET)

        [HttpGet]
        public IActionResult DoctorAddEdit(string? enDoctorID)
        {
            var decodedId = HttpUtility.UrlDecode(enDoctorID);
            int DoctorID = Convert.ToInt32(UrlEncryptDecrypt.Decrypt(decodedId));


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
            ViewBag.UserList = userList;

            DoctorModel doctorModel = new DoctorModel();

            if (DoctorID != null)
            {
                string connectionString = _configuration.GetConnectionString("MyConnectionString");
                using SqlConnection sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();
                using SqlCommand command = sqlConnection.CreateCommand();

                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "PR_Doctor_SelectByID";
                command.Parameters.AddWithValue("@DoctorID", DoctorID);
                using SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    doctorModel.DoctorID = Convert.ToInt32(reader["DoctorID"]);
                    doctorModel.DoctorName = reader["DoctorName"].ToString();
                    doctorModel.Phone = reader["Phone"].ToString();
                    doctorModel.Email = reader["Email"].ToString();
                    doctorModel.Qualification = reader["Qualification"].ToString();
                    doctorModel.Specialization = reader["Specialization"].ToString();
                    doctorModel.IsActive = Convert.ToBoolean(reader["IsActive"]);
                    doctorModel.Created = Convert.ToDateTime(reader["Created"]);
                    doctorModel.Modified = Convert.ToDateTime(reader["Modified"]);
                    doctorModel.UserID = Convert.ToInt32(reader["UserID"]);

                }
            }

            return View(doctorModel);
        }
        #endregion

        #region DoctorAddEdit (POST)

        [HttpPost]
        public IActionResult DoctorAddEdit(DoctorModel doctorModel)
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

                        if (doctorModel.DoctorID > 0)
                        {
                            command.CommandText = "PR_Doctor_Update";
                            command.Parameters.Add("@DoctorID", SqlDbType.Int).Value = doctorModel.DoctorID;
                            successMessage = "Doctor details updated successfully ✨.";
                            successType = "update";
                        }
                        else
                        {
                            command.CommandText = "PR_Doctor_Insert";
                            successMessage = "New doctor added successfully 🥳.";
                            successType = "add";
                        }

                        command.Parameters.Add("@DoctorName", SqlDbType.NVarChar).Value = doctorModel.DoctorName;
                        command.Parameters.Add("@Phone", SqlDbType.NVarChar).Value = doctorModel.Phone;
                        command.Parameters.Add("@Email", SqlDbType.NVarChar).Value = doctorModel.Email;
                        command.Parameters.Add("@Qualification", SqlDbType.NVarChar).Value = doctorModel.Qualification;
                        command.Parameters.Add("@Specialization", SqlDbType.NVarChar).Value = doctorModel.Specialization;
                        command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = doctorModel.IsActive;
                        command.Parameters.Add("@UserID", SqlDbType.Int).Value = doctorModel.UserID;

                        command.ExecuteNonQuery();

                        TempData["SuccessMessage"] = successMessage;
                        TempData["SuccessType"] = successType;
                    }
                }
                catch (Exception ex)
                {

                    TempData["ErrorMessage"] = "An error occurred while saving doctor details.";
                }
            }
            return RedirectToAction("DoctorList");
        }
        #endregion

        #region DoctorDetail

        [HttpGet]
        public IActionResult DoctorDetail(string enDoctorID)
        {
            var decodedId = HttpUtility.UrlDecode(enDoctorID);
            int DoctorID = Convert.ToInt32(UrlEncryptDecrypt.Decrypt(decodedId));

            string connectionString = _configuration.GetConnectionString("MyConnectionString");
            using SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            using SqlCommand command = sqlConnection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_Doctor_SelectByID";
            command.Parameters.AddWithValue("@DoctorID", DoctorID);
            using SqlDataReader reader = command.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(reader);

            DataRow row = dt.Rows.Count > 0 ? dt.Rows[0] : null;

            return View(row);
        }
        #endregion

        #region Export
        [HttpGet]
        public IActionResult ExportDoctors(string exportType)
        {
            // Step 1: Fetch data same as DoctorList
            string ConnectionString = _configuration.GetConnectionString("MyConnectionString");
            using SqlConnection sqlConnection = new SqlConnection(ConnectionString);
            sqlConnection.Open();

            SqlCommand command = sqlConnection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_Doctor_SelectAll";
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
                    return RedirectToAction("DoctorList");
            }
        }

        private FileResult ExportToExcel(DataTable table)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Doctors");

            // Insert DataTable as an Excel table (structured table)
            var xlTable = worksheet.Cell(1, 1).InsertTable(table, "DoctorsTable", true);
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
                        "Doctors.xlsx");
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

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "Doctors.csv");
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
                Paragraph title = new Paragraph("Doctors List", titleFont);
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

                return File(ms.ToArray(), "application/pdf", "Doctors.pdf");
            }
        }



        #endregion 
    }
}
