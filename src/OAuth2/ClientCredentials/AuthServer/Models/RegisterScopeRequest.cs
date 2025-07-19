namespace AuthServer.Models;

using System.ComponentModel.DataAnnotations;

public class RegisterScopeRequest
{
    [Required]
    [MaxLength(255)]
    public string Scope { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; }

    public string? Description { get; set; }
}
