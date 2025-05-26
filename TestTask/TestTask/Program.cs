using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.IO;
using System.Linq;
using TestTask.dal;
using TestTask.dal.Models;
using TestTask.Logic;
using Microsoft.EntityFrameworkCore;

namespace TestTask
{
    /// <summary>Консольное приложение.</summary>
    public static class Program
    {
        private static readonly DateTime StartDate = new DateTime(2025, 1, 1);

        public static void Main(string[] args)
        {
            using var ctx = new BillingContext();
            ctx.Database.EnsureCreated();
            InitializeTariffsAndNorms(ctx);

            while (true)
            {
                // 1. Выбор или создание абонента
                var tenant = SelectOrCreateSubscriber(ctx);

                // 2. Ввод месяца расчета
                var month = ReadMonth();

                // 3. Ввод состава жильцов
                var residents = ReadResidents(tenant.Id, month);

                // 4. Ввод показаний счетчиков
                var readings = ReadMeterReadings(tenant.Id, month);

                // 5. Расчет начислений
                var bill = ComputeBill(ctx, month, residents, readings);
                bill.TenantId = tenant.Id;

                // 6. Вывод результатов
                DisplayBill(bill);

                // 7. Сохранение данных
                if (Confirm("Сохранить результат и перейти к следующему месяцу? (y/n): "))
                    SaveData(ctx, tenant.Id, residents, readings, bill);
            }
        }

        private static Tenant SelectOrCreateSubscriber(BillingContext ctx)
        {
            var tenants = ctx.Tenants.AsNoTracking().ToList();
            Console.WriteLine("Список абонентов:");
            for (int i = 0; i < tenants.Count; i++)
                Console.WriteLine($"{i + 1}. {tenants[i].Name} ({tenants[i].Address})");
            Console.WriteLine("0. Добавить нового абонента");
            Console.Write("Выбор: ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= tenants.Count)
                return tenants[choice - 1];

            Console.Write("Имя абонента: ");
            var name = Console.ReadLine();
            Console.Write("Адрес: ");
            var address = Console.ReadLine();

            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = name,
                Address = address
            };
            ctx.Tenants.Add(tenant);
            ctx.SaveChanges();
            return tenant;
        }

        private static DateTime ReadMonth()
        {
            while (true)
            {
                Console.Write("Введите год и месяц (YYYY-MM): ");
                if (DateTime.TryParseExact(Console.ReadLine(), "yyyy-MM", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var month))
                    return month;
                Console.WriteLine("Неверный формат. Попробуйте снова.");
            }
        }

        private static List<ResidentCount> ReadResidents(Guid subscriberId, DateTime month)
        {
            Console.Write("Жил ли один и тот же состав жильцов весь месяц? (y/n): ");
            bool constant = Confirm("");
            var list = new List<ResidentCount>();

            if (constant)
            {
                Console.Write("Количество проживающих: ");
                int cnt = int.Parse(Console.ReadLine());
                list.Add(new ResidentCount
                {
                    Id = Guid.NewGuid(),
                    TenantId = subscriberId,
                    From = new DateTime(month.Year, month.Month, 1),
                    To = new DateTime(month.Year, month.Month, DateTime.DaysInMonth(month.Year, month.Month)),
                    Count = cnt,
                    Month = month
                });
            }
            else
            {
                Console.Write("Сколько периодов с разным числом жильцов? ");
                int periods = int.Parse(Console.ReadLine());
                for (int i = 0; i < periods; i++)
                {
                    Console.WriteLine($"Период {i + 1}:");
                    Console.Write("  Начало (yyyy-MM-dd): ");
                    var from = DateTime.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);
                    Console.Write("  Конец  (yyyy-MM-dd): ");
                    var to = DateTime.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);
                    Console.Write("  Кол-во жильцов: ");
                    int cnt = int.Parse(Console.ReadLine());
                    list.Add(new ResidentCount
                    {
                        Id = Guid.NewGuid(),
                        TenantId = subscriberId,
                        From = from,
                        To = to,
                        Count = cnt,
                        Month = month
                    });
                }
            }

