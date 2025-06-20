using EduLms_RHS.Dto;
using EduLms_RHS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

[Route("api/[controller]")]
[ApiController]
public class StudentController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;
    private readonly EmailService _emailService;

    public StudentController(IConfiguration configuration,EmailService emailService)
    {
        _configuration = configuration;
        _emailService = emailService;
        _connectionString = _configuration.GetConnectionString("Lms");
    }
    [HttpPost("RegisterStudent")]
    public async Task<IActionResult> RegisterStudent(RegisterStudentDto student)
    {
        using SqlConnection con = new(_connectionString);
        SqlCommand cmd = new("sp_RegisterStudent", con);
        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@FullName", student.FullName);
        cmd.Parameters.AddWithValue("@Email", student.Email);
        cmd.Parameters.AddWithValue("@Password", student.Password);
        cmd.Parameters.AddWithValue("@PhoneNumber", student.PhoneNumber);
        cmd.Parameters.AddWithValue("@DateOfBirth", student.DateOfBirth);
        cmd.Parameters.AddWithValue("@Gender", student.Gender);
        cmd.Parameters.AddWithValue("@Address", student.Address);
        cmd.Parameters.AddWithValue("@GradeLevel", student.GradeLevel);
        cmd.Parameters.AddWithValue("@StudentNo", student.StudentNo);

        con.Open();
        int rows = cmd.ExecuteNonQuery();

        if (rows > 0)
        {
            // Compose welcome email
            string subject = "🎉 Registration Successful - Awaiting Admin Approval";
            string body = $@"
            <p style='font-family:Segoe UI, sans-serif; font-size:14px;'>
                Dear <strong>{student.FullName}</strong>,
            </p>
            <p>
                Thank you for registering on our <strong>Learning Management System (LMS)</strong>! Your registration was successful and is now awaiting admin approval.
            </p>
            <p>
                You will receive an email notification once your account is approved.
            </p>
            <br/>
            <p>
                Best regards,<br/>
                <strong>RHS Team</strong> 🎓
            </p>";

            try
            {
                await _emailService.SendEmailAsync(student.Email, subject, body);
            }
            catch (Exception ex)
            {
                // Log email sending error but don't fail registration
                Console.WriteLine("Failed to send registration email: " + ex.Message);
            }

            return Ok(new { message = "✅ Student registration successful. Waiting for admin approval." });
        }
        else
        {
            return BadRequest("Registration failed.");
        }
    }


    [HttpGet("Profile/{studentId}")]
        public IActionResult GetProfile(int studentId)
        {
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("sp_GetStudentProfile", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@StudentId", studentId);
            con.Open();

            using SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var student = new
                {
                    StudentId = reader["StudentId"],
                    FullName = reader["FullName"],
                    Email = reader["Email"],
                    PhoneNumber = reader["PhoneNumber"],
                    DOB = reader["DateOfBirth"],
                    Gender = reader["Gender"],
                    Address = reader["Address"],
                    GradeLevel = reader["GradeLevel"],
                    StudentNo = reader["StudentNo"]
                };
                return Ok(student);
            }
            return NotFound("Student not found");
        }

        [HttpGet("EnrolledCourses/{studentId}")]
        public IActionResult GetEnrolledCourses(int studentId)
        {
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("sp_GetEnrolledCourses", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@StudentId", studentId);
            con.Open();

            using SqlDataReader reader = cmd.ExecuteReader();
            List<object> courses = new();
            while (reader.Read())
            {
                courses.Add(new
                {
                    CourseId = reader["CourseId"],
                    CourseName = reader["CourseName"],
                    Description = reader["Description"],
                    Category = reader["Category"]
                });
            }
            return Ok(courses);
        }

        [HttpPost("Enroll")]
        public IActionResult EnrollCourse(int studentId, int courseId)
        {
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("sp_EnrollCourse", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@StudentId", studentId);
            cmd.Parameters.AddWithValue("@CourseId", courseId);
            con.Open();
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Enrolled successfully." });
        }


    [HttpGet("MyAssignments/{studentId}")]
    public IActionResult GetMyAssignments(int studentId)
    {
        using SqlConnection con = new(_connectionString);
        SqlCommand cmd = new("GetMyAssignmentsByStudentId", con)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@sid", studentId);

        con.Open();

        using SqlDataReader reader = cmd.ExecuteReader();
        List<object> assignments = new();
        while (reader.Read())
        {
            assignments.Add(new
            {
                AssignmentId = reader["AssignmentId"],
                Title = reader["Title"],
                Description = reader["Description"],
                File = reader["UploadFilePath"],
                DueDate = reader["DueDate"]
            });
        }

        return Ok(assignments);
    }



    [HttpGet("MyCourses/{studentId}")]
    public IActionResult GetMyCourses(int studentId)
    {
        using SqlConnection con = new(_connectionString);
        SqlCommand cmd = new("GetMyCoursesByStudentId", con)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@sid", studentId);

        con.Open();

        List<object> courses = new();
        using SqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            courses.Add(new
            {
                CourseId = reader["CourseId"],
                CourseName = reader["CourseName"],
                Description = reader["Description"],
                Category = reader["Category"],
                PdfFilePath = reader["PdfFilePath"] != DBNull.Value ? reader["PdfFilePath"].ToString() : null
            });
        }

        return Ok(courses);
    }




    [HttpPost("SubmitAssignment")]
        public async Task<IActionResult> SubmitAssignment(
            [FromQuery] int assignmentId,
            [FromQuery] int studentId,
            IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("❌ No file uploaded");

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "StudentSubmissions");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = $"{studentId}_{assignmentId}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/StudentSubmissions/{fileName}";

            using SqlConnection con = new(_connectionString);
            con.Open();

            SqlCommand checkCmd = new("sp_CheckAssignmentAndStudent", con);
            checkCmd.CommandType = CommandType.StoredProcedure;
            checkCmd.Parameters.AddWithValue("@AssignmentId", assignmentId);
            checkCmd.Parameters.AddWithValue("@StudentId", studentId);

            int exists = (int)checkCmd.ExecuteScalar();
            if (exists == 0)
            {
                return BadRequest("❌ Assignment or Student not found.");
            }

            SqlCommand cmd = new("sp_SubmitAssignment", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@AssignmentId", assignmentId);
            cmd.Parameters.AddWithValue("@StudentId", studentId);
            cmd.Parameters.AddWithValue("@FilePath", relativePath);
            cmd.Parameters.AddWithValue("@SubmittedDate", DateTime.Now);

            cmd.ExecuteNonQuery();
            return Ok(new { message = "✅ Assignment uploaded successfully" });
        }

        [HttpGet("PerformanceReport/{studentId}")]
        public IActionResult GetPerformanceReport(int studentId)
        {
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("sp_GetPerformanceReport", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@StudentId", studentId);
            con.Open();

            using SqlDataReader reader = cmd.ExecuteReader();
            List<object> reports = new();
            while (reader.Read())
            {
                reports.Add(new
                {
                    CourseId = reader["CourseId"],
                    CourseName = reader["CourseName"],
                    Grade = reader["AverageGrade"],
                    Remarks = reader["Remarks"]
                });
            }
            return Ok(reports);
        }

    
}