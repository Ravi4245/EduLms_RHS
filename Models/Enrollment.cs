using System;
using System.Collections.Generic;

namespace EduLms_RHS.Models;

public partial class Enrollment
{
    public int EnrollmentId { get; set; }

    public int? StudentId { get; set; }

    public int? CourseId { get; set; }

    public DateTime? EnrollmentDate { get; set; }

    public virtual Course? Course { get; set; }

    public virtual Student? Student { get; set; }
}
