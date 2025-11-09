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

//public int UserID { get; set; }
//public string UserName { get; set; }
//public string Password { get; set; }
//public string Email { get; set; }
//public string MobileNo { get; set; }
//public bool IsActive { get; set; }
//public DateTime Created { get; set; }
//public DateTime Modified { get; set; }
//public IFormFile? ProfileImage { get; set; }

namespace HMS.Controllers
{
    // The namespace declaration indicates that this code is part of the HMS.Controllers namespace.
    // The UserController class is defined within the HMS.Controllers namespace, which is a common practice in ASP.NET Core applications to organize controllers.
    
    public class UserController : Controller
    {
        // The UserController class is a controller in an ASP.NET Core application that handles user-related actions.
        // The UserController class inherits from the Controller base class, which provides methods and properties for handling HTTP requests and responses.

        #region configuration

        //This code is used to inject the IConfiguration service into the UserController.

        // The IConfiguration interface is part of the Microsoft.Extensions.Configuration namespace and is used to access configuration settings in ASP.NET Core applications.
        // It allows you to read configuration values from various sources, such as appsettings.json, environment variables, or command-line arguments.
        // The IConfiguration service is typically registered in the Startup class of an ASP.NET Core application, allowing it to be injected into controllers and other services.
        // This allows the UserController to access configuration settings throughout its methods.
        // The IConfiguration interface provides methods to retrieve configuration values by key, such as GetConnectionString, GetValue, and GetSection.

        // The UserController class has a private field _configuration of type IConfiguration.
        private readonly IConfiguration _configuration;
        // This field is used to store the configuration settings for the application, which can be accessed throughout the UserController methods.

        // The constructor of the UserController class takes an IConfiguration parameter, which is automatically injected by the ASP.NET Core dependency injection system.
        public UserController(IConfiguration configuration)
        {
            _configuration = configuration; // Assign the injected configuration to the private field
        }
        // This constructor allows the UserController to access configuration settings, such as connection strings or application settings, by using the _configuration field.
        // The constructor is called when an instance of the UserController is created, and it ensures that the _configuration field is initialized with the provided IConfiguration instance.
        // This allows the UserController to access configuration settings, such as connection strings or application settings, by using the _configuration field.

        #endregion

        #region UserLogin(GET)
        public IActionResult UserLogin()
        {
            return View();
        }
        #endregion

        #region UserLogin(POST)
        [HttpPost]
        public IActionResult UserLogin(UserLoginModel userLoginModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string connectionString = this._configuration.GetConnectionString("MyConnectionString");
                    SqlConnection sqlConnection = new SqlConnection(connectionString);
                    sqlConnection.Open();
                    SqlCommand sqlCommand = sqlConnection.CreateCommand();
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.CommandText = "PR_User_ValidateLogin";
                    sqlCommand.Parameters.Add("@Username", SqlDbType.VarChar).Value = userLoginModel.Username;
                    sqlCommand.Parameters.Add("@Password", SqlDbType.VarChar).Value = userLoginModel.Password;
                    SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                    DataTable dataTable = new DataTable();
                    dataTable.Load(sqlDataReader);
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dataTable.Rows)
                        {
                            HttpContext.Session.SetString("UserID", dr["UserID"].ToString());
                            HttpContext.Session.SetString("UserName", dr["UserName"].ToString());
                            HttpContext.Session.SetString("EmailAddress", dr["Email"].ToString());
                            HttpContext.Session.SetString("ProfileImage", dr["ProfileImage"].ToString());
                        }

