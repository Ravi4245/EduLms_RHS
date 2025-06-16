using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EduLms_RHS.Models;

public partial class EduLmsGreysoftContext : DbContext
{
    public EduLmsGreysoftContext()
    {
    }

    public EduLmsGreysoftContext(DbContextOptions<EduLmsGreysoftContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Assignment> Assignments { get; set; }

    public virtual DbSet<AssignmentStudent> AssignmentStudents { get; set; }

    public virtual DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    public virtual DbSet<PerformanceReport> PerformanceReports { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentCourse> StudentCourses { get; set; }

    public virtual DbSet<Teacher> Teachers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-5S4O1AS;Initial Catalog=Edu_Lms_Greysoft;Integrated Security=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.AdminId).HasName("PK__Admin__719FE4887AA41571");

            entity.ToTable("Admin");

            entity.HasIndex(e => e.Email, "UQ__Admin__A9D105349E4A4F73").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
        });

        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PK__Assignme__32499E773512DEFB");

            entity.ToTable("Assignment");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.UploadFilePath).HasMaxLength(250);

            entity.HasOne(d => d.Course).WithMany(p => p.Assignments)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__Assignmen__Cours__3B75D760");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Assignments)
                .HasForeignKey(d => d.TeacherId)
                .HasConstraintName("FK__Assignmen__Teach__3C69FB99");
        });

        modelBuilder.Entity<AssignmentStudent>(entity =>
        {
            entity.HasKey(e => e.AssignmentStudentId).HasName("PK__Assignme__653C7FFECB7C3890");

            entity.ToTable("AssignmentStudent");

            entity.Property(e => e.AssignedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Assignment).WithMany(p => p.AssignmentStudents)
                .HasForeignKey(d => d.AssignmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Assignmen__Assig__4E88ABD4");

            entity.HasOne(d => d.Student).WithMany(p => p.AssignmentStudents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Assignmen__Stude__4F7CD00D");
        });

        modelBuilder.Entity<AssignmentSubmission>(entity =>
        {
            entity.HasKey(e => e.SubmissionId).HasName("PK__Assignme__449EE125E5E7DB14");

            entity.ToTable("AssignmentSubmission");

            entity.Property(e => e.SubmittedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SubmittedFilePath).HasMaxLength(250);

            entity.HasOne(d => d.Assignment).WithMany(p => p.AssignmentSubmissions)
                .HasForeignKey(d => d.AssignmentId)
                .HasConstraintName("FK__Assignmen__Assig__403A8C7D");

            entity.HasOne(d => d.Student).WithMany(p => p.AssignmentSubmissions)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__Assignmen__Stude__412EB0B6");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK__Course__C92D71A795CF4624");

            entity.ToTable("Course");

            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.CourseName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByTeacher).WithMany(p => p.Courses)
                .HasForeignKey(d => d.CreatedByTeacherId)
                .HasConstraintName("FK__Course__CreatedB__32E0915F");
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.EnrollmentId).HasName("PK__Enrollme__7F68771B220FAA1A");

            entity.ToTable("Enrollment");

            entity.Property(e => e.EnrollmentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Course).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__Enrollmen__Cours__37A5467C");

            entity.HasOne(d => d.Student).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__Enrollmen__Stude__36B12243");
        });

        modelBuilder.Entity<PerformanceReport>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__Performa__D5BD4805BA819137");

            entity.ToTable("PerformanceReport");

            entity.Property(e => e.GeneratedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Course).WithMany(p => p.PerformanceReports)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__Performan__Cours__45F365D3");

            entity.HasOne(d => d.Student).WithMany(p => p.PerformanceReports)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__Performan__Stude__44FF419A");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__Student__32C52B996E4A6182");

            entity.ToTable("Student");

            entity.HasIndex(e => e.Email, "UQ__Student__A9D1053443C74C50").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(250);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.GradeLevel).HasMaxLength(50);
            entity.Property(e => e.IsApproved).HasDefaultValue(false);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
            entity.Property(e => e.StudentNo).HasMaxLength(10);
        });

        modelBuilder.Entity<StudentCourse>(entity =>
        {
            entity.HasKey(e => e.StudentCourseId).HasName("PK__StudentC__7E3E2F92F1CB9F1B");

            entity.ToTable("StudentCourse");

            entity.Property(e => e.EnrollmentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Course).WithMany(p => p.StudentCourses)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentCo__Cours__4AB81AF0");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentCourses)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentCo__Stude__49C3F6B7");
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasKey(e => e.TeacherId).HasName("PK__Teacher__EDF25964C1F0EF9B");

            entity.ToTable("Teacher");

            entity.HasIndex(e => e.Email, "UQ__Teacher__A9D105344FE4959E").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsApproved).HasDefaultValue(false);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
            entity.Property(e => e.Qualification).HasMaxLength(100);
            entity.Property(e => e.Specialization).HasMaxLength(100);
            entity.Property(e => e.TeacherNo)
                .HasMaxLength(10)
                .HasColumnName("TeacherNO");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
