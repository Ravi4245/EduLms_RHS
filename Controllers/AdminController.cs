

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using EduLms_RHS.Models;
using Microsoft.EntityFrameworkCore;

namespace Edu_LMS_Greysoft.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly string _connStr;
        private readonly EmailService _emailService;

        public AdminController(IConfiguration config, EmailService emailService)
        {
            _config = config;
            _connStr = _config.GetConnectionString("Lms");
            _emailService = emailService;
        }

        [HttpGet("PendingStudents"), Authorize]
        public IActionResult GetPendingStudents()
        {
            using var con = new SqlConnection(_connStr);
            using var cmd = new SqlCommand("sp_GetPendingStudents", con) { CommandType = CommandType.StoredProcedure };
            con.Open();
            var list = new List<Student>();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                list.Add(new Student
                {
                    StudentId = (int)rdr["StudentId"],
                    FullName = rdr["FullName"].ToString(),
                    Email = rdr["Email"].ToString(),
                    PhoneNumber = rdr["PhoneNumber"].ToString()
                });
            return Ok(list);
        }

        [HttpGet("ApprovedStudents"), Authorize]
        public IActionResult GetApprovedStudents()
        {
            using var con = new SqlConnection(_connStr);
            using var cmd = new SqlCommand("sp_GetApprovedStudentsa", con) { CommandType = CommandType.StoredProcedure };
            con.Open();
            var list = new List<Student>();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                list.Add(new Student
                {
                    StudentId = (int)rdr["StudentId"],
                    FullName = rdr["FullName"].ToString(),
                    Email = rdr["Email"].ToString(),
                    PhoneNumber = rdr["PhoneNumber"].ToString()
                });
            return Ok(list);
        }

        [HttpPut("ApproveStudent/{id}"), Authorize]
        public async Task<IActionResult> ApproveStudent(int id)
        {
            using var con = new SqlConnection(_connStr);
            con.Open();
            using var getCmd = new SqlCommand("SELECT FullName, Email FROM Student WHERE StudentId = @Id", con);
            getCmd.Parameters.AddWithValue("@Id", id);
            using var rdr = getCmd.ExecuteReader();
            if (!rdr.Read())
                return NotFound(new { message = "❌ Student not found." });
            var fullName = rdr["FullName"].ToString();
            var email = rdr["Email"].ToString();
            rdr.Close();

            using var cmd = new SqlCommand("sp_ApproveStudent", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Id", id);
            int affected = (int)cmd.ExecuteScalar();
            if (affected == 0)
                return BadRequest(new { message = "❌ Approval failed." });

            string subject = "✅ Your Student Account Has Been Approved!";
            string body = $"<p>Dear <strong>{fullName}</strong>, your student account is now approved.</p>";
            await _emailService.SendEmailAsync(email, subject, body);

            return Ok(new { message = "✅ Approved & notified." });
        }

        [HttpDelete("RejectStudent/{id}"), Authorize]
        public async Task<IActionResult> RejectStudent(int id)
        {
            using var con = new SqlConnection(_connStr);
            con.Open();
            using var getCmd = new SqlCommand("SELECT FullName, Email FROM Student WHERE StudentId = @Id AND IsApproved = 0", con);
            getCmd.Parameters.AddWithValue("@Id", id);
            using var rdr = getCmd.ExecuteReader();
            if (!rdr.Read())
                return NotFound(new { message = "❌ Not found or already approved." });
            var fullName = rdr["FullName"].ToString();
            var email = rdr["Email"].ToString();
            rdr.Close();

            using var cmd = new SqlCommand("sp_RejectStudent", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Id", id);
            int affected = (int)cmd.ExecuteScalar();
            if (affected == 0)
                return BadRequest(new { message = "❌ Delete failed." });

            string subject = "❌ Your Student Registration Was Rejected";
            string body = $"<p>Dear <strong>{fullName}</strong>, we regret to inform you that your registration was rejected.</p>";
            await _emailService.SendEmailAsync(email, subject, body);

            return Ok(new { message = "❌ Rejected & notified." });
        }

        [HttpGet("ApprovedTeachers"), Authorize]
        public IActionResult GetApprovedTeachers()
        {
            using var con = new SqlConnection(_connStr);
            using var cmd = new SqlCommand("sp_GetApprovedTeachersa", con) { CommandType = CommandType.StoredProcedure };
            con.Open();
            var list = new List<Teacher>();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                list.Add(new Teacher
                {
                    TeacherId = (int)rdr["TeacherId"],
                    FullName = rdr["FullName"].ToString(),
                    Email = rdr["Email"].ToString(),
                    PhoneNumber = rdr["PhoneNumber"].ToString()
                });
            return Ok(list);
        }

        [HttpGet("PendingTeachers"), Authorize]
        public IActionResult GetPendingTeachers()
        {
            using var con = new SqlConnection(_connStr);
            using var cmd = new SqlCommand("sp_GetPendingTeachers", con) { CommandType = CommandType.StoredProcedure };
            con.Open();
            var list = new List<Teacher>();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                list.Add(new Teacher
                {
                    TeacherId = (int)rdr["TeacherId"],
                    FullName = rdr["FullName"].ToString(),
                    Email = rdr["Email"].ToString(),
                    PhoneNumber = rdr["PhoneNumber"].ToString()
                });
            return Ok(list);
        }

        [HttpPut("ApproveTeacher/{id}"), Authorize]
        public async Task<IActionResult> ApproveTeacher(int id)
        {
            using var con = new SqlConnection(_connStr);
            con.Open();
            using var getCmd = new SqlCommand("SELECT FullName, Email FROM Teacher WHERE TeacherId = @Id", con);
            getCmd.Parameters.AddWithValue("@Id", id);
            using var rdr = getCmd.ExecuteReader();
            if (!rdr.Read())
                return NotFound(new { message = "❌ Teacher not found." });
            var fullName = rdr["FullName"].ToString();
            var email = rdr["Email"].ToString();
            rdr.Close();

            using var cmd = new SqlCommand("sp_ApproveTeacher", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Id", id);
            if ((int)cmd.ExecuteScalar() == 0)
                return BadRequest(new { message = "❌ Approval failed." });

            string subject = "✅ Your Teacher Account Has Been Approved!";
            string body = $"<p>Dear <strong>{fullName}</strong>, your teacher account is now approved.</p>";
            await _emailService.SendEmailAsync(email, subject, body);

            return Ok(new { message = "✅ Approved & notified." });
        }

        [HttpDelete("RejectTeacher/{id}"), Authorize]
        public async Task<IActionResult> RejectTeacher(int id)
        {
            using var con = new SqlConnection(_connStr);
            con.Open();
            using var getCmd = new SqlCommand("SELECT FullName, Email FROM Teacher WHERE TeacherId = @Id AND IsApproved = 0", con);
            getCmd.Parameters.AddWithValue("@Id", id);
            using var rdr = getCmd.ExecuteReader();
            if (!rdr.Read())
                return NotFound(new { message = "❌ Not found or already approved." });
            var fullName = rdr["FullName"].ToString();
            var email = rdr["Email"].ToString();
            rdr.Close();

            using var cmd = new SqlCommand("sp_RejectTeacher", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Id", id);
            if ((int)cmd.ExecuteScalar() == 0)
                return BadRequest(new { message = "❌ Delete failed." });

            string subject = "❌ Your Teacher Registration Was Rejected";
            string body = $"<p>Dear <strong>{fullName}</strong>, we regret to inform you that your registration was rejected.</p>";
            await _emailService.SendEmailAsync(email, subject, body);

            return Ok(new { message = "❌ Rejected & notified." });
        }

        [HttpGet("Courses")]
        public IActionResult GetCourses()
        {
            using var con = new SqlConnection(_connStr);
            using var cmd = new SqlCommand("sp_GetCourses", con) { CommandType = CommandType.StoredProcedure };
            con.Open();
            var list = new List<Course>();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                list.Add(new Course
                {
                    CourseId = (int)rdr["CourseId"],
                    CourseName = rdr["CourseName"].ToString(),
                    Description = rdr["Description"].ToString(),
                    Category = rdr["Category"].ToString(),
                    CreatedByTeacherId = (int)rdr["CreatedByTeacherId"]
                });
            return Ok(list);
        }

        [HttpPut("UpdateCourse/{id}")]
        public IActionResult UpdateCourse(int id, [FromBody] Course updatedCourse)
        {
            using var con = new SqlConnection(_connStr);
            using var cmd = new SqlCommand("sp_UpdateCoursea", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@CourseName", updatedCourse.CourseName);
            cmd.Parameters.AddWithValue("@Description", updatedCourse.Description);
            cmd.Parameters.AddWithValue("@Category", updatedCourse.Category);
            con.Open();
            int affected = (int)cmd.ExecuteScalar();
            return Ok(new { message = affected > 0 ? "Course Updated Successfully" : "Course Not Found" });
        }

        [HttpDelete("DeleteCourse/{id}")]
        public IActionResult DeleteCourse(int id)
        {
            using var con = new SqlConnection(_connStr);
            using var cmd = new SqlCommand("sp_DeleteCoursea", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Id", id);
            con.Open();
            int affected = (int)cmd.ExecuteScalar();
            return affected > 0
                ? Ok(new { message = "Course deleted successfully" })
                : NotFound(new { message = "Course not found" });
        }

        [HttpGet("StudentPerformance")]
        public IActionResult GetPerformanceReports()
        {
            using var con = new SqlConnection(_connStr);
            using var cmd = new SqlCommand("sp_GetPerformanceReports", con) { CommandType = CommandType.StoredProcedure };
            con.Open();
            var list = new List<PerformanceReport>();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                list.Add(new PerformanceReport
                {
                    ReportId = (int)rdr["ReportId"],
                    StudentId = (int)rdr["StudentId"],
                    CourseId = (int)rdr["CourseId"],
                    AverageGrade = Convert.ToDouble(rdr["AverageGrade"]),
                    Remarks = rdr["Remarks"].ToString()
                });
            return Ok(list);
        }

        [HttpDelete("DeleteApprovedStudent/{id}"), Authorize]
        public IActionResult DeleteApprovedStudent(int id)
        {
            using var con = new SqlConnection(_connStr);
            using var cmd = new SqlCommand("sp_DeleteApprovedStudent", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Id", id);
            con.Open();
            int affected = (int)cmd.ExecuteScalar();
            return affected > 0
                ? Ok(new { message = "Approved Student Deleted Successfully" })
                : NotFound(new { message = "Approved Student Not Found" });
        }

        [HttpDelete("DeleteApprovedTeacher/{id}"), Authorize]
        public IActionResult DeleteApprovedTeacher(int id)
        {
            using var con = new SqlConnection(_connStr);
            using var cmd = new SqlCommand("sp_DeleteApprovedTeacher", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Id", id);
            con.Open();
            int affected = (int)cmd.ExecuteScalar();
            return affected > 0
                ? Ok(new { message = "Approved Teacher Deleted Successfully" })
                : NotFound(new { message = "Approved Teacher Not Found" });
        }





        [HttpPut("UpdateStudent/{id}"), Authorize]
        public IActionResult UpdateStudent(int id, [FromBody] Student updatedStudent)
        {
            using var con = new SqlConnection(_connStr);
            con.Open();

            using var cmd = new SqlCommand("UpdateStudent", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@StudentId", id);
            cmd.Parameters.AddWithValue("@FullName", updatedStudent.FullName);
            cmd.Parameters.AddWithValue("@Email", updatedStudent.Email);
            cmd.Parameters.AddWithValue("@PhoneNumber", (object)updatedStudent.PhoneNumber ?? DBNull.Value);

            // Use ExecuteScalar since the SP returns SELECT @@ROWCOUNT
            int rowsAffected = (int)(cmd.ExecuteScalar() ?? 0);

            if (rowsAffected > 0)
                return Ok(new { message = "✅ Student updated successfully." });
            else
                return NotFound(new { message = "❌ Student not found or not approved." });
        }



        [HttpPut("UpdateTeacher/{id}"), Authorize]
        public IActionResult UpdateTeacher(int id, [FromBody] Teacher updatedTeacher)
        {
            using var con = new SqlConnection(_connStr);
            con.Open();

            using var cmd = new SqlCommand("UpdateTeacher", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@TeacherId", id);
            cmd.Parameters.AddWithValue("@FullName", updatedTeacher.FullName);
            cmd.Parameters.AddWithValue("@Email", updatedTeacher.Email);
            cmd.Parameters.AddWithValue("@PhoneNumber", (object)updatedTeacher.PhoneNumber ?? DBNull.Value);

            // Use ExecuteScalar since the SP returns SELECT @@ROWCOUNT
            int rowsAffected = (int)(cmd.ExecuteScalar() ?? 0);

            if (rowsAffected > 0)
                return Ok(new { message = "✅ Teacher updated successfully." });
            else
                return NotFound(new { message = "❌ Teacher not found or not approved." });
        }







        // ✅ Check if a student can be deleted
        [HttpGet("CanDeleteStudent/{id}"), Authorize]
        public IActionResult CanDeleteStudent(int id)
        {
            using var con = new SqlConnection(_connStr);
            con.Open();

            var cmd = new SqlCommand(@"
        SELECT CASE 
            WHEN EXISTS (SELECT 1 FROM StudentCourse WHERE StudentId = @Id)
              OR EXISTS (SELECT 1 FROM AssignmentSubmission WHERE StudentId = @Id)
            THEN 0 ELSE 1 END", con);

            cmd.Parameters.AddWithValue("@Id", id);
            int canDelete = (int)cmd.ExecuteScalar();
            return Ok(new { canDelete = canDelete == 1 });
        }

        // ✅ Check if a teacher can be deleted
        [HttpGet("CanDeleteTeacher/{id}"), Authorize]
        public IActionResult CanDeleteTeacher(int id)
        {
            using var con = new SqlConnection(_connStr);
            con.Open();

            var cmd = new SqlCommand(@"
        SELECT CASE 
            WHEN EXISTS (SELECT 1 FROM Course WHERE CreatedByTeacherId = @Id)
            THEN 0 ELSE 1 END", con);

            cmd.Parameters.AddWithValue("@Id", id);
            int canDelete = (int)cmd.ExecuteScalar();
            return Ok(new { canDelete = canDelete == 1 });
        }

        // ✅ Check if a course can be deleted
        [HttpGet("CanDeleteCourse/{id}"), Authorize]
        public IActionResult CanDeleteCourse(int id)
        {
            using var con = new SqlConnection(_connStr);
            con.Open();

            var cmd = new SqlCommand(@"
        SELECT CASE 
            WHEN EXISTS (SELECT 1 FROM StudentCourse WHERE CourseId = @Id)
              OR EXISTS (SELECT 1 FROM Assignment WHERE CourseId = @Id)
            THEN 0 ELSE 1 END", con);

            cmd.Parameters.AddWithValue("@Id", id);
            int canDelete = (int)cmd.ExecuteScalar();
            return Ok(new { canDelete = canDelete == 1 });
        }



    }

}