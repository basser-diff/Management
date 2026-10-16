using System.ComponentModel.DataAnnotations;

namespace Resto.Client.Models;

public class Manager
{
    public int Id { get; set; }

    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;
}