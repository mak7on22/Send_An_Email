using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TDList.Models;
using TDList.Servises;

namespace TDList.Controllers
{
    public class UsersController : Controller
    {
        private readonly TDLContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _appEnvironment;
        private readonly IEmailSendler _emailSendler;

        public UsersController(TDLContext context, UserManager<User> userManager, IEmailSendler emailSendler, IWebHostEnvironment appEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _appEnvironment = appEnvironment;
            _emailSendler = emailSendler;
        }
        public async Task<IActionResult> Profile(int? id)
        {
            var currentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            if (id == currentId)
            {
                var user = await _userManager.GetUserAsync(User);
                ViewData["I"] = "DushnilaDima";
                return View(user);
            }
            else
            {
                User anotherUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int.TryParse(currentUserId, out int currentUserIdInt);
                return View(anotherUser);
            }
        }
        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id.ToString());
            if (user != null)
            {
                var result = await _userManager.ChangePasswordAsync(user, model.PasswordHash, model.NewPassword);
                if (result.Succeeded)
                {
                    string message = $"<div style=\"background-color: #181530; border-radius: 20px; padding: 20px; display: flex; flex-direction: column; align-items: center;\"><div style=\"color: white;font-size: x-large;font-weight: 700;\">Здравствуйте, {user.UserName}!</div></br><div style=\"color: white;font-weight: 600;font-size: large;\">Ваш пароль был успешно изменен.</div></br><div style=\"color:white; font-size: x-large; font-weight: 600;\">Спасибо за выбор нашего сервиса!</div></div>";
                    string subject = "Смена пароля пользователя";
                    await _emailSendler.SendEmailAsync(user.Email, subject, message);
                    return Json(new { success = true });
                }
                else
					ViewData["StatusError"] = "Ошибка при изменении пароля";
				return Json(new { success = false, message = "", errors = result.Errors });
            }
            else
				ViewData["StatusErrors"] = "Пользователь не найден";
			return Json(new { success = false, message = "" });
            
        }
        [HttpGet]
        public async Task<IActionResult> MailPost(int id)
        {
			var user = await _userManager.FindByIdAsync(id.ToString());
			string message = $"<div style=\"background-color: #181530; border-radius: 20px; padding: 20px; display: flex; flex-direction: column; align-items: center;\"><div style=\"color: white;font-size: x-large;font-weight: 700;\">Здравствуйте, {user.UserName}!</div></br><div style=\"color: white;font-weight: 600;font-size: large;\">Данные вашей информации записанны ниже!</div></br><div style=\"color: white;font-weight: 500;font-size: large;\"><p>Ваш логин: {user.Login}</p></br><p>Ваша привилегия: {user.RoleName}</p></br></div></br><div style=\"color:white; font-size: x-large; font-weight: 600;\">Спасибо за выбор нашего сервиса!</div></div>";
			string subject = "Данные о пользователе";
			await _emailSendler.SendEmailAsync(user.Email, subject, message);
			return RedirectToAction("Index","Goals");
		}
    }
}
