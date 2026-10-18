using System;
using System.Collections.Generic;

namespace OOPExceptionsApp
{
    // =========================================================
    // 1. ВЛАСНИЙ КЛАС ВИНЯТКУ
    // =========================================================
    public class InsufficientFundsException : Exception
    {
        public InsufficientFundsException(string message) : base(message) { }
    }

    // =========================================================
    // 2. БАЗОВИЙ КЛАС
    // =========================================================
    public abstract class BankAccount : IComparable<BankAccount>
    {
        public string Owner { get; set; }
        public double Balance { get; protected set; }

        protected BankAccount(string owner, double initialBalance)
        {
            Owner = string.IsNullOrWhiteSpace(owner) ? "Невідомий" : owner;
            Balance = initialBalance;
        }

        public virtual void Deposit(double amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Сума поповнення має бути більшою за нуль.");
            Balance += amount;
            Console.WriteLine($"Рахунок поповнено. Поточний баланс: {Balance}");
        }

        public virtual void Withdraw(double amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Сума зняття має бути більшою за нуль.");

            // Базова реалізація: не можна зняти більше, ніж є на балансі
            if (Balance - amount < 0)
                throw new InsufficientFundsException($"Недостатньо коштів. Поточний баланс: {Balance}");

            Balance -= amount;
            Console.WriteLine($"Успішно знято {amount}. Залишок: {Balance}");
        }

        // Порівняння за спаданням балансу (від найбільшого до найменшого)
        public int CompareTo(BankAccount? other)
        {
            if (other == null) return -1;
            return other.Balance.CompareTo(this.Balance);
        }

        public override string ToString()
        {
            return $"Власник: {Owner} | Баланс: {Balance}";
        }
    }

    // =========================================================
    // 3. ОЩАДНИЙ РАХУНОК (SavingsAccount)
    // =========================================================
    public class SavingsAccount : BankAccount
    {
        public double MinimumBalance { get; set; }

        public SavingsAccount(string owner, double initialBalance, double minimumBalance)
            : base(owner, initialBalance)
        {
            MinimumBalance = minimumBalance;
        }

        public override void Withdraw(double amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Сума зняття має бути більшою за нуль.");

            // Граничний випадок: якщо Balance - amount == MinimumBalance, зняття успішне (використовуємо < замість <=)
            if (Balance - amount < MinimumBalance)
                throw new InsufficientFundsException($"Зняття відхилено! Залишок не може бути меншим за мінімальний баланс ({MinimumBalance}).");

            Balance -= amount;
            Console.WriteLine($"Успішно знято {amount}. Залишок: {Balance}");
        }

        public void ApplyInterest(double ratePercent)
        {
            double interest = Balance * (ratePercent / 100);
            Balance += interest;
            Console.WriteLine($"Нараховано відсотки: {interest}. Новий баланс: {Balance}");
        }

        public override string ToString()
        {
            return $"[Ощадний]  {base.ToString()} | Мін. залишок: {MinimumBalance}";
        }
    }

    // =========================================================
    // 4. ПОТОЧНИЙ РАХУНОК (CheckingAccount)
    // =========================================================
    public class CheckingAccount : BankAccount
    {
        public double OverdraftLimit { get; set; }

        public CheckingAccount(string owner, double initialBalance, double overdraftLimit)
            : base(owner, initialBalance)
        {
            OverdraftLimit = overdraftLimit;
        }

        public override void Withdraw(double amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Сума зняття має бути більшою за нуль.");

            // Граничний випадок: якщо Balance - amount == -OverdraftLimit, зняття успішне
            if (Balance - amount < -OverdraftLimit)
                throw new InsufficientFundsException($"Зняття відхилено! Перевищено ліміт овердрафту ({OverdraftLimit}).");

            Balance -= amount;
            Console.WriteLine($"Успішно знято {amount}. Залишок: {Balance}");
        }

        public override string ToString()
        {
            return $"[Поточний] {base.ToString()} | Овердрафт: {OverdraftLimit}";
        }
    }

    // =========================================================
    // 5. ГОЛОВНА ПРОГРАМА
    // =========================================================
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            List<BankAccount> accounts = new List<BankAccount>();
            bool isRunning = true;

