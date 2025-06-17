

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

        public AdminController(IConfiguration configuration)
        {
            _configuration = configuration;
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
        public IActionResult ApproveStudent(int id)
        {
            using SqlConnection con = new(_connectionString);
            con.Open();
            SqlCommand cmd = new("UPDATE Student SET IsApproved = 1 WHERE StudentId = @id", con);
            cmd.Parameters.AddWithValue("@id", id);
            int rows = cmd.ExecuteNonQuery();
            return Ok(new { message = rows > 0 ? "Student Approved" : "Student Not Found" });
        }

        // ------------------ 4. Reject Student ------------------
        [HttpDelete("RejectStudent/{id}")]
        [Authorize]
        public IActionResult RejectStudent(int id)
        {
            using SqlConnection con = new(_connectionString);
            con.Open();
            SqlCommand cmd = new("DELETE FROM Student WHERE StudentId = @id AND IsApproved = 0", con);
            cmd.Parameters.AddWithValue("@id", id);
            int rows = cmd.ExecuteNonQuery();
            return Ok(new { message = rows > 0 ? "Pending Student Deleted" : "Student Not Found" });
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
        public IActionResult ApproveTeacher(int id)
        {
            using SqlConnection con = new(_connectionString);
            con.Open();
            SqlCommand cmd = new("UPDATE Teacher SET IsApproved = 1 WHERE TeacherId = @id", con);
            cmd.Parameters.AddWithValue("@id", id);
            int rows = cmd.ExecuteNonQuery();
            return Ok(new { message = rows > 0 ? "Teacher Approved" : "Teacher Not Found" });
        }

        // ------------------ 8. Reject Teacher ------------------
        [HttpDelete("RejectTeacher/{id}")]
        [Authorize]
        public IActionResult RejectTeacher(int id)
        {
            using SqlConnection con = new(_connectionString);
            con.Open();
            SqlCommand cmd = new("DELETE FROM Teacher WHERE TeacherId = @id AND IsApproved = 0", con);
            cmd.Parameters.AddWithValue("@id", id);
            int rows = cmd.ExecuteNonQuery();
            return Ok(new { message = rows > 0 ? "Pending Teacher Deleted" : "Teacher Not Found" });
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