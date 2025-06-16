using System;
using System.Collections.Generic;

namespace EduLms_RHS.Models;

public partial class StudentCourse
{
    public int StudentCourseId { get; set; }

    public int StudentId { get; set; }

    public int CourseId { get; set; }

    public DateTime? EnrollmentDate { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
