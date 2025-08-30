using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SecurityWebSite.Models;

public partial class User
{
    [Key]
    public Guid Ref { get; set; }

    public string? CardName { get; set; }

    public string? UserName { get; set; }

    public string? UserPassword { get; set; }

    public string? PhoneNumber { get; set; }

    public string? LastName { get; set; }

    public bool? IsPassive { get; set; }
}
