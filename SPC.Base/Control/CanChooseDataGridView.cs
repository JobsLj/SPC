﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.Threading.Tasks;
using System.Data;
using System.Reflection;

namespace SPC.Base.Control
{
    public class CanChooseDataGridView : DevExpress.XtraGrid.Views.Grid.GridView
    {
        private string _ChooseColumnName = "choose";
        public string ChooseColumnName
        {
            get
            {
                return this._ChooseColumnName;
            }
            set
            {
                if(this._ChooseColumnName!=value)
                {
                    this._ChooseColumnName = value;
                    InitSummary();
                }
            }
        }
        private int _ChooseCount = 0;
        private int ChooseCount
        {
            get
            {
                return _ChooseCount;
            }
            set
            {
                this._ChooseCount = value;
            }
        }
        private bool _AllowChoose;
        [DefaultValue(true)]
        public bool AllowChoose
        {
            get
            {
                return this._AllowChoose;
            }
            set
            {
                if (this._AllowChoose != value)
                {
                    if (value)
                        this._AllowChoose = true;
                    else if (!this.AutoMode)
                    {
                        this._AllowChoose = false;
                        this.AllowChooseGroup = false;
                    }
                }
            }
        }
        private bool _AllowChooseGroup;
        [DefaultValue(true)]
        public bool AllowChooseGroup
        {
            get
            {
                return _AllowChooseGroup;
            }
            set
            {
                if (this._AllowChooseGroup != value)
                {
                    if (value)
                    {
                        if (AllowChoose)
                        {
                            this._AllowChooseGroup = true;
                        }
                    }
                    else
                    {
                        this._AllowChooseGroup = false;
                    }
                }
            }
        }
        private bool _AutoMode;
        [DefaultValue(true)]
        public bool AutoMode
        {
            get
            {
                return this._AutoMode;
            }
            set
            {
                if (value != this._AutoMode)
                {
                    this._AutoMode = value;
                    if (value)
                        this.AllowChoose = true;
                }
            }
        }
        private bool _AutoRefresh = true;
        [DefaultValue(true)]
        public bool AutoRefresh
        {
            get
            {
                return this._AutoRefresh;
            }
            set
            {
                if (value != this._AutoRefresh)
                {
                    this._AutoRefresh = value;
                }
            }
        }
        public event EventHandler ChooseChanged;
        private ContextMenuStrip gridViewRightClickPopupMenu = new ContextMenuStrip();
        public CanChooseDataGridView():base()
        {
            this.AutoMode = true;
            this.AllowChooseGroup = true;
            this.AutoRefresh = true;

            this.InitChooseRelationControls();

            this.DataSourceChanged -= DataSourceChangeEventMethod;
            this.DataSourceChanged += DataSourceChangeEventMethod;

            this.CustomDrawGroupRow -= DrawChooseGroupRowEventMethod;
            this.CustomDrawGroupRow += DrawChooseGroupRowEventMethod;


            this.CustomSummaryCalculate -= ChooseSummaryCalculate;
            this.CustomSummaryCalculate += ChooseSummaryCalculate;

            this.Click -= ChooseViewClickEventMethod;
            this.Click += ChooseViewClickEventMethod;

            this.CustomDrawColumnHeader -= CustomDrawChooseColumnHeaderEventMethod;
            this.CustomDrawColumnHeader += CustomDrawChooseColumnHeaderEventMethod;

            this.MouseDown -= MouseDownEventMethod;
            this.MouseDown += MouseDownEventMethod;
            
        }
        private void DataSourceChangeEventMethod(object sender,EventArgs e)
        {
            this.InitData(sender);
        }
        private object CurrentDataVector;
        public Type CurrentDataVectorType;
        public string CurrentDataUpdateEventName;
        
