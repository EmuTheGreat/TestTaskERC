using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTask.dal.Models
{
    /// <summary>
    /// Итоговые начисления по каждому сервису за месяц.
    /// </summary>
    public class Bill
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [ForeignKey(nameof(Tenant))]
        public Guid TenantId { get; set; }
        
        /// <summary>
        /// Месяц начисления.
        /// </summary>
        public DateTime Month { get; set; }
        /// <summary>
        /// Начисление за холодное водоснабжение.
        /// </summary>

        public double ColdWaterCharge { get; set; }
        /// <summary>
        /// Начисление за горячую воду (теплоноситель).
        /// </summary>

        public double HotWaterCharge { get; set; }
        /// <summary>
        /// Начисление за тепловую энергию горячей воды.
        /// </summary>

        public double HotWaterHeatCharge { get; set; }
        /// <summary>
        /// Начисление за электричество по дневному тарифу.
        /// </summary>

        public double ElectricityDayCharge { get; set; }
        /// <summary>
        /// Начисление за электричество по ночному тарифу.
        /// </summary>

        public double ElectricityNightCharge { get; set; }

        /// <summary>
        /// Общая сумма начислений за ээ.
        /// </summary>
        public double ElectricityCharge { get; set; }
        
        /// <summary>
        /// Общая сумма начислений.
        /// </summary>
        public double Total { get; set; }
    }
}
