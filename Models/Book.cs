using System.ComponentModel.DataAnnotations;

namespace LibraryMvc.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Display(Name = "Назва книги")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Автор")]
        public string Author { get; set; } = string.Empty;

        [Display(Name = "ISBN")]
        public string ISBN { get; set; } = string.Empty;

        [Display(Name = "Жанр")]
        public string Genre { get; set; } = string.Empty;

        [Display(Name = "Рік видання")]
        public int PublicationYear { get; set; }

        [Display(Name = "Всього примірників")]
        public int TotalCopies { get; set; }

        [Display(Name = "Доступно примірників")]
        public int AvailableCopies { get; set; }

        // Навігаційна властивість: історія видач цієї книги
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}