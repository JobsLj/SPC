﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace SPC.Analysis.ResultControls
{
    public partial class ContourPlotResultControl : DevExpress.XtraEditors.XtraUserControl
    {
        public ContourPlotResultControl()
        {
            InitializeComponent();
        }
        public void Init(Image pic)
        {
            this.pictureEdit1.Image = pic;
        }
    }
}
