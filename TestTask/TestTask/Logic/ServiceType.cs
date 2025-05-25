using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTask.Logic
{
    /// <summary>
    /// Тип услуги
    /// </summary>
    public enum ServiceType
    {
        /// <summary>
        /// ХВС
        /// </summary>
        ColdWater = 0,

        /// <summary>
        /// ГВС ТЭ
        /// </summary>
        HotWater = 1,

        /// <summary>
        /// ГВС ТН
        /// </summary>
        HotWaterHeat = 2,

        /// <summary>
        /// ЭЭ дневная
        /// </summary>
        ElectricityDay = 3,

        /// <summary>
        /// ЭЭ ночаня
        /// </summary>
        ElectricityNight = 4,
    }
}
