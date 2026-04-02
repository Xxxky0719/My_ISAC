using System.ComponentModel;
using System.Windows.Forms;

namespace ImageSearchAndCopy
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private IContainer components = null;

        // 原始控件声明
        private TextBox txtSoNumber;
        private TextBox txtStepNumber;
        private TextBox txtSearchFolder;
        private TextBox txtTargetFolder;
        private Button btnBrowseSearch;
        private Button btnBrowseTarget;
        private Button btnExecute;
        private Label lblStatus;
        private Label lblSoNumber;
        private Label lblStepNumber;
        private Label lblSearchFolder;
        private Label lblTargetFolder;
        private ProgressBar progressBar;
        private Button btnCancel;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel toolStripStatusLabel;
        private GroupBox groupBoxLog;
        private ListBox listBoxLog;
        private Label lblLogInfo;
        // CSV相关控件
        private TextBox txtCsvFile;
        private Button btnBrowseCsv;
        private Label lblCsvFile;
        private TextBox txtBatchInput;
        // 新增：操作方式选择控件
        private GroupBox groupBoxOperationMode;
        private RadioButton radioSingleMode;
        private RadioButton radioBatchMode;
        private RadioButton radioMultiMode;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtSoNumber = new TextBox();
            txtStepNumber = new TextBox();
            txtSearchFolder = new TextBox();
            txtTargetFolder = new TextBox();
            btnBrowseSearch = new Button();
            btnBrowseTarget = new Button();
            btnExecute = new Button();
            lblStatus = new Label();
            lblSoNumber = new Label();
            lblStepNumber = new Label();
            lblSearchFolder = new Label();
            lblTargetFolder = new Label();
            progressBar = new ProgressBar();
            btnCancel = new Button();
            statusStrip = new StatusStrip();
            toolStripStatusLabel = new ToolStripStatusLabel();
            groupBoxLog = new GroupBox();
            listBoxLog = new ListBox();
            lblLogInfo = new Label();
            txtCsvFile = new TextBox();
            btnBrowseCsv = new Button();
            lblCsvFile = new Label();
            txtBatchInput = new TextBox();
            groupBoxOperationMode = new GroupBox();
            radioMultiMode = new RadioButton();
            radioBatchMode = new RadioButton();
            radioSingleMode = new RadioButton();
            statusStrip.SuspendLayout();
            groupBoxLog.SuspendLayout();
            groupBoxOperationMode.SuspendLayout();
            SuspendLayout();
            // 
            // txtSoNumber
            // 
            txtSoNumber.Location = new Point(100, 70);
            txtSoNumber.Name = "txtSoNumber";
            txtSoNumber.Size = new Size(150, 23);
            txtSoNumber.TabIndex = 2;
            // 
            // txtStepNumber
            // 
            txtStepNumber.Location = new Point(320, 70);
            txtStepNumber.Name = "txtStepNumber";
            txtStepNumber.Size = new Size(150, 23);
            txtStepNumber.TabIndex = 3;
            // 
            // txtSearchFolder
            // 
            txtSearchFolder.Location = new Point(100, 100);
            txtSearchFolder.Name = "txtSearchFolder";
            txtSearchFolder.ReadOnly = true;
            txtSearchFolder.Size = new Size(300, 23);
            txtSearchFolder.TabIndex = 5;
            // 
            // txtTargetFolder
            // 
            txtTargetFolder.Location = new Point(100, 130);
            txtTargetFolder.Name = "txtTargetFolder";
            txtTargetFolder.ReadOnly = true;
            txtTargetFolder.Size = new Size(300, 23);
            txtTargetFolder.TabIndex = 7;
            // 
            // btnBrowseSearch
            // 
            btnBrowseSearch.Location = new Point(410, 98);
            btnBrowseSearch.Name = "btnBrowseSearch";
            btnBrowseSearch.Size = new Size(75, 28);
            btnBrowseSearch.TabIndex = 6;
            btnBrowseSearch.Text = "浏览";
            btnBrowseSearch.Click += btnBrowseSearch_Click;
            // 
            // btnBrowseTarget
            // 
            btnBrowseTarget.Location = new Point(410, 128);
            btnBrowseTarget.Name = "btnBrowseTarget";
            btnBrowseTarget.Size = new Size(75, 28);
            btnBrowseTarget.TabIndex = 8;
            btnBrowseTarget.Text = "浏览";
            btnBrowseTarget.Click += btnBrowseTarget_Click;
            // 
            // btnExecute
            // 
            btnExecute.Location = new Point(200, 165);
            btnExecute.Name = "btnExecute";
            btnExecute.Size = new Size(100, 34);
            btnExecute.TabIndex = 10;
            btnExecute.Text = "执行搜索复制";
            btnExecute.Click += btnExecute_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(20, 220);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(32, 17);
            lblStatus.TabIndex = 11;
            lblStatus.Text = "就绪";
            // 
            // lblSoNumber
            // 
            lblSoNumber.AutoSize = true;
            lblSoNumber.Location = new Point(20, 74);
            lblSoNumber.Name = "lblSoNumber";
            lblSoNumber.Size = new Size(43, 17);
            lblSoNumber.TabIndex = 1;
            lblSoNumber.Text = "PID号:";
            // 
            // lblStepNumber
            // 
            lblStepNumber.AutoSize = true;
            lblStepNumber.Location = new Point(270, 74);
            lblStepNumber.Name = "lblStepNumber";
            lblStepNumber.Size = new Size(35, 17);
            lblStepNumber.TabIndex = 4;
            lblStepNumber.Text = "步数:";
            // 
            // lblSearchFolder
            // 
            lblSearchFolder.AutoSize = true;
            lblSearchFolder.Location = new Point(20, 104);
            lblSearchFolder.Name = "lblSearchFolder";
            lblSearchFolder.Size = new Size(71, 17);
            lblSearchFolder.TabIndex = 9;
            lblSearchFolder.Text = "搜索文件夹:";
            // 
            // lblTargetFolder
            // 
            lblTargetFolder.AutoSize = true;
            lblTargetFolder.Location = new Point(20, 134);
            lblTargetFolder.Name = "lblTargetFolder";
            lblTargetFolder.Size = new Size(71, 17);
            lblTargetFolder.TabIndex = 12;
            lblTargetFolder.Text = "目标文件夹:";
            // 
            // progressBar
            // 
            progressBar.Location = new Point(20, 200);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(460, 23);
            progressBar.TabIndex = 13;
            // 
            // btnCancel
            // 
            btnCancel.Enabled = false;
            btnCancel.Location = new Point(320, 165);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 34);
            btnCancel.TabIndex = 14;
            btnCancel.Text = "取消";
            btnCancel.Click += btnCancel_Click;
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel });
            statusStrip.Location = new Point(0, 523);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(500, 22);
            statusStrip.SizingGrip = false;
            statusStrip.TabIndex = 15;
            statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusLabel
            // 
            toolStripStatusLabel.Name = "toolStripStatusLabel";
            toolStripStatusLabel.Size = new Size(454, 17);
            toolStripStatusLabel.Spring = true;
            toolStripStatusLabel.TextAlign = ContentAlignment.MiddleRight;
            toolStripStatusLabel.Click += toolStripStatusLabel_Click;
            // 
            // groupBoxLog
            // 
            groupBoxLog.Controls.Add(listBoxLog);
            groupBoxLog.Controls.Add(lblLogInfo);
            groupBoxLog.Location = new Point(20, 210);
            groupBoxLog.Name = "groupBoxLog";
            groupBoxLog.Size = new Size(460, 170);
            groupBoxLog.TabIndex = 16;
            groupBoxLog.TabStop = false;
            groupBoxLog.Text = "复制日志";
            // 
            // listBoxLog
            // 
            listBoxLog.FormattingEnabled = true;
            listBoxLog.Location = new Point(10, 23);
            listBoxLog.Name = "listBoxLog";
            listBoxLog.Size = new Size(440, 89);
            listBoxLog.TabIndex = 0;
            listBoxLog.SelectedIndexChanged += listBoxLog_SelectedIndexChanged;
            // 
            // lblLogInfo
            // 
            lblLogInfo.AutoSize = true;
            lblLogInfo.Location = new Point(10, 125);
            lblLogInfo.Name = "lblLogInfo";
            lblLogInfo.Size = new Size(166, 17);
            lblLogInfo.TabIndex = 1;
            lblLogInfo.Text = "显示最近5条记录，共0条记录";
            // 
            // txtCsvFile
            // 
            txtCsvFile.Location = new Point(100, 75);
            txtCsvFile.Name = "txtCsvFile";
            txtCsvFile.ReadOnly = true;
            txtCsvFile.Size = new Size(300, 23);
            txtCsvFile.TabIndex = 18;
            txtCsvFile.Visible = false;
            // 
            // btnBrowseCsv
            // 
            btnBrowseCsv.Location = new Point(410, 73);
            btnBrowseCsv.Name = "btnBrowseCsv";
            btnBrowseCsv.Size = new Size(75, 28);
            btnBrowseCsv.TabIndex = 19;
            btnBrowseCsv.Text = "浏览";
            btnBrowseCsv.Visible = false;
            btnBrowseCsv.Click += btnBrowseCsv_Click;
            // 
            // lblCsvFile
            // 
            lblCsvFile.AutoSize = true;
            lblCsvFile.Location = new Point(20, 79);
            lblCsvFile.Name = "lblCsvFile";
            lblCsvFile.Size = new Size(58, 17);
            lblCsvFile.TabIndex = 20;
            lblCsvFile.Text = "CSV文件:";
            lblCsvFile.Visible = false;
            // 
            // txtBatchInput
            // 
            txtBatchInput.Location = new Point(100, 70);
            txtBatchInput.Multiline = true;
            txtBatchInput.Name = "txtBatchInput";
            txtBatchInput.ScrollBars = ScrollBars.Vertical;
            txtBatchInput.Size = new Size(300, 100);
            txtBatchInput.TabIndex = 22;
            txtBatchInput.Visible = false;
            // 
            // groupBoxOperationMode
            // 
            groupBoxOperationMode.Controls.Add(radioMultiMode);
            groupBoxOperationMode.Controls.Add(radioBatchMode);
            groupBoxOperationMode.Controls.Add(radioSingleMode);
            groupBoxOperationMode.Location = new Point(20, 10);
            groupBoxOperationMode.Name = "groupBoxOperationMode";
            groupBoxOperationMode.Size = new Size(460, 50);
            groupBoxOperationMode.TabIndex = 21;
            groupBoxOperationMode.TabStop = false;
            groupBoxOperationMode.Text = "操作方式";
            // 
            // radioMultiMode
            // 
            radioMultiMode.AutoSize = true;
            radioMultiMode.Location = new Point(280, 20);
            radioMultiMode.Name = "radioMultiMode";
            radioMultiMode.Size = new Size(98, 21);
            radioMultiMode.TabIndex = 2;
            radioMultiMode.Text = "多行输入模式";
            radioMultiMode.UseVisualStyleBackColor = true;
            radioMultiMode.CheckedChanged += OperationModeChanged;
            // 
            // radioBatchMode
            // 
            radioBatchMode.AutoSize = true;
            radioBatchMode.Location = new Point(150, 20);
            radioBatchMode.Name = "radioBatchMode";
            radioBatchMode.Size = new Size(97, 21);
            radioBatchMode.TabIndex = 1;
            radioBatchMode.Text = "CSV导入模式";
            radioBatchMode.UseVisualStyleBackColor = true;
            radioBatchMode.CheckedChanged += OperationModeChanged;
            // 
            // radioSingleMode
            // 
            radioSingleMode.AutoSize = true;
            radioSingleMode.Checked = true;
            radioSingleMode.Location = new Point(20, 20);
            radioSingleMode.Name = "radioSingleMode";
            radioSingleMode.Size = new Size(98, 21);
            radioSingleMode.TabIndex = 0;
            radioSingleMode.TabStop = true;
            radioSingleMode.Text = "单次处理模式";
            radioSingleMode.UseVisualStyleBackColor = true;
            radioSingleMode.CheckedChanged += OperationModeChanged;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(500, 545);
            Controls.Add(groupBoxOperationMode);
            Controls.Add(lblCsvFile);
            Controls.Add(txtCsvFile);
            Controls.Add(txtBatchInput);
            Controls.Add(btnBrowseCsv);
            Controls.Add(groupBoxLog);
            Controls.Add(statusStrip);
            Controls.Add(btnCancel);
            Controls.Add(progressBar);
            Controls.Add(lblSoNumber);
            Controls.Add(txtSoNumber);
            Controls.Add(lblStepNumber);
            Controls.Add(txtStepNumber);
            Controls.Add(lblSearchFolder);
            Controls.Add(txtSearchFolder);
            Controls.Add(btnBrowseSearch);
            Controls.Add(lblTargetFolder);
            Controls.Add(txtTargetFolder);
            Controls.Add(btnBrowseTarget);
            Controls.Add(btnExecute);
            Controls.Add(lblStatus);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Form1";
            Text = "PIDV图片搜索复制工具";
            Load += Form1_Load;
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            groupBoxLog.ResumeLayout(false);
            groupBoxLog.PerformLayout();
            groupBoxOperationMode.ResumeLayout(false);
            groupBoxOperationMode.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}