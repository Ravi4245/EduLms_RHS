using System;
using System.Collections.Generic;

namespace EduLms_RHS.Models;

public partial class Teacher
{
    public int TeacherId { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Qualification { get; set; }

    public int? ExperienceYears { get; set; }

    public string? Specialization { get; set; }

    public string? TeacherNo { get; set; }

    public bool? IsApproved { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
