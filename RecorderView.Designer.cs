namespace Capture
{
    partial class RecorderView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RecorderView));
            this.btnDeleteAll = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cboLogLevelsTCP = new System.Windows.Forms.ComboBox();
            this.cboLogLevelsDisplays = new System.Windows.Forms.ComboBox();
            this.btnLaunchWebDemo = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelLastBrfID = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelMarketOpen = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelEventCt = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.panel1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnDeleteAll
            // 
            this.btnDeleteAll.Location = new System.Drawing.Point(3, 6);
            this.btnDeleteAll.Name = "btnDeleteAll";
            this.btnDeleteAll.Size = new System.Drawing.Size(123, 25);
            this.btnDeleteAll.TabIndex = 21;
            this.btnDeleteAll.Text = "Delete All";
            this.toolTip1.SetToolTip(this.btnDeleteAll, "This will delete everything from the StreamMoments table.");
            this.btnDeleteAll.UseVisualStyleBackColor = true;
            this.btnDeleteAll.Click += new System.EventHandler(this.btnDeleteAll_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.cboLogLevelsTCP);
            this.panel1.Controls.Add(this.cboLogLevelsDisplays);
            this.panel1.Controls.Add(this.btnLaunchWebDemo);
            this.panel1.Controls.Add(this.statusStrip1);
            this.panel1.Controls.Add(this.btnDeleteAll);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 504);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1022, 82);
            this.panel1.TabIndex = 23;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(183, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 13);
            this.label2.TabIndex = 28;
            this.label2.Text = "TCP Logging Level:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(170, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 13);
            this.label1.TabIndex = 28;
            this.label1.Text = "Display Logging Level:";
            // 
            // cboLogLevelsTCP
            // 
            this.cboLogLevelsTCP.FormattingEnabled = true;
            this.cboLogLevelsTCP.Items.AddRange(new object[] {
            "Trace",
            "Debug",
            "Info",
            "Warn",
            "Error",
            "Fatal",
            "Off"});
            this.cboLogLevelsTCP.Location = new System.Drawing.Point(290, 36);
            this.cboLogLevelsTCP.Name = "cboLogLevelsTCP";
            this.cboLogLevelsTCP.Size = new System.Drawing.Size(167, 21);
            this.cboLogLevelsTCP.TabIndex = 27;
            this.cboLogLevelsTCP.Tag = "TcpOutlet";
            this.cboLogLevelsTCP.Text = "Info";
            this.cboLogLevelsTCP.SelectedIndexChanged += new System.EventHandler(this.cboLogLevels_SelectedIndexChanged);
            // 
            // cboLogLevelsDisplays
            // 
            this.cboLogLevelsDisplays.FormattingEnabled = true;
            this.cboLogLevelsDisplays.Items.AddRange(new object[] {
            "Trace",
            "Debug",
            "Info",
            "Warn",
            "Error",
            "Fatal",
            "Off"});
            this.cboLogLevelsDisplays.Location = new System.Drawing.Point(290, 9);
            this.cboLogLevelsDisplays.Name = "cboLogLevelsDisplays";
            this.cboLogLevelsDisplays.Size = new System.Drawing.Size(167, 21);
            this.cboLogLevelsDisplays.TabIndex = 26;
            this.cboLogLevelsDisplays.Tag = "display";
            this.cboLogLevelsDisplays.Text = "Info";
            this.cboLogLevelsDisplays.SelectedIndexChanged += new System.EventHandler(this.cboLogLevels_SelectedIndexChanged);
            // 
            // btnLaunchWebDemo
            // 
            this.btnLaunchWebDemo.Location = new System.Drawing.Point(3, 33);
            this.btnLaunchWebDemo.Name = "btnLaunchWebDemo";
            this.btnLaunchWebDemo.Size = new System.Drawing.Size(123, 25);
            this.btnLaunchWebDemo.TabIndex = 25;
            this.btnLaunchWebDemo.Text = "Launch Web Demo";
            this.btnLaunchWebDemo.UseVisualStyleBackColor = true;
            this.btnLaunchWebDemo.Click += new System.EventHandler(this.btnLaunchWebDemo_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabelLastBrfID,
            this.toolStripStatusLabel2,
            this.toolStripStatusLabelMarketOpen,
            this.toolStripStatusLabel3,
            this.toolStripStatusLabelEventCt});
            this.statusStrip1.Location = new System.Drawing.Point(0, 60);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1022, 22);
            this.statusStrip1.TabIndex = 24;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(107, 17);
            this.toolStripStatusLabel1.Text = "StreamMoment ID:";
            // 
            // toolStripStatusLabelLastBrfID
            // 
            this.toolStripStatusLabelLastBrfID.AutoSize = false;
            this.toolStripStatusLabelLastBrfID.Name = "toolStripStatusLabelLastBrfID";
            this.toolStripStatusLabelLastBrfID.Size = new System.Drawing.Size(180, 17);
            this.toolStripStatusLabelLastBrfID.Text = "---";
            this.toolStripStatusLabelLastBrfID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(79, 17);
            this.toolStripStatusLabel2.Text = "Market Open:";
            // 
            // toolStripStatusLabelMarketOpen
            // 
            this.toolStripStatusLabelMarketOpen.AutoSize = false;
            this.toolStripStatusLabelMarketOpen.Name = "toolStripStatusLabelMarketOpen";
            this.toolStripStatusLabelMarketOpen.Size = new System.Drawing.Size(100, 17);
            this.toolStripStatusLabelMarketOpen.Text = "---";
            this.toolStripStatusLabelMarketOpen.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(44, 17);
            this.toolStripStatusLabel3.Text = "Events:";
            // 
            // toolStripStatusLabelEventCt
            // 
            this.toolStripStatusLabelEventCt.AutoSize = false;
            this.toolStripStatusLabelEventCt.Name = "toolStripStatusLabelEventCt";
            this.toolStripStatusLabelEventCt.Size = new System.Drawing.Size(128, 17);
            this.toolStripStatusLabelEventCt.Text = "---";
            this.toolStripStatusLabelEventCt.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "Market Recorder";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.Click += new System.EventHandler(this.notifyIcon1_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Location = new System.Drawing.Point(0, 0);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(1022, 504);
            this.richTextBox1.TabIndex = 24;
            this.richTextBox1.Text = "";
            // 
            // RecorderView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1022, 586);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RecorderView";
            this.Text = "Market Recorder";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Recorder_FormClosed);
            this.Load += new System.EventHandler(this.Capture_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnDeleteAll;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Button btnLaunchWebDemo;
        public System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelMarketOpen;
        public System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelEventCt;
        public System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelLastBrfID;
        private System.Windows.Forms.ComboBox cboLogLevelsDisplays;
        private System.Windows.Forms.ComboBox cboLogLevelsTCP;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
    }
}

