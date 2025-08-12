using System.Text.Json;

namespace Inventory_Records_system
{
    // Marker interface
    public interface IInventoryEntity
    {
        int Id { get; }
    }

    // Immutable record implementing the marker interface
    public record InventoryItem(int Id, string Name, int Quantity, DateTime DateAdded) : IInventoryEntity;

    // Generic logger for inventory items
    public class InventoryLogger<T> where T : IInventoryEntity
    {
        private List<T> _log = new();
        private readonly string _filePath;

        public InventoryLogger(string filePath)
        {
            _filePath = filePath;
        }

        public void Add(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            _log.Add(item);
        }

        public List<T> GetAll() => new List<T>(_log);

        public void SaveToFile()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_log, options);
                File.WriteAllText(_filePath, json);
                Console.WriteLine($"Data saved to '{_filePath}'.");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Permission error when saving file: {ex.Message}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"I/O error when saving file: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error when saving file: {ex.Message}");
            }
        }

        public void LoadFromFile()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    Console.WriteLine("No saved file found — starting with an empty log.");
                    _log = new List<T>();
                    return;
                }

                string json = File.ReadAllText(_filePath);
                _log = JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
                Console.WriteLine($"Data loaded from '{_filePath}'.");
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Data format error when loading file: {ex.Message}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"I/O error when loading file: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error when loading file: {ex.Message}");
            }
        }

        public void ClearMemoryLog() => _log = new List<T>();
    }

    // Integration layer
    public class InventoryApp
    {
        private readonly InventoryLogger<InventoryItem> _logger;

        public InventoryApp(string filePath)
        {
            _logger = new InventoryLogger<InventoryItem>(filePath);
        }

        public void SeedSampleData()
        {
            // Use DateTime.UtcNow for reproducible timestamps if you prefer
            _logger.Add(new InventoryItem(1, "Laptop", 10, DateTime.Now));
            _logger.Add(new InventoryItem(2, "Mouse", 50, DateTime.Now));
            _logger.Add(new InventoryItem(3, "Keyboard", 20, DateTime.Now));
            _logger.Add(new InventoryItem(4, "Monitor", 15, DateTime.Now));
            _logger.Add(new InventoryItem(5, "Printer", 5, DateTime.Now));
            Console.WriteLine("Sample items seeded into logger (in-memory).");
        }

        public void SaveData() => _logger.SaveToFile();

        public void LoadData() => _logger.LoadFromFile();

        public void PrintAllItems()
        {
            var items = _logger.GetAll();
            if (items.Count == 0)
            {
                Console.WriteLine("(no items)");
                return;
            }

            Console.WriteLine("\nInventory Items:");
            foreach (var item in items)
            {
                Console.WriteLine($"ID: {item.Id}, Name: {item.Name}, Qty: {item.Quantity}, Added: {item.DateAdded}");
            }
        }

        public void ClearMemory() => _logger.ClearMemoryLog();
    }

    // Program entry
    internal class Program
    {
        static void Main(string[] args)
        {
            // File will be created in the working directory (project/bin folder)
            string filePath = "inventory.json";

            var app = new InventoryApp(filePath);

            // 1) Seed and save
            app.SeedSampleData();
            app.SaveData();

            // 2) Simulate new session by clearing in-memory data
            Console.WriteLine("\n--- Simulating new session (clearing memory) ---\n");
            app.ClearMemory();

            // 3) Load from file and print to verify persistence
            app.LoadData();
            app.PrintAllItems();

            Console.WriteLine("\nDone. Press Enter to exit.");
            Console.ReadLine();
        }
    }
}