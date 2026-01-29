using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Calculating.Services
{
    internal interface ICalculatingService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="formula">Формула для вычислени, например, "=({TreeLeave[132542-ewqeerg-322345-fddfdgb].Length + 100) * {TreeLeave[132542-ewqeerg-322345-fddfdgb].Width * {TreeLeave[132542-ewqeerg-322345-fddfdgb].Height} + SIN({TreeLeave[132542-ewqeerg-322345-fddfdgb].Angle}) - 1000"</param>
        /// <returns></returns>
        public string CalculateFormula(string formula);
    }
}
