﻿using System;

namespace FieldLibrary
{
    public class DataChangedEventArgs {
        public DataChangedEventArgs(ChangeInfo change_, string info_) {
            Change = change_;
            info = info_;
        }   
        public ChangeInfo Change { get; set; }

        public string info { get; set; }

        public override string ToString() {
            return "ChangeInfo: " + Change.ToString() + Environment.NewLine + info.ToString();
        }
    }
}
