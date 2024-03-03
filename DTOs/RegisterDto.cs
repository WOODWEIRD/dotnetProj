using System.ComponentModel.DataAnnotations;

namespace API;

public class RegisterDto
{
    [Required]
    public string UserName { get; set; }

    [Required]
    [StringLength(20, MinimumLength = 4)]
    public string Password { get; set; }
    [Required] public string knownAs { get; set; }
    [Required] public string Gender { get; set; }
    [Required] public DateOnly? dateOfBirth { get; set; }
    //optional to make required work otherwise validators wont work
    //therefore min age of 18 wont take action
    //optional here means it can be null but since there are
    //validators it will never be null - client side validation works
    [Required] public string city { get; set; }
    [Required] public string country { get; set; }

}
