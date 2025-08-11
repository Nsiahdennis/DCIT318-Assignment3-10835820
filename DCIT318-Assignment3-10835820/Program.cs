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
