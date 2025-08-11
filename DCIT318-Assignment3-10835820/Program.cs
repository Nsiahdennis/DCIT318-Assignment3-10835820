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
    / Generic inventory repository
    public class InventoryRepository<T> where T : IInventoryItem
    {
        private readonly Dictionary<int, T> _items = new();

        public void AddItem(T item)
        {
            if (_items.ContainsKey(item.Id))
                throw new DuplicateItemException($"Item with ID {item.Id} already exists.");

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
                throw new ItemNotFoundException($"Item with ID {id} not found.");
        }

        public List<T> GetAllItems() => _items.Values.ToList();

        public void UpdateQuantity(int id, int newQuantity)
        {
            if (newQuantity < 0)
                throw new InvalidQuantityException("Quantity cannot be negative.");

            var item = GetItemById(id); // will throw if missing
            item.Quantity = newQuantity;
        }
    }
    // WareHouseManager
    public class WareHouseManager
    {
        private readonly InventoryRepository<ElectronicItem> _electronics = new();
        private readonly InventoryRepository<GroceryItem> _groceries = new();

        public void SeedData()
        {
            try
            {
                _electronics.AddItem(new ElectronicItem(1, "Laptop", 5, "TechBrand", 24));
                _electronics.AddItem(new ElectronicItem(2, "Headphones", 10, "AudioCorp", 12));
                _electronics.AddItem(new ElectronicItem(3, "Monitor", 4, "ViewMax", 36));

                _groceries.AddItem(new GroceryItem(101, "Rice - 25kg", 20, DateTime.Now.AddMonths(12)));
                _groceries.AddItem(new GroceryItem(102, "Milk - 1L", 30, DateTime.Now.AddDays(7)));
                _groceries.AddItem(new GroceryItem(103, "Sugar - 2kg", 15, DateTime.Now.AddMonths(6)));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SeedData error: {ex.Message}");
            }
        }

        public void PrintAllItems<T>(InventoryRepository<T> repo) where T : IInventoryItem
        {
            var items = repo.GetAllItems();
            Console.WriteLine($"--- Listing items of type {typeof(T).Name} ---");
            foreach (var it in items)
            {
                Console.WriteLine(it);
            }
            Console.WriteLine();
        }
        public void IncreaseStock<T>(InventoryRepository<T> repo, int id, int quantity) where T : IInventoryItem
        {
            try
            {
                var item = repo.GetItemById(id); // may throw
                if (quantity < 0) throw new InvalidQuantityException("Quantity to add cannot be negative.");
                repo.UpdateQuantity(id, item.Quantity + quantity);
                Console.WriteLine($"Increased stock for item {id} by {quantity}. New qty: {repo.GetItemById(id).Quantity}");
            }
            catch (Exception ex) when (ex is ItemNotFoundException || ex is InvalidQuantityException)
            {
                Console.WriteLine($"Error increasing stock: {ex.Message}");
            }
        }

        public void RemoveItemById<T>(InventoryRepository<T> repo, int id) where T : IInventoryItem
        {
            try
            {
                repo.RemoveItem(id);
                Console.WriteLine($"Removed item with ID {id} from repository.");
            }
            catch (Exception ex) when (ex is ItemNotFoundException)
            {
                Console.WriteLine($"Error removing item: {ex.Message}");
            }
        }

        public void RunDemo()
        {
            Console.WriteLine("=== WareHouseManager Demo ===");
            SeedData();

            // Print groceries and electronics
            PrintAllItems(_groceries);
            PrintAllItems(_electronics);

            // Try to add a duplicate (electronics ID 1 already exists)
            try
            {
                Console.WriteLine("Attempting to add duplicate electronic item (ID 1)...");
                _electronics.AddItem(new ElectronicItem(1, "Tablet", 3, "TabCo", 12));
            }
            catch (DuplicateItemException ex)
            {
                Console.WriteLine($"Caught exception: {ex.Message}");
            }
            // Try to remove a non-existent item
            RemoveItemById(_groceries, 999); // should show error

            // Try to update with invalid quantity
            try
            {
                Console.WriteLine("Attempting to set invalid quantity (-5) for electronic item ID 2...");
                _electronics.UpdateQuantity(2, -5);
            }
            catch (InvalidQuantityException ex)
            {
                Console.WriteLine($"Caught exception: {ex.Message}");
            }

            Console.WriteLine("WareHouseManager Demo finished.\n");
        }
    }
    // QUESTION 4 - Student Result Processor
    public class Student
    {
        public int Id { get; }
        public string FullName { get; }
        public int Score { get; }

        public Student(int id, string fullName, int score)
        {
            Id = id;
            FullName = fullName;
            Score = score;
        }

        public string GetGrade()
        {
            if (Score >= 80 && Score <= 100) return "A";
            if (Score >= 70 && Score <= 79) return "B";
            if (Score >= 60 && Score <= 69) return "C";
            if (Score >= 50 && Score <= 59) return "D";
            return "F";
        }

        public override string ToString() => $"{FullName} (ID: {Id}): Score = {Score}, Grade = {GetGrade()}";
    }

    // Custom exceptions
    public class InvalidScoreFormatException : Exception
    {
        public InvalidScoreFormatException(string message) : base(message) { }
    }

    public class MissingFieldException : Exception
    {
        public MissingFieldException(string message) : base(message) { }
    }

    public class StudentResultProcessor
    {
        public List<Student> ReadStudentsFromFile(string inputFilePath)
        {
            var students = new List<Student>();

            using var reader = new StreamReader(inputFilePath);
            string? line;
            int lineNumber = 0;
            while ((line = reader.ReadLine()) != null)
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line)) continue; // skip empty lines

                var parts = line.Split(',', StringSplitOptions.TrimEntries);
                if (parts.Length < 3)
                {
                    throw new MissingFieldException($"Line {lineNumber}: Missing fields. Expected 3 values but found {parts.Length}.");
                }

                // parse id
                if (!int.TryParse(parts[0], out int id))
                {
                    throw new InvalidScoreFormatException($"Line {lineNumber}: Invalid ID format.");
                }
                // name
                var name = parts[1];
                if (string.IsNullOrWhiteSpace(name))
                    throw new MissingFieldException($"Line {lineNumber}: Missing student name.");

                // score parse
                if (!int.TryParse(parts[2], out int score))
                {
                    throw new InvalidScoreFormatException($"Line {lineNumber}: Score '{parts[2]}' is not a valid integer.");
                }

                students.Add(new Student(id, name, score));
            }

            return students;
        }

        public void WriteReportToFile(List<Student> students, string outputFilePath)
        {
            using var writer = new StreamWriter(outputFilePath);
            foreach (var s in students)
            {
                writer.WriteLine($"{s.FullName} (ID: {s.Id}): Score = {s.Score}, Grade = {s.GetGrade()}");
            }
        }
    }







