using LibraryMvc.Data;
using LibraryMvc.Extensions;
using LibraryMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LibraryMvc.Controllers
{
    public class LoansController : Controller
    {
        private readonly LibraryContext _context;
        private const string CreateSessionKey = "Draft_CreateLoan";
        private static string EditSessionKey(int id) => $"Draft_EditLoan_{id}";

        public LoansController(LibraryContext context)
        {
            _context = context;
        }

        // GET: Loans
        public async Task<IActionResult> Index(string? statusFilter)
        {
            var loansQuery = _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Reader)
                .AsQueryable();

            if (statusFilter == "active")
                loansQuery = loansQuery.Where(l => l.Status == "Видано");
            else if (statusFilter == "returned")
                loansQuery = loansQuery.Where(l => l.Status == "Повернуто");
            else if (statusFilter == "overdue")
                loansQuery = loansQuery.Where(l => l.Status == "Видано" && l.DueDate < DateTime.Today);

            ViewData["StatusFilter"] = statusFilter;
            return View(await loansQuery.OrderByDescending(l => l.LoanDate).ToListAsync());
        }

        // GET: Loans/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var loan = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Reader)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null) return NotFound();

            return View(loan);
        }

        // GET: Loans/Create
        public IActionResult Create()
        {
            var draft = HttpContext.Session.GetObject<Loan>(CreateSessionKey);
            if (draft != null)
            {
                ViewBag.IsRestoredFromSession = true;
                PopulateDropDowns(draft.BookId, draft.ReaderId);
                return View(draft);
            }

            PopulateDropDowns();
            return View(new Loan
            {
                LoanDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(14),
                Status = "Видано"
            });
        }

        // POST: Loans/SaveCreateDraft
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveCreateDraft(
            [Bind("BookId,ReaderId,LoanDate,DueDate,Notes")] Loan loan)
        {
            loan.Status = "Видано";
            HttpContext.Session.SetObject(CreateSessionKey, loan);
            return Ok();
        }

        // POST: Loans/ClearCreateDraft
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearCreateDraft()
        {
            HttpContext.Session.Remove(CreateSessionKey);
            TempData["InfoMessage"] = "Чернетку видачі очищено із сесії.";
            return RedirectToAction(nameof(Create));
        }

        // POST: Loans/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("BookId,ReaderId,LoanDate,DueDate,Notes")] Loan loan,
            string actionType = "db")
        {
            loan.Status = "Видано";

            if (actionType == "session")
            {
                HttpContext.Session.SetObject(CreateSessionKey, loan);
                TempData["InfoMessage"] = "Чернетку видачі збережено в сесії (у БД не записано).";
                return RedirectToAction(nameof(Create));
            }

            var book = await _context.Books.FindAsync(loan.BookId);
            if (book == null)
            {
                ModelState.AddModelError(nameof(loan.BookId), "Обрану книгу не знайдено.");
            }
            else if (book.AvailableCopies <= 0)
            {
                ModelState.AddModelError(nameof(loan.BookId), "На жаль, вільних примірників цієї книги зараз немає.");
            }

            if (ModelState.IsValid && book != null)
            {
                loan.ReturnDate = null;
                book.AvailableCopies -= 1;

                _context.Add(loan);
                _context.Update(book);
                await _context.SaveChangesAsync();

                HttpContext.Session.Remove(CreateSessionKey);
                TempData["SuccessMessage"] = "Видачу книги оформлено в БД, сесію очищено.";
                return RedirectToAction(nameof(Index));
            }

            HttpContext.Session.SetObject(CreateSessionKey, loan);
            PopulateDropDowns(loan.BookId, loan.ReaderId);
            return View(loan);
        }

        // POST: Loans/ReturnBook/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnBook(int id)
        {
            var loan = await _context.Loans
                .Include(l => l.Book)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null) return NotFound();

            if (loan.Status != "Повернуто")
            {
                loan.Status = "Повернуто";
                loan.ReturnDate = DateTime.Today;

                if (loan.Book != null && loan.Book.AvailableCopies < loan.Book.TotalCopies)
                {
                    loan.Book.AvailableCopies += 1;
                }

                await _context.SaveChangesAsync();
                HttpContext.Session.Remove(EditSessionKey(id));
                TempData["SuccessMessage"] = "Книгу позначено як повернену.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Loans/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var key = EditSessionKey(id.Value);
            var sessionLoan = HttpContext.Session.GetObject<Loan>(key);

            if (sessionLoan != null)
            {
                ViewBag.IsRestoredFromSession = true;
                PopulateDropDowns(sessionLoan.BookId, sessionLoan.ReaderId);
                return View(sessionLoan);
            }

            var dbLoan = await _context.Loans.FindAsync(id);
            if (dbLoan == null) return NotFound();

            HttpContext.Session.SetObject(key, dbLoan);
            ViewBag.IsRestoredFromSession = false;
            PopulateDropDowns(dbLoan.BookId, dbLoan.ReaderId);
            return View(dbLoan);
        }

        // POST: Loans/SaveEditDraft/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveEditDraft(int id,
            [Bind("Id,BookId,ReaderId,LoanDate,DueDate,ReturnDate,Status,Notes")] Loan loan)
        {
            loan.Id = id;
            HttpContext.Session.SetObject(EditSessionKey(id), loan);
            return Ok();
        }

        // POST: Loans/ResetEditDraft/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetEditDraft(int id)
        {
            HttpContext.Session.Remove(EditSessionKey(id));
            TempData["InfoMessage"] = "Чернетку сесії скинуто. Завантажено актуальні дані з БД.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        // POST: Loans/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,BookId,ReaderId,LoanDate,DueDate,ReturnDate,Status,Notes")] Loan loan,
            string actionType = "db")
        {
            if (id != loan.Id) return NotFound();

            var key = EditSessionKey(id);

            if (actionType == "session")
            {
                HttpContext.Session.SetObject(key, loan);
                TempData["InfoMessage"] = "Зміни видачі збережено у сесії. Натисніть «Записати в БД» для збереження.";
                return RedirectToAction(nameof(Edit), new { id });
            }

            if (ModelState.IsValid)
            {
                var existingLoan = await _context.Loans.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
                if (existingLoan == null) return NotFound();

                if (existingLoan.Status == "Видано" && loan.Status == "Повернуто")
                {
                    var book = await _context.Books.FindAsync(loan.BookId);
                    if (book != null && book.AvailableCopies < book.TotalCopies)
                        book.AvailableCopies += 1;
                    loan.ReturnDate ??= DateTime.Today;
                }
                else if (existingLoan.Status == "Повернуто" && loan.Status == "Видано")
                {
                    var book = await _context.Books.FindAsync(loan.BookId);
                    if (book != null && book.AvailableCopies > 0)
                        book.AvailableCopies -= 1;
                    loan.ReturnDate = null;
                }

                _context.Update(loan);
                await _context.SaveChangesAsync();

                HttpContext.Session.Remove(key);
                TempData["SuccessMessage"] = "Запис видачі оновлено в БД, сесію очищено.";
                return RedirectToAction(nameof(Index));
            }

            HttpContext.Session.SetObject(key, loan);
            ViewBag.IsRestoredFromSession = true;
            PopulateDropDowns(loan.BookId, loan.ReaderId);
            return View(loan);
        }

        // GET: Loans/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var loan = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Reader)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null) return NotFound();

            return View(loan);
        }

        // POST: Loans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loan = await _context.Loans.Include(l => l.Book).FirstOrDefaultAsync(l => l.Id == id);
            if (loan != null)
            {
                if (loan.Status == "Видано" && loan.Book != null && loan.Book.AvailableCopies < loan.Book.TotalCopies)
                {
                    loan.Book.AvailableCopies += 1;
                }

                _context.Loans.Remove(loan);
                await _context.SaveChangesAsync();
                HttpContext.Session.Remove(EditSessionKey(id));
            }
            return RedirectToAction(nameof(Index));
        }

        private void PopulateDropDowns(object? selectedBook = null, object? selectedReader = null)
        {
            var books = _context.Books
                .OrderBy(b => b.Title)
                .Select(b => new
                {
                    b.Id,
                    DisplayText = $"{b.Title} — {b.Author} (доступно: {b.AvailableCopies})"
                })
                .ToList();

            var readers = _context.Readers
                .OrderBy(r => r.FullName)
                .Select(r => new
                {
                    r.Id,
                    DisplayText = $"{r.FullName} ({r.LibraryCardNumber})"
                })
                .ToList();

            ViewBag.BookId = new SelectList(books, "Id", "DisplayText", selectedBook);
            ViewBag.ReaderId = new SelectList(readers, "Id", "DisplayText", selectedReader);
        }
    }
}