using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTask.dal.Models
{
    /// <summary>
    /// Количество проживающих в разные периоды месяца
    /// </summary>
    public class ResidentCount
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [ForeignKey(nameof(Tenant))]
        public Guid TenantId { get; set; }
        
        /// <summary>
        /// Дата начала периода проживания
        /// </summary>
        public DateTime From { get; set; }
        /// <summary>
        /// Дата окончания периода проживания
        /// </summary>

        public DateTime To { get; set; }
        /// <summary>
        /// Количество человек
        /// </summary>

        public int Count { get; set; }
        /// <summary>
        /// Месяц расчета
        /// </summary>
        public DateTime Month { get; set; }
    }
}
