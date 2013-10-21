namespace QiNiuDrive
{
    partial class FrmFilterTester
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
            this.lsvFiles = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // lsvFiles
            // 
            this.lsvFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.lsvFiles.FullRowSelect = true;
            this.lsvFiles.Location = new System.Drawing.Point(6, 76);
            this.lsvFiles.Name = "lsvFiles";
            this.lsvFiles.ShowItemToolTips = true;
            this.lsvFiles.Size = new System.Drawing.Size(550, 345);
            this.lsvFiles.TabIndex = 0;
            this.lsvFiles.UseCompatibleStateImageBehavior = false;
            this.lsvFiles.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "文件名";
            this.columnHeader1.Width = 378;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "过滤";
            this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader2.Width = 50;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "应用规则";
            this.columnHeader3.Width = 100;
            // 
            // FrmFilterTester
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(563, 471);
            this.Controls.Add(this.lsvFiles);
            this.Font = new System.Drawing.Font("Microsoft YaHei", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FrmFilterTester";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FrmFilterTester";
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.FrmFilterTester_MouseClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FrmFilterTester_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.FrmFilterTester_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FrmFilterTester_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lsvFiles;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
    }
}