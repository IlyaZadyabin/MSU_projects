﻿using System;
using System.Numerics;

namespace Lab1
{
    public class V1DataOnGrid : V1Data {
        public V1DataOnGrid(string info, DateTime date, Grid grid_) : base(info, date) {
            grid = grid_;
            arr = new Vector3[grid.amount_of_nodes];
        }

        public static implicit operator V1DataCollection(V1DataOnGrid obj) {
            V1DataCollection new_obj = new V1DataCollection(obj.info, obj.date);
            for (int i = 0; i < obj.grid.amount_of_nodes; i++) {
                DataItem dataItem = new DataItem(obj.grid.t0 + obj.grid.time_step * i, obj.arr[i]);
                new_obj.list.Add(dataItem);
            }
            return new_obj;
        }

        public void InitRandom(float minValue, float maxValue) {
            Random random = new System.Random();
            float range = maxValue - minValue;
            for (int i = 0; i < grid.amount_of_nodes; i++)
            {
                arr[i] = new Vector3(
                                        minValue + (float)random.NextDouble() * range,
                                        minValue + (float)random.NextDouble() * range,
                                        minValue + (float)random.NextDouble() * range
                                    );
            }
        }
        public override float[] NearZero(float eps) {
            float[] nodes = new float[grid.amount_of_nodes];
            int length = 0;
            for (int i = 0; i < grid.amount_of_nodes; i++) {
                float tmp = (float)Math.Sqrt(arr[i].X * arr[i].X + arr[i].Y * arr[i].Y + arr[i].Z * arr[i].Z);
                if (tmp < eps) {
                    nodes[length++] = grid.t0 + grid.time_step * i;
                }
            }
            float[] ans = new float[length];

            Array.Copy(nodes, ans, length);
            return ans;
        }

        public override string ToString() {
            return "V1DataOnGrid" + " " + base.ToString() + " " + grid.ToString();
        }

        public override string ToLongString() {
            string str = this.ToString() + Environment.NewLine;
            for (int i = 0; i < grid.amount_of_nodes; i++) {
                str += arr[i].ToString() + Environment.NewLine;
            }
            return str;
        }

        public Grid grid { get; set; }
        public Vector3[] arr { get; set; }
    }
}
