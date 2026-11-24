using System.ComponentModel.DataAnnotations;

namespace LibraryMvc.Models
{
    public class Reader : IValidatableObject
    {
        public int Id { get; set; }

        [Display(Name = "ПІБ читача")]
        [Required(ErrorMessage = "Введіть ПІБ читача.")]
        [StringLength(120, MinimumLength = 3, ErrorMessage = "ПІБ має містити від 3 до 120 символів.")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Введіть електронну пошту.")]
        [EmailAddress(ErrorMessage = "Некоректний формат електронної пошти.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Email не може перевищувати 100 символів.")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Телефон")]
        [Required(ErrorMessage = "Введіть номер телефону.")]
        [RegularExpression(@"^\+380\d{9}$",
            ErrorMessage = "Телефон має відповідати формату +380XXXXXXXXX (наприклад, +380671112233).")]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Номер клубної картки")]
        [Required(ErrorMessage = "Введіть номер читацького квитка.")]
        [RegularExpression(@"^LIB-\d{4}-\d{3}$",
            ErrorMessage = "Формат квитка має бути LIB-РІК-НОМЕР (наприклад, LIB-2026-004).")]
        [StringLength(20)]
        public string LibraryCardNumber { get; set; } = string.Empty;

        [Display(Name = "Дата реєстрації")]
        [Required(ErrorMessage = "Вкажіть дату реєстрації.")]
        [DataType(DataType.Date)]
        public DateTime RegistrationDate { get; set; } = DateTime.Today;

        [Display(Name = "Адреса")]
        [Required(ErrorMessage = "Вкажіть адресу проживання.")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Адреса має містити від 5 до 200 символів.")]
        public string Address { get; set; } = string.Empty;

        // Навігаційна властивість: видачі цього читача
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (RegistrationDate.Date > DateTime.Today)
            {
                yield return new ValidationResult(
                    "Дата реєстрації не може бути в майбутньому.",
                    new[] { nameof(RegistrationDate) });
            }
        }
    }
}