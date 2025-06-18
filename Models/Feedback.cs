using System;
using System.Collections.Generic;

namespace EduLms_RHS.Models;

public partial class Feedback
{
    public int FeedbackId { get; set; }

    public string Email { get; set; } = null!;

    public string Message { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }
}
