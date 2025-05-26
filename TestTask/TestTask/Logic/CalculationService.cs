using Microsoft.EntityFrameworkCore;
using TestTask.dal;
using TestTask.dal.Models;

namespace TestTask.Logic;

/// <summary>Логика расчета объема и суммы.</summary>
public class CalculationService
{
    private readonly BillingContext _ctx;
    public CalculationService(BillingContext ctx) => _ctx = ctx;

    public double GetTariff(ServiceType service, DateTime month)
        => _ctx.Tariffs.AsNoTracking()
            .Where(t => t.Service == service.ToString() && t.EffectiveFrom <= month)
            .OrderByDescending(t => t.EffectiveFrom)
            .First().Value;

    public double GetNorm(ServiceType service, DateTime month)
        => _ctx.Norms.AsNoTracking()
            .Where(n => n.Service == service.ToString() && n.EffectiveFrom <= month)
            .OrderByDescending(n => n.EffectiveFrom)
            .First().Value;

    public double CalculateVolume(ServiceType service, MonthlyReading reading, IEnumerable<ResidentCount> residents)
    {
        if (reading != null)
            return reading.Current - reading.Previous;

        double daysInMonth = DateTime.DaysInMonth(residents.First().Month.Year, residents.First().Month.Month);
        return residents.Sum(rc => rc.Count * GetNorm(service, rc.Month) * ((rc.To - rc.From).Days + 1) / daysInMonth);
    }

    public double CalculateCharge(ServiceType service, double volume, DateTime month)
        => volume * GetTariff(service, month);
}