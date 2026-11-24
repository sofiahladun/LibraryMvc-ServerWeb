using LibraryMvc.Data;
using LibraryMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryMvc.Controllers
{
    public class BooksController : Controller
    {
        private readonly LibraryContext _context;

        public BooksController(LibraryContext context)
        {
            _context = context;
        }

        // GET: Books
        public async Task<IActionResult> Index(string? searchString)
        {
            var booksQuery = _context.Books.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                booksQuery = booksQuery.Where(b =>
                    b.Title.Contains(searchString) ||
                    b.Author.Contains(searchString) ||
                    b.Genre.Contains(searchString) ||
                    b.ISBN.Contains(searchString));
            }

            ViewData["CurrentFilter"] = searchString;
            return View(await booksQuery.OrderBy(b => b.Title).ToListAsync());
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books
                .Include(b => b.Loans)
                    .ThenInclude(l => l.Reader)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null) return NotFound();

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Title,Author,ISBN,Genre,PublicationYear,TotalCopies,AvailableCopies")] Book book)
        {
            if (book.AvailableCopies > book.TotalCopies)
            {
                ModelState.AddModelError("AvailableCopies", "Доступних примірників не може бути більше за загальну кількість.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            return View(book);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,Title,Author,ISBN,Genre,PublicationYear,TotalCopies,AvailableCopies")] Book book)
        {
            if (id != book.Id) return NotFound();

            if (book.AvailableCopies > book.TotalCopies)
            {
                ModelState.AddModelError("AvailableCopies", "Доступних примірників не може бути більше за загальну кількість.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Books.Any(e => e.Id == book.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) return NotFound();

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}