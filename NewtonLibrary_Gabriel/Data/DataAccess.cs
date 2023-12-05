using ConsoleCompanion;
using NewtonLibrary_Gabriel.Models;
using System.Globalization;

namespace NewtonLibrary_Gabriel.Data
{
    internal class DataAccess
    {
        public void CreateAuthor(ConsoleCompanionHelper cc, Context context)
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

        public void CreateBook(ConsoleCompanionHelper cc, Context context)
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

        public void AssociateAuthorsWithBook(ConsoleCompanionHelper cc, Context context)
        {
            string bookTitle = cc.AskForString("Enter the title of the book: ");

            // Find the book by title
            Book? existingBook = context.Books.SingleOrDefault(book => book.Title == bookTitle);

            if (existingBook == null)
            {
                Console.WriteLine($"No book found with the title '{bookTitle}'.");
                return;
            }

            // Make sure authors list is initialized
            if (existingBook.Authors == null)
                existingBook.Authors = new List<Author>();

            string authorNames = cc.AskForString("Enter author names (comma-separated): ");
            string[] authorNamesArray = authorNames.Split(',');

            // Create or associate authors with the book
            foreach (var authorName in authorNamesArray)
            {
                string trimmedAuthorName = authorName.Trim();
                Author? author = context.Authors
                    .FirstOrDefault(a => a.FirstName + " " + a.LastName == trimmedAuthorName);

                if (author == null)
                {
                    // Create a new author if not found
                    string[] nameParts = trimmedAuthorName.Split(' ');
                    if (nameParts.Length >= 2)
                    {
                        author = new Author
                        {
                            FirstName = nameParts[0],
                            LastName = nameParts[1]
                        };

                        context.Authors.Add(author);
                    }
                    else
                    {
                        Console.WriteLine($"Invalid author name: {trimmedAuthorName}, skipping.");
                        continue;
                    }
                }

                // Associate the author with the book
                existingBook.Authors.Add(author);
            }

            context.SaveChanges();

            Console.WriteLine($"Authors successfully associated with the book '{existingBook.Title}'.");
        }

        public void CreateBorrower(ConsoleCompanionHelper cc, Context context)
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

        public void LoanBook(ConsoleCompanionHelper cc, Context context)
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


        public void ReturnBook(ConsoleCompanionHelper cc, Context context)
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

        public void RemoveItem(ConsoleCompanionHelper cc, Context context)
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

            if (choice == 1) // Remove author
            {
                string authorName = cc.AskForString("Enter name of author in the format: (FirstName LastName): ").Trim().ToLower();
                string[] splittedName = authorName.Split(' ');

                if (splittedName.Length < 2) // Not valid name format
                    return;

                // Get the author with the corresponding first and lastname.
                Author? removedAuthor = context.Authors.SingleOrDefault(author => author.FirstName.ToLower() == splittedName[0] && author.LastName.ToLower() == splittedName[1]);
                if (removedAuthor != null) // Author exists
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
            if (choice == 2) // Remove book
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
            if (choice == 3) // Remove borrower (library card)
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

        public void Seed()
        {
            using (Context context = new Context())
            {

                // Clear existing data
                context.Authors.RemoveRange(context.Authors);
                context.Books.RemoveRange(context.Books);
                context.LibraryCards.RemoveRange(context.LibraryCards);
                context.Loans.RemoveRange(context.Loans);

                context.SaveChanges();

                Author seededAuthor01 = new Author();
                seededAuthor01.FirstName = "J.K";
                seededAuthor01.LastName = "Rowling";
                context.Authors.Add(seededAuthor01);

                Author seededAuthor02 = new Author();
                seededAuthor02.FirstName = "J.R.R";
                seededAuthor02.LastName = "Tolkien";
                context.Authors.Add(seededAuthor02);

                Author seededAuthor03 = new Author();
                seededAuthor03.FirstName = "Fyodor";
                seededAuthor03.LastName = "Dostoevsky";
                context.Authors.Add(seededAuthor03);

                Book seededBook01 = new Book();
                seededBook01.Title = "Harry Potter and the Sorcerer's Stone";
                seededBook01.ReleaseDate = new DateTime(1998, 09, 01);
                seededBook01.Isbn = "059035342X";
                seededBook01.Rating = 9;
                seededBook01.Authors = new List<Author>() { seededAuthor01 };
                context.Books.Add(seededBook01);

                Book seededBook02 = new Book();
                seededBook02.Title = "Harry Potter and the Chamber of Secrets";
                seededBook02.ReleaseDate = new DateTime(1998, 07, 02);
                seededBook02.Isbn = "0439064872";
                seededBook02.Rating = 7;
                seededBook02.Authors = new List<Author>() { seededAuthor01 };
                context.Books.Add(seededBook02);

                Book seededBook03 = new Book();
                seededBook03.Title = "Crime and Punishment";
                seededBook03.ReleaseDate = new DateTime(1866, 12, 01);
                seededBook03.Isbn = "0486415872";
                seededBook03.Rating = 10;
                seededBook03.Authors = new List<Author>() { seededAuthor03 };
                context.Books.Add(seededBook03);

                Book seededBook04 = new Book();
                seededBook04.Title = "The Lord of the Rings";
                seededBook04.ReleaseDate = new DateTime(1954, 07, 27);
                seededBook04.Isbn = "9780544003415";
                seededBook04.Rating = 10;
                seededBook04.Authors = new List<Author>() { seededAuthor02 };
                context.Books.Add(seededBook04);

                context.SaveChanges();

                LibraryCard seededLibraryCard01 = new LibraryCard();
                seededLibraryCard01.CardNumber = "84741823";
                seededLibraryCard01.Pin = "4817";
                seededLibraryCard01.FirstName = "John";
                seededLibraryCard01.LastName = "Doe";
                context.LibraryCards.Add(seededLibraryCard01);

                LibraryCard seededLibraryCard02 = new LibraryCard();
                seededLibraryCard02.CardNumber = "74943765";
                seededLibraryCard02.Pin = "8236";
                seededLibraryCard02.FirstName = "Jane";
                seededLibraryCard02.LastName = "Dane";
                context.LibraryCards.Add(seededLibraryCard02);

                context.SaveChanges();

                Loan seededLoan01 = new Loan();
                seededLoan01.BookId = seededBook01.Id;
                seededLoan01.LibraryCardId = seededLibraryCard01.Id;
                seededLoan01.LoanDate = new DateTime(2023, 02, 05);
                seededLoan01.IsBorrowed = true;
                context.Loans.Add(seededLoan01);

                Loan seededLoan02 = new Loan();
                seededLoan02.BookId = seededBook03.Id;
                seededLoan02.LibraryCardId = seededLibraryCard02.Id;
                seededLoan02.LoanDate = new DateTime(2022, 05, 20);
                seededLoan02.ReturnDate = new DateTime(2023, 05, 21);
                seededLoan02.IsBorrowed = false;
                context.Loans.Add(seededLoan02);

                context.SaveChanges();
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
