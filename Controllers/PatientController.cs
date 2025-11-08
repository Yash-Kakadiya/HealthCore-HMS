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

//public int PatientID { get; set; }
//public string PatientName { get; set; }
//public DateTime DateOfBirth { get; set; }
//public string Gender { get; set; }
//public string Email { get; set; }
//public string Phone { get; set; }
//public string Address { get; set; }
//public string City { get; set; }
//public string State { get; set; }
//public bool IsActive { get; set; }
//public DateTime Created { get; set; }
//public DateTime Modified { get; set; }
//public int UserID { get; set; }

namespace HMS.Controllers
{
    [CheckAccess]
    public class PatientController : Controller
    {

        #region configuration



        private readonly IConfiguration _configuration;

        public PatientController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #endregion

        #region PatientList


        [HttpGet]
        public IActionResult PatientList()
        {
            string ConnectionString = this._configuration.GetConnectionString(name: "MyConnectionString");


            using SqlConnection sqlConnection = new SqlConnection(ConnectionString);

            sqlConnection.Open();


            SqlCommand command = sqlConnection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;


            command.CommandText = "PR_Patient_SelectAll";
            using SqlDataReader reader = command.ExecuteReader();

            DataTable table = new DataTable();

            table.Load(reader);

            return View(table);
        }

        #endregion

        #region PatientDelete

        public IActionResult PatientDelete(string enPatientID)
        {
            try
            {
                var decodedId = HttpUtility.UrlDecode(enPatientID);
                int PatientID = Convert.ToInt32(UrlEncryptDecrypt.Decrypt(decodedId));

                string ConnectionString = this._configuration.GetConnectionString("MyConnectionString");
                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    sqlConnection.Open();
                    SqlCommand command = sqlConnection.CreateCommand();
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "PR_Patient_Delete";
                    command.Parameters.Add("@PatientID", SqlDbType.Int).Value = PatientID;
                    command.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "🗑️ Patient record removed successfully.";
                TempData["SuccessType"] = "delete";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Could not delete patient. They may be linked to other records (e.g., appointments).";
            }

            return RedirectToAction("PatientList");
        }
        #endregion

        #region DeleteSelectedPatients
        [HttpPost]
        public IActionResult DeleteSelectedPatients([FromForm] List<string> selectedPatientIds)
        {
            if (selectedPatientIds == null || selectedPatientIds.Count == 0)
            {
                TempData["ErrorMessage"] = "No patients selected for deletion.";
                return RedirectToAction("PatientList");
            }

            string connectionString = _configuration.GetConnectionString("MyConnectionString");
            using SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();

            int deletedCount = 0;
            foreach (var enPatientID in selectedPatientIds)
            {
                try
                {
                    var decodedId = HttpUtility.UrlDecode(enPatientID);
                    int PatientID = Convert.ToInt32(UrlEncryptDecrypt.Decrypt(decodedId));
                    using SqlCommand command = sqlConnection.CreateCommand();
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "PR_Patient_Delete";
                    command.Parameters.Add("@PatientID", SqlDbType.Int).Value = PatientID;
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
                TempData["SuccessMessage"] = $"🗑️ {deletedCount} patient record(s) removed successfully.";
                TempData["SuccessType"] = "delete";
            }
            else
            {
                TempData["ErrorMessage"] = "No patients were deleted.";
            }

            return RedirectToAction("PatientList");
        }
        #endregion


        #region PatientAddEdit (GET)

        [HttpGet]
        public IActionResult PatientAddEdit(string? enPatientID)
        {
            var decodedId = HttpUtility.UrlDecode(enPatientID);
            int PatientID = Convert.ToInt32(UrlEncryptDecrypt.Decrypt(decodedId));


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

            PatientModel patientModel = new PatientModel();

            if (PatientID != null)
            {
                string connectionString = _configuration.GetConnectionString("MyConnectionString");
                using SqlConnection sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();
                using SqlCommand command = sqlConnection.CreateCommand();

                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "PR_Patient_SelectByID";
                command.Parameters.AddWithValue("@PatientID", PatientID);
                using SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    patientModel.PatientID = Convert.ToInt32(reader["PatientID"]);
                    patientModel.PatientName = reader["PatientName"].ToString();
                    patientModel.DateOfBirth = Convert.ToDateTime(reader["DateOfBirth"]);
                    patientModel.Gender = reader["Gender"].ToString();
                    patientModel.Email = reader["Email"].ToString();
                    patientModel.Phone = reader["Phone"].ToString();
                    patientModel.Address = reader["Address"].ToString();
                    patientModel.City = reader["City"].ToString();
                    patientModel.State = reader["State"].ToString();
                    patientModel.IsActive = Convert.ToBoolean(reader["IsActive"]);
                    patientModel.Created = Convert.ToDateTime(reader["Created"]);
                    patientModel.Modified = Convert.ToDateTime(reader["Modified"]);
                    patientModel.UserID = Convert.ToInt32(reader["UserID"]);

                }
            }

            return View(patientModel);
        }
        #endregion


