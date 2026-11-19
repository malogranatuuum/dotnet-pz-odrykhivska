using System;
using System.Collections.Generic;
using System.Text;

namespace GenericsDelegatesEventsApp
{
    // =========================================================
    // 1. ІНТЕРФЕЙС ТА МОДЕЛЬ ПРЕДМЕТНОЇ ОБЛАСТІ
    // =========================================================
    public interface IHasAmount
    {
        decimal Amount { get; }
    }

    public class Expense : IHasAmount
    {
        public string Category { get; }
        public string Shop { get; }
        public decimal Amount { get; }
        public DateTime Date { get; }

        public Expense(string category, string shop, decimal amount, DateTime date)
        {
            Category = category;
            Shop = shop;
            Amount = amount;
            Date = date;
        }

        public override string ToString()
        {
            return $"{Date:yyyy-MM-dd} | Категорія: {Category,-12} | Магазин: {Shop,-12} | Сума: {Amount} грн";
        }
    }

    // =========================================================
    // 2. КЛАС ДАНИХ ПОДІЇ (EventArgs)
    // =========================================================
    public class BudgetExceededEventArgs<T> : EventArgs
    {
        public T Item { get; }
        public decimal TotalAmount { get; }
        public decimal Budget { get; }

        public BudgetExceededEventArgs(T item, decimal totalAmount, decimal budget)
        {
            Item = item;
            TotalAmount = totalAmount;
            Budget = budget;
        }
    }

    // =========================================================
    // 3. УЗАГАЛЬНЕНИЙ КЛАС ОБЛІКУ ВИТРАТ
    // =========================================================
    public class ExpenseLog<T> where T : IHasAmount
    {
        private readonly List<T> _items = new();
        private Func<T, string>? _grouping; // null — групування не задано

        public decimal Budget { get; }
        public int Count => _items.Count;

        public event EventHandler<BudgetExceededEventArgs<T>>? BudgetExceeded;

        public ExpenseLog(decimal budget)
        {
            if (budget < 0)
            {
                throw new ArgumentException("Бюджет не може бути від'ємним.", nameof(budget));
            }
            Budget = budget;
        }

        public void SetGrouping(Func<T, string>? grouping)
        {
            _grouping = grouping;
        }

        public void Add(T item)
        {
            _items.Add(item);

            decimal currentTotal = GetTotal();
            if (currentTotal > Budget)
            {
                OnBudgetExceeded(new BudgetExceededEventArgs<T>(item, currentTotal, Budget));
            }
        }

        public decimal GetTotal()
        {
            decimal total = 0m;
            foreach (T item in _items)
            {
                total += item.Amount;
            }
            return total;
        }

        public Dictionary<string, decimal> GetReport()
        {
            var report = new Dictionary<string, decimal>();

            foreach (T item in _items)
            {
                // Якщо правило групування не задано (null), використовується "Усього"
                string groupName = _grouping?.Invoke(item) ?? "Усього";

                if (report.ContainsKey(groupName))
                {
                    report[groupName] += item.Amount;
                }
                else
                {
                    report[groupName] = item.Amount;
                }
            }

            return report;
        }

        public void ForEach(Action<T> action)
        {
            foreach (T item in _items)
            {
                action(item);
            }
        }

        protected virtual void OnBudgetExceeded(BudgetExceededEventArgs<T> e)
        {
            BudgetExceeded?.Invoke(this, e);
        }
    }

    // =========================================================
    // 4. ГОЛОВНА ПРОГРАМА ТА КОНСОЛЬНЕ МЕНЮ
    // =========================================================
    internal class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            // Створюємо журнал витрат із бюджетом 2000 грн і початковими даними на 1500 грн
            var expenseLog = new ExpenseLog<Expense>(2000m);
            expenseLog.Add(new Expense("Їжа", "Сільпо", 500m, new DateTime(2026, 10, 15)));
            expenseLog.Add(new Expense("Їжа", "АТБ", 400m, new DateTime(2026, 11, 2)));
            expenseLog.Add(new Expense("Транспорт", "WOG", 600m, new DateTime(2026, 11, 10)));

            // Список для збереження покупок, зроблених понад бюджет
            var overBudgetExpenses = new List<Expense>();

            // Підписник 1 на подію — іменований метод
            expenseLog.BudgetExceeded += WarnBudgetExceeded;

            // Підписник 2 на подію — лямбда-вираз із замиканням зовнішнього списку
            expenseLog.BudgetExceeded += (sender, e) => overBudgetExpenses.Add(e.Item);

            string currentGroupingLabel = "без групування";
            bool isRunning = true;

