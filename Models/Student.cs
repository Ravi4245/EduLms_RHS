using System;
using System.Collections.Generic;

namespace EduLms_RHS.Models;

public partial class Student
{
    public int StudentId { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? PhoneNumber { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? Address { get; set; }

    public string? GradeLevel { get; set; }

    public string? StudentNo { get; set; }

    public bool? IsApproved { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AssignmentStudent> AssignmentStudents { get; set; } = new List<AssignmentStudent>();

    public virtual ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; } = new List<AssignmentSubmission>();

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<PerformanceReport> PerformanceReports { get; set; } = new List<PerformanceReport>();

    public virtual ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();
}
