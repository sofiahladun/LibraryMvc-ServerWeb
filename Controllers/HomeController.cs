using LibraryMvc.Data;
using LibraryMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace LibraryMvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly LibraryContext _context;

        public HomeController(LibraryContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalBooks = await _context.Books.CountAsync();
            ViewBag.AvailableCopies = await _context.Books.SumAsync(b => b.AvailableCopies);
            ViewBag.TotalReaders = await _context.Readers.CountAsync();
            ViewBag.ActiveLoans = await _context.Loans.CountAsync(l => l.Status == "Видано");
            ViewBag.OverdueLoans = await _context.Loans.CountAsync(l => l.Status == "Видано" && l.DueDate < DateTime.Today);

            var recentLoans = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Reader)
                .OrderByDescending(l => l.LoanDate)
                .Take(5)
                .ToListAsync();

            return View(recentLoans);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}