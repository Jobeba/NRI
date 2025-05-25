using System.Collections.Generic;
using System.Collections.ObjectModel;

 namespace NRI.Extensions
    {
        public static class ObservableCollectionExtensions
        {
            public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
            {
                if (collection == null || items == null)
                    return;

                foreach (var item in items)
                {
                    collection.Add(item);
                }
            }
        }
    }

