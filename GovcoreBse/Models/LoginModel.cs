using System.ComponentModel.DataAnnotations;

namespace GovcoreBse.Models
{
    public class LoginModel
    {
        [Required]
        [Display(Name ="UserId",ResourceType = typeof(GovcoreBse.Resources.SharedResource))]
        public string UserId { get; set; }

        [Required, MinLength(5)]
        [Display(Name = "Password", ResourceType = typeof(GovcoreBse.Resources.SharedResource))]
        public string Password { get; set; }
    }
}
