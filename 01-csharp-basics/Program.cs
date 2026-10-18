using System;
using System.Collections.Generic;

// Константа для максимальної кількості спроб
const int MaxAttempts = 10;

// Список для збереження результатів (кількість спроб лише у виграшних партіях)
List<int> results = new List<int>();

bool isRunning = true;

// Головний цикл програми
while (isRunning)
{
    Console.WriteLine("\n=== Головне меню: Гра «Вгадай число» ===");
    Console.WriteLine("1. Вихід");
    Console.WriteLine("2. Нова гра");
    Console.WriteLine("3. Показати статистику за сеанс");
    Console.Write("Оберіть пункт меню (1, 2 або 3): ");

    string choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            Console.WriteLine("Вихід з гри. Дякуємо за гру!");
            isRunning = false;
            break;

        case "2":
            int attempts = PlayGame(MaxAttempts);
            // Зберігаємо результат лише якщо гравець виграв (повернуто більше 0)
            if (attempts > 0)
            {
                results.Add(attempts);
            }
            break;

        case "3":
            ShowStatistics(results);
            break;

        default:
            Console.WriteLine("Помилка: Невірний вибір. Введіть 1, 2 або 3.");
            break;
    }
}

// ---------------------------------------------------------
// МЕТОДИ
// ---------------------------------------------------------

// Метод для проведення однієї партії гри
static int PlayGame(int maxAttempts)
{
    Random random = new Random();
    int targetNumber = random.Next(1, 1001); // Генерує випадкове число від 1 до 1000
    int currentAttempt = 1;

    Console.WriteLine($"\n--- Нова гра ---");
    Console.WriteLine($"Я загадав число від 1 до 1000. У вас є {maxAttempts} спроб.");

    while (currentAttempt <= maxAttempts)
    {
        Console.Write($"\nСпроба {currentAttempt} (залишилось {maxAttempts - currentAttempt + 1}). Введіть число: ");
        string input = Console.ReadLine();

        // Валідація: перевіряємо, чи це ціле число та чи воно в діапазоні
        if (!int.TryParse(input, out int userNumber) || userNumber < 1 || userNumber > 1000)
        {
            Console.WriteLine("Некоректне введення! Будь ласка, введіть ціле число від 1 до 1000.");
            continue; // Не зараховуємо цю спробу, даємо ввести ще раз
        }

        // Обробка результату
        if (userNumber == targetNumber)
        {
            // Граничний випадок: коректно відпрацює навіть якщо це 1-ша спроба
            Console.WriteLine($"\nВітаємо! Ви вгадали число {targetNumber} за {currentAttempt} спроб(у/и)!");
            return currentAttempt;
        }
        else if (userNumber > targetNumber)
        {
            Console.WriteLine("Моє число менше.");
        }
        else
        {
            Console.WriteLine("Моє число більше.");
        }

        currentAttempt++;
    }

    Console.WriteLine($"\nВи програли! Спроби вичерпано. Загадане число було: {targetNumber}");
    return -1; // -1 означає, що партію програно
}

// Метод для відображення статистики
static void ShowStatistics(List<int> results)
{
    Console.WriteLine("\n--- Статистика за сеанс ---");

    if (results.Count == 0)
    {
        Console.WriteLine("Немає даних. Ви ще не зіграли жодної успішної партії.");
        return;
    }

    int best = GetBestResult(results);
    double average = GetAverageResult(results);

    Console.WriteLine($"Зіграно успішних партій: {results.Count}");
    Console.WriteLine($"Найкращий результат: {best} спроб(и)");
    Console.WriteLine($"Середня кількість спроб: {average:F1}"); // F1 залишає 1 знак після коми
}

// Метод пошуку мінімальної кількості спроб (найкращий результат)
static int GetBestResult(List<int> results)
{
    int min = results[0];
    foreach (int r in results)
    {
        if (r < min)
        {
            min = r;
        }
    }
    return min;
}

// Метод пошуку середньої кількості спроб
static double GetAverageResult(List<int> results)
{
    double sum = 0;
    foreach (int r in results)
    {
        sum += r;
    }
    return sum / results.Count;
}