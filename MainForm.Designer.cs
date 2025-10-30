namespace ModbusSimulationServer
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            label1 = new Label();
            txtFileName = new TextBox();
            bttnBrowse = new Button();
            bttnStart = new Button();
            gvData = new DataGridView();
            tabControl = new TabControl();
            tabPage1 = new TabPage();
            tabPage2 = new TabPage();
            lbLog = new ListBox();
            tabPage3 = new TabPage();
            linkBuyLicense = new LinkLabel();
            linkDocumentation = new LinkLabel();
            linkWebsite = new LinkLabel();
            lblLicenseNotice = new Label();
            lblCopyright = new Label();
            lblVersion = new Label();
            lblAppName = new Label();
            linkMoreInfo = new LinkLabel();
            lblAppDescription = new Label();
            pictureBox1 = new PictureBox();
            bttnPause = new Button();
            ((System.ComponentModel.ISupportInitialize)gvData).BeginInit();
            tabControl.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 18);
            label1.Name = "label1";
            label1.Size = new Size(105, 15);
            label1.TabIndex = 0;
            label1.Text = "Configuration File:";
            // 
            // txtFileName
            // 
            txtFileName.Location = new Point(123, 15);
            txtFileName.Name = "txtFileName";
            txtFileName.ReadOnly = true;
            txtFileName.Size = new Size(227, 23);
            txtFileName.TabIndex = 1;
            // 
            // bttnBrowse
            // 
            bttnBrowse.Location = new Point(356, 15);
            bttnBrowse.Name = "bttnBrowse";
            bttnBrowse.Size = new Size(75, 23);
            bttnBrowse.TabIndex = 2;
            bttnBrowse.Text = "Browse...";
            bttnBrowse.UseVisualStyleBackColor = true;
            // 
            // bttnStart
            // 
            bttnStart.Location = new Point(490, 15);
            bttnStart.Name = "bttnStart";
            bttnStart.Size = new Size(75, 23);
            bttnStart.TabIndex = 4;
            bttnStart.Text = "Start";
            bttnStart.UseVisualStyleBackColor = true;
            // 
            // gvData
            // 
            gvData.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gvData.Dock = DockStyle.Fill;
            gvData.Location = new Point(3, 3);
            gvData.Name = "gvData";
            gvData.Size = new Size(629, 601);
            gvData.TabIndex = 0;
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabPage1);
            tabControl.Controls.Add(tabPage2);
            tabControl.Controls.Add(tabPage3);
            tabControl.Location = new Point(13, 44);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(643, 635);
            tabControl.TabIndex = 5;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(gvData);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(635, 607);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Simulation Data";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(lbLog);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(635, 607);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Application Log";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // lbLog
            // 
            lbLog.Dock = DockStyle.Fill;
            lbLog.FormattingEnabled = true;
            lbLog.ItemHeight = 15;
            lbLog.Location = new Point(3, 3);
            lbLog.Name = "lbLog";
            lbLog.Size = new Size(629, 601);
            lbLog.TabIndex = 0;
            // 
            // tabPage3
            // 
            tabPage3.AutoScroll = true;
            tabPage3.Controls.Add(linkBuyLicense);
            tabPage3.Controls.Add(linkDocumentation);
            tabPage3.Controls.Add(linkWebsite);
            tabPage3.Controls.Add(lblLicenseNotice);
            tabPage3.Controls.Add(lblCopyright);
            tabPage3.Controls.Add(lblVersion);
            tabPage3.Controls.Add(lblAppName);
            tabPage3.Controls.Add(linkMoreInfo);
            tabPage3.Controls.Add(lblAppDescription);
            tabPage3.Controls.Add(pictureBox1);
            tabPage3.Location = new Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(20);
            tabPage3.Size = new Size(635, 607);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Help & About";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // linkBuyLicense
            // 
            linkBuyLicense.AutoSize = true;
            linkBuyLicense.Location = new Point(23, 485);
            linkBuyLicense.Name = "linkBuyLicense";
            linkBuyLicense.Size = new Size(69, 15);
            linkBuyLicense.TabIndex = 9;
            linkBuyLicense.TabStop = true;
            linkBuyLicense.Text = "Buy License";
            // 
            // linkDocumentation
            // 
            linkDocumentation.AutoSize = true;
            linkDocumentation.Location = new Point(23, 445);
            linkDocumentation.Name = "linkDocumentation";
            linkDocumentation.Size = new Size(90, 15);
            linkDocumentation.TabIndex = 8;
            linkDocumentation.TabStop = true;
            linkDocumentation.Text = "Documentation";
            // 
            // linkWebsite
            // 
            linkWebsite.AutoSize = true;
            linkWebsite.Location = new Point(23, 406);
            linkWebsite.Name = "linkWebsite";
            linkWebsite.Size = new Size(49, 15);
            linkWebsite.TabIndex = 7;
            linkWebsite.TabStop = true;
            linkWebsite.Text = "Website";
            // 
            // lblLicenseNotice
            // 
            lblLicenseNotice.Location = new Point(23, 351);
            lblLicenseNotice.Name = "lblLicenseNotice";
            lblLicenseNotice.Size = new Size(589, 40);
            lblLicenseNotice.TabIndex = 6;
            lblLicenseNotice.Text = "This is proprietary software. A valid license is required to use this application. Unauthorized use is prohibited.";
            // 
            // lblCopyright
            // 
            lblCopyright.AutoSize = true;
            lblCopyright.Location = new Point(23, 316);
            lblCopyright.Name = "lblCopyright";
            lblCopyright.Size = new Size(292, 15);
            lblCopyright.TabIndex = 5;
            lblCopyright.Text = "© 2023-2025 Cyberia Technologies. All rights reserved.";
            // 
            // lblVersion
            // 
            lblVersion.AutoSize = true;
            lblVersion.Location = new Point(23, 291);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new Size(72, 15);
            lblVersion.TabIndex = 4;
            lblVersion.Text = "Version 1.0.0";
            // 
            // lblAppName
            // 
            lblAppName.AutoSize = true;
            lblAppName.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblAppName.Location = new Point(23, 20);
            lblAppName.Name = "lblAppName";
            lblAppName.Size = new Size(250, 25);
            lblAppName.TabIndex = 3;
            lblAppName.Text = "Modbus Simulation Server";
            // 
            // linkMoreInfo
            // 
            linkMoreInfo.AutoSize = true;
            linkMoreInfo.Location = new Point(23, 120);
            linkMoreInfo.Name = "linkMoreInfo";
            linkMoreInfo.Size = new Size(110, 15);
            linkMoreInfo.TabIndex = 2;
            linkMoreInfo.TabStop = true;
            linkMoreInfo.Text = "More information...";
            // 
            // lblAppDescription
            // 
            lblAppDescription.Location = new Point(23, 60);
            lblAppDescription.Name = "lblAppDescription";
            lblAppDescription.Size = new Size(589, 60);
            lblAppDescription.TabIndex = 1;
            lblAppDescription.Text = resources.GetString("lblAppDescription.Text");
            // 
            // pictureBox1
            // 
            pictureBox1.Anchor = AnchorStyles.Bottom;
            pictureBox1.Image = Properties.Resources.CT_logo3_white_bkg;
            pictureBox1.Location = new Point(218, 394);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(394, 210);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // bttnPause
            // 
            bttnPause.Location = new Point(571, 15);
            bttnPause.Name = "bttnPause";
            bttnPause.Size = new Size(75, 23);
            bttnPause.TabIndex = 6;
            bttnPause.Text = "Pause";
            bttnPause.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(667, 691);
            Controls.Add(bttnPause);
            Controls.Add(tabControl);
            Controls.Add(bttnStart);
            Controls.Add(bttnBrowse);
            Controls.Add(txtFileName);
            Controls.Add(label1);
            Name = "MainForm";
            Text = "Modbus Simulation Server";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)gvData).EndInit();
            tabControl.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            tabPage3.ResumeLayout(false);
            tabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox txtFileName;
        private Button bttnBrowse;
        private DataGridView gvData;
        private Button bttnStart;
        private TabControl tabControl;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private Button bttnPause;
        private ListBox lbLog;
        private TabPage tabPage3;
        private PictureBox pictureBox1;
        private Label lblAppDescription;
        private LinkLabel linkMoreInfo;
        private Label lblAppName;
        private Label lblVersion;
        private Label lblCopyright;
        private Label lblLicenseNotice;
        private LinkLabel linkWebsite;
        private LinkLabel linkDocumentation;
        private LinkLabel linkBuyLicense;
    }
}
