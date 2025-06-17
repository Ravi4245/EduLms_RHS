using EduLms_RHS.Dto;
using EduLms_RHS.Models;
using Microsoft.AspNetCore.Authorization; // ✅ Add this
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

[Route("api/[controller]")]
[ApiController]
public class StudentController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public StudentController(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("Lms");
    }

    // ❌ No authorization needed to register
    [HttpPost("RegisterStudent")]
    public IActionResult RegisterStudent(RegisterStudentDto student)
    {
        using SqlConnection con = new(_connectionString);
        con.Open();

        SqlCommand cmd = new("INSERT INTO Student (FullName, Email, Password, PhoneNumber, DateOfBirth, Gender, Address, GradeLevel, StudentNo, CreatedAt) " +
                             "VALUES (@FullName, @Email, @Password, @PhoneNumber, @DateOfBirth, @Gender, @Address, @GradeLevel, @StudentNo, GETDATE())", con);

        cmd.Parameters.AddWithValue("@FullName", student.FullName);
        cmd.Parameters.AddWithValue("@Email", student.Email);
        cmd.Parameters.AddWithValue("@Password", student.Password);
        cmd.Parameters.AddWithValue("@PhoneNumber", student.PhoneNumber);
        cmd.Parameters.AddWithValue("@DateOfBirth", student.DateOfBirth);
        cmd.Parameters.AddWithValue("@Gender", student.Gender);
        cmd.Parameters.AddWithValue("@Address", student.Address);
        cmd.Parameters.AddWithValue("@GradeLevel", student.GradeLevel);
        cmd.Parameters.AddWithValue("@StudentNo", student.StudentNo);

        int rows = cmd.ExecuteNonQuery();
        return rows > 0 ? Ok(new { message = "✅ Student registration successful. Waiting for admin approval." }) : BadRequest("Registration failed.");
    }



    [HttpGet("Profile/{studentId}")]
    public IActionResult GetProfile(int studentId)
    {
        using SqlConnection con = new(_connectionString);
        SqlCommand cmd = new("SELECT * FROM Student WHERE StudentId = @id", con);
        cmd.Parameters.AddWithValue("@id", studentId);
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
        SqlCommand cmd = new(@"
        SELECT c.CourseId, c.CourseName, c.Description, c.Category, c.PdfFilePath
        FROM Course c
        INNER JOIN StudentCourse sc ON sc.CourseId = c.CourseId
        WHERE sc.StudentId = @sid", con);

        cmd.Parameters.AddWithValue("@sid", studentId);
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
                Category = reader["Category"],
                PdfFilePath = reader["PdfFilePath"] != DBNull.Value ? reader["PdfFilePath"].ToString() : null
            });
        }

        return Ok(courses);
    }

    [HttpGet("MyCourses/{studentId}")]
    public IActionResult GetMyCourses(int studentId)
    {
        using SqlConnection con = new SqlConnection(_connectionString);
        SqlCommand cmd = new SqlCommand(@"
        SELECT c.CourseId, c.CourseName, c.Description, c.Category, c.PdfFilePath
        FROM Course c
        INNER JOIN StudentCourse sc ON sc.CourseId = c.CourseId
        WHERE sc.StudentId = @sid", con);

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



    [HttpPost("Enroll")]
    public IActionResult EnrollCourse(int studentId, int courseId)
    {
        using SqlConnection con = new(_connectionString);
        SqlCommand cmd = new("INSERT INTO StudentCourse (StudentId, CourseId) VALUES (@sid, @cid)", con);
        cmd.Parameters.AddWithValue("@sid", studentId);
        cmd.Parameters.AddWithValue("@cid", courseId);
        con.Open();
        cmd.ExecuteNonQuery();
        return Ok(new { message = "Enrolled successfully." });
    }


    [HttpGet("MyAssignments/{studentId}")]
    public IActionResult GetMyAssignments(int studentId)
    {
        using SqlConnection con = new(_connectionString);
        SqlCommand cmd = new(@"
        SELECT a.AssignmentId, a.Title, a.Description, a.UploadFilePath, a.DueDate
        FROM Assignment a
        INNER JOIN AssignmentStudent sa ON sa.AssignmentId = a.AssignmentId
        WHERE sa.StudentId = @sid
        AND NOT EXISTS (
            SELECT 1 FROM AssignmentSubmission sub
            WHERE sub.AssignmentId = a.AssignmentId AND sub.StudentId = @sid
        )", con);

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

        // ✅ Check if AssignmentId and StudentId exist
        SqlCommand checkCmd = new(@"
        SELECT COUNT(*) 
        FROM Assignment a, Student s 
        WHERE a.AssignmentId = @aid AND s.StudentId = @sid", con);
        checkCmd.Parameters.AddWithValue("@aid", assignmentId);
        checkCmd.Parameters.AddWithValue("@sid", studentId);

        int exists = (int)checkCmd.ExecuteScalar();
        if (exists == 0)
        {
            return BadRequest("❌ Assignment or Student not found.");
        }

        SqlCommand cmd = new(@"
        INSERT INTO AssignmentSubmission (AssignmentId, StudentId, SubmittedFilePath, SubmittedDate)
        VALUES (@aid, @sid, @file, @submittedDate)", con);

        cmd.Parameters.AddWithValue("@aid", assignmentId);
        cmd.Parameters.AddWithValue("@sid", studentId);
        cmd.Parameters.AddWithValue("@file", relativePath);
        cmd.Parameters.AddWithValue("@submittedDate", DateTime.Now);

        cmd.ExecuteNonQuery();

        return Ok(new { message = "✅ Assignment uploaded successfully" });
    }





    [HttpGet("PerformanceReport/{studentId}")]
    public IActionResult GetPerformanceReport(int studentId)
    {
        using SqlConnection con = new(_connectionString);
        SqlCommand cmd = new(@"
            SELECT p.CourseId, c.CourseName, p.AverageGrade, p.Remarks
            FROM PerformanceReport p
            INNER JOIN Course c ON c.CourseId = p.CourseId
            WHERE p.StudentId = @sid", con);
        cmd.Parameters.AddWithValue("@sid", studentId);
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
