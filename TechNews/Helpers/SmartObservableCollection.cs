using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace TechNews.Helpers
{
    public class SmartObservableCollection<T> : ObservableCollection<T>
    {
        protected bool DeferNotification = false;

        public void ReplaceAll(IEnumerable<T> collection)
        {
            DeferNotification = true;
            ClearItems();
            AddRange(collection);
        }

        public void AddRange(IEnumerable<T> collection)
        {
            DeferNotification = true;
            foreach (var itm in collection)
            {
                this.Add(itm);
            }
            DeferNotification = false;
            OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
        }

        public void RemoveRange(IEnumerable<T> collection)
        {
            DeferNotification = true;
            foreach (var itm in collection)
            {
                this.Remove(itm);
            }
            DeferNotification = false;
            OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
        }

        protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!DeferNotification)
            {
                base.OnCollectionChanged(e);
            }
        }



    }
}
