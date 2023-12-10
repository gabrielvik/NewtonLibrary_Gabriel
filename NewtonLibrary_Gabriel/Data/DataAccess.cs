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

        public void ConnectAuthorsWithBook(ConsoleCompanionHelper cc, Context context)
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

            // Connect authors with the book
            foreach (var authorName in authorNamesArray)
            {
                string trimmedAuthorName = authorName.Trim();
                Author? author = context.Authors
                    .FirstOrDefault(a => a.FirstName + " " + a.LastName == trimmedAuthorName);

                if (author == null)
                {
                    // Found actor that isn't already connected to the book
                    string[] splittedName = trimmedAuthorName.Split(' ');
                    if (splittedName.Length >= 2)
                    {
                        author = new Author
                        {
                            FirstName = splittedName[0],
                            LastName = splittedName[1]
                        };

                        context.Authors.Add(author);
                    }
                    else
                    {
                        Console.WriteLine($"Invalid author name: {trimmedAuthorName}, skipping.");
                        continue;
                    }
                }

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

            if (libraryCardId == -1)
                return;

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

            if (libraryCardId == -1)
                return;

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
            int choice = GetIntChoice(cc, 1, 3);

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

                if (libraryCardId == -1)
                    return;

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

        public void ShowBorrowHistory(ConsoleCompanionHelper cc, Context context)
        {
            Console.WriteLine("For what would you like to view the history?\n" +
            "1. A borrower\n" +
            "2. A book");

            // Keep going until user enters a correct input option
            int choice = GetIntChoice(cc, 1, 2);

            if(choice == 1)
            {
                int libraryCardId = RequestLibraryCardIdFromCardNumber(cc, context);

                if (libraryCardId == -1)
                    return;

                // Get loans from corresponding library card
                List<Loan>? loaned = context.Loans
                    .Where(loan => loan.LibraryCardId == libraryCardId)
                    .ToList();

                if(loaned == null || loaned.Count == 0)
                {
                    Console.WriteLine("No loans found for this library card.");
                    return;
                }

                Console.WriteLine("Loaned books:");
                for(int i = 0; i < loaned.Count; i++)
                {
                    // The book which has been borrowed in this loan
                    Book? loanedBook = context.Books.FirstOrDefault(book => book.Id == loaned[i].BookId);
                    if(loanedBook == null)
                        continue;

                    if (loaned[i].IsBorrowed)
                        Console.WriteLine($"{i + 1}. {loanedBook.Title}, loaned at {loaned[i].LoanDate}, and is still borrowed.");
                    else
                        Console.WriteLine($"{i + 1}. {loanedBook.Title}, loaned at {loaned[i].LoanDate}, and it was returned at {loaned[i].ReturnDate}.");
                }

            }
            if(choice == 2)
            {
                int bookId = RequestBookIdFromName(cc, context);

                Book? loanedBook = context.Books
                    .Where(book => book.Id == bookId)
                    .FirstOrDefault();

                if (loanedBook == null)
                    return;

                // All the loans of the book
                List<Loan>? loaned = context.Loans
                    .Where(loan => loan.BookId == bookId)
                    .ToList();

                Console.WriteLine($"All loans for the book {loanedBook.Title}");
                for (int i = 0; i < loaned.Count; i++)
                {
                    // The borrower of the loan
                    LibraryCard? libraryCard = context.LibraryCards.FirstOrDefault(card => card.Id == loaned[i].LibraryCardId);
                    
                    if (libraryCard == null)
                        continue;

                    if (loaned[i].IsBorrowed)
                        Console.WriteLine($"{i + 1}. loan by {libraryCard.FirstName} {libraryCard.LastName}. Loaned at {loaned[i].LoanDate}, not yet returned.");
                    else
                        Console.WriteLine($"{i + 1}. loan by {libraryCard.FirstName} {libraryCard.LastName}. Loaned at {loaned[i].LoanDate}, returned at {loaned[i].ReturnDate}");
                }

            }
        }

        public void Seed()
        {
            using (Context context = new Context())
            {
                // Remove existing stuff
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

                List<Book> seededBooks = new List<Book>();

                seededBooks.Add(new Book
                {
                    Title = "Harry Potter and the Sorcerer's Stone",
                    ReleaseDate = new DateTime(1998, 09, 01),
                    Isbn = "059035342X",
                    Rating = 9,
                    Authors = new List<Author> { seededAuthor01 }
                });
                context.Books.Add(seededBooks[0]);
                seededBooks.Add(new Book
                {
                    Title = "Harry Potter and the Chamber of Secrets",
                    ReleaseDate = new DateTime(1998, 07, 02),
                    Isbn = "0439064872",
                    Rating = 7,
                    Authors = new List<Author> { seededAuthor01 }
                });
                context.Books.Add(seededBooks[1]);
                seededBooks.Add(new Book
                {
                    Title = "Crime and Punishment",
                    ReleaseDate = new DateTime(1866, 12, 01),
                    Isbn = "0486415872",
                    Rating = 10,
                    Authors = new List<Author> { seededAuthor03 }
                });
                context.Books.Add(seededBooks[2]);
                seededBooks.Add(new Book
                {
                    Title = "The Lord of the Rings",
                    ReleaseDate = new DateTime(1954, 07, 27),
                    Isbn = "9780544003415",
                    Rating = 10,
                    Authors = new List<Author> { seededAuthor02 }
                });
                context.Books.Add(seededBooks[3]);

                context.SaveChanges(); // Save in between 

                List<string> firstNames = new List<string>() { "John", "Jane", "Jörgen", "Elsa", "Freja", "Jens", "Gabriel", "Wilmer", "Krister", "Madeleine", "Harry" };
                List<string> lastNames = new List<string>() { "Vikström", "Ekström", "Doe", "Dane", "Musk", "Thatcher", "Marley", "Ford", "Zimmerman", "Wilson", "Dawson" };

                // Generate library cards and loans
                Random rnd = new Random();
                for(int i = 0; i < 15; i++)
                {
                    LibraryCard seededLibraryCard = new LibraryCard();
                    seededLibraryCard.CardNumber = rnd.Next(10000000, 99999999).ToString();
                    seededLibraryCard.Pin = rnd.Next(1000, 9999).ToString();
                    seededLibraryCard.FirstName = firstNames[rnd.Next(0, firstNames.Count())];
                    seededLibraryCard.LastName = lastNames[rnd.Next(0, lastNames.Count())];
                    context.LibraryCards.Add(seededLibraryCard);

                    context.SaveChanges(); // Save so the loan can get the correct librarycard ID from DB.

                    for (int j = 0; j < rnd.Next(1, 4); j++) {
                        Loan seededLoan = new Loan();
                        seededLoan.BookId = seededBooks[rnd.Next(0, seededBooks.Count())].Id;
                        seededLoan.LibraryCardId = seededLibraryCard.Id;
                        seededLoan.LoanDate = new DateTime(rnd.Next(2015, 2023), rnd.Next(1, 13), rnd.Next(1, 29), rnd.Next(0, 24), rnd.Next(0, 60), rnd.Next(0, 60), rnd.Next(0, 1000));

                        if (rnd.Next(0, 2) == 1)
                        {
                            seededLoan.IsBorrowed = false;
                            seededLoan.ReturnDate = seededLoan.LoanDate + TimeSpan.FromDays(rnd.Next(1, 60));
                        }
                        else
                        {
                            seededLoan.IsBorrowed = true;
                        }
                        context.Loans.Add(seededLoan);
                    }
                }

                context.SaveChanges();
                Console.WriteLine("Done!");
            }
        }

        public static int GetIntChoice(ConsoleCompanionHelper cc, int minChoice, int maxChoice)
        {
            int choice;
            do
            {
                choice = cc.AskForInt("");
                if (choice > maxChoice || choice < minChoice)
                    Console.WriteLine($"Number must be between {minChoice}-{maxChoice}.");
            } while (choice > maxChoice || choice < minChoice);

            return choice;
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
                string bookName = cc.AskForString("Enter title of book: ").Trim();
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
                string cardNumber = cc.AskForString("Enter the cardnumber: ").Trim();

                LibraryCard? borrower = context.LibraryCards
                    .Where(book => book.CardNumber == cardNumber)
                    .FirstOrDefault();

                if (borrower == null)
                {
                    Console.WriteLine("Borrower not found. Please retry.");
                    return -1;
                }

                string cardPin = cc.AskForString("Enter the PIN code: ").Trim();
                if (cardPin != borrower.Pin)
                {
                    Console.WriteLine("You entered the wrong PIN code!");
                    return -1;
                }
                else // Found the borrower with the card number
                {
                    return borrower.Id;
                }

            } while (true);
        }
    }
}
