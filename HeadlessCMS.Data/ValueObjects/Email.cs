using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace HeadlessCMS.Data.ValueObjects;

[Owned]
public  record Email
{
    [MaxLength(255)]
    public string Value { get; init; }

    public bool IsVerified { get; set; } = false;

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.Contains("@"))
            throw new ArgumentException("Invalid email");
        
        Value = value;
    }
}