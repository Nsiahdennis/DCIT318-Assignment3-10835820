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

    // Patient
    public class Patient
    {
        public int Id { get; }
        public string Name { get; }
        public int Age { get; }
        public string Gender { get; }

        public Patient(int id, string name, int age, string gender)
        {
            Id = id;
            Name = name;
            Age = age;
            Gender = gender;
        }

        public override string ToString() => $"Patient {Name} (ID: {Id}, Age: {Age}, Gender: {Gender})";
    }
    // Prescription
    public class Prescription
    {
        public int Id { get; }
        public int PatientId { get; }
        public string MedicationName { get; }
        public DateTime DateIssued { get; }

        public Prescription(int id, int patientId, string medicationName, DateTime dateIssued)
        {
            Id = id;
            PatientId = patientId;
            MedicationName = medicationName;
            DateIssued = dateIssued;
        }

        public override string ToString() => $"Prescription #{Id}: {MedicationName} (Issued: {DateIssued:yyyy-MM-dd})";
    }

    public class HealthSystemApp
    {
        private readonly Repository<Patient> _patientRepo = new();
        private readonly Repository<Prescription> _prescriptionRepo = new();
        private readonly Dictionary<int, List<Prescription>> _prescriptionMap = new();

        public void SeedData()
        {
            // Add 2-3 patients
            _patientRepo.Add(new Patient(1, "Alice Johnson", 30, "Female"));
            _patientRepo.Add(new Patient(2, "Bob Mensah", 45, "Male"));
            _patientRepo.Add(new Patient(3, "Clara Appiah", 22, "Female"));

            // Add 4-5 prescriptions (valid PatientIds)
            _prescriptionRepo.Add(new Prescription(1, 1, "Amoxicillin", DateTime.Now.AddDays(-10)));
            _prescriptionRepo.Add(new Prescription(2, 1, "Ibuprofen", DateTime.Now.AddDays(-5)));
            _prescriptionRepo.Add(new Prescription(3, 2, "Atorvastatin", DateTime.Now.AddDays(-20)));
            _prescriptionRepo.Add(new Prescription(4, 3, "Metformin", DateTime.Now.AddDays(-2)));
        }

        public void BuildPrescriptionMap()
        {
            _prescriptionMap.Clear();
            var allPrescriptions = _prescriptionRepo.GetAll();
            foreach (var p in allPrescriptions)
            {
                if (!_prescriptionMap.ContainsKey(p.PatientId))
                    _prescriptionMap[p.PatientId] = new List<Prescription>();
                _prescriptionMap[p.PatientId].Add(p);
            }
        }
        public void PrintAllPatients()
        {
            Console.WriteLine("=== Patients ===");
            foreach (var patient in _patientRepo.GetAll())
            {
                Console.WriteLine(patient);
            }
            Console.WriteLine();
        }

        public void PrintPrescriptionsForPatient(int patientId)
        {
            Console.WriteLine($"=== Prescriptions for Patient ID {patientId} ===");
            if (_prescriptionMap.TryGetValue(patientId, out var list))
            {
                foreach (var p in list)
                    Console.WriteLine(p);
            }
            else
            {
                Console.WriteLine("No prescriptions found for this patient.");
            }
            Console.WriteLine();
        }
        public void Run()
        {
            Console.WriteLine("=== HealthSystemApp Demo ===");
            SeedData();
            BuildPrescriptionMap();
            PrintAllPatients();

            // pick one patient id to display
            PrintPrescriptionsForPatient(1);
            Console.WriteLine("HealthSystemApp finished.\n");
        }
    }
    // QUESTION 3 - Warehouse Manager
    // Marker interface for inventory items
    public interface IInventoryItem
    {
        int Id { get; }
        string Name { get; }
        int Quantity { get; set; }
    }
    // ElectronicItem
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
            Name = name;
            Quantity = quantity;
            Brand = brand;
            WarrantyMonths = warrantyMonths;
        }

        public override string ToString() => $"ElectronicItem #{Id}: {Name} ({Brand}) Quantity: {Quantity}, Warranty: {WarrantyMonths} months";
    }
    // GroceryItem
    public class GroceryItem : IInventoryItem
    {
        public int Id { get; }
        public string Name { get; }
        public int Quantity { get; set; }
        public DateTime ExpiryDate { get; }

        public GroceryItem(int id, string name, int quantity, DateTime expiryDate)
        {
            Id = id;
            Name = name;
            Quantity = quantity;
            ExpiryDate = expiryDate;
        }

        public override string ToString() => $"GroceryItem #{Id}: {Name} Quantity: {Quantity}, Expiry: {ExpiryDate:yyyy-MM-dd}";
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






