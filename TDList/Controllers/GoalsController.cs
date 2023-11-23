using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TDList.Models;
using TDList.Sorts;
using TDList.Enums;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Authorization;

namespace TDList.Controllers
{
    public class GoalsController : Controller
    {
        private readonly TDLContext _context;
        private readonly IStringLocalizer<GoalsController> _localizer;

        public GoalsController(TDLContext context, IStringLocalizer<GoalsController> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index(string searchWords, DateTime? startDate, DateTime? endDate, Priority? pr, string? st, int pg = 1, TDLSortState sortState = TDLSortState.NameAsc)
        {
            const int pageSize = 10;
            if (pg < 1)
                pg = 1;
            IQueryable<Goal> tdlContext = _context.Goals;
            if (searchWords != null)
                tdlContext = tdlContext.Where(x => x.Name == searchWords || x.Description.Contains(searchWords));            
            if (startDate != null)
                tdlContext = tdlContext.Where(x => x.Created >= startDate.Value);
            if (endDate != null) 
                tdlContext = tdlContext.Where(x => x.Created <= endDate.Value);
            if (pr != null) 
                tdlContext = tdlContext.Where(x => x.Priority == pr);
            if (st != null && st != "All") 
                tdlContext = tdlContext.Where(x => x.Status == st);

            var goalsCircle = await _context.Goals.ToListAsync();
            foreach (var g in goalsCircle)
            {
                var priority = g.Priority;
                var status = g.Status;
                if (priority == Enums.Priority.Low) g.PriorityValue = 1;
                else if (priority == Enums.Priority.Medium) g.PriorityValue = 2;
                else g.PriorityValue = 3;
                if (status == "Новая") g.StatusValue = 1;
                else if (status == "В процессе") g.StatusValue = 2;
                else g.StatusValue = 3;
            }
            await _context.SaveChangesAsync();
            switch (sortState)
            {
                case TDLSortState.NameAsc: tdlContext = tdlContext.OrderBy(p => p.Name); break;
                case TDLSortState.PriorityValueAsc: tdlContext = tdlContext.OrderBy(p => p.PriorityValue); break;
                case TDLSortState.StatusValueAsc: tdlContext = tdlContext.OrderBy(p => p.StatusValue); break;
                case TDLSortState.CreatedAsc: tdlContext = tdlContext.OrderBy(p => p.Created); break;
                case TDLSortState.NameDesc: tdlContext = tdlContext.OrderByDescending(p => p.Name); break;
                case TDLSortState.PriorityValueDesc: tdlContext = tdlContext.OrderByDescending(p => p.PriorityValue); break;
                case TDLSortState.StatusValueDesc: tdlContext = tdlContext.OrderByDescending(p => p.StatusValue); break;
                case TDLSortState.CreatedDesc: tdlContext = tdlContext.OrderByDescending(p => p.Created); break;
            }
            int recsCount = await tdlContext.CountAsync();
            var pager = new Pager(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = await tdlContext
                .Skip(recSkip)
                .Take(pageSize)
                .ToListAsync();
            this.ViewBag.Pager = pager;
            ViewBag.NameSort = sortState == TDLSortState.NameAsc ? TDLSortState.NameDesc : TDLSortState.NameAsc;
            ViewBag.PriorityValueSort = sortState == TDLSortState.PriorityValueAsc ? TDLSortState.PriorityValueDesc : TDLSortState.PriorityValueAsc;
            ViewBag.StatusValueSort = sortState == TDLSortState.StatusValueAsc ? TDLSortState.StatusValueDesc : TDLSortState.StatusValueAsc;
            ViewBag.CreatedSort = sortState == TDLSortState.CreatedAsc ? TDLSortState.CreatedDesc : TDLSortState.CreatedAsc;
            return View(data);
        }
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();
            var goal = await _context.Goals.FirstOrDefaultAsync(m => m.Id == id);
            if (goal == null)
                return NotFound();
            return View(goal);
        }
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Status,Priority,UserName")] Goal goal)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    goal.Created = DateTime.Now;
                    _context.Add(goal);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex){ ModelState.AddModelError("Status", ex.Message);}
            }

