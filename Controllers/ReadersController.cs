using LibraryMvc.Data;
using LibraryMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryMvc.Controllers
{
    public class ReadersController : Controller
    {
        private readonly LibraryContext _context;

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
            return View(new Reader { RegistrationDate = DateTime.Today });
        }

        // POST: Readers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("FullName,Email,PhoneNumber,LibraryCardNumber,RegistrationDate,Address")] Reader reader)
        {
            if (ModelState.IsValid)
            {
                _context.Add(reader);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(reader);
        }

        // GET: Readers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var reader = await _context.Readers.FindAsync(id);
            if (reader == null) return NotFound();

            return View(reader);
        }

        // POST: Readers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,FullName,Email,PhoneNumber,LibraryCardNumber,RegistrationDate,Address")] Reader reader)
        {
            if (id != reader.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reader);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Readers.Any(e => e.Id == reader.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
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
            }
            return RedirectToAction(nameof(Index));
        }
    }
}