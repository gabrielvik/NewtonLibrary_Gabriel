using Azure.Core;
using ConsoleCompanion;
using NewtonLibrary_Gabriel.Data;
using NewtonLibrary_Gabriel.Models;
using System.Globalization;

namespace NewtonLibrary_Gabriel
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // CompaniorHelper bugs out with several lines as question string
            Console.WriteLine("What would you like to do?\n" +
                "1. Create an author\n" +
                "2. Create a book\n" +
                "3. Create a new borrower\n" +
                "4. Loan a book\n" +
                "5. Return a book\n" +
                "6. Remove item (borrowers, books, authors)");

            ConsoleCompanionHelper cc = new();
            int choice;
            do
            {
                choice = cc.AskForInt("");
                if (choice > 6 || choice < 1)
                {
                    Console.WriteLine("Number must be between 1-6.");
                }
            } while (choice > 6 || choice < 1);

            Context context = new Context();
            switch (choice)
            {
                case 1:
                    CreateAuthor(cc, context);
                    break;
                case 2:
                    CreateBook(cc, context);
                    break;
                case 3:
                    CreateBorrower(cc, context);
                    break;
                case 4:
                    LoanBook(cc, context);
                    break;
                case 5:
                    ReturnBook(cc, context);
                    break;
                case 6:
                    RemoveItem(cc, context);
                    break;
            }
        }

        private static void CreateAuthor(ConsoleCompanionHelper cc, Context context)
        {
            string firstName = cc.AskForString("Enter author's first name: ");
            string lastName = cc.AskForString("Enter author's last name: ");

            Author addedAuthor = new Author();
            addedAuthor.FirstName = firstName;
            addedAuthor.LastName = lastName;

            context.Authors.Add(addedAuthor);
            context.SaveChanges();
        }

        private static void CreateBook(ConsoleCompanionHelper cc, Context context)
        {
            string bookTitle = cc.AskForString("Enter book title: ");
            int bookRating = cc.AskForInt("Enter book rating: ");
            string ISBN = cc.AskForString("Enter book ISBN: ");

            DateTime bookReleaseDate;
            do
            {
                string releaseDateString = cc.AskForString("Enter book release date (yyyy-MM-dd): ");

                if (DateTime.TryParseExact(releaseDateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out bookReleaseDate))
                    break;
                else
                    Console.WriteLine("Invalid date format. Please enter the date in the format yyyy-MM-dd.");
            } while (true);

            Book newBook = new Book
            {
                Title = bookTitle,
                Rating = bookRating,
                Isbn = ISBN,
                ReleaseDate = bookReleaseDate,
            };

            context.Books.Add(newBook);
            context.SaveChanges();
        }

        private static void CreateBorrower(ConsoleCompanionHelper cc, Context context)
        {

        }

        private static void LoanBook(ConsoleCompanionHelper cc, Context context)
        {

        }

        private static void ReturnBook(ConsoleCompanionHelper cc, Context context)
        {

        }

        private static void RemoveItem(ConsoleCompanionHelper cc, Context context)
        {

        }
    }
}