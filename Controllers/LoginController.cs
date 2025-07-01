
using EduLms_RHS.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[Route("api/[controller]")]
[ApiController]
public class LoginController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public LoginController(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("Lms");
    }

   
    private string GenerateJwtToken(string role, int id, string name)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Role, role),
            new Claim(ClaimTypes.NameIdentifier, id.ToString()),
            new Claim(ClaimTypes.Name, name)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Issuer"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpPost("Login")]
    public IActionResult Login(LoginDto loginDto)
    {
        using SqlConnection con = new(_connectionString);
        con.Open();

        using SqlCommand cmd = new("sp_LoginUser", con);
        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@Email", loginDto.Email);
        cmd.Parameters.AddWithValue("@Password", loginDto.Password);

        using SqlDataReader reader = cmd.ExecuteReader();

        // 1. Admin
        if (reader.Read())
        {
            var id = (int)reader["Id"];
            var name = reader["FullName"].ToString();
            var token = GenerateJwtToken("Admin", id, name);

            return Ok(new
            {
                Role = "Admin",
                Id = id,
                Name = name,
                Token = token,
                Message = "✅ Login successful as Admin"
            });
        }

        // 2. Teacher
        if (reader.NextResult() && reader.Read())
        {
            if ((bool)reader["IsApproved"])
            {
                var id = (int)reader["Id"];
                var name = reader["FullName"].ToString();

                return Ok(new
                {
                    Role = "Teacher",
                    Id = id,
                    Name = name,
                    Message = "✅ Login successful as Teacher"
                });
            }
            else
            {
                return Unauthorized(new { message = "Teacher not approved by admin." });
            }
        }

        // 3. Student
        if (reader.NextResult() && reader.Read())
        {
            if ((bool)reader["IsApproved"])
            {
                var id = (int)reader["Id"];
                var name = reader["FullName"].ToString();

                return Ok(new
                {
                    Role = "Student",
                    Id = id,
                    Name = name,
                    Message = "✅ Login successful as Student"
                });
            }
            else
            {
                return Unauthorized(new { message = "Student not approved by admin." });
            }
        }

        return Unauthorized(new { message = "Invalid email or password." });
    }

}