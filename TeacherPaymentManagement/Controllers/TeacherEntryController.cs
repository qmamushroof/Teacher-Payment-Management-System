using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TeacherPaymentManagement.Data;
using TeacherPaymentManagement.Models;

namespace TeacherPaymentManagement.Controllers
{
    public class TeacherEntryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeacherEntryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var entries = await _context.TeacherEntries
                .Include(entry => entry.Teacher)
                .Include(entry => entry.PaymentSetting)
                .ToListAsync();

            return View(entries);
        }


        public async Task<IActionResult> Create()
        {
            ViewData["TeacherId"] = new SelectList(_context.Teachers, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TeacherId,ScriptQuantity,EvaluationDate")] TeacherEntry teacherEntry)
        {
            var paymentSetting = await _context.PaymentSettings
                .FirstOrDefaultAsync(setting => setting.EffectiveDate <= teacherEntry.EvaluationDate && setting.ClosingDate >= teacherEntry.EvaluationDate);

            bool dupliacteExists = await _context.TeacherEntries.AnyAsync(entry => entry.TeacherId == teacherEntry.TeacherId && entry.EvaluationDate == teacherEntry.EvaluationDate);

            if (dupliacteExists)
            {
                ModelState.AddModelError(string.Empty, "Same entry date not allowed.");

                return View(teacherEntry);
            }

            if (paymentSetting != null)
            {
                teacherEntry.PaymentSettingId = paymentSetting.Id;

                paymentSetting.IsUsed = true;
                _context.Update(paymentSetting);
            }

            _context.Add(teacherEntry);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacherEntry = await _context.TeacherEntries.FindAsync(id);
            if (teacherEntry == null)
            {
                return NotFound();
            }

            ViewData["TeacherId"] = new SelectList(_context.Teachers, "Id", "Name", teacherEntry.TeacherId);
            return View(teacherEntry);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TeacherId,ScriptQuantity,EvaluationDate,PaymentSettingId")] TeacherEntry teacherEntry)
        {
            if (id != teacherEntry.Id)
            {
                return NotFound();
            }

            var existingEntry = await _context.TeacherEntries
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (existingEntry == null)
            {
                return NotFound();
            }

            var oldPaymentSettingId = existingEntry.PaymentSettingId;

            try
            {
                var newPaymentSetting = await _context.PaymentSettings
                    .FirstOrDefaultAsync(setting => setting.EffectiveDate <= teacherEntry.EvaluationDate && setting.ClosingDate >= teacherEntry.EvaluationDate);

                if (newPaymentSetting != null)
                {
                    teacherEntry.PaymentSettingId = newPaymentSetting.Id;
                    newPaymentSetting.IsUsed = true;
                    _context.Update(newPaymentSetting);
                }
                else
                {
                    teacherEntry.PaymentSettingId = null;
                }

                _context.Update(teacherEntry);
                await _context.SaveChangesAsync();

                if (oldPaymentSettingId != null && oldPaymentSettingId != teacherEntry.PaymentSettingId)
                {
                    bool isOldSettingStillUsed = await _context.TeacherEntries
                        .AnyAsync(entry => entry.PaymentSettingId == oldPaymentSettingId);

                    if (!isOldSettingStillUsed)
                    {
                        var oldPaymentSetting = await _context.PaymentSettings
                            .FirstOrDefaultAsync(setting => setting.Id == oldPaymentSettingId);

                        if (oldPaymentSetting != null)
                        {
                            oldPaymentSetting.IsUsed = false;
                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TeacherEntryExists(teacherEntry.Id))
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

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacherEntry = await _context.TeacherEntries.FindAsync(id);

            if (teacherEntry == null)
            {
                return NotFound();
            }

            ViewData["TeacherId"] = new SelectList(_context.Teachers, "Id", "Name", teacherEntry.TeacherId);
            return View(teacherEntry);
        }

        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var teacherEntry = await _context.TeacherEntries.FindAsync(id);
        //    _context.TeacherEntries.Remove(teacherEntry);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teacherEntry = await _context.TeacherEntries
                .Include(entry => entry.PaymentSetting)
                .FirstOrDefaultAsync(entry => entry.Id == id);

            if (teacherEntry == null)
            {
                return NotFound();
            }

            var paymentSettingId = teacherEntry.PaymentSettingId;

            _context.TeacherEntries.Remove(teacherEntry);
            await _context.SaveChangesAsync();

            bool isSettingUsed = await _context.TeacherEntries
                .AnyAsync(entry => entry.PaymentSettingId == paymentSettingId);

            if (!isSettingUsed && paymentSettingId != null)
            {
                var paymentSetting = await _context.PaymentSettings
                    .FirstOrDefaultAsync(setting => setting.Id == paymentSettingId);

                if (paymentSetting != null)
                {
                    paymentSetting.IsUsed = false;
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TeacherEntryExists(int id)
        {
            return _context.TeacherEntries.Any(entry => entry.Id == id);
        }
    }
}
