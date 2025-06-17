using EduLms_RHS.Dto;
using EduLms_RHS.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Edu_LMS_Greysoft.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeacherController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private readonly string _connectionString;

        public TeacherController(IConfiguration configuration, EmailService emailService)
        {
            _configuration = configuration;
            _emailService = emailService;
            _connectionString = _configuration.GetConnectionString("Lms");
        }

        [HttpPost("RegisterTeacher")]
        public async Task<IActionResult> RegisterTeacher(RegisterTeacherDto teacher)
        {
            try
            {
                using SqlConnection con = new(_connectionString);
                con.Open();

                SqlCommand cmd = new(@"
                    INSERT INTO Teacher (
                        FullName, Email, Password, PhoneNumber, Qualification,
                        ExperienceYears, Specialization, TeacherNo, CreatedAt
                    )
                    VALUES (
                        @FullName, @Email, @Password, @PhoneNumber, @Qualification,
                        @ExperienceYears, @Specialization, @TeacherNo, GETDATE()
                    )", con);

                cmd.Parameters.AddWithValue("@FullName", teacher.FullName);
                cmd.Parameters.AddWithValue("@Email", teacher.Email);
                cmd.Parameters.AddWithValue("@Password", teacher.Password); // ⚠️ Use hashing!
                cmd.Parameters.AddWithValue("@PhoneNumber", teacher.PhoneNumber);
                cmd.Parameters.AddWithValue("@Qualification", teacher.Qualification);
                cmd.Parameters.AddWithValue("@ExperienceYears", teacher.ExperienceYears);
                cmd.Parameters.AddWithValue("@Specialization", teacher.Specialization);
                cmd.Parameters.AddWithValue("@TeacherNo", teacher.TeacherNo);

                int rows = cmd.ExecuteNonQuery();

                string subject = "🎉 Registration Received – Awaiting Approval";

                string body = $@"
                    <p style='font-family:Segoe UI, sans-serif; font-size:14px;'>
                        Dear <strong>{teacher.FullName}</strong>, 👋
                    </p>
                    <p>
                        We're excited to welcome you to our <strong>Learning Management System (LMS)</strong> community. 📚<br/>
                        Thank you for registering as a teacher – we truly value your expertise and commitment to education. 🙌
                    </p>
                    <p>
                        Your account has been successfully created and is currently <strong>awaiting admin approval</strong>. 🔐<br/>
                        You’ll be able to access your dashboard shortly.
                    </p>
                    <p>
                        If you have any questions, feel free to reach out anytime.
                    </p>
                    <br/>
                    <p>
                        Best regards,<br/> 
                        <strong>RHS Team</strong> 🎓
                    </p>";

                await _emailService.SendEmailAsync(teacher.Email, subject, body);

                return Ok(new { message = "✅ Teacher registered successfully. Awaiting admin approval." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "❌ Error occurred during teacher registration.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    



        [HttpGet("Profile/{teacherId}")]
        public IActionResult GetTeacherProfile(int teacherId)
        {
            using SqlConnection con = new(_configuration.GetConnectionString("Lms"));
            SqlCommand cmd = new(@"SELECT FullName, Email, ExperienceYears, Specialization, PhoneNumber 
                           FROM Teacher 
                           WHERE TeacherId = @tid", con);
            cmd.Parameters.AddWithValue("@tid", teacherId);
            con.Open();
            using SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var profile = new
                {
                    FullName = reader["FullName"].ToString(),
                    Email = reader["Email"].ToString(),
                    Experience = Convert.ToInt32(reader["ExperienceYears"]),
                    Specialization = reader["Specialization"].ToString(),
                    Phone = reader["PhoneNumber"].ToString()
                };
                return Ok(profile);
            }
            return NotFound(new { message = "Teacher profile not found" });
        }


        [HttpPost("CreateCourse")]
        public async Task<IActionResult> CreateCourse([FromForm] CourseFormDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(ms => ms.Value.Errors.Count > 0)
                    .Select(ms => new
                    {
                        Field = ms.Key,
                        Messages = ms.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    });

                return BadRequest(new { message = "Validation failed", errors });
            }

            string? filePath = null;

            if (dto.PdfFile != null && dto.PdfFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "CoursePdfs");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{dto.PdfFile.FileName}";
                var fullPath = Path.Combine(uploadsFolder, uniqueFileName);

                using var stream = new FileStream(fullPath, FileMode.Create);
                await dto.PdfFile.CopyToAsync(stream);

                filePath = $"/CoursePdfs/{uniqueFileName}";
            }

            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("INSERT INTO Course (CourseName, Description, Category, CreatedByTeacherId, PdfFilePath) VALUES (@name, @desc, @cat, @tid, @pdf)", con);
            cmd.Parameters.AddWithValue("@name", dto.CourseName);
            cmd.Parameters.AddWithValue("@desc", dto.Description);
            cmd.Parameters.AddWithValue("@cat", dto.Category);
            cmd.Parameters.AddWithValue("@tid", dto.CreatedByTeacherId);
            cmd.Parameters.AddWithValue("@pdf", (object?)filePath ?? DBNull.Value);

            con.Open();
            cmd.ExecuteNonQuery();

            return Ok(new { message = "Course Created with PDF" });
        }







        [HttpGet("MyCourses/{teacherId}")]
        public IActionResult GetCoursesByTeacher(int teacherId)
        {
            List<Course> courses = new();
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("SELECT * FROM Course WHERE CreatedByTeacherId = @tid", con);
            cmd.Parameters.AddWithValue("@tid", teacherId);
            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                courses.Add(new Course
                {
                    CourseId = (int)reader["CourseId"],
                    CourseName = reader["CourseName"].ToString(),
                    Description = reader["Description"].ToString(),
                    Category = reader["Category"].ToString(),
                    CreatedByTeacherId = (int)reader["CreatedByTeacherId"],
                    PdfFilePath = reader["PdfFilePath"] != DBNull.Value ? reader["PdfFilePath"].ToString() : null

                });
            }
            return Ok(courses);
        }



        [HttpPut("UpdateCourse")]
        public IActionResult UpdateCourse(Course course)
        {
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("UPDATE Course SET CourseName = @name, Description = @desc, Category = @cat WHERE CourseId = @cid AND CreatedByTeacherId = @tid", con);
            cmd.Parameters.AddWithValue("@name", course.CourseName);
            cmd.Parameters.AddWithValue("@desc", course.Description);
            cmd.Parameters.AddWithValue("@cat", course.Category);
            cmd.Parameters.AddWithValue("@cid", course.CourseId);
            cmd.Parameters.AddWithValue("@tid", course.CreatedByTeacherId);
            con.Open();
            int rows = cmd.ExecuteNonQuery();
            return Ok(new { message = "Course Updated" });
        }

        [HttpPost("AssignCourseToStudent")]
        public async Task<IActionResult> AssignCourseToStudent(int studentId, int courseId)
        {
            try
            {
                using SqlConnection con = new(_connectionString);
                con.Open();

                // Insert assignment
                SqlCommand insertCmd = new("INSERT INTO StudentCourse (StudentId, CourseId) VALUES (@sid, @cid)", con);
                insertCmd.Parameters.AddWithValue("@sid", studentId);
                insertCmd.Parameters.AddWithValue("@cid", courseId);
                insertCmd.ExecuteNonQuery();

                // Get student details
                SqlCommand studentCmd = new("SELECT FullName, Email FROM Student WHERE StudentId = @sid", con);
                studentCmd.Parameters.AddWithValue("@sid", studentId);
                var reader = studentCmd.ExecuteReader();

                if (!reader.Read())
                {
                    return NotFound(new { message = "Student not found." });
                }

                string studentName = reader["FullName"].ToString();
                string studentEmail = reader["Email"].ToString();
                reader.Close();

                // Get course name
                SqlCommand courseCmd = new("SELECT CourseName FROM Course WHERE CourseId = @cid", con);
                courseCmd.Parameters.AddWithValue("@cid", courseId);
                string courseName = courseCmd.ExecuteScalar()?.ToString() ?? "your course";

                // Compose email content
                string subject = "📚 New Course Assigned to You!";
                string body = $@"
            <p>Dear <strong>{studentName}</strong>,</p>
            <p>You have been assigned a new course: <strong>{courseName}</strong>.</p>
            <p>Please log in to your dashboard to start learning.</p>
            <br/>
            <p>Best regards,<br/>EduLMS Team 🎓</p>";

                // Send email
                await _emailService.SendEmailAsync(studentEmail, subject, body);

                return Ok(new { message = "Course assigned to student successfully and email sent." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error occurred while assigning course.", error = ex.Message });
            }
        }



        [HttpDelete("DeleteCourse/{courseId}/{teacherId}")]
        public IActionResult DeleteCourse(int courseId, int teacherId)
        {
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("DELETE FROM Course WHERE CourseId = @cid AND CreatedByTeacherId = @tid", con);
            cmd.Parameters.AddWithValue("@cid", courseId);
            cmd.Parameters.AddWithValue("@tid", teacherId);
            con.Open();
            int rows = cmd.ExecuteNonQuery();
            return Ok(new { message = "Course Deleted" });
        }


        [HttpPost("CreateAssignment")]
        public IActionResult CreateAssignment([FromForm] Assignment assignment, IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                assignment.UploadFilePath = "Uploads/" + uniqueFileName;
            }

            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("INSERT INTO Assignment (CourseId, TeacherId, Title, Description, UploadFilePath, DueDate) VALUES (@cid, @tid, @title, @desc, @file, @due)", con);
            cmd.Parameters.AddWithValue("@cid", assignment.CourseId);
            cmd.Parameters.AddWithValue("@tid", assignment.TeacherId);
            cmd.Parameters.AddWithValue("@title", assignment.Title);
            cmd.Parameters.AddWithValue("@desc", assignment.Description);
            cmd.Parameters.AddWithValue("@file", assignment.UploadFilePath ?? string.Empty);
            cmd.Parameters.AddWithValue("@due", assignment.DueDate);
            con.Open();
            cmd.ExecuteNonQuery();

            return Ok(new { message = "Assignment Created with PDF file" });
        }



        [HttpPost("AssignAssignmentToStudent")]
        public IActionResult AssignAssignmentToStudent(int assignmentId, int studentId)
        {
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("INSERT INTO AssignmentStudent (AssignmentId, StudentId) VALUES (@aid, @sid)", con);
            cmd.Parameters.AddWithValue("@aid", assignmentId);
            cmd.Parameters.AddWithValue("@sid", studentId);
            con.Open();
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Assignment assigned to student successfully" });
        }

        [HttpGet("MyAssignments/{teacherId}")]
        public IActionResult GetAssignmentsByTeacher(int teacherId)
        {
            List<Assignment> assignments = new();
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("SELECT * FROM Assignment WHERE TeacherId = @tid", con);
            cmd.Parameters.AddWithValue("@tid", teacherId);
            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                assignments.Add(new Assignment
                {
                    AssignmentId = (int)reader["AssignmentId"],
                    CourseId = reader["CourseId"] != DBNull.Value ? (int?)reader["CourseId"] : null,
                    TeacherId = reader["TeacherId"] != DBNull.Value ? (int?)reader["TeacherId"] : null,
                    Title = reader["Title"].ToString(),
                    Description = reader["Description"].ToString(),
                    UploadFilePath = reader["UploadFilePath"].ToString(),
                    DueDate = reader["DueDate"] != DBNull.Value ? DateOnly.FromDateTime(Convert.ToDateTime(reader["DueDate"])) : null,
                    CreatedAt = reader["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedAt"]) : null
                });
            }
            return Ok(assignments);
        }



        [HttpGet("Submissions/{assignmentId}")]
        public IActionResult GetSubmissions(int assignmentId)
        {
            List<AssignmentSubmission> submissions = new();
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("SELECT * FROM AssignmentSubmission WHERE AssignmentId = @aid", con);
            cmd.Parameters.AddWithValue("@aid", assignmentId);
            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                submissions.Add(new AssignmentSubmission
                {
                    SubmissionId = (int)reader["SubmissionId"],
                    AssignmentId = (int)reader["AssignmentId"],
                    StudentId = (int)reader["StudentId"],
                    SubmittedFilePath = reader["SubmittedFilePath"].ToString(),
                    SubmittedDate = Convert.ToDateTime(reader["SubmittedDate"]),
                    Grade = reader["Grade"] != DBNull.Value ? Convert.ToInt32(reader["Grade"]) : 0,
                    Feedback = reader["Feedback"].ToString()
                });
            }
            return Ok(submissions);
        }


        [HttpPut("GradeSubmission")]
        public IActionResult GradeSubmission(AssignmentSubmission submission)
        {
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("UPDATE AssignmentSubmission SET Grade = @grade, Feedback = @fb WHERE SubmissionId = @sid", con);
            cmd.Parameters.AddWithValue("@grade", submission.Grade);
            cmd.Parameters.AddWithValue("@fb", submission.Feedback ?? "");
            cmd.Parameters.AddWithValue("@sid", submission.SubmissionId);
            con.Open();
            int rows = cmd.ExecuteNonQuery();
            return Ok(new { message = rows > 0 ? "Graded Successfully" : "Submission Not Found" });
        }


        [HttpPost("CreatePerformance")]
        public IActionResult CreatePerformance(PerformanceReport report)
        {
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("INSERT INTO PerformanceReport (StudentId, CourseId, AverageGrade, Remarks) VALUES (@sid, @cid, @grade, @remarks)", con);
            cmd.Parameters.AddWithValue("@sid", report.StudentId);
            cmd.Parameters.AddWithValue("@cid", report.CourseId);
            cmd.Parameters.AddWithValue("@grade", report.AverageGrade);
            cmd.Parameters.AddWithValue("@remarks", report.Remarks ?? "");
            con.Open();
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Report Created" });
        }


        [HttpGet("SubmittedAssignments/{teacherId}")]
        public IActionResult GetSubmittedAssignments(int teacherId)
        {
            using SqlConnection con = new(_configuration.GetConnectionString("Lms"));
            SqlCommand cmd = new(@"
        SELECT 
            s.SubmissionId,
            st.FullName AS StudentName,
            a.Title AS AssignmentTitle,
            s.SubmittedFilePath,
            s.SubmittedDate,
            s.Grade,
            s.Feedback
        FROM AssignmentSubmission s
        INNER JOIN Assignment a ON s.AssignmentId = a.AssignmentId
        INNER JOIN Student st ON s.StudentId = st.StudentId
        WHERE a.TeacherId = @tid", con);

            cmd.Parameters.AddWithValue("@tid", teacherId);

            con.Open();
            using SqlDataReader reader = cmd.ExecuteReader();
            List<object> submissions = new();

            while (reader.Read())
            {
                submissions.Add(new
                {
                    SubmissionId = reader["SubmissionId"],
                    StudentName = reader["StudentName"],
                    AssignmentTitle = reader["AssignmentTitle"],
                    SubmittedFilePath = reader["SubmittedFilePath"],
                    SubmittedDate = Convert.ToDateTime(reader["SubmittedDate"]).ToString("yyyy-MM-dd HH:mm"),
                    Grade = reader["Grade"] is DBNull ? null : reader["Grade"],
                    Feedback = reader["Feedback"]?.ToString()
                });
            }

            return Ok(submissions);
        }


        [HttpGet("ApprovedStudents")]
        public IActionResult GetApprovedStudents()
        {
            List<object> approvedStudents = new();

            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new(@"
        SELECT 
            StudentId,
            FullName AS StudentName,
            Email
        FROM Student
        WHERE IsApproved = 1", con);

            con.Open();
            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                approvedStudents.Add(new
                {
                    StudentId = reader["StudentId"],
                    StudentName = reader["StudentName"].ToString(),
                    Email = reader["Email"].ToString()
                });
            }

            return Ok(approvedStudents);
        }







        [HttpPut("UpdatePerformance")]
        public IActionResult UpdatePerformance(PerformanceReport report)
        {
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("UPDATE PerformanceReport SET AverageGrade = @grade, Remarks = @remarks WHERE ReportId = @rid", con);
            cmd.Parameters.AddWithValue("@grade", report.AverageGrade);
            cmd.Parameters.AddWithValue("@remarks", report.Remarks ?? "");
            cmd.Parameters.AddWithValue("@rid", report.ReportId);
            con.Open();
            int rows = cmd.ExecuteNonQuery();
            return Ok(new { message = rows > 0 ? "Report Updated" : "Report Not Found" });
        }

        [HttpGet("StudentAssignments/{studentId}")]
        public IActionResult GetStudentAssignments(int studentId)
        {
            List<object> assignments = new();
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new(@"
        SELECT A.AssignmentId, A.Title, A.Description, A.UploadFilePath, A.DueDate, C.CourseName
        FROM AssignmentStudent AS ASI
        JOIN Assignment AS A ON A.AssignmentId = ASI.AssignmentId
        JOIN Course AS C ON A.CourseId = C.CourseId
        WHERE ASI.StudentId = @sid", con);

            cmd.Parameters.AddWithValue("@sid", studentId);
            con.Open();
            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                assignments.Add(new
                {
                    AssignmentId = reader["AssignmentId"],
                    Title = reader["Title"],
                    Description = reader["Description"],
                    UploadFilePath = reader["UploadFilePath"],
                    DueDate = reader["DueDate"],
                    CourseName = reader["CourseName"]
                });
            }
            return Ok(assignments);
        }

    }
}