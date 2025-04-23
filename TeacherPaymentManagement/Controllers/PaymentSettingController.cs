using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeacherPaymentManagement.Data;
using TeacherPaymentManagement.Models;

namespace TeacherPaymentManagement.Controllers
{
    public class PaymentSettingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentSettingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.PaymentSettings.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,SettingName,EffectiveDate,ClosingDate,HonoriumAmount,AdditionalAmount")] PaymentSetting paymentSetting)
        {
            if (ModelState.IsValid)
            {
                var overlappingSettings = await _context.PaymentSettings
                    .Where(setting =>
                        (paymentSetting.EffectiveDate >= setting.EffectiveDate && paymentSetting.EffectiveDate <= setting.ClosingDate) ||
                        (paymentSetting.ClosingDate >= setting.EffectiveDate && paymentSetting.ClosingDate <= setting.ClosingDate) ||
                        (setting.EffectiveDate >= paymentSetting.EffectiveDate && setting.EffectiveDate <= paymentSetting.ClosingDate) ||
                        (setting.ClosingDate >= paymentSetting.EffectiveDate && setting.ClosingDate <= paymentSetting.ClosingDate))
                    .ToListAsync();

                if (overlappingSettings.Any())
                {
                    ModelState.AddModelError(string.Empty, "The effective and closing dates overlap with an existing payment setting.");
                    return View(paymentSetting);
                }

                _context.Add(paymentSetting);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(paymentSetting);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paymentSetting = await _context.PaymentSettings.FindAsync(id);
            if (paymentSetting == null)
            {
                return NotFound();
            }

            if (paymentSetting.IsUsed)
            {
                TempData["ErrorMessage"] = "This payment setting has been used and cannot be edited.";
                return RedirectToAction(nameof(Index));
            }

            return View(paymentSetting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SettingName,EffectiveDate,ClosingDate,HonoriumAmount,AdditionalAmount")] PaymentSetting paymentSetting)
        {
            if (id != paymentSetting.Id)
            {
                return NotFound();
            }

            var existingSetting = await _context.PaymentSettings.AsNoTracking().FirstOrDefaultAsync(ps => ps.Id == id);
            if (existingSetting.IsUsed)
            {
                TempData["ErrorMessage"] = "This payment setting has been used and cannot be edited.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var overlappingSettings = await _context.PaymentSettings
                        .Where(setting => setting.Id != paymentSetting.Id &&
                            ((paymentSetting.EffectiveDate >= setting.EffectiveDate && paymentSetting.EffectiveDate <= setting.ClosingDate) ||
                            (paymentSetting.ClosingDate >= setting.EffectiveDate && paymentSetting.ClosingDate <= setting.ClosingDate) ||
                            (setting.EffectiveDate >= paymentSetting.EffectiveDate && setting.EffectiveDate <= paymentSetting.ClosingDate) ||
                            (setting.ClosingDate >= paymentSetting.EffectiveDate && setting.ClosingDate <= paymentSetting.ClosingDate)))
                        .ToListAsync();

                    if (overlappingSettings.Any())
                    {
                        ModelState.AddModelError(string.Empty, "The effective and closing dates overlap with an existing payment setting.");
                        return View(paymentSetting);
                    }

                    _context.Update(paymentSetting);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentSettingExists(paymentSetting.Id))
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
            return View(paymentSetting);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paymentSetting = await _context.PaymentSettings
                .FirstOrDefaultAsync(setting => setting.Id == id);
            if (paymentSetting == null)
            {
                return NotFound();
            }

            if (paymentSetting.IsUsed)
            {
                TempData["ErrorMessage"] = "This payment setting has been used and cannot be deleted.";
                return RedirectToAction(nameof(Index));
            }

            return View(paymentSetting);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var paymentSetting = await _context.PaymentSettings.FindAsync(id);

            if (paymentSetting.IsUsed)
            {
                TempData["ErrorMessage"] = "This payment setting has been used and cannot be deleted.";
                return RedirectToAction(nameof(Index));
            }

            _context.PaymentSettings.Remove(paymentSetting);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PaymentSettingExists(int id)
        {
            return _context.PaymentSettings.Any(setting => setting.Id == id);
        }
    }
}

