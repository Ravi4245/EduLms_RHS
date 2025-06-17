using System.ComponentModel.DataAnnotations;

namespace EduLms_RHS.Dto
{
    public class CourseFormDto
    {
        [Required]
        public string CourseName { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        public int CreatedByTeacherId { get; set; }

        public IFormFile? PdfFile { get; set; }
    }

}