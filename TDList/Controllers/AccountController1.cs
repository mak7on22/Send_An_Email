using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Text;
using TDList.Models;
using TDList.Servises;
using TDList.ViewModels;
using System.Resources;
using System.Globalization;


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
                    ViewData["StatusError"] = "Пользователь с таким Login уже существует.";
                if (existingUserEmail != null)
                    ViewData["StatusErrors"] = "Пользователь с таким Email уже существует.";
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
                    ViewData["StatusErrorUser"] = "Пользователь не найден.";
                    return View();
                }
            }
            else
            {
                ViewData["StatusErrorCode"] = "Неверный код подтверждения. Пожалуйста, повторите попытку.";
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
                ViewData["StatusErrorLog"] = "Неправильный логин или пароль";
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
        [HttpGet]
        public IActionResult Resurrection()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Resurrection(ResurrectionViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    string newPassword = GenerateRandomPassword();
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
                    if (result.Succeeded)
                    {
                        var preferredCulture = HttpContext.Features.Get<IRequestCultureFeature>().RequestCulture.Culture.Name;
                        Console.WriteLine($"Preferred culture: {preferredCulture}");
                        string message;
                        string subject;
                        if (preferredCulture == "ru-RU")
                        {
                            subject = "Сброс пароля";
                            message = $"<div style=\"background-color: #181530; border-radius: 20px; padding: 20px; display: flex; flex-direction: column; align-items: center;\"><div style=\"color: white;font-size: x-large;font-weight: 700;\">Здравствуйте, {user.UserName}!</div></br><div style=\"color: white;font-weight: 600;font-size: large;\">Пароль был успешно изменен!</div></br><div style=\"color: white;font-weight: 500;font-size: large;\"><p>Ваш логин: {user.Login}</p></br><p>Ваш новый пароль: {newPassword}</p></br></div></br><div style=\"color:white; font-size: x-large; font-weight: 600;\">Спасибо за выбор нашего сервиса!</div></div>";
                        }
                        else if (preferredCulture == "en-US")
                        {
                            subject = "Password Reset";
                            message = $"<div style=\"background-color: #181530; border-radius: 20px; padding: 20px; display: flex; flex-direction: column; align-items: center;\"><div style=\"color: white;font-size: x-large;font-weight: 700;\">Hello, {user.UserName}!</div></br><div style=\"color: white;font-weight: 600;font-size: large;\">The password has been successfully changed!</div></br><div style=\"color: white;font-weight: 500;font-size: large;\"><p>Your login: {user.Login}</p></br><p>Your new password: {newPassword}</p></br></div></br><div style=\"color:white; font-size: x-large; font-weight: 600;\">Thank you for choosing our service!</div></div>";
                        }
                        else if (preferredCulture == "zh-CN")
                        {
                            subject = "密码重置";
                            message = $"<div style=\"background-color: #181530; border-radius: 20px; padding: 20px; display: flex; flex-direction: column; align-items: center;\"><div style=\"color: white;font-size: x-large;font-weight: 700;\">你好！, {user.UserName}!</div></br><div style=\"color: white;font-weight: 600;font-size: large;\">密码已成功更改！</div></br><div style=\"color: white;font-weight: 500;font-size: large;\"><p>你的登入: {user.Login}</p></br><p>您的新密码: {newPassword}</p></br></div></br><div style=\"color:white; font-size: x-large; font-weight: 600;\">感谢您选择我们的服务!</div></div>";
                        }
                        else
                        {
                            subject = "Password Reset";
                            message = $"<div style=\"background-color: #181530; border-radius: 20px; padding: 20px; display: flex; flex-direction: column; align-items: center;\"><div style=\"color: white;font-size: x-large;font-weight: 700;\">Hello, {user.UserName}!</div></br><div style=\"color: white;font-weight: 600;font-size: large;\">The password has been successfully changed!</div></br><div style=\"color: white;font-weight: 500;font-size: large;\"><p>Your login: {user.Login}</p></br><p>Your new password: {newPassword}</p></br></div></br><div style=\"color:white; font-size: x-large; font-weight: 600;\">Thank you for choosing our service!</div></div>";
                        }

                        await _emailSendler.SendEmailAsync(user.Email, subject, message);

                        return RedirectToAction("Index", "Goals");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Пользователь с указанным адресом электронной почты не найден.");
                }
            }
            return View(model);
        }
        private string GenerateRandomPassword()
        {
            var password = new StringBuilder();
            var random = new Random();
            for (int i = 0; i < 12; i++)
            {
                string characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()_+";
                char randomChar = characters[random.Next(characters.Length)];
                password.Append(randomChar);
            }

            return password.ToString();
        }
        public IActionResult ChangeCulture(string culture)
        {
            CultureInfo preferredCulture = CultureInfo.CurrentCulture;

            var cultureInfo = new CultureInfo(culture);
            var requestCulture = new RequestCulture(cultureInfo);
            var cookieValue = CookieRequestCultureProvider.MakeCookieValue(requestCulture);

            Response.Cookies.Append(
     CookieRequestCultureProvider.DefaultCookieName,
     cookieValue,
     new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
 );

            Console.WriteLine($"Changed culture to: {culture}");
            Console.WriteLine($"Culture in ChangeCulture method: {CultureInfo.CurrentCulture}");

            var preferredCultureCookie = HttpContext.Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
            if (!string.IsNullOrEmpty(preferredCultureCookie))
            {
                var requestCultureFeature = HttpContext.Features.Get<IRequestCultureFeature>();
                preferredCulture = requestCultureFeature?.RequestCulture?.Culture ?? CultureInfo.CurrentCulture;
            }
            Console.WriteLine($"Preferred culture (from cookie): {preferredCulture}");

            return Redirect(Request.Headers["Referer"].ToString() ?? "/");
        }
    }
}
