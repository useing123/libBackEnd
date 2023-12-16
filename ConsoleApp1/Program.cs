using LiteDB;
using System;

class Program
{
    static void Main(string[] args)
    {
        using (var db = new LiteDatabase(@"MyData.db"))
        {
            var bookManager = new BookManager(db.GetCollection<Book>("books"));
            bookManager.Run();
        }
    }
}

public class BookManager
{
    private readonly ILiteCollection<Book> _collection;

    public BookManager(ILiteCollection<Book> collection)
    {
        _collection = collection;
    }

    public void Run()
    {
        while (true)
        {
            Console.WriteLine("1. Add Book\n2. List Books\n3. Search Books\n4. Delete Book\n5. Update Book\n6. Exit");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    AddBook();
                    break;
                case "2":
                    ListBooks();
                    break;
                case "3":
                    SearchBooks();
                    break;
                case "4":
                    DeleteBook();
                    break;
                case "5":
                    UpdateBook();
                    break;
                case "6":
                    return;
                default:
                    Console.WriteLine("Invalid choice");
                    break;
            }
        }
    }

    private void AddBook()
    {
        Console.Write("Enter book title: ");
        var title = Console.ReadLine();
        Console.Write("Enter author: ");
        var author = Console.ReadLine();

        var book = new Book { Title = title, Author = author };
        _collection.Insert(book);
    }

    private void ListBooks()
    {
        var books = _collection.FindAll();
        foreach (var book in books)
        {
            Console.WriteLine($"{book.Id}: {book.Title} by {book.Author}");
        }
    }

    private void SearchBooks()
    {
        Console.Write("Enter keyword to search: ");
        var keyword = Console.ReadLine().ToLower();

        var books = _collection.Find(b => b.Title.ToLower().Contains(keyword) || b.Author.ToLower().Contains(keyword));
        foreach (var book in books)
        {
            Console.WriteLine($"{book.Id}: {book.Title} by {book.Author}");
        }
    }

    private void DeleteBook()
    {
        Console.Write("Enter book ID to delete: ");
        if (int.TryParse(Console.ReadLine(), out int id))
        {
            var result = _collection.Delete(id);
            if (result)
                Console.WriteLine("Book deleted successfully.");
            else
                Console.WriteLine("Book not found.");
        }
        else
        {
            Console.WriteLine("Invalid ID.");
        }
    }

    private void UpdateBook()
    {
        Console.Write("Enter book ID to update: ");
        if (int.TryParse(Console.ReadLine(), out int id))
        {
            var book = _collection.FindById(id);
            if (book != null)
            {
                Console.Write("Enter new title (leave blank to keep current): ");
                var title = Console.ReadLine();
                Console.Write("Enter new author (leave blank to keep current): ");
                var author = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(title))
                    book.Title = title;
                if (!string.IsNullOrWhiteSpace(author))
                    book.Author = author;

                _collection.Update(book);
                Console.WriteLine("Book updated successfully.");
            }
            else
            {
                Console.WriteLine("Book not found.");
            }
        }
        else
        {
            Console.WriteLine("Invalid ID.");
        }
    }
}

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
}
