

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using EduLms_RHS.Models;

namespace Edu_LMS_Greysoft.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class AdminController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly EmailService _emailService;

        public AdminController(IConfiguration configuration, EmailService emailService)
        {
            _configuration = configuration;
            _emailService = emailService;
            _connectionString = _configuration.GetConnectionString("Lms");
        }

        // ------------------ 1. View Pending Students ------------------
        [HttpGet("PendingStudents")]
        [Authorize]
        public IActionResult GetPendingStudents()
        {
            List<Student> students = new();
            using SqlConnection con = new(_connectionString);
            con.Open();
            SqlCommand cmd = new("SELECT * FROM Student WHERE IsApproved = 0", con);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                students.Add(new Student
                {
                    StudentId = (int)reader["StudentId"],
                    FullName = reader["FullName"].ToString(),
                    Email = reader["Email"].ToString(),
                    PhoneNumber = reader["PhoneNumber"].ToString()
                });
            }
            return Ok(students);
        }

        // ------------------ 2. View Approved Students ------------------
        [HttpGet("ApprovedStudents")]
        [Authorize]
        public IActionResult GetApprovedStudents()
        {
            List<Student> students = new();
            using SqlConnection con = new(_connectionString);
            con.Open();
            SqlCommand cmd = new("SELECT * FROM Student WHERE IsApproved = 1", con);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                students.Add(new Student
                {
                    StudentId = (int)reader["StudentId"],
                    FullName = reader["FullName"].ToString(),
                    Email = reader["Email"].ToString(),
                    PhoneNumber = reader["PhoneNumber"].ToString()
                });
            }
            return Ok(students);
        }

        // ------------------ 3. Approve Student ------------------
        [HttpPut("ApproveStudent/{id}")]
        [Authorize]
        public async Task<IActionResult> ApproveStudent(int id)
        {
            try
            {
                using SqlConnection con = new(_connectionString);
                con.Open();

                // Get student details for email
                SqlCommand getCmd = new("SELECT FullName, Email FROM Student WHERE StudentId = @id", con);
                getCmd.Parameters.AddWithValue("@id", id);
                SqlDataReader reader = getCmd.ExecuteReader();

                if (!reader.Read())
                {
                    return NotFound(new { message = "❌ Student not found." });
                }

                string fullName = reader["FullName"].ToString();
                string email = reader["Email"].ToString();
                reader.Close();

                // Update approval status
                SqlCommand updateCmd = new("UPDATE Student SET IsApproved = 1 WHERE StudentId = @id", con);
                updateCmd.Parameters.AddWithValue("@id", id);
                int rowsAffected = updateCmd.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    return BadRequest(new { message = "❌ Failed to approve student." });
                }

                // Send approval email
                string subject = "✅ Your Student Account Has Been Approved!";
                string body = $@"
            <p style='font-family:Segoe UI, sans-serif; font-size:14px;'>
                Dear <strong>{fullName}</strong>,
            </p>
            <p>
                Congratulations! Your account on our <strong>Learning Management System (LMS)</strong> has been approved by the admin. 🎉
            </p>
            <p>
                You can now log in and start accessing your courses and learning materials.
            </p>
            <br/>
            <p>
                Best regards,<br/>
                <strong>RHS Team</strong> 🎓
            </p>";

                await _emailService.SendEmailAsync(email, subject, body);

                return Ok(new { message = "✅ Student approved and email notification sent." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "❌ Error occurred while approving student.",
                    error = ex.Message
                });
            }
        }


        // ------------------ 4. Reject Student ------------------
        [HttpDelete("RejectStudent/{id}")]
        [Authorize]
        public async Task<IActionResult> RejectStudent(int id)
        {
            try
            {
                using SqlConnection con = new(_connectionString);
                con.Open();

                // Step 1: Get student details for email before deleting
                SqlCommand getCmd = new("SELECT FullName, Email FROM Student WHERE StudentId = @id AND IsApproved = 0", con);
                getCmd.Parameters.AddWithValue("@id", id);
                SqlDataReader reader = getCmd.ExecuteReader();

                if (!reader.Read())
                {
                    return NotFound(new { message = "❌ Student not found or already approved." });
                }

                string fullName = reader["FullName"].ToString();
                string email = reader["Email"].ToString();
                reader.Close();

                // Step 2: Delete the student
                SqlCommand deleteCmd = new("DELETE FROM Student WHERE StudentId = @id AND IsApproved = 0", con);
                deleteCmd.Parameters.AddWithValue("@id", id);
                int rowsAffected = deleteCmd.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    return BadRequest(new { message = "❌ Failed to delete student." });
                }

                // Step 3: Send rejection email
                string subject = "❌ Your Student Registration Was Rejected";
                string body = $@"
            <p style='font-family:Segoe UI, sans-serif; font-size:14px;'>
                Dear <strong>{fullName}</strong>,
            </p>
            <p>
                We regret to inform you that your student registration request on our <strong>Learning Management System (LMS)</strong> has been rejected by the admin.
            </p>
            <p>
                If you believe this was a mistake or have questions, feel free to contact our support team.
            </p>
            <br/>
            <p>
                Best regards,<br/>
                <strong>RHS Team</strong> 🎓
            </p>";

                await _emailService.SendEmailAsync(email, subject, body);

                return Ok(new { message = "❌ Pending student deleted and rejection email sent." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "❌ Error occurred while rejecting student.",
                    error = ex.Message
                });
            }
        }


        // ------------------ 5. View Approved Teachers ------------------
        [HttpGet("ApprovedTeachers")]
        [Authorize]
        public IActionResult GetApprovedTeachers()
        {
            List<Teacher> teachers = new();
            using SqlConnection con = new(_connectionString);
            con.Open();
            SqlCommand cmd = new("SELECT * FROM Teacher WHERE IsApproved = 1", con);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                teachers.Add(new Teacher
                {
                    TeacherId = (int)reader["TeacherId"],
                    FullName = reader["FullName"].ToString(),
                    Email = reader["Email"].ToString(),
                    PhoneNumber = reader["PhoneNumber"].ToString()
                });
            }
            return Ok(teachers);
        }

        // ------------------ 6. View Pending Teachers ------------------
        [HttpGet("PendingTeachers")]
        [Authorize]
        public IActionResult GetPendingTeachers()
        {
            List<Teacher> teachers = new();
            using SqlConnection con = new(_connectionString);
            con.Open();
            SqlCommand cmd = new("SELECT * FROM Teacher WHERE IsApproved = 0", con);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                teachers.Add(new Teacher
                {
                    TeacherId = (int)reader["TeacherId"],
                    FullName = reader["FullName"].ToString(),
                    Email = reader["Email"].ToString(),
                    PhoneNumber = reader["PhoneNumber"].ToString()
                });
            }
            return Ok(teachers);
        }

        // ------------------ 7. Approve Teacher ------------------
        [HttpPut("ApproveTeacher/{id}")]
        [Authorize]
        public async Task<IActionResult> ApproveTeacher(int id)
        {
            try
            {
                using SqlConnection con = new(_connectionString);
                con.Open();

                // First, get the teacher's details
                SqlCommand getCmd = new("SELECT FullName, Email FROM Teacher WHERE TeacherId = @TeacherId", con);
                getCmd.Parameters.AddWithValue("@TeacherId", id);
                SqlDataReader reader = getCmd.ExecuteReader();

                if (!reader.Read())
                {
                    return NotFound(new { message = "❌ Teacher not found." });
                }

                string fullName = reader["FullName"].ToString();
                string email = reader["Email"].ToString();
                reader.Close();

                // Update approval status
                SqlCommand updateCmd = new("UPDATE Teacher SET IsApproved = 1 WHERE TeacherId = @TeacherId", con);
                updateCmd.Parameters.AddWithValue("@TeacherId", id);

                int rowsAffected = updateCmd.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    return BadRequest(new { message = "❌ Failed to approve teacher." });
                }

                // Send email to teacher
                string subject = "✅ Your Teacher Account Has Been Approved!";
                string body = $@"
            <p style='font-family:Segoe UI, sans-serif; font-size:14px;'>
                Dear <strong>{fullName}</strong>,
            </p>
            <p>
                Great news! Your account on our <strong>Learning Management System (LMS)</strong> has been approved by the admin. 🎉
            </p>
            <p>
                You can now log in and start managing your courses and students.
            </p>
            <p>
                Welcome aboard!
            </p>
            <br/>
            <p>
                Best regards,<br/>
                <strong>RHS Team</strong> 🎓
            </p>";

                await _emailService.SendEmailAsync(email, subject, body);

                return Ok(new { message = "✅ Teacher approved and email notification sent." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "❌ Error occurred while approving teacher.",
                    error = ex.Message
                });
            }
        }


        [HttpDelete("RejectTeacher/{id}")]
        [Authorize]
        public async Task<IActionResult> RejectTeacher(int id)
        {
            try
            {
                using SqlConnection con = new(_connectionString);
                con.Open();

                // Step 1: Get teacher details before deleting
                SqlCommand getCmd = new("SELECT FullName, Email FROM Teacher WHERE TeacherId = @id AND IsApproved = 0", con);
                getCmd.Parameters.AddWithValue("@id", id);
                SqlDataReader reader = getCmd.ExecuteReader();

                if (!reader.Read())
                {
                    return NotFound(new { message = "❌ Teacher not found or already approved." });
                }

                string fullName = reader["FullName"].ToString();
                string email = reader["Email"].ToString();
                reader.Close();

                // Step 2: Delete the teacher
                SqlCommand deleteCmd = new("DELETE FROM Teacher WHERE TeacherId = @id AND IsApproved = 0", con);
                deleteCmd.Parameters.AddWithValue("@id", id);
                int rowsAffected = deleteCmd.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    return BadRequest(new { message = "❌ Failed to delete teacher." });
                }

                // Step 3: Send rejection email
                string subject = "❌ Your Teacher Registration Was Rejected";
                string body = $@"
            <p style='font-family:Segoe UI, sans-serif; font-size:14px;'>
                Dear <strong>{fullName}</strong>,
            </p>
            <p>
                We regret to inform you that your teacher registration request on our <strong>Learning Management System (LMS)</strong> has been rejected by the admin.
            </p>
            <p>
                If you believe this was a mistake or have questions, feel free to contact our support team.
            </p>
            <br/>
            <p>
                Best regards,<br/>
                <strong>RHS Team</strong> 🎓
            </p>";

                await _emailService.SendEmailAsync(email, subject, body);

                return Ok(new { message = "❌ Pending teacher deleted and rejection email sent." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "❌ Error occurred while rejecting teacher.",
                    error = ex.Message
                });
            }
        }


        // ------------------ 9. Get All Courses ------------------
        [HttpGet("Courses")]
        public IActionResult GetCourses()
        {
            List<Course> courses = new();
            using SqlConnection con = new(_connectionString);
            con.Open();
            SqlCommand cmd = new("SELECT * FROM Course", con);
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

        // ------------------ 10. Delete Course ------------------



        // ------------------ 11. Edit/Update Course ------------------
        [HttpPut("UpdateCourse/{id}")]
        public IActionResult UpdateCourse(int id, [FromBody] Course updatedCourse)
        {
            using SqlConnection con = new(_connectionString);
            con.Open();
            SqlCommand cmd = new("UPDATE Course SET CourseName = @CourseName, Description = @Description, Category = @Category WHERE CourseId = @id", con);
            cmd.Parameters.AddWithValue("@CourseName", updatedCourse.CourseName);
            cmd.Parameters.AddWithValue("@Description", updatedCourse.Description);
            cmd.Parameters.AddWithValue("@Category", updatedCourse.Category);
            cmd.Parameters.AddWithValue("@id", id);
            int rows = cmd.ExecuteNonQuery();
            return Ok(new { message = rows > 0 ? "Course Updated Successfully" : "Course Not Found" });
        }

        [HttpDelete("DeleteCourse/{id}")]
        public IActionResult DeleteCourse(int id)
        {
            using SqlConnection con = new(_connectionString);
            con.Open();

            // First, delete all StudentCourse entries for this CourseId
            SqlCommand deleteRelated = new("DELETE FROM StudentCourse WHERE CourseId = @id", con);
            deleteRelated.Parameters.AddWithValue("@id", id);
            deleteRelated.ExecuteNonQuery();

            // Now delete the Course itself
            SqlCommand deleteCourse = new("DELETE FROM Course WHERE CourseId = @id", con);
            deleteCourse.Parameters.AddWithValue("@id", id);
            int rows = deleteCourse.ExecuteNonQuery();

            if (rows > 0)
            {
                return Ok(new { message = "Course deleted successfully" });
            }
            else
            {
                return NotFound(new { message = "Course not found" });
            }
        }



        // ------------------ 12. View Student Performance ------------------
        [HttpGet("StudentPerformance")]
        public IActionResult GetPerformanceReports()
        {
            List<PerformanceReport> reports = new();
            using SqlConnection con = new(_connectionString);
            con.Open();
            SqlCommand cmd = new("SELECT * FROM PerformanceReport", con);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                reports.Add(new PerformanceReport
                {
                    ReportId = (int)reader["ReportId"],
                    StudentId = (int)reader["StudentId"],
                    CourseId = (int)reader["CourseId"],
                    AverageGrade = Convert.ToDouble(reader["AverageGrade"]),
                    Remarks = reader["Remarks"].ToString()
                });
            }
            return Ok(reports);
        }

        // ------------------ 13. Delete Approved Student ------------------
        [HttpDelete("DeleteApprovedStudent/{id}")]
        [Authorize]
        public IActionResult DeleteApprovedStudent(int id)
        {
            using SqlConnection con = new(_connectionString);
            con.Open();

            SqlCommand checkCmd = new("SELECT COUNT(*) FROM Student WHERE StudentId = @id AND IsApproved = 1", con);
            checkCmd.Parameters.AddWithValue("@id", id);
            int exists = (int)checkCmd.ExecuteScalar();

            if (exists == 0)
            {
                return NotFound(new { message = "Approved Student Not Found" });
            }

            SqlCommand deleteCmd = new("DELETE FROM Student WHERE StudentId = @id AND IsApproved = 1", con);
            deleteCmd.Parameters.AddWithValue("@id", id);
            deleteCmd.ExecuteNonQuery();

            return Ok(new { message = "Approved Student Deleted Successfully" });
        }

        // ------------------ 14. Delete Approved Teacher ------------------
        [HttpDelete("DeleteApprovedTeacher/{id}")]
        [Authorize]
        public IActionResult DeleteApprovedTeacher(int id)
        {
            using SqlConnection con = new(_connectionString);
            con.Open();

            SqlCommand checkCmd = new("SELECT COUNT(*) FROM Teacher WHERE TeacherId = @id AND IsApproved = 1", con);
            checkCmd.Parameters.AddWithValue("@id", id);
            int exists = (int)checkCmd.ExecuteScalar();

            if (exists == 0)
            {
                return NotFound(new { message = "Approved Teacher Not Found" });
            }

            SqlCommand deleteCmd = new("DELETE FROM Teacher WHERE TeacherId = @id AND IsApproved = 1", con);
            deleteCmd.Parameters.AddWithValue("@id", id);
            deleteCmd.ExecuteNonQuery();

            return Ok(new { message = "Approved Teacher Deleted Successfully" });
        }

    }
}