            while (isRunning)
            {
                Console.WriteLine("\n=== СИСТЕМА УПРАВЛІННЯ РАХУНКАМИ ===");
                Console.WriteLine("1. Вихід");
                Console.WriteLine("2. Додати рахунок");
                Console.WriteLine("3. Поповнити рахунок");
                Console.WriteLine("4. Зняти кошти з рахунка");
                Console.WriteLine("5. Показати список усіх рахунків (за спаданням балансу)");
                Console.WriteLine("6. Нарахувати відсотки (тільки для ощадних)");
                Console.Write("Оберіть пункт: ");

                string choice = Console.ReadLine() ?? "";

                switch (choice)
                {
                    case "1":
                        isRunning = false;
                        break;
                    case "2":
                        AddAccount(accounts);
                        break;
                    case "3":
                        DepositToAccount(accounts);
                        break;
                    case "4":
                        WithdrawFromAccount(accounts);
                        break;
                    case "5":
                        ShowAccounts(accounts);
                        break;
                    case "6":
                        ApplyInterestToSavings(accounts);
                        break;
                    default:
                        Console.WriteLine("Невірний вибір. Спробуйте ще раз.");
                        break;
                }
            }
        }

        static void AddAccount(List<BankAccount> accounts)
        {
            Console.WriteLine("Оберіть тип рахунку: 1 - Ощадний (Savings), 2 - Поточний (Checking)");
            string type = Console.ReadLine() ?? "";

            Console.Write("Введіть ПІБ власника: ");
            string owner = Console.ReadLine() ?? "";

            Console.Write("Введіть початковий баланс: ");
            if (!double.TryParse(Console.ReadLine(), out double balance))
            {
                Console.WriteLine("Помилка вводу балансу.");
                return;
            }

            if (type == "1")
            {
                Console.Write("Введіть мінімальний залишок (MinimumBalance): ");
                if (double.TryParse(Console.ReadLine(), out double minBal))
                {
                    accounts.Add(new SavingsAccount(owner, balance, minBal));
                    Console.WriteLine("Ощадний рахунок успішно додано.");
                }
            }
            else if (type == "2")
            {
                Console.Write("Введіть ліміт овердрафту (OverdraftLimit): ");
                if (double.TryParse(Console.ReadLine(), out double overdraft))
                {
                    accounts.Add(new CheckingAccount(owner, balance, overdraft));
                    Console.WriteLine("Поточний рахунок успішно додано.");
                }
            }
            else
            {
                Console.WriteLine("Невідомий тип рахунку.");
            }
        }

        static void DepositToAccount(List<BankAccount> accounts)
        {
            BankAccount? acc = SelectAccount(accounts);
            if (acc == null) return;

            Console.Write("Введіть суму поповнення: ");
            if (double.TryParse(Console.ReadLine(), out double amount))
            {
                try
                {
                    acc.Deposit(amount);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Помилка: {ex.Message}");
                }
            }
        }

        static void WithdrawFromAccount(List<BankAccount> accounts)
        {
            BankAccount? acc = SelectAccount(accounts);
            if (acc == null) return;

            Console.Write("Введіть суму для зняття: ");
            if (double.TryParse(Console.ReadLine(), out double amount))
            {
                try
                {
                    acc.Withdraw(amount);
                }
                catch (InsufficientFundsException ex)
                {
                    Console.WriteLine($"[ПОМИЛКА ЗНЯТТЯ]: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Помилка: {ex.Message}");
                }
            }
        }

        static void ShowAccounts(List<BankAccount> accounts)
        {
            if (accounts.Count == 0)
            {
                Console.WriteLine("Список рахунків порожній.");
                return;
            }

            accounts.Sort(); // Сортування за спаданням завдяки реалізації IComparable

            Console.WriteLine("\n--- Список рахунків ---");
            for (int i = 0; i < accounts.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {accounts[i]}");
            }
        }

        static void ApplyInterestToSavings(List<BankAccount> accounts)
        {
            Console.Write("Введіть відсоткову ставку (наприклад, 5 для 5%): ");
            if (double.TryParse(Console.ReadLine(), out double rate))
            {
                foreach (var acc in accounts)
                {
                    if (acc is SavingsAccount savings)
                    {
                        savings.ApplyInterest(rate);
                    }
                }
            }
        }

        static BankAccount? SelectAccount(List<BankAccount> accounts)
        {
            if (accounts.Count == 0)
            {
                Console.WriteLine("Список рахунків порожній. Спочатку додайте рахунок.");
                return null;
            }

            Console.WriteLine("Доступні рахунки:");
            for (int i = 0; i < accounts.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {accounts[i].Owner} ({accounts[i].GetType().Name})");
            }

            Console.Write("Оберіть номер рахунку: ");
            if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= accounts.Count)
            {
                return accounts[index - 1];
            }

            Console.WriteLine("Невірний номер рахунку.");
            return null;
        }
    }
}