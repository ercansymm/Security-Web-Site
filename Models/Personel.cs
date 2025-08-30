using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SecurityWebSite.Models;

public partial class Personel
{
    public Guid Ref { get; set; }

    public string? CardName { get; set; }

    public string? LastName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? City { get; set; }

    public bool? Working { get; set; }

    public bool? Gun { get; set; }

    public bool? Shift { get; set; }

    public int? YearsOld { get; set; }

    public byte[]? image { get; set; }

    public bool? IsPassive { get; set; }

    public bool? IsAdvice { get; set; }
}
