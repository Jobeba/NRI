using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRI.DiceRoll
{
    public static class DictionaryExtensions
    {
        public static ObservableDictionary ToObservableDictionary(this Dictionary<string, string> dictionary)
        {
            var result = new ObservableDictionary();
            foreach (var item in dictionary)
            {
                result.Add(item.Key, item.Value);
            }
            return result;
        }
    }
}
