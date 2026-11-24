using LibraryMvc.Models;

namespace LibraryMvc.Data
{
    public static class DbInitializer
    {
        public static void Seed(LibraryContext context)
        {
            if (context.Books.Any() || context.Readers.Any())
                return;

            var books = new List<Book>
            {
                new Book
                {
                    Title = "Запаморочення",
                    Author = "Франк Тільє",
                    ISBN = "978-966-03-8211-4",
                    Genre = "Трилер",
                    PublicationYear = 2020,
                    TotalCopies = 5,
                    AvailableCopies = 4
                },
                new Book
                {
                    Title = "Сад метеликів",
                    Author = "Дот Гатчісон",
                    ISBN = "978-617-12-5092-5",
                    Genre = "Трилер",
                    PublicationYear = 2019,
                    TotalCopies = 4,
                    AvailableCopies = 3
                },
                new Book
                {
                    Title = "Убивство Роджера Екройда",
                    Author = "Агата Крісті",
                    ISBN = "978-617-7544-01-1",
                    Genre = "Детектив",
                    PublicationYear = 2021,
                    TotalCopies = 3,
                    AvailableCopies = 3
                },
                new Book
                {
                    Title = "1984",
                    Author = "Джордж Орвелл",
                    ISBN = "978-966-917-015-6",
                    Genre = "Антиутопія",
                    PublicationYear = 2022,
                    TotalCopies = 6,
                    AvailableCopies = 5
                }
            };

            var readers = new List<Reader>
            {
                new Reader
                {
                    FullName = "Коваленко Олександр Ігорович",
                    Email = "o.kovalenko@example.com",
                    PhoneNumber = "+380671112233",
                    LibraryCardNumber = "LIB-2026-001",
                    RegistrationDate = new DateTime(2026, 1, 15),
                    Address = "м. Львів, вул. С. Бандери, 12"
                },
                new Reader
                {
                    FullName = "Мельник Анна Петрівна",
                    Email = "a.melnyk@example.com",
                    PhoneNumber = "+380932223344",
                    LibraryCardNumber = "LIB-2026-002",
                    RegistrationDate = new DateTime(2026, 2, 10),
                    Address = "м. Львів, пр. Свободи, 24"
                },
                new Reader
                {
                    FullName = "Шевчук Максим Андрійович",
                    Email = "m.shevchuk@example.com",
                    PhoneNumber = "+380503334455",
                    LibraryCardNumber = "LIB-2026-003",
                    RegistrationDate = new DateTime(2026, 3, 5),
                    Address = "м. Львів, вул. Городоцька, 85"
                }
            };

            context.Books.AddRange(books);
            context.Readers.AddRange(readers);
            context.SaveChanges();

            var loans = new List<Loan>
            {
                new Loan
                {
                    BookId = books[0].Id,
                    ReaderId = readers[0].Id,
                    LoanDate = DateTime.Today.AddDays(-5),
                    DueDate = DateTime.Today.AddDays(9),
                    Status = "Видано",
                    Notes = "Стан книги відмінний"
                },
                new Loan
                {
                    BookId = books[1].Id,
                    ReaderId = readers[1].Id,
                    LoanDate = DateTime.Today.AddDays(-20),
                    DueDate = DateTime.Today.AddDays(-6),
                    ReturnDate = DateTime.Today.AddDays(-7),
                    Status = "Повернуто",
                    Notes = "Повернуто вчасно"
                },
                new Loan
                {
                    BookId = books[3].Id,
                    ReaderId = readers[2].Id,
                    LoanDate = DateTime.Today.AddDays(-18),
                    DueDate = DateTime.Today.AddDays(-4),
                    Status = "Видано",
                    Notes = "Протермінована видача"
                }
            };

            context.Loans.AddRange(loans);
            context.SaveChanges();
        }
    }
}