            while (isRunning)
            {
                Console.WriteLine($"\n=== ОБЛІК ВИТРАТ (Бюджет: {expenseLog.Budget} грн | Витрачено: {expenseLog.GetTotal()} грн) ===");
                Console.WriteLine("1. Вихід");
                Console.WriteLine("2. Додати витрату");
                Console.WriteLine("3. Показати всі витрати");
                Console.WriteLine("4. Показати звіт за поточним групуванням");
                Console.WriteLine("5. Обрати групування");
                Console.WriteLine("6. Показати покупки понад бюджет");

                int choice = ReadInt("Ваш вибір: ", 1, 6);

                switch (choice)
                {
                    case 1:
                        isRunning = false;
                        Console.WriteLine("Роботу програми завершено.");
                        break;
                    case 2:
                        HandleAddExpense(expenseLog);
                        break;
                    case 3:
                        HandleShowAll(expenseLog);
                        break;
                    case 4:
                        HandleShowReport(expenseLog, currentGroupingLabel);
                        break;
                    case 5:
                        currentGroupingLabel = HandleChooseGrouping(expenseLog);
                        break;
                    case 6:
                        HandleShowOverBudget(overBudgetExpenses);
                        break;
                }
            }
        }

        // Іменований обробник події перевищення бюджету
        static void WarnBudgetExceeded(object? sender, BudgetExceededEventArgs<Expense> e)
        {
            Console.WriteLine($"Увага! Витрачено {e.TotalAmount} грн із бюджету {e.Budget} грн (покупка: {e.Item.Category}, {e.Item.Shop} — {e.Item.Amount} грн).");
        }

        static void HandleAddExpense(ExpenseLog<Expense> log)
        {
            Console.WriteLine("\n--- Нова витрата ---");
            string category = ReadNonEmpty("Категорія: ");
            string shop = ReadNonEmpty("Магазин: ");
            decimal amount = ReadPositiveDecimal("Сума (грн): ");

            var expense = new Expense(category, shop, amount, DateTime.Now);
            log.Add(expense);
            Console.WriteLine("Витрату успішно додано.");
        }

        static void HandleShowAll(ExpenseLog<Expense> log)
        {
            Console.WriteLine("\n--- Усі витрати ---");
            if (log.Count == 0)
            {
                Console.WriteLine("Список витрат порожній.");
                return;
            }

            // Використання методу ForEach із лямбда-виразом Action<Expense>
            log.ForEach(e => Console.WriteLine($"- {e}"));
            Console.WriteLine($"Загальна сума: {log.GetTotal()} грн");
        }

        static void HandleShowReport(ExpenseLog<Expense> log, string groupingLabel)
        {
            Console.WriteLine($"\n--- Звіт ({groupingLabel}) ---");
            if (log.Count == 0)
            {
                Console.WriteLine("Немає даних для побудови звіту.");
                return;
            }

            Dictionary<string, decimal> report = log.GetReport();
            foreach (var entry in report)
            {
                Console.WriteLine($"{entry.Key}: {entry.Value} грн");
            }
        }

        static string HandleChooseGrouping(ExpenseLog<Expense> log)
        {
            Console.WriteLine("\n--- Вибір групування ---");
            Console.WriteLine("0. Без групування (усього)");
            Console.WriteLine("1. За категорією");
            Console.WriteLine("2. За магазином");
            Console.WriteLine("3. За місяцем");

            int mode = ReadInt("Оберіть режим (0–3): ", 0, 3);

            // Передача різних лямбда-виразів Func<Expense, string> або null для роботи за замовчуванням
            switch (mode)
            {
                case 1:
                    log.SetGrouping(e => e.Category);
                    Console.WriteLine("Встановлено групування: за категорією.");
                    return "за категорією";
                case 2:
                    log.SetGrouping(e => e.Shop);
                    Console.WriteLine("Встановлено групування: за магазином.");
                    return "за магазином";
                case 3:
                    log.SetGrouping(e => e.Date.ToString("yyyy-MM"));
                    Console.WriteLine("Встановлено групування: за місяцем.");
                    return "за місяцем";
                default:
                    log.SetGrouping(null);
                    Console.WriteLine("Групування скасовано (вивід загальної суми).");
                    return "без групування";
            }
        }

        static void HandleShowOverBudget(List<Expense> overBudgetExpenses)
        {
            Console.WriteLine("\n--- Покупки понад бюджет ---");
            if (overBudgetExpenses.Count == 0)
            {
                Console.WriteLine("Витрат понад встановлений бюджет немає.");
                return;
            }

            foreach (var item in overBudgetExpenses)
            {
                Console.WriteLine($"- {item}");
            }
        }

        // =========================================================
        // ДОПОМІЖНІ МЕТОДИ ВАЛІДАЦІЇ ВВЕДЕННЯ
        // =========================================================
        static string ReadNonEmpty(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input))
                {
                    return input.Trim();
                }
                Console.WriteLine("Помилка: значення не може бути порожнім.");
            }
        }

        static int ReadInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int value) && value >= min && value <= max)
                {
                    return value;
                }
                Console.WriteLine($"Помилка: введіть ціле число від {min} до {max}.");
            }
        }

        static decimal ReadPositiveDecimal(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (decimal.TryParse(Console.ReadLine(), out decimal value) && value > 0)
                {
                    return value;
                }
                Console.WriteLine("Помилка: введіть додатне число.");
            }
        }
    }
}