            return View(goal);
        }
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
           
            if (id == null)
                return NotFound();
            var goal = await _context.Goals.FindAsync(id);
            if (goal == null)
                return NotFound();
            return View(goal);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Priority,Description,Status,UserName")] Goal goal)
        {
           
            if (ModelState.IsValid)
            {
                try
                {
                    var existingGoal = await _context.Goals.FindAsync(id);
                    if (existingGoal == null)
                        return NotFound();
                    if (existingGoal.Status == "В процессе" || existingGoal.Status == "Завершена")
                    {
                        ViewData["StatusError"] = "Нельзя редактировать задачу в данном состоянии.";
                        return View(goal);
                    }
                    if (goal.Status != null)
                    {
                        existingGoal.Name = goal.Name;
                        existingGoal.Priority = goal.Priority;
                        existingGoal.Description = goal.Description;
                        existingGoal.UserName = goal.UserName;
                        _context.Update(existingGoal);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    else
                        ViewData["StatusErrors"] = "Неверное состояние задачи.";
                }catch (ArgumentException ex) { ModelState.AddModelError("Status", ex.Message); }
            }
            return View(goal);
        }
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
           
            if (id == null)
                return NotFound();
            var goal = await _context.Goals.FirstOrDefaultAsync(m => m.Id == id);
            if (goal == null)
                return NotFound();
            return View(goal);
        }
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
           
            var goal = await _context.Goals.FindAsync(id);
            if (goal != null)
            {
                try
                {
                    if (goal.Status == "Новая" || goal.Status == "Завершена")
                    {
                        _context.Goals.Remove(goal);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        ViewData["StatusError"] = "Нельзя удалить задачу с данным статусом.";
                        return View(goal);
                    }
                }
                catch (Exception ex){ ModelState.AddModelError("Status", ex.Message);  return View(goal);}
            }
            return RedirectToAction(nameof(Index));
        }
        [Authorize]
        private bool GoalExists(int id)
        {
            return _context.Goals.Any(e => e.Id == id);
        }



        [Authorize]
        public async Task<IActionResult> Start(int? id)
        {
           
            if (id == null)
                return NotFound();
            var goal = await _context.Goals.FindAsync(id);
            if (goal == null)
                return NotFound();
            try
            {
                if (goal.Status == "Новая")
                {
                    goal.Started = DateTime.Now;
                    goal.Status = "В процессе";
                    _context.Update(goal);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ViewData["StatusError1"] = "Нельзя начать выполнение задачи в данном состоянии.";
                    return View(nameof(Index), await _context.Goals.ToListAsync());
                }
            }
            catch
            {
                ViewData["StatusError2"] = "Произошла ошибка при запуске задачи.";
                return View(nameof(Index), await _context.Goals.ToListAsync());
            }
        }
        [Authorize]
        public async Task<IActionResult> Complete(int? id)
        {
            if (id == null)
                return NotFound();
            var goal = await _context.Goals.FindAsync(id);
            if (goal == null)
                return NotFound();
            try
            {
                if (goal.Status == "В процессе")
                {
                    goal.Ended = DateTime.Now;
                    goal.Status = "Завершена";
                    _context.Update(goal);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ViewData["StatusError3"] = "Нельзя завершить задачу в данном состоянии.";
                    return View(nameof(Index), await _context.Goals.ToListAsync());
                }
            }
            catch
            {
                ViewData["StatusError4"] = "Произошла ошибка при завершении задачи.";
                return View(nameof(Index), await _context.Goals.ToListAsync());
            }
        }
        public IActionResult ChangeCulture(string culture)
        {
            var cultureInfo = new CultureInfo(culture);
            var requestCulture = new RequestCulture(cultureInfo);
            var cookieValue = CookieRequestCultureProvider.MakeCookieValue(requestCulture);

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                cookieValue,
                new CookieOptions{  Expires = DateTimeOffset.UtcNow.AddYears(1)}
            );
            return Redirect(Request.Headers["Referer"].ToString() ?? "/");
        }

    }
}
