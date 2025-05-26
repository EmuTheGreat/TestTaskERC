using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestTask.Logic;

namespace TestTask.dal.Models
{
    /// <summary>
    /// Показания счетчика за указанный месяц для конкретного сервиса
    /// </summary>
    public class MonthlyReading
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [ForeignKey(nameof(Tenant))]
        public Guid TenantId { get; set; }

        /// <summary>
        /// Тип услгуи
        /// </summary>
        public ServiceType Service { get; set; }

        /// <summary>
        /// Значение объёма услуг за предыдущий период
        /// </summary>
        public double Previous { get; set; }

        /// <summary>
        /// Значение объёма услуг за нынешний период
        /// </summary>
        public double Current { get; set; }

        /// <summary>
        /// Месяц расчёта
        /// </summary>
        public DateTime Month { get; set; }
    }
}
