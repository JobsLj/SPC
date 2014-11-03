﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace SPC.Monitor.DrawBoards
{
    public partial class SampleXYRelationDrawBoard : DevExpress.XtraEditors.XtraUserControl,IDrawBoard
    {
        public SampleXYRelationDrawBoard()
        {
            InitializeComponent();
            this.barCheckItem1.Checked = true;
        }
        public Control GetChart()
        {
            return this.chartControl1;
        }
        protected override void OnControlRemoved(ControlEventArgs e)
        {
            if (this.Parent!=null)
                this.Parent.Controls.Remove(this);
        }

        private void repositoryItemToggleSwitch1_EditValueChanging(object sender, DevExpress.XtraEditors.Controls.ChangingEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                foreach (DevExpress.XtraCharts.Series s in this.chartControl1.Series)
                {
                    s.ChangeView(DevExpress.XtraCharts.ViewType.Line);

                    (s.View as DevExpress.XtraCharts.LineSeriesView).LineStyle.Thickness = 1;
                    this.barEditItem1.Caption = "折线";
                }
            }
            else
            {
                foreach (DevExpress.XtraCharts.Series s in this.chartControl1.Series)
                {
                    s.ChangeView(DevExpress.XtraCharts.ViewType.Point);
                    (s.View as DevExpress.XtraCharts.PointSeriesView).PointMarkerOptions.Size = 4;
                    this.barEditItem1.Caption = "散点";
                }
            }
        }


        private void chartControl1_CustomShowRightClickPopupMenu(object sender, Base.Control.AdvChartControl.ShowRightClickPopupMenuEventArgs e)
        {
            e.RightClickPopupMenu.AddItem(this.barEditItem1);
            e.RightClickPopupMenu.AddItem(this.barCheckItem1);
            e.Handle = true;
        }

        private void barCheckItem1_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if ((e.Item as DevExpress.XtraBars.BarCheckItem).Checked)
                this.chartControl1.Legend.Visible = true;
            else
                this.chartControl1.Legend.Visible = false;
        }

    }
}