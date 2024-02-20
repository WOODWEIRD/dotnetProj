using System.ComponentModel.DataAnnotations;

namespace API;

public class RegisterDto
{
    [Required]
    public string UserName { get; set; }
    [Required]
    [StringLength(20, MinimumLength = 4)]
    public string Password { get; set; }
}
