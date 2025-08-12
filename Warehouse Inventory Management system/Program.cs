namespace WarehouseInventorySystem
{
    // Marker interface
    public interface IInventoryItem
    {
        int Id { get; }
        string Name { get; }
        int Quantity { get; set; }
    }

    // Product types
    public class ElectronicItem : IInventoryItem
    {
        public int Id { get; }
        public string Name { get; }
        public int Quantity { get; set; }
        public string Brand { get; }
        public int WarrantyMonths { get; }

        public ElectronicItem(int id, string name, int quantity, string brand, int warrantyMonths)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Quantity = quantity;
            Brand = brand ?? throw new ArgumentNullException(nameof(brand));
            WarrantyMonths = warrantyMonths;
        }

        public override string ToString() =>
            $"ElectronicItem(Id={Id}, Name={Name}, Qty={Quantity}, Brand={Brand}, Warranty={WarrantyMonths}mo)";
    }

    public class GroceryItem : IInventoryItem
    {
        public int Id { get; }
        public string Name { get; }
        public int Quantity { get; set; }
        public DateTime ExpiryDate { get; }

        public GroceryItem(int id, string name, int quantity, DateTime expiryDate)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Quantity = quantity;
            ExpiryDate = expiryDate;
        }

        public override string ToString() =>
            $"GroceryItem(Id={Id}, Name={Name}, Qty={Quantity}, Expires={ExpiryDate:d})";
    }

    // Custom exceptions
    public class DuplicateItemException : Exception
    {
        public DuplicateItemException(string message) : base(message) { }
    }

    public class ItemNotFoundException : Exception
    {
        public ItemNotFoundException(string message) : base(message) { }
    }

    public class InvalidQuantityException : Exception
    {
        public InvalidQuantityException(string message) : base(message) { }
    }

    // Generic repository constrained to IInventoryItem
    public class InventoryRepository<T> where T : IInventoryItem
    {
        private readonly Dictionary<int, T> _items = new();

        public void AddItem(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (_items.ContainsKey(item.Id))
                throw new DuplicateItemException($"An item with ID {item.Id} already exists.");
            _items[item.Id] = item;
        }

        public T GetItemById(int id)
        {
            if (!_items.TryGetValue(id, out var item))
                throw new ItemNotFoundException($"Item with ID {id} not found.");
            return item;
        }

        public void RemoveItem(int id)
        {
            if (!_items.Remove(id))
                throw new ItemNotFoundException($"Item with ID {id} not found and cannot be removed.");
        }

        public List<T> GetAllItems() => new List<T>(_items.Values);

        public void UpdateQuantity(int id, int newQuantity)
        {
            if (newQuantity < 0)
                throw new InvalidQuantityException("Quantity cannot be negative.");

            var item = GetItemById(id); // will throw ItemNotFoundException if missing
            item.Quantity = newQuantity;
        }
    }

    // Warehouse manager containing repositories and helper methods
    public class WareHouseManager
    {
        public InventoryRepository<ElectronicItem> Electronics { get; } = new();
        public InventoryRepository<GroceryItem> Groceries { get; } = new();

        public void SeedData()
        {
            // Electronics
            Electronics.AddItem(new ElectronicItem(1, "Laptop", 5, "Dell", 24));
            Electronics.AddItem(new ElectronicItem(2, "Smartphone", 10, "Samsung", 12));
            Electronics.AddItem(new ElectronicItem(3, "Monitor", 7, "LG", 36));

            // Groceries
            Groceries.AddItem(new GroceryItem(101, "Apples", 50, DateTime.Today.AddDays(7)));
            Groceries.AddItem(new GroceryItem(102, "Milk", 20, DateTime.Today.AddDays(3)));
            Groceries.AddItem(new GroceryItem(103, "Rice (5kg)", 30, DateTime.Today.AddMonths(12)));
        }

        public void PrintAllItems<T>(InventoryRepository<T> repo) where T : IInventoryItem
        {
            var items = repo.GetAllItems();
            if (items.Count == 0)
            {
                Console.WriteLine("(no items)");
                return;
            }

            foreach (var it in items)
                Console.WriteLine(it);
        }

        // Increase stock (wrap with try-catch if caller wants friendly messages)
        public void IncreaseStock<T>(InventoryRepository<T> repo, int id, int quantity) where T : IInventoryItem
        {
            if (quantity <= 0)
                throw new InvalidQuantityException("Increase quantity must be positive.");

            var item = repo.GetItemById(id); // may throw ItemNotFoundException
            repo.UpdateQuantity(id, item.Quantity + quantity);
        }

        public void RemoveItemById<T>(InventoryRepository<T> repo, int id) where T : IInventoryItem
        {
            repo.RemoveItem(id); // may throw ItemNotFoundException
        }
    }

    // Program entry
    internal class Program
    {
        private static void Main()
        {
            var manager = new WareHouseManager();

            Console.WriteLine("Seeding data...");
            manager.SeedData();

            Console.WriteLine("\n--- Grocery Items ---");
            manager.PrintAllItems(manager.Groceries);

            Console.WriteLine("\n--- Electronic Items ---");
            manager.PrintAllItems(manager.Electronics);

            Console.WriteLine("\n\n--- Exception tests ---");

            // 1) Add duplicate item (should throw DuplicateItemException)
            try
            {
                Console.WriteLine("\nAttempting to add a duplicate electronic item (ID = 1)...");
                manager.Electronics.AddItem(new ElectronicItem(1, "Another Laptop", 2, "HP", 12));
            }
            catch (DuplicateItemException ex)
            {
                Console.WriteLine("DuplicateItemException: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error adding duplicate item: " + ex.Message);
            }

            // 2) Remove a non-existent item (should throw ItemNotFoundException)
            try
            {
                Console.WriteLine("\nAttempting to remove a non-existent grocery item (ID = 999)...");
                manager.RemoveItemById(manager.Groceries, 999);
            }
            catch (ItemNotFoundException ex)
            {
                Console.WriteLine("ItemNotFoundException: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error removing item: " + ex.Message);
            }

            // 3) Update with invalid quantity (negative) (should throw InvalidQuantityException)
            try
            {
                Console.WriteLine("\nAttempting to set a negative quantity for grocery item ID = 101...");
                manager.Groceries.UpdateQuantity(101, -5);
            }
            catch (InvalidQuantityException ex)
            {
                Console.WriteLine("InvalidQuantityException: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error updating quantity: " + ex.Message);
            }

            // 4) Try IncreaseStock using manager method but pass invalid quantity
            try
            {
                Console.WriteLine("\nAttempting to increase stock with invalid value (quantity = 0) for electronic ID = 2...");
                manager.IncreaseStock(manager.Electronics, 2, 0);
            }
            catch (InvalidQuantityException ex)
            {
                Console.WriteLine("InvalidQuantityException: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error increasing stock: " + ex.Message);
            }

            // 5) Successful operations demonstration
            try
            {
                Console.WriteLine("\nIncreasing stock of electronic ID = 2 by 5...");
                manager.IncreaseStock(manager.Electronics, 2, 5);
                Console.WriteLine("Updated electronics list:");
                manager.PrintAllItems(manager.Electronics);

                Console.WriteLine("\nRemoving grocery item ID = 102...");
                manager.RemoveItemById(manager.Groceries, 102);
                Console.WriteLine("Updated groceries list:");
                manager.PrintAllItems(manager.Groceries);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error during successful ops demo: " + ex.Message);
            }

            Console.WriteLine("\nDone. Press Enter to exit.");
            Console.ReadLine();
        }
    }
}