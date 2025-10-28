using System.ComponentModel.DataAnnotations;

namespace M_UserLogin.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "The {0} must be at {2} and at max {1} characters long.")]
        [DataType(DataType.Password)]
        [Compare("ConfirmPassword", ErrorMessage = "Password does not match.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        // 🧩 NEW FIELD: Secret code to validate password change requests
        [Required(ErrorMessage = "Secret Code is required.")]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "Secret Code must be between 4 and 20 characters.")]
        [Display(Name = "Secret Code")]
        public string SecretCode { get; set; }
    }
}
