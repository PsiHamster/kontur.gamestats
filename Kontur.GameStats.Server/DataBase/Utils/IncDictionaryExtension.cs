using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kontur.GameStats.Server.DataBase {
    internal static class IncDictionary {

        /// <summary>
        /// Увеличить значение в словаре по ключу
        /// на 1/создать с значением 1, если не существует
        /// </summary>
        public static void IncDict<T>(this IDictionary<T, int> dict, T key) {
            if (dict.ContainsKey(key)) {
                dict[key] += 1;
            } else {
                dict[key] = 1;
            }
        }


    }
}
