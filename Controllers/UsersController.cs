using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InfertilityApp.Models;
using System.Security.Cryptography;
using System.Text;

namespace InfertilityApp.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(u => u.Doctor)
                .ToListAsync();
            return View(users);
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Doctor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            var doctorsWithoutUsers = _context.Doctors
                .Where(d => !_context.Users.Where(u => u.DoctorId.HasValue).Select(u => u.DoctorId).Contains(d.Id))
                .ToList();
            ViewData["DoctorId"] = new SelectList(doctorsWithoutUsers, "Id", "FullName");
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user, string Password)
        {
            try
            {
                // Xử lý IsActive từ form
                user.IsActive = true; // Mặc định là true

                // Kiểm tra và gán mật khẩu
                if (string.IsNullOrEmpty(Password))
                {
                    ModelState.AddModelError("Password", "Mật khẩu là bắt buộc");
                    var doctorsWithoutUsers = _context.Doctors
                        .Where(d => !_context.Users.Where(u => u.DoctorId.HasValue).Select(u => u.DoctorId).Contains(d.Id))
                        .ToList();
                    ViewData["DoctorId"] = new SelectList(doctorsWithoutUsers, "Id", "FullName", user.DoctorId);
                    return View(user);
                }

                // Gán mật khẩu vào PasswordHash
                user.PasswordHash = Password;

                // Kiểm tra username đã tồn tại chưa
                if (_context.Users.Any(u => u.Username == user.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                    var doctorsWithoutUsers = _context.Doctors
                        .Where(d => !_context.Users.Where(u => u.DoctorId.HasValue).Select(u => u.DoctorId).Contains(d.Id))
                        .ToList();
                    ViewData["DoctorId"] = new SelectList(doctorsWithoutUsers, "Id", "FullName", user.DoctorId);
                    return View(user);
                }

                // Thiết lập các giá trị mặc định
                user.CreatedAt = DateTime.Now;
                
                // Thêm người dùng vào database
                _context.Add(user);
                await _context.SaveChangesAsync();
                
                // Chuyển hướng đến trang danh sách
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ
                ModelState.AddModelError("", "Có lỗi xảy ra khi tạo người dùng: " + ex.Message);
                
                // Nếu có lỗi, hiển thị lại form với thông báo lỗi
                var doctorsWithoutUsers = _context.Doctors
                    .Where(d => !_context.Users.Where(u => u.DoctorId.HasValue).Select(u => u.DoctorId).Contains(d.Id))
                    .ToList();
                ViewData["DoctorId"] = new SelectList(doctorsWithoutUsers, "Id", "FullName", user.DoctorId);
                return View(user);
            }
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            
            var doctorsWithoutUsers = _context.Doctors
                .Where(d => !_context.Users.Where(u => u.DoctorId.HasValue && u.Id != id).Select(u => u.DoctorId).Contains(d.Id))
                .ToList();
            ViewData["DoctorId"] = new SelectList(doctorsWithoutUsers, "Id", "FullName", user.DoctorId);
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Username,FullName,Email,PhoneNumber,Role,IsActive,DoctorId")] User user, string Password)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            // Lấy thông tin người dùng hiện tại từ database
            var existingUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (existingUser == null)
            {
                return NotFound();
            }

            // Giữ nguyên các giá trị không được chỉnh sửa
            user.CreatedAt = existingUser.CreatedAt;
            user.LastLogin = existingUser.LastLogin;
            
            // Xử lý mật khẩu
            if (!string.IsNullOrEmpty(Password))
            {
                user.PasswordHash = Password;
            }
            else
            {
                user.PasswordHash = existingUser.PasswordHash;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            
            var doctorsWithoutUsers = _context.Doctors
                .Where(d => !_context.Users.Where(u => u.DoctorId.HasValue && u.Id != id).Select(u => u.DoctorId).Contains(d.Id))
                .ToList();
            ViewData["DoctorId"] = new SelectList(doctorsWithoutUsers, "Id", "FullName", user.DoctorId);
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Doctor)
                .FirstOrDefaultAsync(m => m.Id == id);
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
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
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
                ViewData["Error"] = "Username and password are required";
                return View();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                ViewData["Error"] = "Invalid username or password";
                return View();
            }

            if (!user.IsActive)
            {
                ViewData["Error"] = "Your account is inactive. Please contact administrator";
                return View();
            }

            // Update last login time
            user.LastLogin = DateTime.Now;
            _context.Update(user);
            await _context.SaveChangesAsync();

            // Store user info in session
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.Username);
            HttpContext.Session.SetString("UserFullName", user.FullName);
            HttpContext.Session.SetString("UserRole", user.Role);

            return RedirectToAction("Index", "Home");
        }

        // GET: Users/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // GET: Users/ToggleActive/5
        public async Task<IActionResult> ToggleActive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Toggle the IsActive status
            user.IsActive = !user.IsActive;
            _context.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        private string HashPassword(string password)
        {
            return password;
        }

        private bool VerifyPassword(string password, string hash)
        {
            // Kiểm tra trực tiếp mật khẩu văn bản thô
            return password == hash;
            
            // Code cũ sử dụng băm
            // return HashPassword(password) == hash;
        }
    }
} 