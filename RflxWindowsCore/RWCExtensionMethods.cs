using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/**
 * Class for Common Utilities.
 * <p>Compatible with Windows 10 : Version 10.0.10586 and above...</p>
 *
 * @author  Nikhil Menon
 * @version 1.0.1 11/18/2016
 * @since   7/20/2015
 */
namespace RflxWindowsCore
{
    public static class RWCExtensionMethods
    {
        public static bool IsNumberBetweenRange(this Double number, double start, double end)
        {
            return Comparer<double>.Default.Compare(number, start) >= 0 &&
                Comparer<double>.Default.Compare(number, end) <= 0;
        }





        public static void AddRangeToObservableCollection<T>(this ObservableCollection<T> collection, ObservableCollection<T> newList)
        {
            if (newList != null)
            {
                foreach (var item in newList)
                {
                    collection.Add(item);
                }
            }
        }


        //public static void ForEach<T>(this ObservableCollection<T> collection,)

        public static IEnumerable<TSource> ForEachForObservableCollection<TSource>(this IEnumerable<TSource> enumerable, Predicate<TSource> method)
        {
            foreach (TSource item in enumerable)
            {
                yield return item;
            }
        }

    }
}
