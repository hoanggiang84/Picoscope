namespace SAM.Picoscope
{
    partial class DemoForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
            this.tbStatus = new System.Windows.Forms.TextBox();
            this.cbbDevices = new System.Windows.Forms.ComboBox();
            this.btnGetData = new System.Windows.Forms.Button();
            this.graphData = new ZedGraph.ZedGraphControl();
            this.tbFrequency = new System.Windows.Forms.TextBox();
            this.cbbSignalType = new System.Windows.Forms.ComboBox();
            this.cbbSampleRate = new System.Windows.Forms.ComboBox();
            this.timerStreamData = new System.Windows.Forms.Timer(this.components);
            this.toolTipCommon = new System.Windows.Forms.ToolTip(this.components);
            this.timerCountBlocks = new System.Windows.Forms.Timer(this.components);
            this.tableLayoutPanelMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanelMain
            // 
            this.tableLayoutPanelMain.ColumnCount = 6;
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33444F));
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33444F));
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33112F));
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanelMain.Controls.Add(this.tbStatus, 2, 6);
            this.tableLayoutPanelMain.Controls.Add(this.cbbDevices, 1, 6);
            this.tableLayoutPanelMain.Controls.Add(this.btnGetData, 1, 1);
            this.tableLayoutPanelMain.Controls.Add(this.graphData, 2, 1);
            this.tableLayoutPanelMain.Controls.Add(this.tbFrequency, 1, 2);
            this.tableLayoutPanelMain.Controls.Add(this.cbbSignalType, 1, 3);
            this.tableLayoutPanelMain.Controls.Add(this.cbbSampleRate, 1, 4);
            this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelMain.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            this.tableLayoutPanelMain.RowCount = 8;
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelMain.Size = new System.Drawing.Size(624, 442);
            this.tableLayoutPanelMain.TabIndex = 0;
            // 
            // tbStatus
            // 
            this.tbStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanelMain.SetColumnSpan(this.tbStatus, 3);
            this.tbStatus.Location = new System.Drawing.Point(123, 392);
            this.tbStatus.Name = "tbStatus";
            this.tbStatus.Size = new System.Drawing.Size(452, 20);
            this.tbStatus.TabIndex = 1;
            this.tbStatus.Text = "PicoScope 6000 Series Driver Example Program";
            // 
            // cbbDevices
            // 
            this.cbbDevices.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cbbDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbDevices.FormattingEnabled = true;
            this.cbbDevices.Location = new System.Drawing.Point(23, 391);
            this.cbbDevices.Name = "cbbDevices";
            this.cbbDevices.Size = new System.Drawing.Size(94, 21);
            this.cbbDevices.TabIndex = 3;
            this.cbbDevices.DropDown += new System.EventHandler(this.cbbDevices_DropDown);
            this.cbbDevices.SelectedIndexChanged += new System.EventHandler(this.cbbDevices_SelectedIndexChanged);
            // 
            // btnGetData
            // 
            this.btnGetData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGetData.Location = new System.Drawing.Point(23, 26);
            this.btnGetData.Name = "btnGetData";
            this.btnGetData.Size = new System.Drawing.Size(94, 23);
            this.btnGetData.TabIndex = 4;
            this.btnGetData.Text = "Get Data";
            this.btnGetData.UseVisualStyleBackColor = true;
            this.btnGetData.Click += new System.EventHandler(this.btnGetData_Click);
            // 
            // graphData
            // 
            this.tableLayoutPanelMain.SetColumnSpan(this.graphData, 3);
            this.graphData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphData.Location = new System.Drawing.Point(123, 23);
            this.graphData.Name = "graphData";
            this.tableLayoutPanelMain.SetRowSpan(this.graphData, 5);
            this.graphData.ScrollGrace = 0D;
            this.graphData.ScrollMaxX = 0D;
            this.graphData.ScrollMaxY = 0D;
            this.graphData.ScrollMaxY2 = 0D;
            this.graphData.ScrollMinX = 0D;
            this.graphData.ScrollMinY = 0D;
            this.graphData.ScrollMinY2 = 0D;
            this.graphData.Size = new System.Drawing.Size(452, 356);
            this.graphData.TabIndex = 5;
            this.graphData.UseExtendedPrintDialog = true;
            // 
            // tbFrequency
            // 
            this.tbFrequency.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFrequency.Location = new System.Drawing.Point(23, 62);
            this.tbFrequency.Name = "tbFrequency";
            this.tbFrequency.Size = new System.Drawing.Size(94, 20);
            this.tbFrequency.TabIndex = 6;
            this.tbFrequency.Text = "5000";
            this.tbFrequency.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTipCommon.SetToolTip(this.tbFrequency, "External trigger frequency");
            this.tbFrequency.TextChanged += new System.EventHandler(this.tbFrequency_TextChanged);
            // 
            // cbbSignalType
            // 
            this.cbbSignalType.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cbbSignalType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbSignalType.FormattingEnabled = true;
            this.cbbSignalType.Items.AddRange(new object[] {
            "Sine",
            "Square",
            "Triangle",
            "Ramp Up\t",
            "RampDown",
            "Sinc",
            "Gaussian",
            "Half Sine",
            "DC Voltage",
            "White Noise",
            "None"});
            this.cbbSignalType.Location = new System.Drawing.Point(23, 97);
            this.cbbSignalType.Name = "cbbSignalType";
            this.cbbSignalType.Size = new System.Drawing.Size(94, 21);
            this.cbbSignalType.TabIndex = 7;
            this.toolTipCommon.SetToolTip(this.cbbSignalType, "Generation signal type");
            this.cbbSignalType.SelectedIndexChanged += new System.EventHandler(this.cbbSignalType_SelectedIndexChanged);
            // 
            // cbbSampleRate
            // 
            this.cbbSampleRate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cbbSampleRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbSampleRate.FormattingEnabled = true;
            this.cbbSampleRate.Items.AddRange(new object[] {
            "5G",
            "2.5G",
            "1.25G",
            "625M",
            "312.5M"});
            this.cbbSampleRate.Location = new System.Drawing.Point(23, 132);
            this.cbbSampleRate.Name = "cbbSampleRate";
            this.cbbSampleRate.Size = new System.Drawing.Size(94, 21);
            this.cbbSampleRate.TabIndex = 8;
            this.toolTipCommon.SetToolTip(this.cbbSampleRate, "Sample rate");
            this.cbbSampleRate.SelectedIndexChanged += new System.EventHandler(this.cbbSampleRate_SelectedIndexChanged);
            // 
            // timerStreamData
            // 
            this.timerStreamData.Interval = 10;
            this.timerStreamData.Tick += new System.EventHandler(this.timerStreamData_Tick);
            // 
            // timerCountBlocks
            // 
            this.timerCountBlocks.Interval = 1;
            this.timerCountBlocks.Tick += new System.EventHandler(this.timerCountBlocks_Tick);
            // 
            // DemoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 442);
            this.Controls.Add(this.tableLayoutPanelMain);
            this.MinimumSize = new System.Drawing.Size(640, 480);
            this.Name = "DemoForm";
            this.Text = "Picoscope";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DemoForm_FormClosing);
            this.tableLayoutPanelMain.ResumeLayout(false);
            this.tableLayoutPanelMain.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
        private System.Windows.Forms.TextBox tbStatus;
        private System.Windows.Forms.ComboBox cbbDevices;
        private System.Windows.Forms.Button btnGetData;
        private ZedGraph.ZedGraphControl graphData;
        private System.Windows.Forms.TextBox tbFrequency;
        private System.Windows.Forms.ComboBox cbbSignalType;
        private System.Windows.Forms.Timer timerStreamData;
        private System.Windows.Forms.ToolTip toolTipCommon;
        private System.Windows.Forms.ComboBox cbbSampleRate;
        private System.Windows.Forms.Timer timerCountBlocks;
    }
}