            return list;
        }

        private static List<MonthlyReading> ReadMeterReadings(Guid subscriberId, DateTime month)
        {
            var list = new List<MonthlyReading>();
            foreach (var svc in new[] { ServiceType.ColdWater, ServiceType.HotWater })
            {
                if (Confirm($"Есть прибор учета для {svc}? (y/n): "))
                {
                    var rd = ReadReading(svc, month);
                    rd.Id = Guid.NewGuid();
                    rd.TenantId = subscriberId;
                    list.Add(rd);
                }
            }

            if (Confirm("Есть прибор учета электроэнергии? (y/n): "))
            {
                var day = ReadReading(ServiceType.ElectricityDay, month, "дневная шкала");
                day.Id = Guid.NewGuid(); day.TenantId = subscriberId;
                list.Add(day);

                var night = ReadReading(ServiceType.ElectricityNight, month, "ночная шкала");
                night.Id = Guid.NewGuid(); night.TenantId = subscriberId;
                list.Add(night);
            }

            return list;
        }

        private static MonthlyReading ReadReading(ServiceType service, DateTime month, string label = "")
        {
            Console.Write($"  Предыдущее значение {label}: ");
            var prev = double.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);
            Console.Write($"  Текущее значение {label}: ");
            var curr = double.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);
            return new MonthlyReading { Service = service, Previous = prev, Current = curr, Month = month };
        }

        private static Bill ComputeBill(BillingContext ctx, DateTime month, List<ResidentCount> residents,
            List<MonthlyReading> readings)
        {
            var calc = new CalculationService(ctx);
            var bill = new Bill { Id = Guid.NewGuid(), Month = month };

            // ХВС
            bill.ColdWaterCharge = calc.CalculateCharge(
                ServiceType.ColdWater,
                calc.CalculateVolume(ServiceType.ColdWater,
                    readings.FirstOrDefault(r => r.Service == ServiceType.ColdWater), residents),
                month);

            // ГВС (вода)
            bill.HotWaterCharge = calc.CalculateCharge(
                ServiceType.HotWater,
                calc.CalculateVolume(ServiceType.HotWater,
                    readings.FirstOrDefault(r => r.Service == ServiceType.HotWater), residents),
                month);

            // ГВС (тепло)
            var volHW = calc.CalculateVolume(
                ServiceType.HotWater,
                readings.FirstOrDefault(r => r.Service == ServiceType.HotWater), residents);
            bill.HotWaterHeatCharge = calc.CalculateCharge(
                ServiceType.HotWaterHeat,
                volHW * calc.GetNorm(ServiceType.HotWaterHeat, month),
                month);

            // Электроэнергия
            if (readings.Any(r => r.Service == ServiceType.ElectricityDay))
            {
                var volDay = calc.CalculateVolume(ServiceType.ElectricityDay,
                    readings.First(r => r.Service == ServiceType.ElectricityDay), residents);
                var volNight = calc.CalculateVolume(ServiceType.ElectricityNight,
                    readings.First(r => r.Service == ServiceType.ElectricityNight), residents);

                bill.ElectricityDayCharge = calc.CalculateCharge(ServiceType.ElectricityDay, volDay, month);
                bill.ElectricityNightCharge = calc.CalculateCharge(ServiceType.ElectricityNight, volNight, month);
            }
            else
            {
                var totalVolE = calc.CalculateVolume(ServiceType.Electricity, null, residents);
                var totalCharge = calc.CalculateCharge(ServiceType.Electricity, totalVolE, month);
                bill.ElectricityDayCharge = totalCharge;
                bill.ElectricityNightCharge = 0;
            }

            // Сохранение итоговых полей
            bill.ElectricityCharge = bill.ElectricityDayCharge + bill.ElectricityNightCharge;
            bill.Total = bill.ColdWaterCharge + bill.HotWaterCharge + bill.HotWaterHeatCharge + bill.ElectricityCharge;

            return bill;
        }

        private static void DisplayBill(Bill bill)
        {
            Console.WriteLine($"\nРезультаты за {bill.Month:yyyy-MM}:");
            Console.WriteLine($"  ХВС:           {bill.ColdWaterCharge:F2} руб.");
            Console.WriteLine($"  ГВС (вода):    {bill.HotWaterCharge:F2} руб.");
            Console.WriteLine($"  ГВС (тепло):   {bill.HotWaterHeatCharge:F2} руб.");
            Console.WriteLine($"  Электроэнергия: {bill.ElectricityCharge:F2} руб.");
            if (bill.ElectricityNightCharge > 0)
            {
                Console.WriteLine($"    - дневная:    {bill.ElectricityDayCharge:F2} руб.");
                Console.WriteLine($"    - ночная:     {bill.ElectricityNightCharge:F2} руб.");
            }
            Console.WriteLine($"  Общая сумма:   {bill.Total:F2} руб.\n");
        }

        private static bool Confirm(string message)
        {
            if (!string.IsNullOrEmpty(message)) Console.Write(message);
            return Console.ReadLine()?.Trim().ToLower() == "y";
        }

        private static void SaveData(BillingContext ctx, Guid subscriberId, List<ResidentCount> residents,
            List<MonthlyReading> readings, Bill bill)
        {
            residents.ForEach(r => r.TenantId = subscriberId);
            readings.ForEach(r => r.TenantId = subscriberId);
            bill.TenantId = subscriberId;

            ctx.ResidentCounts.AddRange(residents);
            ctx.MonthlyReadings.AddRange(readings);
            ctx.Bills.Add(bill);
            ctx.SaveChanges();
        }

        private static void InitializeTariffsAndNorms(BillingContext ctx)
        {
            if (!ctx.Tariffs.Any())
            {
                ctx.Tariffs.AddRange(new List<ServiceTariff>
                {
                    new ServiceTariff { Service = ServiceType.ColdWater.ToString(), Value = 35.78, EffectiveFrom = StartDate },
                    new ServiceTariff { Service = ServiceType.HotWater.ToString(), Value = 35.78, EffectiveFrom = StartDate },
                    new ServiceTariff { Service = ServiceType.HotWaterHeat.ToString(), Value = 998.69, EffectiveFrom = StartDate },
                    new ServiceTariff { Service = ServiceType.ElectricityDay.ToString(), Value = 4.90, EffectiveFrom = StartDate },
                    new ServiceTariff { Service = ServiceType.ElectricityNight.ToString(), Value = 2.31, EffectiveFrom = StartDate },
                    new ServiceTariff { Service = ServiceType.Electricity.ToString(), Value = 4.28, EffectiveFrom = StartDate }
                });
                ctx.SaveChanges();
            }
            if (!ctx.Norms.Any(n => n.Service == ServiceType.Electricity.ToString()))
            {
                ctx.Norms.AddRange(new List<ServiceNorm>
                {
                    new ServiceNorm { Service = ServiceType.ColdWater.ToString(), Value = 4.85, EffectiveFrom = StartDate },
                    new ServiceNorm { Service = ServiceType.HotWater.ToString(), Value = 4.01, EffectiveFrom = StartDate },
                    new ServiceNorm { Service = ServiceType.HotWaterHeat.ToString(), Value = 0.05349, EffectiveFrom = StartDate },
                    new ServiceNorm { Service = ServiceType.ElectricityDay.ToString(), Value = 0, EffectiveFrom = StartDate },
                    new ServiceNorm { Service = ServiceType.ElectricityNight.ToString(), Value = 0, EffectiveFrom = StartDate },
                    new ServiceNorm { Service = ServiceType.Electricity.ToString(), Value = 164, EffectiveFrom = StartDate }
                });
                ctx.SaveChanges();
            }
        }
    }
}