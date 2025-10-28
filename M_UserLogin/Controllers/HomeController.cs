using M_UserLogin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace M_UserLogin.Controllers
{
    [Authorize] // Most actions require login
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<Users> _userManager;
        private readonly SignInManager<Users> _signInManager;

        public HomeController(ILogger<HomeController> logger, UserManager<Users> userManager, SignInManager<Users> signInManager)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // Dashboard
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }

        // Privacy / Profile (passes user)
        public async Task<IActionResult> Privacy()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }

        // Welcome page (public). The view decides whether to show public hero or "welcome back" for logged in users.
        [AllowAnonymous]
        public async Task<IActionResult> Welcome()
        {
            // pass user model if logged in (null when not)
            Users user = null;
            if (_signInManager.IsSignedIn(User))
            {
                user = await _userManager.GetUserAsync(User);
            }

            return View(user);
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            return View();
        }
    }
}
