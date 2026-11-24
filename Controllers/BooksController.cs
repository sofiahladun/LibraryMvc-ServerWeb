using LibraryMvc.Data;
using LibraryMvc.Extensions;
using LibraryMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryMvc.Controllers
{
    public class BooksController : Controller
    {
        private readonly LibraryContext _context;
        private const string CreateSessionKey = "Draft_CreateBook";
        private static string EditSessionKey(int id) => $"Draft_EditBook_{id}";

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
            var draft = HttpContext.Session.GetObject<Book>(CreateSessionKey);
            if (draft != null)
            {
                ViewBag.IsRestoredFromSession = true;
                return View(draft);
            }

            return View(new Book { PublicationYear = DateTime.Today.Year, TotalCopies = 1, AvailableCopies = 1 });
        }

        // POST: Books/SaveCreateDraft (AJAX автозбереження в сесію)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveCreateDraft(
            [Bind("Title,Author,ISBN,Genre,PublicationYear,TotalCopies,AvailableCopies")] Book book)
        {
            HttpContext.Session.SetObject(CreateSessionKey, book);
            return Ok();
        }

        // POST: Books/ClearCreateDraft
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearCreateDraft()
        {
            HttpContext.Session.Remove(CreateSessionKey);
            TempData["InfoMessage"] = "Чернетку створення книги очищено із сесії.";
            return RedirectToAction(nameof(Create));
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Title,Author,ISBN,Genre,PublicationYear,TotalCopies,AvailableCopies")] Book book,
            string actionType = "db")
        {
            if (actionType == "session")
            {
                HttpContext.Session.SetObject(CreateSessionKey, book);
                TempData["InfoMessage"] = "Дані збережено в сесії як чернетку (у БД ще не записано).";
                return RedirectToAction(nameof(Create));
            }

            // Перевірка унікальності ISBN
            if (await _context.Books.AnyAsync(b => b.ISBN == book.ISBN))
            {
                ModelState.AddModelError(nameof(book.ISBN), "Книга з таким ISBN вже існує в каталозі.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(book);
                await _context.SaveChangesAsync();

                // Очистка сесії після успішного запису в БД
                HttpContext.Session.Remove(CreateSessionKey);
                TempData["SuccessMessage"] = $"Книгу «{book.Title}» успішно додано в БД, чернетку сесії очищено.";
                return RedirectToAction(nameof(Index));
            }

            // Зберігаємо поточні дані у сесії при помилці валідації
            HttpContext.Session.SetObject(CreateSessionKey, book);
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var key = EditSessionKey(id.Value);
            var sessionBook = HttpContext.Session.GetObject<Book>(key);

            if (sessionBook != null)
            {
                ViewBag.IsRestoredFromSession = true;
                return View(sessionBook);
            }

            var dbBook = await _context.Books.FindAsync(id);
            if (dbBook == null) return NotFound();

            // Завантажуємо дані з БД у сесію для редагування
            HttpContext.Session.SetObject(key, dbBook);
            ViewBag.IsRestoredFromSession = false;
            return View(dbBook);
        }

        // POST: Books/SaveEditDraft/5 (AJAX автозбереження змін у сесію)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveEditDraft(int id,
            [Bind("Id,Title,Author,ISBN,Genre,PublicationYear,TotalCopies,AvailableCopies")] Book book)
        {
            book.Id = id;
            HttpContext.Session.SetObject(EditSessionKey(id), book);
            return Ok();
        }

        // POST: Books/ResetEditDraft/5 (Скинути сесію та повернути дані з БД)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetEditDraft(int id)
        {
            HttpContext.Session.Remove(EditSessionKey(id));
            TempData["InfoMessage"] = "Чернетку сесії скинуто. Завантажено актуальні дані з бази даних.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,Title,Author,ISBN,Genre,PublicationYear,TotalCopies,AvailableCopies")] Book book,
            string actionType = "db")
        {
            if (id != book.Id) return NotFound();

            var key = EditSessionKey(id);

            if (actionType == "session")
            {
                HttpContext.Session.SetObject(key, book);
                TempData["InfoMessage"] = "Зміни збережено у сесії. Для оновлення БД натисніть «Записати в БД».";
                return RedirectToAction(nameof(Edit), new { id });
            }

            if (await _context.Books.AnyAsync(b => b.ISBN == book.ISBN && b.Id != book.Id))
            {
                ModelState.AddModelError(nameof(book.ISBN), "Інша книга з таким ISBN вже існує в каталозі.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();

                    // Очищаємо сесію після запису в БД
                    HttpContext.Session.Remove(key);
                    TempData["SuccessMessage"] = $"Зміни для книги «{book.Title}» записано в БД, сесію очищено.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Books.Any(e => e.Id == book.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            HttpContext.Session.SetObject(key, book);
            ViewBag.IsRestoredFromSession = true;
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
                HttpContext.Session.Remove(EditSessionKey(id));
            }
            return RedirectToAction(nameof(Index));
        }
    }
}