                        return RedirectToAction("Index", "Dashboard");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "User is not found";
                        return RedirectToAction("UserLogin", "User");
                    }

                }
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = e.Message;
            }

            return RedirectToAction("UserLogin", "User");
        }
        #endregion

        #region UserLogout
        public IActionResult UserLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("UserLogin", "User");
        }
        #endregion

        #region UserList
        [CheckAccess]
        // This action method retrieves a list of users from the database and returns it to the UserList view.

        [HttpGet] // This attribute indicates that this method should handle HTTP GET requests.
        public IActionResult UserList() // This method handles GET requests to the UserList action.
        {
            // The connection string is retrieved from the configuration file using
            string ConnectionString = this._configuration.GetConnectionString(name: "MyConnectionString");
            // the name "MyConnectionString" will be used to connect to the database.
            // The GetConnectionString method is part of the IConfiguration interface, which allows you to access configuration settings in ASP.NET Core applications.
            // The connection string is typically defined in the appsettings.json file or other configuration sources.
            // It contains the necessary information to establish a connection to the database, such as the server name, database name, user credentials, and other connection parameters.

            // The SqlConnection class is used to create a connection to a SQL Server database.
            using SqlConnection sqlConnection = new SqlConnection(ConnectionString);
            // A new SqlConnection object is created using the connection string.
            // The SqlConnection class is part of the System.Data.SqlClient namespace, which provides classes for working with SQL Server databases.
            // The SqlConnection object is used to establish a connection to the database.
            sqlConnection.Open(); // Open the SQL connection to the database.


            SqlCommand command = sqlConnection.CreateCommand(); // Create a new SqlCommand object to execute a SQL command against the database.
            command.CommandType = CommandType.StoredProcedure; // Set the command type to StoredProcedure, indicating that the command will execute a stored procedure.
            // The CommandType enumeration is part of the System.Data namespace and specifies the type of command being executed.


            command.CommandText = "PR_User_SelectAll"; // Set the command text to the name of the stored procedure that will be executed.
            using SqlDataReader reader = command.ExecuteReader(); // Execute the command and retrieve the results using a SqlDataReader object.
            // The ExecuteReader method executes the command and returns a SqlDataReader object that can be used to read the results of the query.
            // The SqlDataReader class is part of the System.Data.SqlClient namespace and provides a way to read a forward-only stream of rows from a SQL Server database.
            // The SqlDataReader object allows you to read the results of the query row by row.

            DataTable table = new DataTable(); // Create a new DataTable object to hold the results of the query.
            // The DataTable class is part of the System.Data namespace and represents an in-memory table of data.
            // A DataTable can hold multiple rows and columns of data, making it suitable for storing query results.
            // The DataTable object is used to store the results of the query in a tabular format.
            // The DataTable class provides methods and properties to manipulate and access the data in a tabular format.

            // The Load method of the DataTable class is used to load the results from the SqlDataReader into the DataTable.
            // The Load method reads the data from the SqlDataReader and populates the DataTable with the results.
            // This allows you to work with the results in a structured way, such as binding it to a view or performing further processing.
            // The DataTable can then be used to display the results in a view or perform further processing.
            table.Load(reader); // Load the results from the SqlDataReader into the DataTable.

            return View(table); // Return the DataTable to the UserList view.
        }

        #endregion

        #region UserDelete
        [CheckAccess]
        // This action method deletes a user from the database based on the provided UserID.

        //[HttpDelete] // This attribute indicates that this method should handle HTTP DELETE requests.
        // This method handles DELETE requests to the UserDelete action, where @UserID is the ID of the user to be deleted.
        public IActionResult UserDelete(string enUserID)
        {
            try
            {
                var decodedId = HttpUtility.UrlDecode(enUserID);
                int UserID = Convert.ToInt32(UrlEncryptDecrypt.Decrypt(decodedId));

                string ConnectionString = this._configuration.GetConnectionString("MyConnectionString");
                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    sqlConnection.Open();
                    SqlCommand command = sqlConnection.CreateCommand();
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "PR_User_Delete";
                    command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
                    command.ExecuteNonQuery();
                }

                // If the code reaches here, it was successful
                TempData["SuccessMessage"] = "User deleted successfully.";
                TempData["SuccessType"] = "delete";
            }
            catch (Exception ex)
            {
                // If ANY error occurs in the 'try' block (like a SQL error), this code will run.
                // In a real production app, you might log the 'ex' variable for debugging.

                TempData["ErrorMessage"] = "Could not delete user. They may be linked to other records (e.g., appointments).";

            }

            // This will run after either the try or the catch block finishes.
            return RedirectToAction("UserList");
        }
        #endregion

        #region DeleteSelectedUsers
        [CheckAccess]
        [HttpPost]
        public IActionResult DeleteSelectedUsers([FromForm] List<string> selectedUserIds)
        {
            if (selectedUserIds == null || selectedUserIds.Count == 0)
            {
                TempData["ErrorMessage"] = "No users selected for deletion.";
                return RedirectToAction("UserList");
            }

            string connectionString = _configuration.GetConnectionString("MyConnectionString");
            using SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();

            int deletedCount = 0;
            foreach (var enUserID in selectedUserIds)
            {
                try
                {
                    var decodedId = HttpUtility.UrlDecode(enUserID);
                    int UserID = Convert.ToInt32(UrlEncryptDecrypt.Decrypt(decodedId));
                    using SqlCommand command = sqlConnection.CreateCommand();
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "PR_User_Delete";
                    command.Parameters.Add("@UserID", SqlDbType.Int).Value = UserID;
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
                TempData["SuccessMessage"] = $"{deletedCount} user(s) deleted successfully.";
                TempData["SuccessType"] = "delete";
            }
            else
                TempData["ErrorMessage"] = "No users were deleted.";

            return RedirectToAction("UserList");
        }
        #endregion

        #region UserAddEdit (GET)
        [CheckAccess]
        // This action method retrieves a user by UserID for editing or adds a new user if UserID is null.
        // It returns a UserModel object to the UserAddEdit view.
        [HttpGet] // This attribute indicates that this method should handle HTTP GET requests.
        public IActionResult UserAddEdit(string? enUserID) // This method handles GET requests to the UserAddEdit action, where UserID is an optional parameter.
        {

            var decodedId = HttpUtility.UrlDecode(enUserID);
            int UserID = Convert.ToInt32(UrlEncryptDecrypt.Decrypt(decodedId));
            UserModel userModel = new UserModel(); // Create a new instance of the UserModel class to hold user data.

            if (UserID == null)
            {
                // If UserID is null, we are adding a new user, so userModel will remain with default values.
                return View(userModel); // Return the UserModel object to the UserAddEdit view for display or editing.
            }
            // If UserID is not null, we are editing an existing user, so we retrieve the user data from the database.
            string connectionString = _configuration.GetConnectionString("MyConnectionString"); // Retrieve the connection string from the configuration file using the name "MyConnection
            using SqlConnection sqlConnection = new SqlConnection(connectionString); // Create a new SqlConnection object using the connection string.
            sqlConnection.Open(); // Open the SQL connection to the database.
            using SqlCommand command = sqlConnection.CreateCommand(); // Create a new SqlCommand object to execute a SQL command against the database.
            command.CommandType = CommandType.StoredProcedure; // Set the command type to StoredProcedure, indicating that the command will execute a stored procedure.
            command.CommandText = "PR_User_SelectByID"; // Set the command text to the name of the stored procedure that will be executed to retrieve the user by UserID.
            command.Parameters.AddWithValue("@UserID", UserID); // Add a parameter to the command for the UserID, which is the ID of the user to be retrieved.
            using SqlDataReader reader = command.ExecuteReader(); // Execute the command and retrieve the results using a SqlDataReader object.
            if (reader.Read()) // Check if the SqlDataReader has any rows to read, indicating that a user with the specified UserID was found.
            {
                userModel.UserID = Convert.ToInt32(reader["UserID"]);
                userModel.UserName = reader["UserName"].ToString();
                userModel.Password = reader["Password"].ToString();
                userModel.Email = reader["Email"].ToString();
                userModel.MobileNo = reader["MobileNo"].ToString();
                userModel.IsActive = Convert.ToBoolean(reader["IsActive"]);
                userModel.Created = Convert.ToDateTime(reader["Created"]);
                userModel.Modified = Convert.ToDateTime(reader["Modified"]);
                userModel.ProfileImagePath = reader["ProfileImage"].ToString();
                // Populate the userModel properties with the values retrieved from the database.
            }
            return View(userModel); // Return the UserModel object to the UserAddEdit view for display or editing.s
        }
        #endregion

        #region UserAddEdit (POST)
        [CheckAccess]
        // This action method handles the form submission for adding or editing a user.
        [HttpPost] // This attribute indicates that this method should handle HTTP POST requests.
        public IActionResult UserAddEdit(UserModel userModel) // This method handles POST requests to the UserAddEdit action, where userModel is the model containing user data submitted from the form.
        {
            string connectionString = _configuration.GetConnectionString("MyConnectionString");
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                // --- NEW VALIDATION LOGIC START ---
                if (!string.IsNullOrEmpty(userModel.Email))
                {
                    // Check if the email already exists for a different user
                    string checkEmailSql = "SELECT COUNT(*) FROM [User] WHERE Email = @Email AND UserID != @UserID";
                    using SqlCommand checkCmd = new SqlCommand(checkEmailSql, sqlConnection);
                    checkCmd.Parameters.AddWithValue("@Email", userModel.Email);
                    // For a new user, UserID is 0, so this correctly checks all existing users.
                    // For an existing user, it checks all users *except* the current one.
                    checkCmd.Parameters.AddWithValue("@UserID", userModel.UserID);
                    int existingUserCount = (int)checkCmd.ExecuteScalar();

                    if (existingUserCount > 0)
                    {
                        // Manually add an error to ModelState. This will be displayed by the asp-validation-for tag helper.
                        ModelState.AddModelError("Email", "This email address is already registered.");
                    }
                }
                // --- NEW VALIDATION LOGIC END ---

                if (!ModelState.IsValid)
                {
                    // If validation fails (either from data annotations or our custom check),
                    // return to the form to display errors.
                    return View(userModel);
                }

                try
                {
                    using (SqlCommand command = sqlConnection.CreateCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        string successMessage;
                        string successType;
                        if (userModel.UserID > 0)
                        {
                            command.CommandText = "PR_User_Update";
                            command.Parameters.Add("@UserID", SqlDbType.Int).Value = userModel.UserID;
                            successMessage = "User details updated successfully.";
                            successType = "update";
                        }
                        else
                        {
                            command.CommandText = "PR_User_Insert";
                            successMessage = "New user added successfully.";
                            successType = "add";
                        }

                        command.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = userModel.UserName;
                        command.Parameters.Add("@Password", SqlDbType.NVarChar).Value = userModel.Password;
                        command.Parameters.Add("@Email", SqlDbType.NVarChar).Value = userModel.Email;
                        command.Parameters.Add("@MobileNo", SqlDbType.NVarChar).Value = userModel.MobileNo;
                        command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = userModel.IsActive;

                        string filePath = null;
                        if (userModel.ProfileImage != null && userModel.ProfileImage.Length > 0)
                        {
                            try
                            {
                                filePath = ImageHelper.SaveImage(imageFile: userModel.ProfileImage, dir: "Profile");
                            }
                            catch (System.Exception)
                            {
                                Console.WriteLine("file not provided mostly");
                            }
                        }
                        else if (userModel.UserID == 0)
                        {
                            filePath = "Profile/default.png";
                        }
                        command.Parameters.Add("@ProfileImage", SqlDbType.NVarChar).Value = filePath ?? (object)DBNull.Value;
                        command.ExecuteNonQuery();

                        // Set success message after the operation completes
                        TempData["SuccessMessage"] = successMessage;
                        TempData["SuccessType"] = successType;
                    }
                }
                catch (Exception ex)
                {
                    // Set an error message if anything goes wrong in the 'try' block
                    // In a real app, you would log the exception 'ex'
                    TempData["ErrorMessage"] = "An error occurred while saving user details.";
                }
            }

            return RedirectToAction("UserList");
        }
        #endregion

        #region UserDetail
        [CheckAccess]
        // This action method retrieves the details of a user by UserID and returns it to the UserDetail view.
        [HttpGet] // This attribute indicates that this method should handle HTTP GET requests.
        public IActionResult UserDetail(string enUserID) // This method handles GET requests to the UserDetail action, where UserID is the ID of the user whose details are to be retrieved.
        {

            var decodedId = HttpUtility.UrlDecode(enUserID);
            int UserID = Convert.ToInt32(UrlEncryptDecrypt.Decrypt(decodedId));
            string connectionString = _configuration.GetConnectionString("MyConnectionString"); // Retrieve the connection string from the configuration file using the name "MyConnectionString".
            using SqlConnection sqlConnection = new SqlConnection(connectionString); // Create a new SqlConnection object using the connection string.
            sqlConnection.Open(); // Open the SQL connection to the database.
            using SqlCommand command = sqlConnection.CreateCommand(); // Create a new SqlCommand object to execute a SQL command against the database.
            command.CommandType = CommandType.StoredProcedure; // Set the command type to StoredProcedure, indicating that the command will execute a stored procedure.
            command.CommandText = "PR_User_SelectByID"; // Set the command text to the name of the stored procedure that will be executed to retrieve the user by UserID.
            command.Parameters.AddWithValue("@UserID", UserID); // Add a parameter to the command for the UserID, which is the ID of the user whose details are to be retrieved.
            using SqlDataReader reader = command.ExecuteReader(); // Execute the command and retrieve the results using a SqlDataReader object.
            DataTable dt = new DataTable(); // Create a new DataTable object to hold the user details retrieved from the database.
            dt.Load(reader); // Load the results from the SqlDataReader into the DataTable.

            DataRow row = dt.Rows.Count > 0 ? dt.Rows[0] : null; // Check if the DataTable has any rows, and if so, get the first row; otherwise, set row to null.
                                                                 // The DataRow class is part of the System.Data namespace and represents a single row in a DataTable.
            return View(row); // Return the DataRow object to the UserDetail view for display.
        }
        #endregion

        #region Export
        [CheckAccess]
        [HttpGet]
        public IActionResult ExportUsers(string exportType)
        {
            // Step 1: Fetch data same as UserList
            string ConnectionString = _configuration.GetConnectionString("MyConnectionString");
            using SqlConnection sqlConnection = new SqlConnection(ConnectionString);
            sqlConnection.Open();

            SqlCommand command = sqlConnection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_User_SelectAll";
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
                    return RedirectToAction("UserList");
            }
        }

        private FileResult ExportToExcel(DataTable table)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Users");

            // Insert DataTable as an Excel table (structured table)
            var xlTable = worksheet.Cell(1, 1).InsertTable(table, "UsersTable", true);
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
                        "Users.xlsx");
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

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "Users.csv");
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
                Paragraph title = new Paragraph("Users List", titleFont);
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

                return File(ms.ToArray(), "application/pdf", "Users.pdf");
            }
        }



        #endregion

    }
}