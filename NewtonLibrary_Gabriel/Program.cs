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
                    "3. Connect authors to a book\n" +
                    "4. Create a new borrower\n" +
                    "5. Loan a book\n" +
                    "6. Return a book\n" +
                    "7. Remove item (borrowers, books, authors)\n" +
                    "8. DEV: Seed database (Will remove all current data in DB, and replace it with stuff in Seed())");

                ConsoleCompanionHelper cc = new ConsoleCompanionHelper();

                // User selects option
                int choice;
                do
                {
                    choice = cc.AskForInt("");
                    if (choice > 8 || choice < 1)
                        Console.WriteLine("Number must be between 1-8.");
                } while (choice > 8 || choice < 1);

                // Run the desired option in dataaccess
                Context context = new Context();
                DataAccess dataAccess = new DataAccess();
                switch (choice)
                {
                    case 1:
                        dataAccess.CreateAuthor(cc, context);
                        break;
                    case 2:
                        dataAccess.CreateBook(cc, context);
                        break;
                    case 3:
                        dataAccess.ConnectAuthorsWithBook(cc, context);
                        break;
                    case 4:
                        dataAccess.CreateBorrower(cc, context);
                        break;
                    case 5:
                        dataAccess.LoanBook(cc, context);
                        break;
                    case 6:
                        dataAccess.ReturnBook(cc, context);
                        break;
                    case 7:
                        dataAccess.RemoveItem(cc, context);
                        break;
                    case 8:
                        dataAccess.Seed();
                        break;
                }
            }
        }
    }
}