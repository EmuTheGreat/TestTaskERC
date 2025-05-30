﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestTask.Logic;

namespace TestTask.dal.Models
{
    public class ServiceTariff
    {
        public int Id { get; set; }

        /// <summary>
        /// Тип услуги
        /// </summary>
        public string Service { get; set; }

        /// <summary>
        /// Значение тарифа
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Дата начала действия тарифа
        /// </summary>
        public DateTime EffectiveFrom { get; set; }
    }
}
