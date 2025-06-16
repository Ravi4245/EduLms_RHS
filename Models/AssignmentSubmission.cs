using System;
using System.Collections.Generic;

namespace EduLms_RHS.Models;

public partial class AssignmentSubmission
{
    public int SubmissionId { get; set; }

    public int? AssignmentId { get; set; }

    public int? StudentId { get; set; }

    public string? SubmittedFilePath { get; set; }

    public DateTime? SubmittedDate { get; set; }

    public int? Grade { get; set; }

    public string? Feedback { get; set; }

    public virtual Assignment? Assignment { get; set; }

    public virtual Student? Student { get; set; }
}
