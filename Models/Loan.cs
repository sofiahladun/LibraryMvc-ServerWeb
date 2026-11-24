using System.ComponentModel.DataAnnotations;

namespace LibraryMvc.Models
{
    public class Loan
    {
        public int Id { get; set; }

        [Display(Name = "Книга")]
        [Required(ErrorMessage = "Оберіть книгу.")]
        [Range(1, int.MaxValue, ErrorMessage = "Оберіть книгу зі списку.")]
        public int BookId { get; set; }

        [Display(Name = "Читач")]
        [Required(ErrorMessage = "Оберіть читача.")]
        [Range(1, int.MaxValue, ErrorMessage = "Оберіть читача зі списку.")]
        public int ReaderId { get; set; }

        [Display(Name = "Дата видачі")]
        [Required(ErrorMessage = "Вкажіть дату видачі.")]
        [DataType(DataType.Date)]
        public DateTime LoanDate { get; set; } = DateTime.Today;

        [Display(Name = "Повернути до")]
        [Required(ErrorMessage = "Вкажіть планову дату повернення.")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(14);

        [Display(Name = "Фактична дата повернення")]
        [DataType(DataType.Date)]
        public DateTime? ReturnDate { get; set; }

        [Display(Name = "Статус")]
        [Required(ErrorMessage = "Вкажіть статус видачі.")]
        [StringLength(30, ErrorMessage = "Статус не може перевищувати 30 символів.")]
        public string Status { get; set; } = "Видано";

        [Display(Name = "Примітки")]
        [StringLength(300, ErrorMessage = "Примітки не можуть перевищувати 300 символів.")]
        public string? Notes { get; set; }

        // Навігаційні властивості (nullable, щоб не блокувати ModelState при POST)
        public Book? Book { get; set; }
        public Reader? Reader { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DueDate.Date < LoanDate.Date)
            {
                yield return new ValidationResult(
                    "Планова дата повернення не може бути раніше дати видачі.",
                    new[] { nameof(DueDate) });
            }

            if (ReturnDate.HasValue && ReturnDate.Value.Date < LoanDate.Date)
            {
                yield return new ValidationResult(
                    "Фактична дата повернення не може бути раніше дати видачі.",
                    new[] { nameof(ReturnDate) });
            }
        }
    }
}