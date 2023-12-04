using Azure.Core;
using ConsoleCompanion;
using NewtonLibrary_Gabriel.Data;
using NewtonLibrary_Gabriel.Models;
using System.Globalization;
using System.Net;

namespace NewtonLibrary_Gabriel
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true) // Always keep going unless user quits
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

                // User selects option
                int choice;
                do
                {
                    choice = cc.AskForInt("");
                    if (choice > 6 || choice < 1)
                        Console.WriteLine("Number must be between 1-6.");
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
        }

        private static void CreateAuthor(ConsoleCompanionHelper cc, Context context)
        {
            string firstName = cc.AskForString("Enter author's first name: ");
            string lastName = cc.AskForString("Enter author's last name: ");

            Author newAuthor = new Author 
            { 
                FirstName = firstName,
                LastName = lastName,
            };

            context.Authors.Add(newAuthor);
            context.SaveChanges();

            Console.WriteLine("Author succesfully created\n");
        }

        private static void CreateBook(ConsoleCompanionHelper cc, Context context)
        {
            string bookTitle = cc.AskForString("Enter book title: ");
            int bookRating = cc.AskForInt("Enter book rating: ");
            string ISBN = cc.AskForString("Enter book ISBN: ");

            // Keep asking for date until correct format is inputted
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

            Console.WriteLine("Book sucessfully created\n");
        }

        private static void CreateBorrower(ConsoleCompanionHelper cc, Context context)
        {
            string firstName = cc.AskForString("Enter first name: ");
            string lastName = cc.AskForString("Enter last name: ");
            string cardNumber = cc.AskForString("Enter card number: ");
            string cardPin = cc.AskForString("Enter card pin: ");

            LibraryCard newBorrower = new LibraryCard
            {
                FirstName = firstName,
                LastName = lastName,
                CardNumber = cardNumber,
                Pin = cardPin
            };

            context.LibraryCards.Add(newBorrower);
            context.SaveChanges();

            Console.WriteLine("Borrower sucessfully created\n");
        }

        private static void LoanBook(ConsoleCompanionHelper cc, Context context)
        {
            int bookId = RequestBookIdFromName(cc, context);
            int libraryCardId = RequestLibraryCardIdFromCardNumber(cc, context);

            bool isLoaned = context.Loans.Any(loan => loan.BookId == bookId && libraryCardId == loan.LibraryCardId);
            if (isLoaned)
            {
                Console.WriteLine("This person has already loaned this book.\n");
                return;
            }

            Loan newLoan = new Loan
            {
                BookId = bookId,
                LibraryCardId = libraryCardId,
                LoanDate = DateTime.Now,
                IsBorrowed = true,
            };

            context.Loans.Add(newLoan);
            context.SaveChanges();

            Console.WriteLine("Book loan successfully created\n");
        }


        private static void ReturnBook(ConsoleCompanionHelper cc, Context context)
        {
            // User inputs the book title and library card
            int bookId = RequestBookIdFromName(cc, context);
            int libraryCardId = RequestLibraryCardIdFromCardNumber(cc, context);

            // Get a list of the loaned books (in case there is several instances of same book for some reason)
            List<Loan> loansToUpdate = context.Loans
                .Where(loan => loan.BookId == bookId && libraryCardId == loan.LibraryCardId)
                .ToList();

            foreach (Loan updatedLoan in loansToUpdate)
            {
                updatedLoan.IsBorrowed = false;
                updatedLoan.ReturnDate = DateTime.Now;
            }

            context.SaveChanges();

            Console.WriteLine("Book successfully returned\n");
        }

        private static void RemoveItem(ConsoleCompanionHelper cc, Context context)
        {
            Console.WriteLine("What would you like to remove?\n" +
            "1. An Author\n" +
            "2. A book\n" +
            "3. A borrower");

            // Keep going until user enters a correct input option
            int choice;
            do
            {
                choice = cc.AskForInt("");
                if (choice > 3 || choice < 1)
                    Console.WriteLine("Number must be between 1-3.");
            } while (choice > 3 || choice < 1);

            if(choice == 1) // Remove author
            {
                string authorName = cc.AskForString("Enter name of author in the format: (FirstName LastName): ").Trim().ToLower();
                string[] splittedName = authorName.Split(' ');

                if (splittedName.Length < 2) // Not valid name format
                    return;

                // Get the author with the corresponding first and lastname.
                Author? removedAuthor = context.Authors.SingleOrDefault(author => author.FirstName.ToLower() == splittedName[0] && author.LastName.ToLower() == splittedName[1]);
                if(removedAuthor != null) // Author exists
                {
                    context.Authors.Remove(removedAuthor);
                    context.SaveChanges();

                    Console.WriteLine("Removed author successfully.");
                }
                else
                {
                    Console.WriteLine("No such author found.");
                }
            }
            if(choice == 2) // Remove book
            {
                int bookId = RequestBookIdFromName(cc, context);

                Book? removedBook = context.Books.SingleOrDefault(book => book.Id == bookId);
                if (removedBook != null)
                {
                    context.Books.Remove(removedBook);
                    context.SaveChanges();

                    Console.WriteLine("Removed book successfully.");
                }
                else
                {
                    Console.WriteLine("No such book found.");
                }
            }
            if(choice == 3) // Remove borrower (library card)
            {
                int libraryCardId = RequestLibraryCardIdFromCardNumber(cc, context);

                LibraryCard? removedBorrower = context.LibraryCards.SingleOrDefault(libraryCard => libraryCard.Id == libraryCardId);
                if (removedBorrower != null)
                {
                    context.LibraryCards.Remove(removedBorrower);
                    context.SaveChanges();

                    Console.WriteLine("Removed borrower successfully.");
                }
                else
                {
                    Console.WriteLine("No such borrower found.");
                }
            }
        }

        /// <summary>
        /// Will open a prompt and ask for the name of a book, and return the id it has in the DB
        /// </summary>
        /// <param name="cc"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private static int RequestBookIdFromName(ConsoleCompanionHelper cc, Context context)
        {
            // Keep trying to ask to select a valid book name in DB.
            do
            {
                string bookName = cc.AskForString("Enter title of book: ");
                int? bookId;

                bookId = context.Books
                    .Where(book => book.Title == bookName)
                    .Select(book => (int?)book.Id)
                    .FirstOrDefault();

                if (bookId == null)
                {
                    Console.WriteLine("Book not found. Please retry.");
                }
                else // Found the book with the name
                {
                    return (int)bookId;
                }
            } while (true);
        }

        /// <summary>
        /// Will open a prompt and ask for Card Number, and return the id it has in the DB
        /// </summary>
        /// <param name="cc"></param>
        /// <param name="context"></param>
        /// <returns>Library Card ID from DB</returns>
        private static int RequestLibraryCardIdFromCardNumber(ConsoleCompanionHelper cc, Context context)
        {
            do
            {
                string cardNumber = cc.AskForString("Enter the cardnumber: ");
                int? borrowerId;

                borrowerId = context.LibraryCards
                    .Where(book => book.CardNumber == cardNumber)
                    .Select(book => (int?)book.Id)
                    .FirstOrDefault();

                if (borrowerId == null)
                {
                    Console.WriteLine("Borrower not found. Please retry.");
                }
                else // Found the borrower with the card number
                {
                    return (int)borrowerId;

                }
            } while (true);
        }
    }
}