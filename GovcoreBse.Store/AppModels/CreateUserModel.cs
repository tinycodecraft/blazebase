using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GovcoreBse.Store.AppModels;

#nullable disable warnings
public class CreateUserModel
{

    [Required(ErrorMessage = "Please Enter User ID..")]
    [Display(Name = "User ID")]

    public string UserId { get; set; }

    [Required(ErrorMessage = "Please Enter Username..")]
    [Display(Name = "User Name")]

    public string UserName { get; set; }

    [Required(ErrorMessage = "Please Enter Password...")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]

    public string Pwd { get; set; }

    [Required(ErrorMessage = "Please Enter the Confirm Password...")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("Pwd")]
    public string Confirmpwd { get; set; }

    [RegularExpression("^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$", ErrorMessage = "Invalid Email Format")]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Please Enter the Cotnact No ...")]
    [Display(Name = "Contact No")]
    public string Tel { get; set; }


    [Required(ErrorMessage = "Please Enter the Post ...")]
    [Display(Name = "Post")]
    [Compare(nameof(UserId), ErrorMessage = "User ID must equal to Post. Please input again before register.")]
    public string Post { get; set; }

    [Required(ErrorMessage = "Please Enter Is Admin ...")]
    [Display(Name = "Admin?")]
    public bool IsAdmin { get; set; }

}
