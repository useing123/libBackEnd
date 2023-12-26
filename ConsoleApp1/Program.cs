using LiteDB;
using System;
using System.IO;
using dotenv.net;

class Program
{
    static void Main(string[] args)
    {
        DotEnv.Load(); // Загружаем переменные окружения

        var dbPath = Environment.GetEnvironmentVariable("DATABASE_PATH") ?? @"MyData.db";
        Logger.SetLogPath(Environment.GetEnvironmentVariable("LOG_FILE_PATH") ?? "logs.txt");
        Logger.Log("Application started");

        try
        {
            using (var db = new LiteDatabase(dbPath))
            {
                var bookManager = new BookManager(db.GetCollection<Book>("books"));
                bookManager.Run();
            }
        }
        catch (Exception ex)
        {
            Logger.Log($"Unexpected error: {ex.Message}", "ERROR");
        }
        Logger.Log("Application finished");
    }
}


public class BookManager
{
    private readonly ILiteCollection<Book> _collection;
    /// <summary>
    /// Конструктор который инициалзирует коллекцию
    /// </summary>
    /// <param name="collection">LiteDB collection for books</param>
    public BookManager(ILiteCollection<Book> collection)
    {
        _collection = collection;
    }
    /// <summary>
    /// Цикл который запускает нашу программу в консоли и не закрывает ее
    /// </summary>
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
    /// <summary>
    /// Создание книг в базе данных с переменными(id, title, author)
    /// </summary>
    private void AddBook()
    {
        try
        {
            Console.Write("Enter book title: ");
            var title = Console.ReadLine();
            Console.Write("Enter author: ");
            var author = Console.ReadLine();

            var book = new Book { Title = title, Author = author };
            _collection.Insert(book);
            Logger.Log($"Added book: {title} by {author}");
        }
        catch (Exception ex)
        {
            Logger.Log($"Error adding book: {ex.Message}", "ERROR");
        }
    }
    /// <summary>
    /// Вывод всех существующих книг в коллекции
    /// </summary>
    private void ListBooks()
    {
        try
        {
            var books = _collection.FindAll();
            foreach (var book in books)
            {
                Console.WriteLine($"{book.Id}: {book.Title} by {book.Author}");
            }
            Logger.Log("Listed all books");
        }
        catch (Exception ex)
        {
            Logger.Log($"Error listing books: {ex.Message}", "ERROR");
        }
    }
    /// <summary>
    /// Поиск книг в коллекции сначало по названию книги потом по автору и выводит похожие
    /// </summary>
    private void SearchBooks()
    {
        try
        {
            Console.Write("Enter keyword to search: ");
            var keyword = Console.ReadLine().ToLower();

            var books = _collection.Find(b => b.Title.ToLower().Contains(keyword) || b.Author.ToLower().Contains(keyword));
            foreach (var book in books)
            {
                Console.WriteLine($"{book.Id}: {book.Title} by {book.Author}");
            }
            Logger.Log($"Searched books with keyword: {keyword}");
        }
        catch (Exception ex)
        {
            Logger.Log($"Error searching books: {ex.Message}", "ERROR");
        }
    }
    /// <summary>
    /// Удаление книги из библиотеки
    /// </summary>
    private void DeleteBook()
    {
        try
        {
            Console.Write("Enter book ID to delete: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var result = _collection.Delete(id);
                if (result)
                {
                    Console.WriteLine("Book deleted successfully.");
                    Logger.Log($"Deleted book with ID: {id}");
                }
                else
                {
                    Console.WriteLine("Book not found.");
                    Logger.Log($"Attempted to delete non-existing book with ID: {id}", "WARNING");
                }
            }
            else
            {
                Console.WriteLine("Invalid ID.");
                Logger.Log("Invalid ID entered for deletion", "WARNING");
            }
        }
        catch (Exception ex)
        {
            Logger.Log($"Error deleting book: {ex.Message}", "ERROR");
        }
    }
    /// <summary>
    /// Обновление информации
    /// </summary>
    private void UpdateBook()
    {
        try
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
                    Logger.Log($"Updated book with ID: {id}");
                }
                else
                {
                    Console.WriteLine("Book not found.");
                    Logger.Log($"Attempted to update non-existing book with ID: {id}", "WARNING");
                }
            }
            else
            {
                Console.WriteLine("Invalid ID.");
                Logger.Log("Invalid ID entered for update", "WARNING");
            }
        }
        catch (Exception ex)
        {
            Logger.Log($"Error updating book: {ex.Message}", "ERROR");
        }
    }
}
/// <summary>
/// Класс книги который содержит в себе (int, Title, Author)
/// </summary>
public class Book
{
    // Properties for Book class
    public int Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
}

public static class Logger
{
    private static string logFilePath = "logs.txt";

    public static void SetLogPath(string path)
    {
        logFilePath = path;
    }

    public static void Log(string message, string level = "INFO")
    {
        var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
        try
        {
            File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing to log file: {ex.Message}");
        }
    }
}