        #region PatientAddEdit (POST)

        [HttpPost]
        public IActionResult PatientAddEdit(PatientModel patientModel)
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

                        if (patientModel.PatientID > 0)
                        {
                            command.CommandText = "PR_Patient_Update";
                            command.Parameters.Add("@PatientID", SqlDbType.Int).Value = patientModel.PatientID;
                            successMessage = "📝 Patient record updated successfully.";
                            successType = "update";
                        }
                        else
                        {
                            command.CommandText = "PR_Patient_Insert";
                            successMessage = "🧑‍⚕️ New patient registered successfully.";
                            successType = "add";
                        }


                        command.Parameters.Add("@PatientName", SqlDbType.NVarChar).Value = patientModel.PatientName;
                        command.Parameters.Add("@DateOfBirth", SqlDbType.DateTime).Value = patientModel.DateOfBirth;
                        command.Parameters.Add("@Gender", SqlDbType.NVarChar).Value = patientModel.Gender;
                        command.Parameters.Add("@Email", SqlDbType.NVarChar).Value = patientModel.Email;
                        command.Parameters.Add("@Phone", SqlDbType.NVarChar).Value = patientModel.Phone;
                        command.Parameters.Add("@Address", SqlDbType.NVarChar).Value = patientModel.Address;
                        command.Parameters.Add("@City", SqlDbType.NVarChar).Value = patientModel.City;
                        command.Parameters.Add("@State", SqlDbType.NVarChar).Value = patientModel.State;
                        command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = patientModel.IsActive;
                        command.Parameters.Add("@UserID", SqlDbType.Int).Value = patientModel.IsActive;

                        command.ExecuteNonQuery();

                        TempData["SuccessMessage"] = successMessage;
                        TempData["SuccessType"] = successType;
                    }
                }
                catch (Exception ex)
                {

                    TempData["ErrorMessage"] = "An error occurred while saving the patient's record.";
                }
            }
            return RedirectToAction("PatientList");
        }
        #endregion

        #region PatientDetail

        [HttpGet]
        public IActionResult PatientDetail(string enPatientID)
        {
            var decodedId = HttpUtility.UrlDecode(enPatientID);
            int PatientID = Convert.ToInt32(UrlEncryptDecrypt.Decrypt(decodedId));

            string connectionString = _configuration.GetConnectionString("MyConnectionString");
            using SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            using SqlCommand command = sqlConnection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_Patient_SelectByID";
            command.Parameters.AddWithValue("@PatientID", PatientID);
            using SqlDataReader reader = command.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(reader);

            DataRow row = dt.Rows.Count > 0 ? dt.Rows[0] : null;

            return View(row);
        }
        #endregion

        #region Export
        [HttpGet]
        public IActionResult ExportPatients(string exportType)
        {
            // Step 1: Fetch data same as PatientList
            string ConnectionString = _configuration.GetConnectionString("MyConnectionString");
            using SqlConnection sqlConnection = new SqlConnection(ConnectionString);
            sqlConnection.Open();

            SqlCommand command = sqlConnection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_Patient_SelectAll";
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
                    return RedirectToAction("PatientList");
            }
        }

        private FileResult ExportToExcel(DataTable table)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Patients");

            // Insert DataTable as an Excel table (structured table)
            var xlTable = worksheet.Cell(1, 1).InsertTable(table, "PatientsTable", true);
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
                        "Patients.xlsx");
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

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "Patients.csv");
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
                Paragraph title = new Paragraph("Patients List", titleFont);
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

                return File(ms.ToArray(), "application/pdf", "Patients.pdf");
            }
        }



        #endregion

    }
}
