using System;
using System.Collections.Generic;

namespace EduLms_RHS.Models;

public partial class PerformanceReport
{
    public int ReportId { get; set; }

    public int? StudentId { get; set; }

    public int? CourseId { get; set; }

    public double? AverageGrade { get; set; }

    public DateTime? GeneratedAt { get; set; }

    public string? Remarks { get; set; }

    public virtual Course? Course { get; set; }

    public virtual Student? Student { get; set; }
}
