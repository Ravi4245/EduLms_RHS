namespace EduLms_RHS.Dto
{
    public class LoginResponse
    {
        public string Role { get; set; } // Admin / Teacher / Student
        public int Id { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }

        public string Token { get; set; }
    }
}