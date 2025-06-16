using System;
using System.Collections.Generic;

namespace EduLms_RHS.Models;

public partial class Course
{
    public int CourseId { get; set; }

    public string? CourseName { get; set; }

    public string? Description { get; set; }

    public string? Category { get; set; }

    public int? CreatedByTeacherId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    public virtual Teacher? CreatedByTeacher { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<PerformanceReport> PerformanceReports { get; set; } = new List<PerformanceReport>();

    public virtual ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();
}
