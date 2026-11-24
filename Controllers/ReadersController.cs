using LibraryMvc.Data;
using LibraryMvc.Extensions;
using LibraryMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryMvc.Controllers
{
    public class ReadersController : Controller
    {
        private readonly LibraryContext _context;
        private const string CreateSessionKey = "Draft_CreateReader";
        private static string EditSessionKey(int id) => $"Draft_EditReader_{id}";

        public ReadersController(LibraryContext context)
        {
            _context = context;
        }

        // GET: Readers
        public async Task<IActionResult> Index(string? searchString)
        {
            var readersQuery = _context.Readers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                readersQuery = readersQuery.Where(r =>
                    r.FullName.Contains(searchString) ||
                    r.LibraryCardNumber.Contains(searchString) ||
                    r.Email.Contains(searchString));
            }

            ViewData["CurrentFilter"] = searchString;
            return View(await readersQuery.OrderBy(r => r.FullName).ToListAsync());
        }

        // GET: Readers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var reader = await _context.Readers
                .Include(r => r.Loans)
                    .ThenInclude(l => l.Book)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reader == null) return NotFound();

            return View(reader);
        }

        // GET: Readers/Create
        public IActionResult Create()
        {
            var draft = HttpContext.Session.GetObject<Reader>(CreateSessionKey);
            if (draft != null)
            {
                ViewBag.IsRestoredFromSession = true;
                return View(draft);
            }

            return View(new Reader { RegistrationDate = DateTime.Today });
        }

        // POST: Readers/SaveCreateDraft
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveCreateDraft(
            [Bind("FullName,Email,PhoneNumber,LibraryCardNumber,RegistrationDate,Address")] Reader reader)
        {
            HttpContext.Session.SetObject(CreateSessionKey, reader);
            return Ok();
        }

        // POST: Readers/ClearCreateDraft
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearCreateDraft()
        {
            HttpContext.Session.Remove(CreateSessionKey);
            TempData["InfoMessage"] = "Чернетку нового читача очищено із сесії.";
            return RedirectToAction(nameof(Create));
        }

        // POST: Readers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("FullName,Email,PhoneNumber,LibraryCardNumber,RegistrationDate,Address")] Reader reader,
            string actionType = "db")
        {
            if (actionType == "session")
            {
                HttpContext.Session.SetObject(CreateSessionKey, reader);
                TempData["InfoMessage"] = "Дані читача збережено в сесії як чернетку (у БД не записано).";
                return RedirectToAction(nameof(Create));
            }

            if (await _context.Readers.AnyAsync(r => r.LibraryCardNumber == reader.LibraryCardNumber))
            {
                ModelState.AddModelError(nameof(reader.LibraryCardNumber), "Читач із таким номером квитка вже зареєстрований.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(reader);
                await _context.SaveChangesAsync();

                HttpContext.Session.Remove(CreateSessionKey);
                TempData["SuccessMessage"] = $"Читача «{reader.FullName}» зареєстровано в БД, сесію очищено.";
                return RedirectToAction(nameof(Index));
            }

            HttpContext.Session.SetObject(CreateSessionKey, reader);
            return View(reader);
        }

        // GET: Readers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var key = EditSessionKey(id.Value);
            var sessionReader = HttpContext.Session.GetObject<Reader>(key);

            if (sessionReader != null)
            {
                ViewBag.IsRestoredFromSession = true;
                return View(sessionReader);
            }

            var dbReader = await _context.Readers.FindAsync(id);
            if (dbReader == null) return NotFound();

            HttpContext.Session.SetObject(key, dbReader);
            ViewBag.IsRestoredFromSession = false;
            return View(dbReader);
        }

        // POST: Readers/SaveEditDraft/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveEditDraft(int id,
            [Bind("Id,FullName,Email,PhoneNumber,LibraryCardNumber,RegistrationDate,Address")] Reader reader)
        {
            reader.Id = id;
            HttpContext.Session.SetObject(EditSessionKey(id), reader);
            return Ok();
        }

        // POST: Readers/ResetEditDraft/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetEditDraft(int id)
        {
            HttpContext.Session.Remove(EditSessionKey(id));
            TempData["InfoMessage"] = "Чернетку сесії скинуто. Завантажено актуальні дані з БД.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        // POST: Readers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,FullName,Email,PhoneNumber,LibraryCardNumber,RegistrationDate,Address")] Reader reader,
            string actionType = "db")
        {
            if (id != reader.Id) return NotFound();

            var key = EditSessionKey(id);

            if (actionType == "session")
            {
                HttpContext.Session.SetObject(key, reader);
                TempData["InfoMessage"] = "Зміни збережено у сесії. Натисніть «Записати в БД» для збереження.";
                return RedirectToAction(nameof(Edit), new { id });
            }

            if (await _context.Readers.AnyAsync(r => r.LibraryCardNumber == reader.LibraryCardNumber && r.Id != reader.Id))
            {
                ModelState.AddModelError(nameof(reader.LibraryCardNumber), "Інший читач уже має цей номер квитка.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reader);
                    await _context.SaveChangesAsync();

                    HttpContext.Session.Remove(key);
                    TempData["SuccessMessage"] = $"Дані читача «{reader.FullName}» оновлено в БД, сесію очищено.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Readers.Any(e => e.Id == reader.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            HttpContext.Session.SetObject(key, reader);
            ViewBag.IsRestoredFromSession = true;
            return View(reader);
        }

        // GET: Readers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var reader = await _context.Readers.FirstOrDefaultAsync(r => r.Id == id);
            if (reader == null) return NotFound();

            return View(reader);
        }

        // POST: Readers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reader = await _context.Readers.FindAsync(id);
            if (reader != null)
            {
                _context.Readers.Remove(reader);
                await _context.SaveChangesAsync();
                HttpContext.Session.Remove(EditSessionKey(id));
            }
            return RedirectToAction(nameof(Index));
        }
    }
}