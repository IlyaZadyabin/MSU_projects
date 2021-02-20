﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text.Json;
using System.Security.Permissions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Xml.Serialization;
using FluentAssertions.Formatting;

namespace FieldLibrary
{
    public enum ChangeInfo {
        ItemChanged,
        Add,
        Remove,
        Replace
    }

    [Serializable]
    public class V1MainCollection : IEnumerable<V1Data>, INotifyCollectionChanged
    {
        [field: NonSerialized]
        public event DataChangedEventHandler DataChanged;

        [field: NonSerialized]
        public event System.Collections.Specialized.NotifyCollectionChangedEventHandler CollectionChanged;

        public void DataChangesCollector(object sender, PropertyChangedEventArgs args) {
            if (DataChanged != null) {
                DataChanged(this, new DataChangedEventArgs(ChangeInfo.ItemChanged, args.PropertyName));
            }
        }

        public void OnNotifyCollectionChanged(object sender, NotifyCollectionChangedEventArgs args) {
            if (CollectionChanged != null) {
                IsCollectionChanged = true;
                //CollectionChanged(this, args);
            }
         }

        public delegate void NotifyCollectionChangedEventHandler(object sender, NotifyCollectionChangedEventArgs args);
        public delegate void DataChangedEventHandler(object source, DataChangedEventArgs args);

        public bool IsCollectionChanged = false;

        public void Save(string filename)
        {
            FileStream fileStream = null;
            try
            {
                fileStream = File.Create(filename);
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(fileStream, this);
                IsCollectionChanged = false;
            }
            catch (Exception ex)
            { Console.WriteLine("Save\n " + ex.Message); }
            finally
            { if (fileStream != null) fileStream.Close(); }
        }

        public void Load(string filename)
        {
            FileStream fileStream = null;
            V1MainCollection res = null;
            try
            {
                fileStream = File.OpenRead(filename);
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                res = binaryFormatter.Deserialize(fileStream) as V1MainCollection;
            }
            catch (Exception ex)
            { Console.WriteLine("Load\n " + ex.Message); }
            finally
            { if (fileStream != null) fileStream.Close(); }

            if (res != null)
                list = res.list;
        }

        public V1Data this[int index] {
            get { return list[index]; }
            set {
                if (list[index] != value) {
                    if (DataChanged != null) {
                        DataChanged(this, new DataChangedEventArgs(ChangeInfo.Replace, list[index].ToLongString()
                            + Environment.NewLine + "with " + value.ToLongString()));
                    }
                    if (CollectionChanged != null)
                        OnNotifyCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, list[index], index));

                    list[index].PropertyChanged -= DataChangesCollector;
                    list[index] = value;
                    list[index].PropertyChanged += DataChangesCollector;

                    CollectionChanged += OnNotifyCollectionChanged;
                }
            }
        }

        public int Count {
            get {
                return list.Count;
            }
        }
        
        public void Add(V1Data item) {
            list.Add(item);
            if (DataChanged != null)
                DataChanged(this, new DataChangedEventArgs(ChangeInfo.Add, item.ToLongString()));
            list[^1].PropertyChanged += DataChangesCollector;

            if (CollectionChanged != null)
                OnNotifyCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            CollectionChanged += OnNotifyCollectionChanged;
            // list[^1].PropertyChanged += OnNotifyCollectionChanged;
        }

        public bool Remove(string id, DateTime dateTime) {
            int cnt_before_removal = list.Count;
            int idx = -1;

            while ((idx = list.FindIndex(
                        item => (item.Info == id) && (item.Date == dateTime))
                    ) >= 0) {
                list.ElementAt(idx).PropertyChanged -= DataChangesCollector;
                CollectionChanged += OnNotifyCollectionChanged;
                var elem = list.ElementAt(idx);
                list.RemoveAt(idx);
                if (DataChanged != null)
                    DataChanged(this, new DataChangedEventArgs(ChangeInfo.Remove, elem.ToLongString()));

                if (CollectionChanged != null)
                    OnNotifyCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, elem));
            }
            return cnt_before_removal != list.Count;
        }
        public void AddDefaults() {
            DateTime time = new DateTime(2008, 5, 1, 8, 30, 52);
            Grid grid = new Grid(0, 1, 4);
            V1DataCollection col1 = new V1DataCollection("col1", time);
            V1DataCollection col2 = new V1DataCollection("col2", time);
            V1DataCollection col3 = new V1DataCollection("col3", time); //V1DataCollection with empty List<DataItem>
            V1DataOnGrid grid1 = new V1DataOnGrid("grid1", new DateTime(2008, 5, 1, 7, 30, 52), grid);
            V1DataOnGrid grid2 = new V1DataOnGrid("grid2", time, grid);
            V1DataOnGrid grid3 = new V1DataOnGrid("grid3", time, new Grid(1.0f, 1.5f, 0)); //V1DataOnGrid with zero nodes

            col1.InitRandom(3, 1, 5, 1, 15);
            col2.InitRandom(4, 0, 3, 11, 14);
            grid1.InitRandom(3, 7);
            grid2.InitRandom(1, 20);

            Add(col1);
            Add(col2);
            Add(col3);
            Add(grid1);
            Add(grid2);
            Add(grid3);
        }
        public override string ToString() {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < list.Count; i++) {
                sb.AppendLine(list[i].ToString());
            }
            return sb.ToString();
        }
        public string ToLongString(string format) {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < list.Count; i++) {
                sb.AppendLine(list[i].ToLongString(format));
            }
            return sb.ToString();
        }
        public IEnumerator<V1Data> GetEnumerator() {
            return list.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        
        //void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    info.AddValue("list", list, typeof(V1Data));
        //}

        
        //public V1MainCollection(SerializationInfo info, StreamingContext streamingContext)
        //{
        //    list = (List<V1Data>)info.GetValue("list", typeof(List<V1Data>));
        //}

        public V1MainCollection()
        {
        }

        public int GetMaxAmount {
            get {
                int amount1 = ( from v1DataCollection in list.OfType<V1DataCollection>()
                                select v1DataCollection.list.Count
                              ).Max();

                int amount2 = ( from v1DataOnGrid in list.OfType<V1DataOnGrid>()
                                select v1DataOnGrid.grid.amount_of_nodes
                              ).Max();

                return Math.Max(amount1, amount2);
            }
        }
        public IEnumerable<DataItem> GetSorted {
            get {
                var query1 = from v1DataCollection in list.OfType<V1DataCollection>()
                             from v1DataItem in v1DataCollection
                             select v1DataItem;

                var query2 = from v1DataOnGrid in list.OfType<V1DataOnGrid>() //Implicit conversion from V1DataOnGrid to DataItem in V1DataOnGrid GetEnuminator()
                             from v1DataItem in v1DataOnGrid
                             select v1DataItem;

                var query3 = query1.Union(query2);

                return from dataItem in query3
                       orderby dataItem.vec.Length() descending
                       select dataItem;
            }
        }

        public IEnumerable<float> UniqueTime {
            get {
                var query1 = GetSorted;
                var query2 = from dataItem in query1
                             select dataItem.t;

                return from g in query2.GroupBy(x => x)
                       where g.Count() == 1
                       select g.First();
            }
        }
        private List<V1Data> list = new List<V1Data>();
    }
}
