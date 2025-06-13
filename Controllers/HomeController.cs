using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using InfertilityApp.Models;
using Microsoft.EntityFrameworkCore;
using InfertilityApp.BusinessLogicLayer.Interfaces;

namespace InfertilityApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IPatientService _patientService;
    private readonly IDoctorService _doctorService;
    private readonly ITreatmentService _treatmentService;
    private readonly IAppointmentService _appointmentService;
    private readonly IUserService _userService;

    public HomeController(ILogger<HomeController> logger, IPatientService patientService, 
        IDoctorService doctorService, ITreatmentService treatmentService, 
        IAppointmentService appointmentService, IUserService userService)
    {
        _logger = logger;
        _patientService = patientService;
        _doctorService = doctorService;
        _treatmentService = treatmentService;
        _appointmentService = appointmentService;
        _userService = userService;
    }

    public async Task<IActionResult> Index()
    {
        // Kiểm tra session đăng nhập
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
        {
            return RedirectToAction("Login", "Users");
        }

        // Removed DashboardViewModel - use ViewBag instead

        // Lấy thông tin tổng quan cho dashboard
        var allPatients = await _patientService.GetAllPatientsAsync();
        var allDoctors = await _doctorService.GetAllDoctorsAsync();
        var activeTreatments = await _treatmentService.GetActiveTreatmentsAsync();
        
        ViewData["TotalPatients"] = allPatients.Count();
        ViewData["TotalDoctors"] = allDoctors.Count();
        ViewData["ActiveTreatments"] = activeTreatments.Count();
        
        // Lấy các cuộc hẹn hôm nay
        var today = DateTime.Today;
        var todayAppointments = await _appointmentService.GetAppointmentsByDateAsync(today);
        ViewData["TodayAppointments"] = todayAppointments.Where(a => a.Status == "Scheduled")
                                                          .OrderBy(a => a.AppointmentTime);
        
        // Lấy các cuộc hẹn sắp tới trong 7 ngày
        var nextWeek = today.AddDays(7);
        var upcomingAppointments = await _appointmentService.GetAppointmentsByDateRangeAsync(today.AddDays(1), nextWeek);
        ViewData["UpcomingAppointments"] = upcomingAppointments.Where(a => a.Status == "Scheduled")
                                                              .OrderBy(a => a.AppointmentDate)
                                                              .ThenBy(a => a.AppointmentTime);
        
        // Lấy các điều trị mới nhất
        var allTreatments = await _treatmentService.GetAllTreatmentsAsync();
        var recentTreatments = allTreatments.OrderByDescending(t => t.StartDate).Take(5);
        ViewData["RecentTreatments"] = recentTreatments;
        
        // Thống kê nhanh
        var completedTreatments = await _treatmentService.GetCompletedTreatmentsAsync();
        var successfulTreatments = completedTreatments.Where(t => t.Outcome == "Successful");
        
        ViewData["CompletedTreatments"] = completedTreatments.Count();
        ViewData["SuccessfulTreatments"] = successfulTreatments.Count();
        ViewData["SuccessRate"] = completedTreatments.Any() ? 
            (double)successfulTreatments.Count() / completedTreatments.Count() * 100 : 0;
        
        // Lấy thông tin người dùng hiện tại
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId.HasValue)
        {
            var currentUser = await _userService.GetUserByIdAsync(userId.Value);
            ViewData["CurrentUser"] = currentUser;
        }
        
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
        var allPatients = await _patientService.GetAllPatientsAsync();
        var allDoctors = await _doctorService.GetAllDoctorsAsync();
        var activeTreatments = await _treatmentService.GetActiveTreatmentsAsync();
        
        ViewData["TotalPatients"] = allPatients.Count();
        ViewData["TotalDoctors"] = allDoctors.Count();
        ViewData["ActiveTreatments"] = activeTreatments.Count();
        
        // Thống kê cuộc hẹn tháng này
        var thisMonth = DateTime.Today.AddDays(1 - DateTime.Today.Day); // Đầu tháng
        var nextMonth = thisMonth.AddMonths(1);
        var monthlyAppointments = await _appointmentService.GetAppointmentsByDateRangeAsync(thisMonth, nextMonth);
        ViewData["TotalAppointmentsThisMonth"] = monthlyAppointments.Count();
        
        // Thống kê điều trị
        var completedTreatments = await _treatmentService.GetCompletedTreatmentsAsync();
        var successfulTreatments = completedTreatments.Where(t => t.Outcome == "Successful");
        
        ViewData["CompletedTreatments"] = completedTreatments.Count();
        ViewData["SuccessfulTreatments"] = successfulTreatments.Count();
        ViewData["SuccessRate"] = completedTreatments.Any() ? 
            (double)successfulTreatments.Count() / completedTreatments.Count() * 100 : 0;
        
        // Thống kê theo loại điều trị
        var treatmentTypeStats = completedTreatments
            .GroupBy(t => t.TreatmentType ?? "Không xác định")
            .Select(g => new { 
                TreatmentType = g.Key, 
                Count = g.Count(),
                SuccessCount = g.Count(t => t.Outcome == "Successful")
            })
            .OrderByDescending(x => x.Count)
            .ToList();
        
        ViewData["TreatmentTypeStats"] = treatmentTypeStats;
        
        // Thống kê theo tháng trong năm hiện tại
        int currentYear = DateTime.Today.Year;
        var allTreatments = await _treatmentService.GetAllTreatmentsAsync();
        var thisYearTreatments = allTreatments.Where(t => t.StartDate.Year == currentYear);
        
        var monthlyStats = thisYearTreatments
            .GroupBy(t => t.StartDate.Month)
            .Select(g => new {
                Month = g.Key,
                Count = g.Count()
            })
            .ToList();
        
        var monthlyData = Enumerable.Range(1, 12).Select(month => new {
            Month = month,
            MonthName = new DateTime(currentYear, month, 1).ToString("MMM"),
            Count = monthlyStats.FirstOrDefault(m => m.Month == month)?.Count ?? 0
        }).ToList();
        
        ViewData["MonthlyData"] = monthlyData;
        
        // Lấy các cuộc hẹn hôm nay
        var today = DateTime.Today;
        var todayAppointments = await _appointmentService.GetAppointmentsByDateAsync(today);
        ViewData["TodayAppointments"] = todayAppointments.OrderBy(a => a.AppointmentTime);
        
        // Lấy các bệnh nhân mới nhất
        var recentPatients = allPatients.OrderByDescending(p => p.CreatedAt).Take(5);
        ViewData["RecentPatients"] = recentPatients;
        
        // Nếu là bác sĩ, lấy thêm thông tin về bệnh nhân của họ
        if (userRole == "Doctor")
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                var user = await _userService.GetUserByIdAsync(userId.Value);
                    
                if (user != null && user.DoctorId.HasValue)
                {
                    int doctorId = user.DoctorId.Value;
                    
                    // Đếm số lượng bệnh nhân đang điều trị
                    var doctorTreatments = await _treatmentService.GetTreatmentsByDoctorAsync(doctorId);
                    var activeDoctorTreatments = doctorTreatments.Where(t => t.Status == "Active");
                    var patientsCount = activeDoctorTreatments.Select(t => t.PatientId).Distinct().Count();
                    ViewData["MyPatients"] = patientsCount;
                    
                    // Đếm số lượng cuộc hẹn hôm nay
                    var doctorAppointments = await _appointmentService.GetAppointmentsByDoctorAsync(doctorId);
                    var todayDoctorAppointments = doctorAppointments.Where(a => a.AppointmentDate == today);
                    ViewData["MyAppointmentsToday"] = todayDoctorAppointments.Count();
                    
                    // Đếm tổng số điều trị
                    ViewData["MyTreatments"] = doctorTreatments.Count();
                }
            }
        }
        
        return View();
    }

    // Action để load dữ liệu cho chart
    public async Task<IActionResult> GetChartData(string chartType)
    {
        try
        {
            switch (chartType)
            {
                case "treatments":
                    var treatmentStats = await _treatmentService.GetTreatmentsByStatusStatisticsAsync();
                    return Json(treatmentStats);
                    
                case "appointments":
                    var appointmentStats = await _appointmentService.GetAppointmentsByStatusStatisticsAsync();
                    return Json(appointmentStats);
                    
                case "patients":
                    var patientStats = await _patientService.GetPatientsByGenderStatisticsAsync();
                    return Json(patientStats);
                    
                default:
                    return Json(new { error = "Invalid chart type" });
            }
        }
        catch (Exception ex)
        {
            return Json(new { error = ex.Message });
        }
    }
}
