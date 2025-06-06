# Расчёт коммунальных услуг

## Описание
Консольное приложение для расчёта и учёта коммунальных услуг (ХВС, ГВС, электроэнергия) с использованием **EF Core** и **SQLite**. Позволяет:

- Управлять списком абонентов (лицевых счетов)
- Вводить показания счётчиков и данные о составе жильцов
- Считать показания для разного кол-ва проживающих
- Автоматически рассчитывать начисления по тарифам и нормативам
- Версионировать тарифы и нормативы в базе данных
- Хранить историю расчётов для каждого абонента

## Стек технологий

- .NET 6+ (C#)
- Entity Framework Core
- SQLite
- Консольный интерфейс

## Быстрый старт

1. **Клонировать репозиторий**
   ```bash
   git clone https://github.com/ВАШ_ПРОФИЛЬ/utility-billing.git
   cd utility-billing
   ```

2. **Установить зависимости**
   ```bash
   dotnet restore
   ```

3. **Создать и применить миграции**  
   > Вместо `EnsureCreated()` рекомендуется использовать миграции EF Core:
   ```bash
   dotnet tool install --global dotnet-ef          # если ещё не установлен
   dotnet ef migrations add InitialCreate          # первая миграция схемы
   dotnet ef database update                       # применить миграции к billing.db
   ```

4. **Запустить приложение**
   ```bash
   dotnet run --project TestTask
   ```
   При первом запуске создаётся файл `billing.db` в корне проекта.

## Структура проекта

- `Program.cs` — консольный интерфейс (выбор абонента, ввод данных, расчёт, сохранение)  
- `CalculationService.cs` — бизнес-логика расчёта объёмов и сумм  
- `Models/` — сущности EF Core: `Tenant`, `ResidentCount`, `MonthlyReading`, `Bill`, `ServiceTariff`, `ServiceNorm`  
- `Migrations/` — автосгенерированные миграции схемы

## Особенности реализации

1. **Абоненты** сохраняются в таблице `Tenants` c GUID PK и полями `Name`, `Address`.  
2. **Тарифы и нормативы** версионируются по полю `EffectiveFrom`.  
3. **Электроэнергия**:  
   - При наличии счётчика вводятся дневная и ночная шкалы.  
   - Без счётчика рассчитывается общий объём по нормативу `V = n * N`.  
4. **Горячая вода** разбита на тепловую (ГВС-ТН) и водную (ГВС-ТЭ) части.

## Настройка

- Измените строку подключения в `BillingContext.OnConfiguring` для другого пути к `billing.db`.  

