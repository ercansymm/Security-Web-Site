using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SecurityWebSite.Models;

public partial class Firm
{
    [Key]
    public Guid Ref { get; set; }

    public Guid? PersonelRef { get; set; }

    public string? CardName { get; set; }

    public string? PhoneNumber { get; set; }

    public bool? IsPassive { get; set; }
}
