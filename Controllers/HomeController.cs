using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using InfertilityApp.Models;
using Microsoft.EntityFrameworkCore;

namespace InfertilityApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Kiểm tra đăng nhập
        if (HttpContext.Session.GetInt32("UserId") == null)
        {
            return RedirectToAction("Login", "Users");
        }

        // Lấy thông tin tổng quan cho dashboard
        ViewData["TotalPatients"] = await _context.Patients.CountAsync();
        ViewData["TotalDoctors"] = await _context.Doctors.CountAsync();
        ViewData["ActiveTreatments"] = await _context.Treatments.Where(t => t.Status == "Active").CountAsync();
        
        // Lấy các cuộc hẹn hôm nay
        var today = DateTime.Today;
        var todayAppointments = await _context.Appointments
            .Include(a => a.Doctor)
            .Include(a => a.Patient)
            .Where(a => a.AppointmentDate == today && a.Status == "Scheduled")
            .OrderBy(a => a.AppointmentTime)
            .ToListAsync();
        ViewData["TodayAppointments"] = todayAppointments;
        
        // Lấy các cuộc hẹn sắp tới trong 7 ngày
        var nextWeek = today.AddDays(7);
        var upcomingAppointments = await _context.Appointments
            .Include(a => a.Doctor)
            .Include(a => a.Patient)
            .Where(a => a.AppointmentDate > today && a.AppointmentDate <= nextWeek && a.Status == "Scheduled")
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.AppointmentTime)
            .ToListAsync();
        ViewData["UpcomingAppointments"] = upcomingAppointments;
        
        // Lấy các điều trị mới nhất
        var recentTreatments = await _context.Treatments
            .Include(t => t.Doctor)
            .Include(t => t.Patient)
            .OrderByDescending(t => t.StartDate)
            .Take(5)
            .ToListAsync();
        ViewData["RecentTreatments"] = recentTreatments;
        
        // Thống kê nhanh
        var completedTreatmentsCount = await _context.Treatments.Where(t => t.Status == "Completed").CountAsync();
        var successfulTreatmentsCount = await _context.Treatments.Where(t => t.Status == "Completed" && t.Outcome == "Successful").CountAsync();
        
        ViewData["CompletedTreatments"] = completedTreatmentsCount;
        ViewData["SuccessfulTreatments"] = successfulTreatmentsCount;
        ViewData["SuccessRate"] = completedTreatmentsCount > 0 ? 
            (double)successfulTreatmentsCount / completedTreatmentsCount * 100 : 0;
        
        // Lấy thông tin người dùng hiện tại
        var userId = HttpContext.Session.GetInt32("UserId");
        var currentUser = await _context.Users.FindAsync(userId);
        ViewData["CurrentUser"] = currentUser;
        
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public async Task<IActionResult> Dashboard()
    {
        // Kiểm tra đăng nhập
        if (HttpContext.Session.GetInt32("UserId") == null)
        {
            return RedirectToAction("Login", "Users");
        }

        // Lấy role của người dùng
        string userRole = HttpContext.Session.GetString("UserRole") ?? "";
        
        // Thống kê chung
        ViewData["TotalPatients"] = await _context.Patients.CountAsync();
        ViewData["TotalDoctors"] = await _context.Doctors.CountAsync();
        ViewData["ActiveTreatments"] = await _context.Treatments.Where(t => t.Status == "Active").CountAsync();
        ViewData["TotalAppointmentsThisMonth"] = await _context.Appointments
            .Where(a => a.AppointmentDate.Month == DateTime.Today.Month && 
                   a.AppointmentDate.Year == DateTime.Today.Year)
            .CountAsync();
        
        // Thống kê điều trị
        var completedTreatmentsCount = await _context.Treatments.Where(t => t.Status == "Completed").CountAsync();
        var successfulTreatmentsCount = await _context.Treatments.Where(t => t.Status == "Completed" && t.Outcome == "Successful").CountAsync();
        
        ViewData["CompletedTreatments"] = completedTreatmentsCount;
        ViewData["SuccessfulTreatments"] = successfulTreatmentsCount;
        ViewData["SuccessRate"] = completedTreatmentsCount > 0 ? 
            (double)successfulTreatmentsCount / completedTreatmentsCount * 100 : 0;
        
        // Thống kê theo loại điều trị
        var treatmentTypeStats = await _context.Treatments
            .Where(t => t.Status == "Completed")
            .GroupBy(t => t.TreatmentType)
            .Select(g => new { 
                TreatmentType = g.Key, 
                Count = g.Count(),
                SuccessCount = g.Count(t => t.Outcome == "Successful")
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();
        
        ViewData["TreatmentTypeStats"] = treatmentTypeStats;
        
        // Thống kê theo tháng trong năm hiện tại
        int currentYear = DateTime.Today.Year;
        var monthlyStats = await _context.Treatments
            .Where(t => t.StartDate.Year == currentYear)
            .GroupBy(t => t.StartDate.Month)
            .Select(g => new {
                Month = g.Key,
                Count = g.Count()
            })
            .ToListAsync();
        
        var monthlyData = Enumerable.Range(1, 12).Select(month => new {
            Month = month,
            MonthName = new DateTime(currentYear, month, 1).ToString("MMM"),
            Count = monthlyStats.FirstOrDefault(m => m.Month == month)?.Count ?? 0
        }).ToList();
        
        ViewData["MonthlyData"] = monthlyData;
        
        // Lấy các cuộc hẹn hôm nay
        var today = DateTime.Today;
        var todayAppointments = await _context.Appointments
            .Include(a => a.Doctor)
            .Include(a => a.Patient)
            .Where(a => a.AppointmentDate == today)
            .OrderBy(a => a.AppointmentTime)
            .ToListAsync();
        
        ViewData["TodayAppointments"] = todayAppointments;
        
        // Lấy các bệnh nhân mới nhất
        var recentPatients = await _context.Patients
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .ToListAsync();
        
        ViewData["RecentPatients"] = recentPatients;
        
        // Nếu là bác sĩ, lấy thêm thông tin về bệnh nhân của họ
        if (userRole == "Doctor")
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                var userIdValue = userId.Value;
                var user = await _context.Users.FindAsync(userIdValue);
                    
                if (user != null && user.DoctorId.HasValue)
                {
                    int doctorId = user.DoctorId.Value;
                    
                    // Đếm số lượng bệnh nhân đang điều trị
                    var patientsCount = await _context.Treatments
                        .Where(t => t.DoctorId == doctorId && t.Status == "Active")
                        .Select(t => t.PatientId)
                        .Distinct()
                        .CountAsync();
                    ViewData["MyPatients"] = patientsCount;
                    
                    // Đếm số lượng cuộc hẹn hôm nay
                    var appointmentsCount = await _context.Appointments
                        .CountAsync(a => a.DoctorId == doctorId && a.AppointmentDate == today);
                    ViewData["MyAppointmentsToday"] = appointmentsCount;
                    
                    // Đếm tổng số điều trị
                    var treatmentsCount = await _context.Treatments
                        .CountAsync(t => t.DoctorId == doctorId);
                    ViewData["MyTreatments"] = treatmentsCount;
                }
            }
        }
        
        return View();
    }
}
