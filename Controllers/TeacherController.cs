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
    public partial class TeacherController : ControllerBase
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
        public IActionResult RegisterTeacher(Teacher teacher)
        {
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("sp_RegisterTeacher", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FullName", teacher.FullName);
            cmd.Parameters.AddWithValue("@Email", teacher.Email);
            cmd.Parameters.AddWithValue("@Password", teacher.Password);
            cmd.Parameters.AddWithValue("@PhoneNumber", teacher.PhoneNumber);
            cmd.Parameters.AddWithValue("@Qualification", teacher.Qualification);
            cmd.Parameters.AddWithValue("@ExperienceYears", teacher.ExperienceYears);
            cmd.Parameters.AddWithValue("@Specialization", teacher.Specialization);
            cmd.Parameters.AddWithValue("@TeacherNo", teacher.TeacherNo);
            con.Open();
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Teacher Registered" });
        }

        [HttpGet("GetTeacherProfile/{teacherId}")]
        public IActionResult GetTeacherProfile(int teacherId)
        {
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("sp_GetTeacherProfile", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var profile = new
                {
                    FullName = reader["FullName"].ToString(),
                    Email = reader["Email"].ToString(),
                    ExperienceYears = Convert.ToInt32(reader["ExperienceYears"]),
                    Specialization = reader["Specialization"].ToString(),
                    PhoneNumber = reader["PhoneNumber"].ToString()
                };
                return Ok(profile);
            }
            return NotFound();
        }


        [HttpPost("CreateCourse")]
        public IActionResult CreateCourse(Course course)
        {
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("sp_CreateCourse", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CourseName", course.CourseName);
            cmd.Parameters.AddWithValue("@Description", course.Description);
            cmd.Parameters.AddWithValue("@Category", course.Category);
            cmd.Parameters.AddWithValue("@CreatedByTeacherId", course.CreatedByTeacherId);
            cmd.Parameters.AddWithValue("@PdfFilePath", course.PdfFilePath ?? "");
            con.Open();
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Course Created" });
        }

        [HttpGet("GetCoursesByTeacher/{teacherId}")]
        public IActionResult GetCoursesByTeacher(int teacherId)
        {
            List<Course> courses = new();
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("sp_GetCoursesByTeacher", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
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
                    PdfFilePath = reader["PdfFilePath"].ToString()
                });
            }
            return Ok(courses);
        }

        [HttpPut("UpdateCourse")]
        public IActionResult UpdateCourse(Course course)
        {
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("sp_UpdateCourse", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CourseId", course.CourseId);
            cmd.Parameters.AddWithValue("@CreatedByTeacherId", course.CreatedByTeacherId);
            cmd.Parameters.AddWithValue("@CourseName", course.CourseName);
            cmd.Parameters.AddWithValue("@Description", course.Description);
            cmd.Parameters.AddWithValue("@Category", course.Category);
            con.Open();
            int rows = cmd.ExecuteNonQuery();
            return Ok(new { message = rows > 0 ? "Course Updated" : "Course Not Found" });
        }

        [HttpPost("AssignCourseToStudent")]
        public IActionResult AssignCourseToStudent(int studentId, int courseId)
        {
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("sp_AssignCourseToStudent", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@StudentId", studentId);
            cmd.Parameters.AddWithValue("@CourseId", courseId);
            con.Open();
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Course Assigned to Student" });
        }

        [HttpDelete("DeleteCourse")]
        public IActionResult DeleteCourse(int courseId, int teacherId)
        {
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("sp_DeleteCourse", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CourseId", courseId);
            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
            con.Open();
            int rows = cmd.ExecuteNonQuery();
            return Ok(new { message = rows > 0 ? "Course Deleted" : "Course Not Found or Unauthorized" });
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
            SqlCommand cmd = new("sp_CreateAssignment", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CourseId", assignment.CourseId);
            cmd.Parameters.AddWithValue("@TeacherId", assignment.TeacherId);
            cmd.Parameters.AddWithValue("@Title", assignment.Title);
            cmd.Parameters.AddWithValue("@Description", assignment.Description);
            cmd.Parameters.AddWithValue("@UploadFilePath", assignment.UploadFilePath ?? string.Empty);
            cmd.Parameters.AddWithValue("@DueDate", assignment.DueDate);
            con.Open();
            cmd.ExecuteNonQuery();

            return Ok(new { message = "Assignment Created with PDF file" });
        }

        [HttpPost("AssignAssignmentToStudent")]
        public IActionResult AssignAssignmentToStudent(int assignmentId, int studentId)
        {
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("sp_AssignAssignmentToStudent", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@AssignmentId", assignmentId);
            cmd.Parameters.AddWithValue("@StudentId", studentId);
            con.Open();
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Assignment assigned to student successfully" });
        }

        [HttpGet("MyAssignments/{teacherId}")]
        public IActionResult GetAssignmentsByTeacher(int teacherId)
        {
            List<Assignment> assignments = new();
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("sp_GetAssignmentsByTeacher", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
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

        [HttpPut("UpdateAssignment/{id}")]
        public IActionResult UpdateAssignment(int id, [FromBody] Assignment updatedAssignment)
        {
            if (id != updatedAssignment.AssignmentId)
                return BadRequest(new { message = "Assignment ID mismatch" });

            try
            {
                using SqlConnection con = new(_connectionString);
                SqlCommand cmd = new("sp_UpdateAssignment", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@AssignmentId", updatedAssignment.AssignmentId);
                cmd.Parameters.AddWithValue("@Title", updatedAssignment.Title ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Description", updatedAssignment.Description ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@DueDate", updatedAssignment.DueDate);
                cmd.Parameters.AddWithValue("@UploadFilePath", updatedAssignment.UploadFilePath ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CourseId", updatedAssignment.CourseId);

                con.Open();
                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                    return Ok(new { message = "Assignment updated successfully" });
                else
                    return NotFound(new { message = "Assignment not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating assignment", detail = ex.Message });
            }
        }

        [HttpDelete("DeleteAssignment/{id}")]
        public IActionResult DeleteAssignment(int id)
        {
            try
            {
                using SqlConnection con = new(_connectionString);
                SqlCommand cmd = new("sp_DeleteAssignment", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@AssignmentId", id);
                con.Open();

                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                    return Ok(new { message = "Assignment deleted successfully" });
                else
                    return NotFound(new { message = "Assignment not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting assignment", detail = ex.Message });
            }
        }


        [HttpGet("GetSubmissions/{assignmentId}")]
        public IActionResult GetSubmissions(int assignmentId)
        {
            List<AssignmentSubmission> submissions = new();
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("sp_GetSubmissionsByAssignmentId", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@AssignmentId", assignmentId);
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
            SqlCommand cmd = new("sp_GradeAssignmentSubmission", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@SubmissionId", submission.SubmissionId);
            cmd.Parameters.AddWithValue("@Grade", submission.Grade);
            cmd.Parameters.AddWithValue("@Feedback", submission.Feedback ?? "");
            con.Open();
            int rows = cmd.ExecuteNonQuery();
            return Ok(new { message = rows > 0 ? "Graded Successfully" : "Submission Not Found" });
        }


        [HttpPost("CreatePerformance")]
        public IActionResult CreatePerformance(PerformanceReport report)
        {
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("sp_CreatePerformanceReport", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@StudentId", report.StudentId);
            cmd.Parameters.AddWithValue("@CourseId", report.CourseId);
            cmd.Parameters.AddWithValue("@AverageGrade", report.AverageGrade);
            cmd.Parameters.AddWithValue("@Remarks", report.Remarks ?? "");
            con.Open();
            cmd.ExecuteNonQuery();
            return Ok(new { message = "Report Created" });
        }

        [HttpPut("UpdatePerformance")]
        public IActionResult UpdatePerformance(PerformanceReport report)
        {
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("sp_UpdatePerformanceReport", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ReportId", report.ReportId);
            cmd.Parameters.AddWithValue("@AverageGrade", report.AverageGrade);
            cmd.Parameters.AddWithValue("@Remarks", report.Remarks ?? "");
            con.Open();
            int rows = cmd.ExecuteNonQuery();
            return Ok(new { message = rows > 0 ? "Report Updated" : "Report Not Found" });
        }



        [HttpGet("SubmittedAssignments/{teacherId}")]
        public IActionResult GetSubmittedAssignments(int teacherId)
        {
            List<object> submissions = new();
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("sp_GetSubmittedAssignmentsByTeacher", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@TeacherId", teacherId);
            con.Open();
            using SqlDataReader reader = cmd.ExecuteReader();
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
            SqlCommand cmd = new("sp_GetApprovedStudents", con);
            cmd.CommandType = CommandType.StoredProcedure;
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

        [HttpGet("StudentAssignments/{studentId}")]
        public IActionResult GetStudentAssignments(int studentId)
        {
            List<object> assignments = new();
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("sp_GetStudentAssignments", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@StudentId", studentId);
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

        [HttpGet("AssignedStudentsByCourse/{courseId}")]
        public IActionResult GetAssignedStudentsByCourse(int courseId)
        {
            List<object> assignedStudents = new();
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("sp_GetAssignedStudentsByCourse", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CourseId", courseId);
            con.Open();
            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                assignedStudents.Add(new
                {
                    StudentId = reader["StudentId"],
                    StudentName = reader["StudentName"].ToString(),
                    Email = reader["Email"].ToString()
                });
            }
            return Ok(assignedStudents);
        }

        [HttpGet("AssignedStudentsByAssignment/{assignmentId}")]
        public IActionResult GetAssignedStudentsByAssignment(int assignmentId)
        {
            List<object> assignedStudents = new();
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("sp_GetAssignedStudentsByAssignment", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@AssignmentId", assignmentId);
            con.Open();
            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                assignedStudents.Add(new
                {
                    StudentId = reader["StudentId"],
                    StudentName = reader["StudentName"].ToString(),
                    Email = reader["Email"].ToString()
                });
            }
            return Ok(assignedStudents);
        }

        [HttpPost("AssignAssignmentToStudentByName")]
        public IActionResult AssignAssignmentToStudentByName(string studentName, string assignmentTitle)
        {
            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new("sp_AssignAssignmentToStudentByName", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@StudentName", studentName);
            cmd.Parameters.AddWithValue("@AssignmentTitle", assignmentTitle);
            con.Open();
            int rows = cmd.ExecuteNonQuery();
            return Ok(new { message = rows > 0 ? "✅ Assignment assigned to student successfully using names." : "❌ Student or Assignment not found." });
        }
    }
}






      


      
       