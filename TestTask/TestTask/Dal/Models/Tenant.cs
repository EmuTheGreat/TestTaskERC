namespace TestTask.dal.Models;

/// <summary>
/// Представляет жильца с лицевым счетом.
/// </summary>
public class Tenant
{
    public Guid Id { get; set; }

    /// <summary>
    /// Имя или описание абонента (квартиры).
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Адрес абонента.
    /// </summary>
    public string Address { get; set; }

    public ICollection<ResidentCount> ResidentCounts { get; set; }
    public ICollection<MonthlyReading> MonthlyReadings { get; set; }
    public ICollection<Bill> Bills { get; set; }
}