using AuthBI.Helpers;
using AuthBI.Models.Domain;
using AuthBI.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthBI.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }


        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl; // guarda a url de retorno
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(loginViewModel);

            // 1️⃣ Buscar usuário pelo e-mail
            var user = await _userManager.FindByEmailAsync(loginViewModel.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "E-mail ou senha inválidos.");
                return View(loginViewModel);
            }

            //Autenticar usando UserName
            var result = await _signInManager.PasswordSignInAsync(
                user.UserName,
                loginViewModel.Password,
                loginViewModel.RememberMe,
                lockoutOnFailure: true
            );

            if (result.Succeeded)
            {
                // Redireciona para a página original ou para Home
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
                

            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "Usuário bloqueado temporariamente.");
                return View(loginViewModel);
            }

            ModelState.AddModelError("", "E-mail ou senha inválidos.");
            return View(loginViewModel);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            //validar o modelo
            if (!ModelState.IsValid)
                return View(registerViewModel);

            var user = new ApplicationUser
            {
                UserName = registerViewModel.Username,
                Email = registerViewModel.Email
            };

            //criar o usuário
            var result = await _userManager.CreateAsync(user, registerViewModel.Password);

            //verificar se usuario foi criado com sucesso
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {                   
                    ModelState.AddModelError("", error.Description);
                }

                return View(registerViewModel);
            }

            // Garantir role User
            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(
                    new ApplicationRole { Name = "User" }
                );
            }

            await _userManager.AddToRoleAsync(user, "User");
            await _signInManager.SignInAsync(user, isPersistent: false);

            TempData.Success("Usuário cadastrado com sucesso!");
            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            // Lógica de recuperação aqui
            ViewBag.Message = "Se o e-mail existir, você receberá um link de recuperação.";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
