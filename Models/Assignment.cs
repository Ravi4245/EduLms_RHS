using System;
using System.Collections.Generic;

namespace EduLms_RHS.Models;

public partial class Assignment
{
    public int AssignmentId { get; set; }

    public int? CourseId { get; set; }

    public int? TeacherId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? UploadFilePath { get; set; }

    public DateOnly? DueDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AssignmentStudent> AssignmentStudents { get; set; } = new List<AssignmentStudent>();

    public virtual ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; } = new List<AssignmentSubmission>();

    public virtual Course? Course { get; set; }

    public virtual Teacher? Teacher { get; set; }
}
