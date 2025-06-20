using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InfertilityApp.Models;
using InfertilityApp.BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Http;

namespace InfertilityApp.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: Users
        public async Task<IActionResult> Index(string searchString, string role)
        {
            var users = await _userService.GetAllUsersAsync();

            // Tìm kiếm theo tên hoặc email
            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(u =>
                    u.FullName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    u.Username.Contains(searchString, StringComparison.OrdinalIgnoreCase));
            }

            // Lọc theo role
            if (!string.IsNullOrEmpty(role))
            {
                users = await _userService.GetUsersByRoleAsync(role);
            }

            ViewData["SearchString"] = searchString;
            ViewData["Role"] = role;
            ViewData["Roles"] = new SelectList(
                new[] { "Admin", "Doctor", "Nurse", "Receptionist" },
                role);

            return View(users.OrderBy(u => u.FullName));
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userService.GetUserByIdAsync(id.Value);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            ViewData["Roles"] = new SelectList(
                new[] { "Admin", "Doctor", "Nurse", "Receptionist" });

            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Username,PasswordHash,FullName,Email,PhoneNumber,Role,IsActive")] User user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _userService.CreateUserAsync(user);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            ViewData["Roles"] = new SelectList(
                new[] { "Admin", "Doctor", "Nurse", "Receptionist" });
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userService.GetUserByIdAsync(id.Value);
            if (user == null)
            {
                return NotFound();
            }

            ViewData["Roles"] = new SelectList(
                new[] { "Admin", "Doctor", "Nurse", "Receptionist" },
                user.Role);

            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Username,PasswordHash,FullName,Email,PhoneNumber,Role,IsActive")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _userService.UpdateUserAsync(user);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            ViewData["Roles"] = new SelectList(
                new[] { "Admin", "Doctor", "Nurse", "Receptionist" },
                user.Role);
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userService.GetUserByIdAsync(id.Value);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _userService.DeleteUserAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var user = await _userService.GetUserByIdAsync(id);
                return View(user);
            }
        }

        // GET: Users/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Users/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewData["Error"] = "Vui lòng nhập tên đăng nhập và mật khẩu";
                return View();
            }

            try
            {
                var user = await _userService.AuthenticateUserAsync(username, password);
                if (user != null && user.IsActive)
                {
                    HttpContext.Session.SetString("UserId", user.Id.ToString());
                    HttpContext.Session.SetString("UserFullName", user.FullName);
                    HttpContext.Session.SetString("UserRole", user.Role);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewData["Error"] = "Tên đăng nhập hoặc mật khẩu không đúng";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewData["Error"] = "Đã xảy ra lỗi: " + ex.Message;
                return View();
            }
        }

        // GET: Users/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // GET: Users/ChangePassword/5
        public async Task<IActionResult> ChangePassword(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userService.GetUserByIdAsync(id.Value);
            if (user == null)
            {
                return NotFound();
            }

            ViewBag.User = user;
            return View();
        }

        // POST: Users/ChangePassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(int id, string currentPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu xác nhận không khớp.");
                var user = await _userService.GetUserByIdAsync(id);
                ViewBag.User = user;
                return View();
            }

            try
            {
                await _userService.ChangePasswordAsync(id, currentPassword, newPassword);
                TempData["Success"] = "Đổi mật khẩu thành công!";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var user = await _userService.GetUserByIdAsync(id);
                ViewBag.User = user;
                return View();
            }
        }

        // POST: Users/UpdateRole
        [HttpPost]
        public async Task<IActionResult> UpdateRole(int userId, string newRole)
        {
            try
            {
                await _userService.UpdateUserRoleAsync(userId, newRole);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // GET: Users/Statistics
        public async Task<IActionResult> Statistics()
        {
            var totalUsers = await _userService.GetTotalUsersCountAsync();
            var usersByRole = await _userService.GetUsersByRoleStatisticsAsync();
            var activeUsersStats = await _userService.GetActiveUsersStatisticsAsync();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.UsersByRole = usersByRole;
            ViewBag.ActiveUsersStats = activeUsersStats;

            return View();
        }

        // GET: Users/ToggleActive/5
        public async Task<IActionResult> ToggleActive(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "Không tìm thấy người dùng.";
                    return RedirectToAction(nameof(Index));
                }

                // Toggle IsActive status
                user.IsActive = !user.IsActive;
                await _userService.UpdateUserAsync(user);

                string statusMessage = user.IsActive ? "kích hoạt" : "vô hiệu hóa";
                TempData["Success"] = $"Đã {statusMessage} người dùng {user.FullName} thành công!";

                // Redirect back to Index
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // In case of error, redirect back with error message
                TempData["Error"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
} 