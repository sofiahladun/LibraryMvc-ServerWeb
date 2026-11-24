using System.ComponentModel.DataAnnotations;

namespace LibraryMvc.Models
{
    public class Book : IValidatableObject
    {
        public int Id { get; set; }

        [Display(Name = "Назва книги")]
        [Required(ErrorMessage = "Поле «Назва книги» є обов'язковим.")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "Назва книги має містити від 2 до 150 символів.")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Автор")]
        [Required(ErrorMessage = "Вкажіть автора книги.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Ім'я автора має містити від 2 до 100 символів.")]
        public string Author { get; set; } = string.Empty;

        [Display(Name = "ISBN")]
        [Required(ErrorMessage = "Поле «ISBN» є обов'язковим.")]
        [StringLength(20, ErrorMessage = "ISBN не може перевищувати 20 символів.")]
        [RegularExpression(@"^[0-9\-X]{10,20}$",
            ErrorMessage = "Некоректний формат ISBN (дозволено цифри, дефіс та X, довжина 10–20 символів, напр. 978-966-03-8211-4).")]
        public string ISBN { get; set; } = string.Empty;

        [Display(Name = "Жанр")]
        [Required(ErrorMessage = "Вкажіть жанр книги.")]
        [StringLength(60, MinimumLength = 2, ErrorMessage = "Назва жанру має містити від 2 до 60 символів.")]
        public string Genre { get; set; } = string.Empty;

        [Display(Name = "Рік видання")]
        [Required(ErrorMessage = "Вкажіть рік видання.")]
        [Range(1450, 2026, ErrorMessage = "Рік видання має бути в межах від 1900 до 2027.")]
        public int PublicationYear { get; set; }

        [Display(Name = "Всього примірників")]
        [Required(ErrorMessage = "Вкажіть загальну кількість примірників.")]
        [Range(1, 1000, ErrorMessage = "Загальна кількість примірників має бути від 1 до 50.")]
        public int TotalCopies { get; set; }

        [Display(Name = "Доступно примірників")]
        [Required(ErrorMessage = "Вкажіть кількість доступних примірників.")]
        [Range(0, 1000, ErrorMessage = "Кількість доступних примірників має бути від 0 до 50.")]
        public int AvailableCopies { get; set; }

        // Навігаційна властивість: історія видач цієї книги
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (AvailableCopies > TotalCopies)
            {
                yield return new ValidationResult(
                    "Кількість доступних примірників не може перевищувати загальну кількість.",
                    new[] { nameof(AvailableCopies) });
            }
        }
    }
}