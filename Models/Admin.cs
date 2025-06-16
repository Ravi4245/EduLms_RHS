using System;
using System.Collections.Generic;

namespace EduLms_RHS.Models;

public partial class Admin
{
    public int AdminId { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? PhoneNumber { get; set; }

    public DateTime? CreatedAt { get; set; }
}
