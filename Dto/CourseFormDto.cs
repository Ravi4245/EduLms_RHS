namespace EduLms_RHS.Dto
{
    public class CourseFormDto
    {
        public string CourseName { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public int CreatedByTeacherId { get; set; }
        public IFormFile? PdfFile { get; set; }
    }
}