        public void InitData(object sender)
        {
            DevExpress.XtraGrid.Columns.GridColumn temp;
            if (this.AutoMode || this.AllowChoose)
            {
                if ((temp = this.Columns.ColumnByFieldName(ChooseColumnName)) != null)
                {
                    temp.VisibleIndex = 0;
                    temp.OptionsColumn.AllowEdit = false;
                    if(temp.Summary.IndexOf(ChooseNeedSummary)<0)
                        temp.Summary.Add(ChooseNeedSummary);
                }
                else if (!this.AutoMode)
                {
                    var choosecolumn = new DevExpress.XtraGrid.Columns.GridColumn();
                    choosecolumn.Name = ChooseColumnName;
                    choosecolumn.FieldName = ChooseColumnName;
                    choosecolumn.VisibleIndex = 0;
                    choosecolumn.OptionsColumn.AllowEdit = false;
                    this.Columns.Insert(0, choosecolumn);
                    choosecolumn.Summary.Add(ChooseNeedSummary);
                }
            }
            if (AllowChooseGroup && this.GroupSummary.IndexOf(GroupChooseNeedSummary) < 0)
                this.GroupSummary.Add(GroupChooseNeedSummary);
            else if (!AllowChooseGroup && this.GroupSummary.IndexOf(GroupChooseNeedSummary) >= 0)
                this.GroupSummary.Remove(GroupChooseNeedSummary);
            //if(this.CurrentDataVector!=null)
            //{
            //    CurrentDataVectorType.GetEvent(CurrentDataUpdateEventName).RemoveEventHandler(CurrentDataVector, new DataRowChangeEventHandler(DataValueChangedEventMethod));
            //    CurrentDataVectorType = null;
            //    CurrentDataVector = null;
            //    CurrentDataUpdateEventName = null;
            //}
            //GetDataVector(this.GridControl);
            //if (CurrentDataVector != null)
            //    CurrentDataVectorType.GetEvent(CurrentDataUpdateEventName).AddEventHandler(CurrentDataVector, new DataRowChangeEventHandler(DataValueChangedEventMethod));
        }
        private void GetDataVector(object target)
        {
            if(target==null)
                return;
            if(target is DataTable)
            {
                this.CurrentDataVector = target;
                this.CurrentDataVectorType = typeof(DataTable);
                this.CurrentDataUpdateEventName = "RowChanged";
            }
            else 
            {
                var datasource = target.GetType().GetProperty("DataSource").GetValue(target,null);
                var datamember = target.GetType().GetProperty("DataMember").GetValue(target,null);
                if (datasource != null && datamember != null && datamember.ToString().Trim() != "" && datasource.GetType().GetProperty("Tables")!=null)
                    GetDataVector(datasource.GetType().GetProperty("Tables").GetValue(datasource,new object[]{datamember}));
                else if (datasource != null)
                    GetDataVector(datasource);
            } 
        }
        private void InitChooseRelationControls()
        {
            InitPopMenu();
            InitSummary();
        }
        public void InitPopMenu()
        {
            this.gridViewRightClickPopupMenu.ShowImageMargin = false;
            this.gridViewRightClickPopupMenu.RenderMode = ToolStripRenderMode.Professional;
            this.gridViewRightClickPopupMenu.Items.Add(new ToolStripButton("选       择", Resource.CheckMark_Glyph, this.MultiChooseButtonClick));
            this.gridViewRightClickPopupMenu.Items.Add(new ToolStripButton("取消选择", Resource.CrossMark_Glyph, this.MultiUnChooseButtonClick));
        }
        public void InitSummary()
        {
            GroupChooseNeedSummary = new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Custom, ChooseColumnName, null, "Count:{0}");
            ChooseNeedSummary = new DevExpress.XtraGrid.GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Custom, "", "{0}");
        }
        private void DataValueChangedEventMethod(object sender, EventArgs e)
        {
            if(this.AutoRefresh)
                this.RefreshChoose();
        }
        public void RefreshChoose()
        {
            this.UpdateSummary();
        }
        private void MultiChooseButtonClick(object sender,EventArgs e)
        {
            int[] index = this.GetSelectedRows();
            this.BeginDataUpdate();
            foreach(var i in index)
            {
                this.SetChooseRow(i,true);
            }
            this.EndDataUpdate();
        }
        private void MultiUnChooseButtonClick(object sender, EventArgs e)
        {
            int[] index = this.GetSelectedRows();
            this.BeginDataUpdate();
            foreach (var i in index)
            {
                this.SetChooseRow(i,false);
            }
            this.EndDataUpdate();
        }
        public void SetChooseRow(int index,bool result)
        {
            this.SetRowCellValue(index, ChooseColumnName, result);
        }
        private DevExpress.XtraGrid.GridGroupSummaryItem GroupChooseNeedSummary;
        private DevExpress.XtraGrid.GridColumnSummaryItem ChooseNeedSummary;
        private void CustomDrawChooseColumnHeaderEventMethod(object sender, DevExpress.XtraGrid.Views.Grid.ColumnHeaderCustomDrawEventArgs e)
        {
            if (this.AllowChoose && e.Column != null && e.Column.FieldName == ChooseColumnName)
            {
                e.Info.InnerElements.Clear();
                e.Info.Caption = "";
                e.Painter.DrawObject(e.Info);
                var repositoryCheck = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
                repositoryCheck.Caption = "";
                var info = repositoryCheck.CreateViewInfo() as DevExpress.XtraEditors.ViewInfo.CheckEditViewInfo;
                var painter = repositoryCheck.CreatePainter() as DevExpress.XtraEditors.Drawing.CheckEditPainter;
                if (this.ChooseCount==this.DataRowCount)
                    info.EditValue = true;
                else if (this.ChooseCount > 0)
                    info.EditValue = null;
                else
                    info.EditValue = false;
                info.Bounds = e.Bounds;
                info.CalcViewInfo(e.Graphics);
                var args = new DevExpress.XtraEditors.Drawing.ControlGraphicsInfoArgs(info, new DevExpress.Utils.Drawing.GraphicsCache(e.Graphics), e.Bounds);
                painter.Draw(args);
                args.Cache.Dispose();
                e.Handled = true;
            }

        }
        private MouseButtons currentGridViewMouseButton;
        private void MouseDownEventMethod(object sender, MouseEventArgs e)
        {
            this.currentGridViewMouseButton = e.Button;
        }
        private void ChooseViewClickEventMethod(object sender, EventArgs e)
        {
            //this.ClearSorting();
            if (this.AllowChoose && this.DataRowCount > 0 && this.GridControl != null)
            {
                if (this.currentGridViewMouseButton == System.Windows.Forms.MouseButtons.Left)
                {
                    this.PostEditor();
                    DevExpress.XtraGrid.Views.Grid.ViewInfo.GridHitInfo info;
                    Point pt = this.GridControl.PointToClient(System.Windows.Forms.Control.MousePosition);
                    info = this.CalcHitInfo(pt);
                    if (info.InColumn && info.Column != null && info.Column.FieldName == ChooseColumnName)
                    {
                        bool re = !(this.ChooseCount==this.DataRowCount);
                        this.BeginDataUpdate();
                        /*Parallel.For(0, this.DataRowCount, (i) => */for(int i = 0; i < this.DataRowCount; i++)
                        {
                            this.SetChooseRow(i, re);
                        };
                        this.EndDataUpdate();
                        if (ChooseChanged != null)
                            this.ChooseChanged(this, new EventArgs());
                    }
                    else if (this.AllowChooseGroup && info.RowHandle < 0 && groupCheckBoxPosition - info.HitPoint.X < 15)
                    {
                        int start = this.GetDataRowHandleByGroupRowHandle(info.RowHandle);
                        int end = ((GroupSummaryDataType)this.GetGroupSummaryValue(info.RowHandle, GroupChooseNeedSummary)).end;
                        int count = ((GroupSummaryDataType)this.GetGroupSummaryValue(info.RowHandle, GroupChooseNeedSummary)).count;
                        if (count < end - start + 1)
                        {
                            this.BeginDataUpdate();
                            for (int i = start; i <= end; i++)
                            {
                                bool re = Convert.ToBoolean(this.GetRowCellValue(i, ChooseColumnName));
                                if (!re)
                                    this.SetChooseRow(i,true);
                            }
                            this.EndDataUpdate();
                            if (ChooseChanged != null)
                                this.ChooseChanged(this, new EventArgs());
                        }
                        else
                        {
                            this.BeginDataUpdate();
                            for (int i = start; i <= end; i++)
                                this.SetChooseRow(i,false);
                            this.EndDataUpdate();
                            if (ChooseChanged != null)
                                this.ChooseChanged(this, new EventArgs());
                        }
                    }
                    else if (info.Column != null && info.Column.FieldName == ChooseColumnName && info.RowHandle >= 0)
                    {
                        this.BeginDataUpdate();
                        bool re = Convert.ToBoolean(this.GetRowCellValue(info.RowHandle, ChooseColumnName));
                        if (re)
                            this.SetChooseRow(info.RowHandle,false);
                        else
                            this.SetChooseRow(info.RowHandle,true);
                        this.EndDataUpdate();
                        if (ChooseChanged != null) 
                            this.ChooseChanged(this,new EventArgs());
                    }
                }
                else if (this.OptionsSelection.MultiSelect && this.OptionsSelection.MultiSelectMode == DevExpress.XtraGrid.Views.Grid.GridMultiSelectMode.RowSelect && this.currentGridViewMouseButton == System.Windows.Forms.MouseButtons.Right && this.SelectedRowsCount > 0)
                {
                    this.gridViewRightClickPopupMenu.Show(System.Windows.Forms.Control.MousePosition);
                }
            }
        }
        private int groupCheckBoxPosition = int.MaxValue;

        private void DrawChooseGroupRowEventMethod(object sender, DevExpress.XtraGrid.Views.Base.RowObjectCustomDrawEventArgs e)
        {
            e.Painter.DrawObject(e.Info);
            if (this.AllowChooseGroup)
            {
                var repositoryCheck = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
                repositoryCheck.Caption = "";
                repositoryCheck.GlyphAlignment = DevExpress.Utils.HorzAlignment.Far;
                var info = repositoryCheck.CreateViewInfo() as DevExpress.XtraEditors.ViewInfo.CheckEditViewInfo;
                var painter = repositoryCheck.CreatePainter() as DevExpress.XtraEditors.Drawing.CheckEditPainter;
                int start = this.GetDataRowHandleByGroupRowHandle(e.RowHandle);
                int end = ((GroupSummaryDataType)this.GetGroupSummaryValue(e.RowHandle,GroupChooseNeedSummary)).end;
                int count = ((GroupSummaryDataType)this.GetGroupSummaryValue(e.RowHandle, GroupChooseNeedSummary)).count;
                if (count == end - start + 1)
                    info.EditValue = true;
                else if (count == 0)
                    info.EditValue = false;
                else
                    info.EditValue = null;
                info.Bounds = e.Bounds;
                info.CalcViewInfo(e.Graphics);
                groupCheckBoxPosition = e.Bounds.Left + e.Bounds.Width;
                var args = new DevExpress.XtraEditors.Drawing.ControlGraphicsInfoArgs(info, new DevExpress.Utils.Drawing.GraphicsCache(e.Graphics), e.Bounds);
                painter.Draw(args);
                args.Cache.Dispose();
            }
            e.Handled = true;
        }
        private int TotalCount = 0;
        private int TempGroupCount = 0;
        private void ChooseSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            if (this.AllowChooseGroup&& e.Item == this.GroupChooseNeedSummary)
            {
                if (e.SummaryProcess == DevExpress.Data.CustomSummaryProcess.Start)
                {
                    TempGroupCount = 0;
                }
                else if (e.SummaryProcess == DevExpress.Data.CustomSummaryProcess.Calculate && Convert.ToBoolean(this.GetRowCellValue(e.RowHandle, ChooseColumnName)))
                {
                    TempGroupCount++;
                }
                else if (e.SummaryProcess == DevExpress.Data.CustomSummaryProcess.Finalize)
                {
                    e.TotalValue = new GroupSummaryDataType(TempGroupCount,e.RowHandle);
                }
            }
            else if(this.AllowChoose&&e.Item == this.ChooseNeedSummary)
            {
                if (e.SummaryProcess == DevExpress.Data.CustomSummaryProcess.Start)
                {
                    this.TotalCount = 0;
                }
                else if (e.SummaryProcess == DevExpress.Data.CustomSummaryProcess.Calculate && Convert.ToBoolean(this.GetRowCellValue(e.RowHandle, ChooseColumnName)))
                {
                    this.TotalCount++;
                }
                else if (e.SummaryProcess == DevExpress.Data.CustomSummaryProcess.Finalize)
                {
                    e.TotalValue = this.TotalCount;
                    this.ChooseCount = this.TotalCount;
                }
            }
        }
        private struct GroupSummaryDataType
        {
            public int count;
            public int end;
            public GroupSummaryDataType(int c,int e)
            {
                this.count = c;
                this.end = e;
            }
            public override string ToString()
            {
                return count.ToString();
            }
        }

    }

}