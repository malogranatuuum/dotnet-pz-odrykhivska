using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace LinqAsyncParcelsApp
{
    // =========================================================
    // 1. МОДЕЛІ ДАНИХ (RECORD)
    // =========================================================
    public record Parcel(
        string TrackingNumber,
        string Carrier,
        string City,
        string Status,
        double WeightKg,
        int DaysInTransit);

    public record Order(
        string OrderId,
        string Customer,
        string TrackingNumber);

    // =========================================================
    // 2. ГОЛОВНА ПРОГРАМА ТА КОНСОЛЬНЕ МЕНЮ
    // =========================================================
    internal class Program
    {
        // Завантажені дані про посилки з усіх служб доставки
        static List<Parcel> _parcels = new();

        // Налаштування JSON: форматування з відступами та кирилиця без екранування
        static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };

        // Друга колекція — замовлення магазину (11 записів; 2 номери відсутні у службах доставки)
        static readonly List<Order> _orders = new()
        {
            new("ORD-101", "Олена Коваленко", "SP-1001"),
            new("ORD-102", "Олена Коваленко", "SP-9999"),   // неіснуючий номер (граничний випадок)
            new("ORD-103", "Іван Петренко", "ED-2001"),
            new("ORD-104", "Марія Бойко", "P24-3002"),
            new("ORD-105", "Олексій Ткаченко", "SP-1004"),
            new("ORD-106", "Наталія Мельник", "ED-2005"),
            new("ORD-107", "Дмитро Шевченко", "P24-3006"),
            new("ORD-108", "Іван Петренко", "SP-1007"),
            new("ORD-109", "Софія Бондаренко", "ED-0000"),  // неіснуючий номер (граничний випадок)
            new("ORD-110", "Марія Бойко", "ED-2008"),
            new("ORD-111", "Андрій Савченко", "P24-3001")
        };

        static async Task Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            bool isRunning = true;

            while (isRunning)
            {
                Console.WriteLine("\n=== ВІДСТЕЖЕННЯ ПОСИЛОК ІНТЕРНЕТ-МАГАЗИНУ ===");
                Console.WriteLine("1. Вихід");
                Console.WriteLine("2. Завантажити дані служб доставки (одночасно)");
                Console.WriteLine("3. Завантажити дані служб доставки (послідовно)");
                Console.WriteLine("4. Статистика за службами доставки");
                Console.WriteLine("5. Середній термін доставки отриманих посилок");
                Console.WriteLine("6. Затримані посилки (в дорозі довше N днів)");
                Console.WriteLine("7. Замовлення клієнта");
                Console.WriteLine("8. Зберегти звіт про замовлення у JSON-файл");

                int choice = ReadInt("Ваш вибір: ", 1, 8);

                isRunning = choice switch
                {
                    1 => false,
                    2 => await HandleLoadAsync(parallel: true),
                    3 => await HandleLoadAsync(parallel: false),
                    4 => HandleCarrierStats(),
                    5 => HandleAverageDeliveryTime(),
                    6 => HandleDelayedParcels(),
                    7 => HandleCustomerOrders(),
                    8 => await HandleSaveReportAsync(),
                    _ => true
                };
            }

            Console.WriteLine("Роботу програми завершено.");
        }

        // =========================================================
        // 3. АСИНХРОННІ ДЖЕРЕЛА ДАНИХ (24 ПОСИЛКИ З 3 СЛУЖБ)
        // =========================================================
        static async Task<List<Parcel>> LoadShvydkoPoshtaAsync()
        {
            await Task.Delay(2000); // Імітація затримки сервера 2,0 с
            return new List<Parcel>
            {
                new("SP-1001", "ШвидкоПошта", "Київ", "Отримано", 2.4, 2),
                new("SP-1002", "ШвидкоПошта", "Львів", "В дорозі", 5.1, 4),
                new("SP-1003", "ШвидкоПошта", "Вінниця", "У відділенні", 1.2, 2),
                new("SP-1004", "ШвидкоПошта", "Одеса", "В дорозі", 8.5, 6),
                new("SP-1005", "ШвидкоПошта", "Дніпро", "Отримано", 3.0, 3),
                new("SP-1006", "ШвидкоПошта", "Харків", "У відділенні", 0.8, 3),
                new("SP-1007", "ШвидкоПошта", "Тернопіль", "Отримано", 4.2, 2),
                new("SP-1008", "ШвидкоПошта", "Полтава", "В дорозі", 6.7, 5)
            };
        }

        static async Task<List<Parcel>> LoadExpressDostavkaAsync()
        {
            await Task.Delay(1500); // Імітація затримки сервера 1,5 с
            return new List<Parcel>
            {
                new("ED-2001", "ЕкспресДоставка", "Вінниця", "Отримано", 1.5, 1),
                new("ED-2002", "ЕкспресДоставка", "Київ", "В дорозі", 12.0, 7),
                new("ED-2003", "ЕкспресДоставка", "Житомир", "У відділенні", 2.3, 2),
                new("ED-2004", "ЕкспресДоставка", "Рівне", "Отримано", 3.4, 2),
                new("ED-2005", "ЕкспресДоставка", "Львів", "У відділенні", 1.9, 1),
                new("ED-2006", "ЕкспресДоставка", "Черкаси", "В дорозі", 7.8, 3),
                new("ED-2007", "ЕкспресДоставка", "Луцьк", "Отримано", 2.1, 1),
                new("ED-2008", "ЕкспресДоставка", "Хмельницький", "В дорозі", 4.6, 5)
            };
        }

        static async Task<List<Parcel>> LoadPosylka24Async()
        {
            await Task.Delay(1000); // Імітація затримки сервера 1,0 с
            return new List<Parcel>
            {
                new("P24-3001", "Посилка24", "Одеса", "Отримано", 6.3, 4),
                new("P24-3002", "Посилка24", "Київ", "В дорозі", 3.5, 8),
                new("P24-3003", "Посилка24", "Запоріжжя", "У відділенні", 9.2, 5),
                new("P24-3004", "Посилка24", "Вінниця", "Отримано", 1.1, 3),
                new("P24-3005", "Посилка24", "Івано-Франківськ", "В дорозі", 4.8, 2),
                new("P24-3006", "Посилка24", "Миколаїв", "У відділенні", 2.7, 4),
                new("P24-3007", "Посилка24", "Чернівці", "Отримано", 5.5, 5),
                new("P24-3008", "Посилка24", "Суми", "В дорозі", 11.4, 6)
            };
        }

        // =========================================================
        // 4. ОБРОБНИКИ ПУНКТІВ МЕНЮ
        // =========================================================
        static async Task<bool> HandleLoadAsync(bool parallel)
        {
            Console.WriteLine(parallel
                ? "\nЗавантаження даних з 3 служб одночасно (Task.WhenAll)..."
                : "\nЗавантаження даних з 3 служб послідовно (await по черзі)...");

            var sw = Stopwatch.StartNew();
            List<Parcel>[] parts;

            if (parallel)
            {
                parts = await Task.WhenAll(
                    LoadShvydkoPoshtaAsync(),
                    LoadExpressDostavkaAsync(),
                    LoadPosylka24Async());
            }
            else
            {
                parts = new[]
                {
                    await LoadShvydkoPoshtaAsync(),
                    await LoadExpressDostavkaAsync(),
                    await LoadPosylka24Async()
                };
            }

            sw.Stop();

            // SelectMany об'єднує списки з усіх джерел в єдину колекцію
            _parcels = parts.SelectMany(p => p).ToList();

            Console.WriteLine($"Завантажено посилок: {_parcels.Count}. Час виконання: {sw.ElapsedMilliseconds} мс.");
            return true;
        }

        // Запит LINQ №1 (пункт 4 меню): GroupBy + агрегатні функції + анонімний тип
        static bool HandleCarrierStats()
        {
            if (!HasData()) return true;

            Console.WriteLine("\n--- Статистика за службами доставки ---");
            var stats = _parcels
                .GroupBy(p => p.Carrier)
                .Select(g => new
                {
                    Carrier = g.Key,
                    TotalCount = g.Count(),
                    InTransit = g.Count(p => p.Status == "В дорозі"),
                    AtBranch = g.Count(p => p.Status == "У відділенні"),
                    Delivered = g.Count(p => p.Status == "Отримано"),
                    TotalWeight = g.Sum(p => p.WeightKg)
                })
                .OrderBy(s => s.Carrier)
                .ToList();

            foreach (var s in stats)
            {
                Console.WriteLine(
                    $"{s.Carrier,-16} | Усього: {s.TotalCount} | В дорозі: {s.InTransit} | " +
                    $"У відділенні: {s.AtBranch} | Отримано: {s.Delivered} | Сумарна вага: {s.TotalWeight:F1} кг");
            }

            return true;
        }

        // Запит LINQ №2 (пункт 5 меню): Where + GroupBy + Average + OrderBy/ThenBy
        static bool HandleAverageDeliveryTime()
        {
            if (!HasData()) return true;

            Console.WriteLine("\n--- Середній термін доставки отриманих посилок ---");
            var delivered = _parcels.Where(p => p.Status == "Отримано").ToList();

            if (!delivered.Any())
            {
                Console.WriteLine("Отриманих посилок у базі немає.");
                return true;
            }

            var ranking = delivered
                .GroupBy(p => p.Carrier)
                .Select(g => new
                {
                    Carrier = g.Key,
                    Count = g.Count(),
                    AvgDays = g.Average(p => p.DaysInTransit)
                })
                .OrderBy(x => x.AvgDays)
                .ThenBy(x => x.Carrier)
                .ToList();

            int place = 1;
            foreach (var item in ranking)
            {
                Console.WriteLine(
                    $"{place}. {item.Carrier,-16} — середній термін: {item.AvgDays:F2} дн. (отримано посилок: {item.Count})");
                place++;
            }

            return true;
        }

        // Запит LINQ №3 (пункт 6 меню): query-синтаксис із where, orderby descending, thenby
        static bool HandleDelayedParcels()
        {
            if (!HasData()) return true;

            int minDays = ReadPositiveInt("\nВведіть граничну кількість днів у дорозі (N > 0): ");

            var delayed =
                (from p in _parcels
                 where p.Status == "В дорозі" && p.DaysInTransit > minDays
                 orderby p.DaysInTransit descending, p.Carrier ascending
                 select new
                 {
                     p.TrackingNumber,
                     p.Carrier,
                     p.City,
                     p.DaysInTransit,
                     p.WeightKg
                 }).ToList();

            Console.WriteLine($"\n--- Посилки зі статусом «В дорозі» довше {minDays} дн. ---");
            if (!delayed.Any())
            {
                Console.WriteLine("Затриманих посилок за вказаним критерієм не знайдено.");
                return true;
            }

            foreach (var d in delayed)
            {
                Console.WriteLine(
                    $"ТТН: {d.TrackingNumber,-9} | Служба: {d.Carrier,-16} | Місто: {d.City,-16} | " +
                    $"У дорозі: {d.DaysInTransit} дн. | Вага: {d.WeightKg:F1} кг");
            }

            return true;
        }

        // Запит LINQ №4 (пункт 7 меню): Any + GroupJoin (обробка невідомого трек-номера)
        static bool HandleCustomerOrders()
        {
            if (!HasData()) return true;

            Console.WriteLine("\nДоступні клієнти у базі: Олена Коваленко, Іван Петренко, Марія Бойко, Олексій Ткаченко, Наталія Мельник, Дмитро Шевченко, Софія Бондаренко, Андрій Савченко");
            string customer = ReadNonEmpty("Введіть ім'я та прізвище клієнта: ");

            var customerOrders = _orders
                .Where(o => o.Customer.Equals(customer, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!customerOrders.Any())
            {
                Console.WriteLine("Замовлень не знайдено.");
                return true;
            }

            // GroupJoin зберігає всі замовлення клієнта, навіть якщо посилку не знайдено в жодній службі
            var details = customerOrders
                .GroupJoin(
                    _parcels,
                    order => order.TrackingNumber,
                    parcel => parcel.TrackingNumber,
                    (order, parcelsGroup) =>
                    {
                        var parcel = parcelsGroup.FirstOrDefault();
                        return new
                        {
                            order.OrderId,
                            order.TrackingNumber,
                            Status = parcel?.Status ?? "статус невідомий",
                            City = parcel?.City ?? "—",
                            Carrier = parcel?.Carrier ?? "—"
                        };
                    })
                .ToList();

            Console.WriteLine($"\n--- Замовлення клієнта: {customerOrders[0].Customer} ---");
            foreach (var item in details)
            {
                Console.WriteLine(
                    $"Замовлення {item.OrderId} | ТТН: {item.TrackingNumber,-8} | " +
                    $"Статус: {item.Status,-17} | Місто: {item.City,-15} | Служба: {item.Carrier}");
            }

            return true;
        }

        // Пункт 8 меню: асинхронне збереження звіту в JSON з перехопленням IOException
        static async Task<bool> HandleSaveReportAsync()
        {
            if (!HasData()) return true;

            var report = _orders
                .GroupJoin(
                    _parcels,
                    order => order.TrackingNumber,
                    parcel => parcel.TrackingNumber,
                    (order, parcelsGroup) =>
                    {
                        var parcel = parcelsGroup.FirstOrDefault();
                        return new
                        {
                            order.OrderId,
                            order.Customer,
                            order.TrackingNumber,
                            Status = parcel?.Status ?? "статус невідомий",
                            Carrier = parcel?.Carrier ?? "невідомо",
                            City = parcel?.City ?? "невідомо"
                        };
                    })
                .ToList();

            string json = JsonSerializer.Serialize(report, _jsonOptions);
            const string fileName = "orders_report.json";

            try
            {
                await File.WriteAllTextAsync(fileName, json);
                Console.WriteLine($"Звіт про всі замовлення ({report.Count} записів) успішно збережено у файл {fileName}.");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Помилка запису файлу звіту: {ex.Message}");
            }

            return true;
        }

        // =========================================================
        // 5. ДОПОМІЖНІ МЕТОДИ ВАЛІДАЦІЇ ВВЕДЕННЯ
        // =========================================================
        static bool HasData()
        {
            if (_parcels.Count > 0)
            {
                return true;
            }
            Console.WriteLine("Дані ще не завантажено. Спочатку оберіть пункт 2 або 3.");
            return false;
        }

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
                Console.WriteLine("Помилка: рядок не може бути порожнім.");
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

        static int ReadPositiveInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int value) && value > 0)
                {
                    return value;
                }
                Console.WriteLine("Помилка: введіть ціле число більше 0.");
            }
        }
    }
}