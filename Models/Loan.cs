using System.ComponentModel.DataAnnotations;

namespace LibraryMvc.Models
{
    public class Loan
    {
        public int Id { get; set; }

        [Display(Name = "Книга")]
        public int BookId { get; set; }

        [Display(Name = "Читач")]
        public int ReaderId { get; set; }

        [Display(Name = "Дата видачі")]
        [DataType(DataType.Date)]
        public DateTime LoanDate { get; set; } = DateTime.Today;

        [Display(Name = "Повернути до")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(14);

        [Display(Name = "Фактична дата повернення")]
        [DataType(DataType.Date)]
        public DateTime? ReturnDate { get; set; }

        [Display(Name = "Статус")]
        public string Status { get; set; } = "Видано";

        [Display(Name = "Примітки")]
        public string? Notes { get; set; }

        // Навігаційні властивості (nullable, щоб не блокувати ModelState при POST)
        public Book? Book { get; set; }
        public Reader? Reader { get; set; }
    }
}