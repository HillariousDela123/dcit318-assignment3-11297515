namespace A_Finance_Management_System
{
    // Core model using a record (immutable by default)
    public record Transaction(int Id, DateTime Date, decimal Amount, string Category);

    // Interface for transaction processing behavior
    public interface ITransactionProcessor
    {
        void Process(Transaction transaction);
    }

    // Concrete processors
    public class BankTransferProcessor : ITransactionProcessor
    {
        public void Process(Transaction transaction)
        {
            Console.WriteLine($"[BankTransfer] Processing {transaction.Category} of {transaction.Amount:C} (ID: {transaction.Id})");
        }
    }

    public class MobileMoneyProcessor : ITransactionProcessor
    {
        public void Process(Transaction transaction)
        {
            Console.WriteLine($"[MobileMoney] Processed {transaction.Category}: amount {transaction.Amount:C} (ID: {transaction.Id})");
        }
    }

    public class CryptoWalletProcessor : ITransactionProcessor
    {
        public void Process(Transaction transaction)
        {
            Console.WriteLine($"[CryptoWallet] Executing {transaction.Category} payment of {transaction.Amount:C} (ID: {transaction.Id})");
        }
    }

    // Base Account class
    public class Account
    {
        public string AccountNumber { get; }
        public decimal Balance { get; protected set; }

        public Account(string accountNumber, decimal initialBalance)
        {
            AccountNumber = accountNumber ?? throw new ArgumentNullException(nameof(accountNumber));
            Balance = initialBalance;
        }

        public virtual void ApplyTransaction(Transaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            Balance -= transaction.Amount;
            Console.WriteLine($"[Account] Applied transaction {transaction.Id}. New balance: {Balance:C}");
        }
    }

    // Specialized account
    public sealed class SavingsAccount : Account
    {
        public SavingsAccount(string accountNumber, decimal initialBalance)
            : base(accountNumber, initialBalance) { }

        public override void ApplyTransaction(Transaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));

            if (transaction.Amount > Balance)
            {
                Console.WriteLine($"[SavingsAccount] Insufficient funds for transaction {transaction.Id} ({transaction.Amount:C}). Current balance: {Balance:C}");
                return;
            }

            Balance -= transaction.Amount;
            Console.WriteLine($"[SavingsAccount] Transaction {transaction.Id} applied. New balance: {Balance:C}");
        }
    }

    // Main finance app
    public class FinanceApp
    {
        private readonly List<Transaction> _transactions = new();

        public void Run()
        {
            var account = new SavingsAccount("SA-001-2025", 1000m);
            Console.WriteLine($"Created SavingsAccount {account.AccountNumber} with initial balance {account.Balance:C}\n");

            var t1 = new Transaction(1, DateTime.Now, 150m, "Groceries");
            var t2 = new Transaction(2, DateTime.Now, 200m, "Utilities");
            var t3 = new Transaction(3, DateTime.Now, 700m, "Entertainment");

            ITransactionProcessor proc1 = new MobileMoneyProcessor();
            ITransactionProcessor proc2 = new BankTransferProcessor();
            ITransactionProcessor proc3 = new CryptoWalletProcessor();

            proc1.Process(t1);
            proc2.Process(t2);
            proc3.Process(t3);
            Console.WriteLine();

            account.ApplyTransaction(t1);
            account.ApplyTransaction(t2);
            account.ApplyTransaction(t3);
            Console.WriteLine();

            _transactions.Add(t1);
            _transactions.Add(t2);
            _transactions.Add(t3);

            Console.WriteLine("All transactions added to the system:");
            foreach (var tx in _transactions)
            {
                Console.WriteLine($" - ID {tx.Id}: {tx.Category} {tx.Amount:C} on {tx.Date}");
            }

            Console.WriteLine($"\nFinal account balance for {account.AccountNumber}: {account.Balance:C}");
        }
    }

    // Program entry point
    internal class Program
    {
        static void Main(string[] args)
        {
            var app = new FinanceApp();
            app.Run();
        }
    }
}