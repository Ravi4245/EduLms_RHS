using System;
using System.Collections.Generic;

namespace EduLms_RHS.Models;

public partial class AssignmentStudent
{
    public int AssignmentStudentId { get; set; }

    public int AssignmentId { get; set; }

    public int StudentId { get; set; }

    public DateTime? AssignedDate { get; set; }

    public virtual Assignment Assignment { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
