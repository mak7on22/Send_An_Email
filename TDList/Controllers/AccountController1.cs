using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using TDList.Models;
using TDList.Servises;
using TDList.ViewModels;

namespace TDList.Controllers
{
        [ResponseCache(CacheProfileName = "Cashing")]
        public class AccountController : Controller
        {
            private readonly TDLContext _context;
            private readonly UserManager<User> _userManager;
            private readonly SignInManager<User> _signInManager;
            private readonly IEmailSendler _emailSendler;
            public AccountController(UserManager<User> userManager, IEmailSendler emailSendler, SignInManager<User> signInManager, TDLContext context)
            {
                _userManager = userManager;
                _signInManager = signInManager;
                _context = context;
                _emailSendler = emailSendler;
            }
            [HttpGet]
            public IActionResult Register()
            {
                return View();
            }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUserLogin = await _userManager.FindByNameAsync(model.Login);
                var existingUserEmail = await _userManager.FindByEmailAsync(model.Email);

                if (existingUserLogin != null)
                    ModelState.AddModelError(string.Empty, "Пользователь с таким Login уже существует.");
                if (existingUserEmail != null)
                    ModelState.AddModelError(string.Empty, "Пользователь с таким Email уже существует.");
                if (existingUserLogin != null || existingUserEmail != null)
                    return View(model);
                User user = new User
                {
                    Login = model.Login,
                    Email = model.Email,
                    UserName = model.Login,
                    RoleName = "Member"
                };
                IdentityResult result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    Random random = new();
                    int confirmationCode = random.Next(1000, 10000);
                    string codeAsString = confirmationCode.ToString();
                    char[] codeArray = codeAsString.ToCharArray();
                    char digit1 = codeArray[0];
                    char digit2 = codeArray[1];
                    char digit3 = codeArray[2];
                    char digit4 = codeArray[3];
                    string message = $"<div style=\"background-color: #181530; border-radius: 20px; padding: 20px; display: flex; flex-direction: column; align-items: center;\"><div style=\"color: white;font-size: x-large;font-weight: 700;\">Приветствуем вас!</div></br><div style=\"color: white;font-weight: 600;font-size: large;\">Для завершения регистрации введите следующий код подтверждения в поле на сайте</div></br><div style=\"display: flex;color: white;gap: 15px;font-size: x-large;font-weight: 800;\"><p style=\"border: 2px solid #3a1a3c;padding: 10px; border-radius: 10px; background-color: #0b070b;\">{digit1}</p><p style=\"border: 2px solid #3a1a3c;padding: 10px; border-radius: 10px; background-color: #0b070b;\">{digit2}</p><p style=\"border: 2px solid #3a1a3c;padding: 10px; border-radius: 10px; background-color: #0b070b;\">{digit3}</p><p style=\"border: 2px solid #3a1a3c;padding: 10px; border-radius: 10px; background-color: #0b070b;\">{digit4}</p></div></br><div style=\"color:white; font-size: x-large; font-weight: 600;\">Спасибо за выбор нашего сервиса!</div></div>";
                    string subject = "Подтверждение регистрации на сайте";
                    TempData["ConfirmationCode"] = confirmationCode;
                    await _emailSendler.SendEmailAsync(model.Email, subject, message);
                    return RedirectToAction("ConfirmRegistration", new { email = model.Email });
                }
                else
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }
		public IActionResult Werifications(int? id)
		{
			User user = _context.Users.FirstOrDefault(u => u.Id == id)!;
			if (user != null)
			{
				string userEmail = user.Email;
				return RedirectToAction("ConfirmRegistration", new { email = userEmail });
			}
			else
				return RedirectToAction("Error");
		}
		[HttpGet]
        public IActionResult ConfirmRegistration(string email)
        {
            ViewData["Email"] = email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmRegistration(string email, int confirmationCode)
        {
            int storedCode = (int)TempData["ConfirmationCode"]!;
            if (confirmationCode == storedCode)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    user.EmailConfirmed = true;
                    user.IsConfirmed = true;
                    await _userManager.UpdateAsync(user);
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Goals");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Пользователь не найден.");
                    return View();
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Неверный код подтверждения. Пожалуйста, повторите попытку.");
                return View();
            }
        }
        [HttpGet]
            public IActionResult Login(string returnUrl = null!)
            {
                return View(new LoginViewModel { ReturnUrl = returnUrl });
            }
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Login(LoginViewModel model)
            {
                if (ModelState.IsValid)
                {
                    User user = null!;
                    if (!string.IsNullOrEmpty(model.Email))
                    {
                        if (!model.Email.Contains('@'))
                            user = await _userManager.FindByNameAsync(model.Email);
                        else
                            user = await _userManager.FindByEmailAsync(model.Email);
                    }
                    else if (!string.IsNullOrEmpty(model.Login))
                        user = await _userManager.FindByNameAsync(model.Login);
                    if (user != null)
                    {
                        Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(
                            user,
                            model.Password,
                            model.RememberMe,
                            false
                        );
                        if (result.Succeeded)
                        {
                            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                                return Redirect(model.ReturnUrl);
                            else
                                return RedirectToAction("Index", "Goals");
                        }
                    }

                    ModelState.AddModelError("", "Неправильный логин или пароль");
                }
                return View(model);
            }
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> LogOff()
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Index", "Goals");
            }
            [HttpGet]
            [Authorize(Roles = "MainAdmin,Manager")]
            public IActionResult CreateUser()
            {
                return View();
            }
        }
}
