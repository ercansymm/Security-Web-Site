using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SecurityWebSite.Models;

public partial class WebDe
{
    [Key]
    public Guid Ref { get; set; }

    public string? CardDescription { get; set; }
}
