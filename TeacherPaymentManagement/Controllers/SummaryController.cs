using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeacherPaymentManagement.Data;
using TeacherPaymentManagement.Models;

namespace TeacherPaymentManagement.Controllers
{
    public class SummaryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SummaryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> TeacherSummary()
        {
            var teachers = await _context.Teachers.ToListAsync();
            var entries = await _context.TeacherEntries
                .Include(entry => entry.PaymentSetting)
                .ToListAsync();

            var summaries = teachers.Select(teacher =>
            {
                var teacherEntries = entries.Where(entry => entry.TeacherId == teacher.Id).ToList();

                var totalScripts = teacherEntries.Sum(entry => entry.ScriptQuantity);
                var totalHonoriumAmount = teacherEntries
                    .Where(entry => entry.PaymentSetting != null)
                    .Sum(entry => entry.ScriptQuantity * entry.PaymentSetting.HonoriumAmount);


                //var additionalAmount = teacher.TeacherType == TeacherType.Expert
                //    ? teacherEntries
                //        .Where(entry => entry.PaymentSetting != null)
                //        .Sum(entry => entry.PaymentSetting.AdditionalAmount)
                //    : 0;

                var additionalAmount = teacher.TeacherType == TeacherType.Expert
                    ? teacherEntries
                        .Where(entry => entry.PaymentSetting != null)
                        .GroupBy(entry => entry.PaymentSettingId)
                        .Sum(groupedEntry => groupedEntry.First().PaymentSetting.AdditionalAmount)
                    : 0;

                var totalAmount = totalHonoriumAmount;

                var withAdditionalAmount = totalAmount + additionalAmount;

                return new TeacherSummaryViewModel
                {
                    TeacherId = teacher.Id,
                    TeacherName = teacher.Name,
                    TotalScripts = totalScripts,
                    HonoriumAmount = totalHonoriumAmount,
                    AdditionalAmount = additionalAmount,
                    TotalAmount = totalAmount,
                    WithAdditionalAmount = withAdditionalAmount
                };
            }).ToList();

            return View(summaries);
        }

        public async Task<IActionResult> Report()
        {
            var report = new ReportViewModel
            {
                Teachers = await _context.Teachers.ToListAsync(),
                TeacherEntries = await _context.TeacherEntries
                    .Include(entry => entry.Teacher)
                    .Include(entry => entry.PaymentSetting)
                    .ToListAsync(),
                PaymentSettings = await _context.PaymentSettings.ToListAsync()
            };

            var entries = report.TeacherEntries;
            report.TeacherSummaries = report.Teachers.Select(teacher =>
            {
                var teacherEntries = entries.Where(entry => entry.TeacherId == teacher.Id).ToList();

                var totalScripts = teacherEntries.Sum(entry => entry.ScriptQuantity);
                var honoriumAmount = teacherEntries
                    .Where(entry => entry.PaymentSetting != null)
                    .Sum(entry => entry.ScriptQuantity * entry.PaymentSetting.HonoriumAmount);

                var additionalAmount = teacher.TeacherType == TeacherType.Expert
                    ? teacherEntries
                        .Where(entry => entry.PaymentSetting != null)
                        .GroupBy(entry => entry.PaymentSettingId)
                        .Sum(groupedEntry => groupedEntry.First().PaymentSetting.AdditionalAmount)
                    : 0;

                var totalAmount = honoriumAmount;
                var withAdditionalAmount = totalAmount + additionalAmount;

                return new TeacherSummaryViewModel
                {
                    TeacherId = teacher.Id,
                    TeacherName = teacher.Name,
                    TotalScripts = totalScripts,
                    HonoriumAmount = honoriumAmount,
                    AdditionalAmount = additionalAmount,
                    TotalAmount = totalAmount,
                    WithAdditionalAmount = withAdditionalAmount
                };
            }).ToList();

            return View(report);
        }
    }

    public class TeacherSummaryViewModel
    {
        public int TeacherId { get; set; }
        public string TeacherName { get; set; }
        public int TotalScripts { get; set; }
        public decimal HonoriumAmount { get; set; }
        public decimal AdditionalAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal WithAdditionalAmount { get; set; }
    }

    public class ReportViewModel
    {
        public List<Teacher> Teachers { get; set; }
        public List<TeacherEntry> TeacherEntries { get; set; }
        public List<PaymentSetting> PaymentSettings { get; set; }
        public List<TeacherSummaryViewModel> TeacherSummaries { get; set; }
    }
}