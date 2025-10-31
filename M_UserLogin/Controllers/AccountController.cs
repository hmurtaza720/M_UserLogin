using M_UserLogin.Models;
using M_UserLogin.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace M_UserLogin.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Users> signInManager;
        private readonly UserManager<Users> userManager;

        public AccountController(SignInManager<Users> signInManager, UserManager<Users> userManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
        }

        // ========================== LOGIN ==========================
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(
                    model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                    return RedirectToAction("Index", "Home");

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        // ========================== REGISTER ==========================
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                Users users = new Users()
                {
                    FullName = model.Name,
                    Email = model.Email,
                    UserName = model.Email,
                    SecretCode = model.SecretCode
                };

                var result = await userManager.CreateAsync(users, model.Password);

                if (result.Succeeded)
                    return RedirectToAction("Login", "Account");

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        // ========================== VERIFY EMAIL (FORGOT PASSWORD STEP 1) ==========================
        [HttpGet]
        public IActionResult VerifyEmail()
        {
            return View(); // Shows VerifyEmail.cshtml
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "❌ Email not found in our system.");
                return View(model);
            }

            // Redirect to Change Password page with username (email)
            return RedirectToAction("ChangePassword", "Account", new { username = user.UserName });
        }

        // ========================== CHANGE PASSWORD (FORGOT PASSWORD STEP 2) ==========================
        [HttpGet]
        public IActionResult ChangePassword(string username)
        {
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("VerifyEmail", "Account");

            return View(new ChangePasswordViewModel { Email = username });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Please fill in all required fields correctly.");
                return View(model);
            }

            var user = await userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "❌ Email not found.");
                return View(model);
            }

            // Verify Secret Code before changing password
            if (user.SecretCode != model.SecretCode)
            {
                ModelState.AddModelError("", "❌ Invalid Secret Code!");
                return View(model);
            }

            // Remove old password and set new one
            var removeResult = await userManager.RemovePasswordAsync(user);
            if (removeResult.Succeeded)
            {
                var addResult = await userManager.AddPasswordAsync(user, model.NewPassword);
                if (addResult.Succeeded)
                {
                    TempData["Message"] = "✅ Password changed successfully!";
                    return RedirectToAction("Login", "Account");
                }

                foreach (var error in addResult.Errors)
                    ModelState.AddModelError("", error.Description);
            }
            else
            {
                foreach (var error in removeResult.Errors)
                    ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        // ========================== LOGOUT ==========================
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Welcome", "Home");
        }
    }
}
