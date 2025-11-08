using System;
using System.ComponentModel.DataAnnotations;

namespace HMS.Models
{
    public class AppointmentModel
    {
        /*
         * CREATE TABLE Appointment
            (
                AppointmentID INT PRIMARY KEY IDENTITY(1,1),
                DoctorID INT NOT NULL,
                PatientID INT NOT NULL,
                AppointmentDate DATETIME NOT NULL,
                AppointmentStatus NVARCHAR(20) NOT NULL,
                Description NVARCHAR(250) NOT NULL,
                SpecialRemarks NVARCHAR(100) NOT NULL,
                Created DATETIME NOT NULL DEFAULT GETDATE(),
                Modified DATETIME NOT NULL,
                UserID INT NOT NULL,
                TotalConsultedAmount DECIMAL(5,2) NULL,

                CONSTRAINT FK_Appointment_Doctor FOREIGN KEY (DoctorID) REFERENCES Doctor(DoctorID),
                CONSTRAINT FK_Appointment_Patient FOREIGN KEY (PatientID) REFERENCES Patient(PatientID),
                CONSTRAINT FK_Appointment_User FOREIGN KEY (UserID) REFERENCES [User](UserID)
            );
         */
        public int AppointmentID { get; set; } // Identity column, no validation needed

        [Required(ErrorMessage = "DoctorID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "DoctorID must be a valid positive number.")]
        public int DoctorID { get; set; }

        [Required(ErrorMessage = "PatientID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "PatientID must be a valid positive number.")]
        public int PatientID { get; set; }

        [Required(ErrorMessage = "Appointment Date is required.")]
        [DataType(DataType.DateTime)]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Appointment Status is required.")]
        [StringLength(20, ErrorMessage = "Appointment Status cannot exceed 20 characters.")]
        public string AppointmentStatus { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Special Remarks are required.")]
        [StringLength(100, ErrorMessage = "Special Remarks cannot exceed 100 characters.")]
        public string SpecialRemarks { get; set; }

        public DateTime Created { get; set; } = DateTime.Now; // default server time

        [Required(ErrorMessage = "Modified Date is required.")]
        public DateTime Modified { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "UserID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "UserID must be a valid positive number.")]
        public int UserID { get; set; }

        [Range(0, 99999999.99, ErrorMessage = "Total Consulted Amount must be between 0 and 100M")]
        public decimal? TotalConsultedAmount { get; set; } // nullable as per DB schema

    }
}
