namespace SimpleCrawler
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.lblWebSite = new System.Windows.Forms.Label();
            this.txtWebSite = new System.Windows.Forms.TextBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.dataGridViewTitleURL = new System.Windows.Forms.DataGridView();
            this.ColumnTitle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnURL = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnStopRoll = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTitleURL)).BeginInit();
            this.SuspendLayout();
            // 
            // lblWebSite
            // 
            this.lblWebSite.AutoSize = true;
            this.lblWebSite.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblWebSite.Location = new System.Drawing.Point(12, 13);
            this.lblWebSite.Name = "lblWebSite";
            this.lblWebSite.Size = new System.Drawing.Size(114, 20);
            this.lblWebSite.TabIndex = 0;
            this.lblWebSite.Text = "网站域名：";
            // 
            // txtWebSite
            // 
            this.txtWebSite.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtWebSite.Location = new System.Drawing.Point(132, 13);
            this.txtWebSite.Name = "txtWebSite";
            this.txtWebSite.Size = new System.Drawing.Size(931, 25);
            this.txtWebSite.TabIndex = 1;
            // 
            // btnStart
            // 
            this.btnStart.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStart.Location = new System.Drawing.Point(12, 47);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(110, 29);
            this.btnStart.TabIndex = 2;
            this.btnStart.Text = "开始抓取";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // dataGridViewTitleURL
            // 
            this.dataGridViewTitleURL.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewTitleURL.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewTitleURL.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnTitle,
            this.ColumnURL});
            this.dataGridViewTitleURL.Location = new System.Drawing.Point(12, 83);
            this.dataGridViewTitleURL.Name = "dataGridViewTitleURL";
            this.dataGridViewTitleURL.RowHeadersWidth = 80;
            this.dataGridViewTitleURL.RowTemplate.Height = 27;
            this.dataGridViewTitleURL.Size = new System.Drawing.Size(1051, 459);
            this.dataGridViewTitleURL.TabIndex = 3;
            this.dataGridViewTitleURL.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.dataGridViewTitleURL_RowsAdded);
            // 
            // ColumnTitle
            // 
            this.ColumnTitle.HeaderText = "标题";
            this.ColumnTitle.Name = "ColumnTitle";
            this.ColumnTitle.ReadOnly = true;
            this.ColumnTitle.Width = 450;
            // 
            // ColumnURL
            // 
            this.ColumnURL.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColumnURL.HeaderText = "URL";
            this.ColumnURL.Name = "ColumnURL";
            this.ColumnURL.ReadOnly = true;
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStop.Location = new System.Drawing.Point(132, 48);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(110, 29);
            this.btnStop.TabIndex = 2;
            this.btnStop.Text = "停止抓取";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnStopRoll
            // 
            this.btnStopRoll.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStopRoll.Location = new System.Drawing.Point(260, 48);
            this.btnStopRoll.Name = "btnStopRoll";
            this.btnStopRoll.Size = new System.Drawing.Size(110, 29);
            this.btnStopRoll.TabIndex = 2;
            this.btnStopRoll.Text = "停止滚动";
            this.btnStopRoll.UseVisualStyleBackColor = true;
            this.btnStopRoll.Click += new System.EventHandler(this.btnStopRoll_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1075, 554);
            this.Controls.Add(this.dataGridViewTitleURL);
            this.Controls.Add(this.btnStopRoll);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.txtWebSite);
            this.Controls.Add(this.lblWebSite);
            this.Name = "MainForm";
            this.Text = "SimpleCrawler";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTitleURL)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblWebSite;
        private System.Windows.Forms.TextBox txtWebSite;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.DataGridView dataGridViewTitleURL;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnURL;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnTitle;
        private System.Windows.Forms.Button btnStopRoll;
    }
}

