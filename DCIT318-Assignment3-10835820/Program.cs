// Program.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

// Namespace for the entire assignment
namespace DCIT318_Assignment3
{
    // --------------------------
    // QUESTION 1 - Finance App
    // --------------------------

    // a) Transaction record
    public record Transaction(int Id, DateTime Date, decimal Amount, string Category);

    // b) Interface for transaction processors
    public interface ITransactionProcessor
    {
        void Process(Transaction transaction);
    }

    // c) Concrete processors
    public class BankTransferProcessor : ITransactionProcessor
    {
        public void Process(Transaction transaction)
        {
            Console.WriteLine($"[BankTransfer] Processing transaction #{transaction.Id}: {transaction.Amount:C} for {transaction.Category}");
        }
    }

    public class MobileMoneyProcessor : ITransactionProcessor
    {
        public void Process(Transaction transaction)
        {
            Console.WriteLine($"[MobileMoney] Processing transaction #{transaction.Id}: {transaction.Amount:C} for {transaction.Category}");
        }
    }

    public class CryptoWalletProcessor : ITransactionProcessor
    {
        public void Process(Transaction transaction)
        {
            Console.WriteLine($"[CryptoWallet] Processing transaction #{transaction.Id}: {transaction.Amount:C} for {transaction.Category}");
        }
    }

    // d) Base Account class
    public class Account
    {
        public string AccountNumber { get; }
        public decimal Balance { get; protected set; }

        public Account(string accountNumber, decimal initialBalance)
        {
            AccountNumber = accountNumber;
            Balance = initialBalance;
        }

        public virtual void ApplyTransaction(Transaction transaction)
        {
            // By default deduct amount
            Balance -= transaction.Amount;
            Console.WriteLine($"Account {AccountNumber}: Applied transaction #{transaction.Id}. New balance: {Balance:C}");
        }
    }
    // e) Sealed SavingsAccount
    public sealed class SavingsAccount : Account
    {
        public SavingsAccount(string accountNumber, decimal initialBalance) : base(accountNumber, initialBalance) { }

        public override void ApplyTransaction(Transaction transaction)
        {
            if (transaction.Amount > Balance)
            {
                Console.WriteLine($"Insufficient funds for transaction #{transaction.Id} ({transaction.Amount:C}). Current balance: {Balance:C}");
            }
            else
            {
                Balance -= transaction.Amount;
                Console.WriteLine($"SavingsAccount {AccountNumber}: Transaction #{transaction.Id} applied. Updated balance: {Balance:C}");
            }
        }
    }
    // f) FinanceApp
    public class FinanceApp
    {
        private readonly List<Transaction> _transactions = new();

        public void Run()
        {
            Console.WriteLine("=== FinanceApp Demo ===");

            // i. Instantiate SavingsAccount
            var account = new SavingsAccount("SA-1001", 1000m);

            // ii. Create 3 transactions
            var t1 = new Transaction(1, DateTime.Now, 150.00m, "Groceries");
            var t2 = new Transaction(2, DateTime.Now, 250.00m, "Utilities");
            var t3 = new Transaction(3, DateTime.Now, 900.00m, "Entertainment"); // large to test insufficient funds

            // iii. Use processors
            ITransactionProcessor p1 = new MobileMoneyProcessor();
            ITransactionProcessor p2 = new BankTransferProcessor();
            ITransactionProcessor p3 = new CryptoWalletProcessor();

            p1.Process(t1);
            p2.Process(t2);
            p3.Process(t3);

            // iv. Apply to account
            account.ApplyTransaction(t1);
            account.ApplyTransaction(t2);
            account.ApplyTransaction(t3); // Should show insufficient funds if amount > balance

            // v. Add to _transactions
            _transactions.AddRange(new[] { t1, t2, t3 });

            Console.WriteLine("FinanceApp finished.\n");
        }
    }

    // QUESTION 2 - Health System

    // Generic repository
    public class Repository<T>
    {
        private readonly List<T> items = new();

        public void Add(T item) => items.Add(item);

        public List<T> GetAll() => new List<T>(items);

        public T? GetById(Func<T, bool> predicate) => items.FirstOrDefault(predicate);

        public bool Remove(Func<T, bool> predicate)
        {
            var item = items.FirstOrDefault(predicate);
            if (item == null) return false;
            items.Remove(item);
            return true;
        }
    }


