using EduLms_RHS.Dto;
using EduLms_RHS.Models;
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
    




[HttpPost("CreateCourse")]
        public IActionResult CreateCourse(Course course)
        {
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("INSERT INTO Course (CourseName, Description, Category, CreatedByTeacherId) VALUES (@name, @desc, @cat, @tid)", con);
            cmd.Parameters.AddWithValue("@name", course.CourseName);
            cmd.Parameters.AddWithValue("@desc", course.Description);
            cmd.Parameters.AddWithValue("@cat", course.Category);
            cmd.Parameters.AddWithValue("@tid", course.CreatedByTeacherId);
            con.Open();
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Course Created" });
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
                    CreatedByTeacherId = (int)reader["CreatedByTeacherId"]
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
        public IActionResult AssignCourseToStudent(int studentId, int courseId)
        {
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("INSERT INTO StudentCourse (StudentId, CourseId) VALUES (@sid, @cid)", con);
            cmd.Parameters.AddWithValue("@sid", studentId);
            cmd.Parameters.AddWithValue("@cid", courseId);
            con.Open();
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Course assigned to student successfully" });
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
        public IActionResult CreateAssignment(Assignment assignment)
        {
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("INSERT INTO Assignment (CourseId, TeacherId, Title, Description, UploadFilePath, DueDate) VALUES (@cid, @tid, @title, @desc, @file, @due)", con);
            cmd.Parameters.AddWithValue("@cid", assignment.CourseId);
            cmd.Parameters.AddWithValue("@tid", assignment.TeacherId);
            cmd.Parameters.AddWithValue("@title", assignment.Title);
            cmd.Parameters.AddWithValue("@desc", assignment.Description);
            cmd.Parameters.AddWithValue("@file", assignment.UploadFilePath);
            cmd.Parameters.AddWithValue("@due", assignment.DueDate);
            con.Open();
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Assignment Created" });
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


        [HttpGet("EnrolledStudents/{teacherId}")]
        public IActionResult GetEnrolledStudents(int teacherId)
        {
            List<object> enrolledStudents = new();

            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new(@"
        SELECT 
            sc.StudentId,
            st.FullName AS StudentName,
            st.Email,
            c.CourseId,
            c.CourseName
        FROM StudentCourse sc
        INNER JOIN Student st ON sc.StudentId = st.StudentId
        INNER JOIN Course c ON sc.CourseId = c.CourseId
        WHERE c.CreatedByTeacherId = @tid", con);

            cmd.Parameters.AddWithValue("@tid", teacherId);

            con.Open();
            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                enrolledStudents.Add(new
                {
                    StudentId = reader["StudentId"],
                    StudentName = reader["StudentName"].ToString(),
                    Email = reader["Email"].ToString(),
                    CourseId = reader["CourseId"],
                    CourseName = reader["CourseName"].ToString()
                });
            }

            return Ok(enrolledStudents);
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
    }
}



