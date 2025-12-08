using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SWI2.Models.Authentication
{
    public class RegisterViewModel
    {

		[Required]
        [Display(Name = "name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "surname")]
        public string Surname { get; set; }

        [Required]
        [Display(Name = "login")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        [Display(Name = "email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "confirmPassword")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        [Display(Name = "clientURI")]
        public string ClientURI { get; set; }
        [Display(Name = "language")]
        public string Language { get; set; }
    }
}
