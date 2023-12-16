using LiteDB;
using System;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        using (var db = new LiteDatabase(@"MyData.db"))
        {
            var books = db.GetCollection<Book>("books");

            while (true)
            {
                Console.WriteLine("1. Add Book\n2. List Books\n3. Exit");
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        AddBook(books);
                        break;
                    case "2":
                        ListBooks(books);
                        break;
                    case "3":
                        return;
                    default:
                        Console.WriteLine("Invalid choice");
                        break;
                }
            }
        }
    }

    static void AddBook(ILiteCollection<Book> collection)
    {
        Console.Write("Enter book title: ");
        var title = Console.ReadLine();
        Console.Write("Enter author: ");
        var author = Console.ReadLine();

        var book = new Book { Title = title, Author = author };
        collection.Insert(book);
    }

    static void ListBooks(ILiteCollection<Book> collection)
    {
        foreach (var book in collection.FindAll())
        {
            Console.WriteLine($"{book.Id}: {book.Title} by {book.Author}");
        }
    }
}

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
}