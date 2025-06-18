using EduLms_RHS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace EduLms_RHS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly string _connectionString;

        public FeedbackController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Lms");
        }

        [HttpPost("Submit")]
        public IActionResult SubmitFeedback([FromBody] Feedback feedback)
        {
            if (string.IsNullOrWhiteSpace(feedback.Email) || string.IsNullOrWhiteSpace(feedback.Message))
                return BadRequest("Email and Message are required.");

            using SqlConnection con = new(_connectionString);
            SqlCommand cmd = new(
                "INSERT INTO Feedback (Email, Message) VALUES (@Email, @Message)", con);
            cmd.Parameters.AddWithValue("@Email", feedback.Email);
            cmd.Parameters.AddWithValue("@Message", feedback.Message);

            con.Open();
            cmd.ExecuteNonQuery();

            return Ok(new { message = "Feedback submitted successfully!" });
        }
    }
}
