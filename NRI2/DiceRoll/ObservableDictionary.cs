using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NRI.DiceRoll
{
    public class ObservableDictionary : INotifyPropertyChanged, INotifyCollectionChanged, IEnumerable<ObservableKeyValuePair>
    {
        private readonly ObservableCollection<ObservableKeyValuePair> _items = new ObservableCollection<ObservableKeyValuePair>();

        public ObservableDictionary()
        {
            _items.CollectionChanged += (s, e) =>
            {
                CollectionChanged?.Invoke(this, e);
                OnPropertyChanged(nameof(Count));
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public int Count => _items.Count;

        public void Add(string key, string value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (_items.Any(i => i.Key == key))
                throw new ArgumentException($"Ключ '{key}' уже существует в словаре");

            _items.Add(new ObservableKeyValuePair { Key = key, Value = value });
        }

        public void Clear()
        {
            _items.Clear();
            OnPropertyChanged(nameof(Count));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }


        public bool ContainsKey(string key)
        {
            return _items.Any(item => item.Key == key);
        }

        public bool Remove(string key)
        {
            var item = _items.FirstOrDefault(i => i.Key == key);
            if (item != null)
            {
                return _items.Remove(item);
            }
            return false;
        }

        public bool TryGetValue(string key, out string value)
        {
            var item = _items.FirstOrDefault(i => i.Key == key);
            if (item != null)
            {
                value = item.Value;
                return true;
            }
            value = null;
            return false;
        }

        public string this[string key]
        {
            get
            {
                var item = _items.FirstOrDefault(i => i.Key == key);
                return item?.Value;
            }
            set
            {
                var item = _items.FirstOrDefault(i => i.Key == key);
                if (item != null)
                {
                    item.Value = value;
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        public ICollection<string> Keys => _items.Select(item => item.Key).ToList();
        public ICollection<string> Values => _items.Select(item => item.Value).ToList();

        public IEnumerator<ObservableKeyValuePair> GetEnumerator() => _items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
