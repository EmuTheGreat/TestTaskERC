using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestTask.Logic;

namespace TestTask.dal.Models
{
    public class ServiceNorm
    {
        public int Id { get; set; }

        /// <summary>
        /// Тип услуги
        /// </summary>
        public string Service { get; set; }

        /// <summary>
        /// Нома потребления
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Дата начала действия нормы
        /// </summary>
        public DateTime EffectiveFrom { get; set; }
    }
}
