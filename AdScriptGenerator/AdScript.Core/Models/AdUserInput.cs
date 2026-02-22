using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdScript.Core.Models;

public class AdUserInput
{
    //string.Empty is used to avoid null reference exceptions
    // when the properties are accessed before being set.
    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    [RegularExpression(@"^[A-Za-z0-9]+$", ErrorMessage = "EmployeeId can only contain letters and numbers.")]
    public string EmployeeId { get; set; } = string.Empty;

    [Required, MaxLength(10)]
    [RegularExpression(@"^[A-Za-z0-9_-]+$", ErrorMessage = "Campus can only contain letters, numbers, _ or -.")]
    public string Campus { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    [RegularExpression(@"^[A-Za-z0-9_-]+$", ErrorMessage = "Team can only contain letters, numbers, _ or -.")]
    public string Team { get; set; } = string.Empty;

    // allow user to optionally include the "@" in the UPN suffix, but we will trim it off in the generator
    [Required, MaxLength(100)]
    [RegularExpression(@"^@?[A-Za-z0-9.-]+\.[A-Za-z]{2,}$", ErrorMessage = "UPN suffix must look like cats.local or @cats.local.")]
    public string UpnSuffix { get; set; } = string.Empty;
}
