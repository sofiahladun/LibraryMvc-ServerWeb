using System.ComponentModel.DataAnnotations;

namespace LibraryMvc.Models
{
    public class Reader
    {
        public int Id { get; set; }

        [Display(Name = "ПІБ читача")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Телефон")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Номер клубної картки")]
        public string LibraryCardNumber { get; set; } = string.Empty;

        [Display(Name = "Дата реєстрації")]
        [DataType(DataType.Date)]
        public DateTime RegistrationDate { get; set; } = DateTime.Today;

        [Display(Name = "Адреса")]
        public string Address { get; set; } = string.Empty;

        // Навігаційна властивість: видачі цього читача
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}