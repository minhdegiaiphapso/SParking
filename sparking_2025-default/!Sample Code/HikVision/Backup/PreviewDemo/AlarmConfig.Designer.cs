namespace PreviewDemo
{
    partial class AlarmConfig
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        /// 
        public int m_lUserID = -1;
        public int m_iAlarmInNum = 0;
        public int m_iAlarmOutNum = 0;
        public CHCNetSDK.NET_DVR_ALARMINCFG_V30 m_struAlarmInCfg;
        public CHCNetSDK.NET_DVR_ALARMOUTCFG_V30 m_struAlarmOutCfg;
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBoxAlarmType = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxAlarmInName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxAlarmIn = new System.Windows.Forms.ComboBox();
            this.btnAlarmInCfg = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxAlarmOut = new System.Windows.Forms.ComboBox();
            this.btnAlarmOutCfg = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.textBoxAlarmOutName = new System.Windows.Forms.TextBox();
            this.comboBoxAlarmOutDelay = new System.Windows.Forms.ComboBox();
            this.btnSetAlarmOut = new System.Windows.Forms.Button();
            this.comboBoxAlarmOutStatic = new System.Windows.Forms.ComboBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.comboBoxAlarmType);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.textBoxAlarmInName);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.comboBoxAlarmIn);
            this.groupBox1.Controls.Add(this.btnAlarmInCfg);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(454, 153);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Alarm Input Setup";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(208, 79);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 6;
            this.label4.Text = "Alarm Type";
            // 
            // comboBoxAlarmType
            // 
            this.comboBoxAlarmType.FormattingEnabled = true;
            this.comboBoxAlarmType.Location = new System.Drawing.Point(283, 75);
            this.comboBoxAlarmType.Name = "comboBoxAlarmType";
            this.comboBoxAlarmType.Size = new System.Drawing.Size(121, 20);
            this.comboBoxAlarmType.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(27, 75);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "Name";
            // 
            // textBoxAlarmInName
            // 
            this.textBoxAlarmInName.Location = new System.Drawing.Point(83, 71);
            this.textBoxAlarmInName.Name = "textBoxAlarmInName";
            this.textBoxAlarmInName.Size = new System.Drawing.Size(100, 21);
            this.textBoxAlarmInName.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 28);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "Alarm Input";
            // 
            // comboBoxAlarmIn
            // 
            this.comboBoxAlarmIn.FormattingEnabled = true;
            this.comboBoxAlarmIn.Location = new System.Drawing.Point(83, 20);
            this.comboBoxAlarmIn.Name = "comboBoxAlarmIn";
            this.comboBoxAlarmIn.Size = new System.Drawing.Size(121, 20);
            this.comboBoxAlarmIn.TabIndex = 1;
            this.comboBoxAlarmIn.SelectedIndexChanged += new System.EventHandler(this.comboBoxAlarmIn_SelectedIndexChanged);
            // 
            // btnAlarmInCfg
            // 
            this.btnAlarmInCfg.Location = new System.Drawing.Point(373, 114);
            this.btnAlarmInCfg.Name = "btnAlarmInCfg";
            this.btnAlarmInCfg.Size = new System.Drawing.Size(75, 23);
            this.btnAlarmInCfg.TabIndex = 0;
            this.btnAlarmInCfg.Text = "Setup";
            this.btnAlarmInCfg.UseVisualStyleBackColor = true;
            this.btnAlarmInCfg.Click += new System.EventHandler(this.btnAlarmInCfg_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.comboBoxAlarmOutStatic);
            this.groupBox2.Controls.Add(this.btnSetAlarmOut);
            this.groupBox2.Controls.Add(this.comboBoxAlarmOutDelay);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.textBoxAlarmOutName);
            this.groupBox2.Controls.Add(this.comboBoxAlarmOut);
            this.groupBox2.Controls.Add(this.btnAlarmOutCfg);
            this.groupBox2.Location = new System.Drawing.Point(12, 176);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(454, 183);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Alarm Output Setup";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "Alarm Output";
            // 
            // comboBoxAlarmOut
            // 
            this.comboBoxAlarmOut.FormattingEnabled = true;
            this.comboBoxAlarmOut.Location = new System.Drawing.Point(149, 20);
            this.comboBoxAlarmOut.Name = "comboBoxAlarmOut";
            this.comboBoxAlarmOut.Size = new System.Drawing.Size(121, 20);
            this.comboBoxAlarmOut.TabIndex = 1;
            this.comboBoxAlarmOut.SelectedIndexChanged += new System.EventHandler(this.comboBoxAlarmOut_SelectedIndexChanged);
            // 
            // btnAlarmOutCfg
            // 
            this.btnAlarmOutCfg.Location = new System.Drawing.Point(354, 117);
            this.btnAlarmOutCfg.Name = "btnAlarmOutCfg";
            this.btnAlarmOutCfg.Size = new System.Drawing.Size(75, 23);
            this.btnAlarmOutCfg.TabIndex = 0;
            this.btnAlarmOutCfg.Text = "Setup";
            this.btnAlarmOutCfg.UseVisualStyleBackColor = true;
            this.btnAlarmOutCfg.Click += new System.EventHandler(this.btnAlarmOutCfg_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(35, 119);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(269, 12);
            this.label5.TabIndex = 7;
            this.label5.Text = "You need to add other parameters if you need";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(2, 117);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(269, 12);
            this.label6.TabIndex = 7;
            this.label6.Text = "You need to add other parameters if you need";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(201, 60);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(113, 12);
            this.label7.TabIndex = 8;
            this.label7.Text = "Alarm Output Delay";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(20, 60);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(29, 12);
            this.label8.TabIndex = 4;
            this.label8.Text = "Name";
            // 
            // textBoxAlarmOutName
            // 
            this.textBoxAlarmOutName.Location = new System.Drawing.Point(76, 57);
            this.textBoxAlarmOutName.Name = "textBoxAlarmOutName";
            this.textBoxAlarmOutName.Size = new System.Drawing.Size(100, 21);
            this.textBoxAlarmOutName.TabIndex = 3;
            // 
            // comboBoxAlarmOutDelay
            // 
            this.comboBoxAlarmOutDelay.FormattingEnabled = true;
            this.comboBoxAlarmOutDelay.Location = new System.Drawing.Point(320, 57);
            this.comboBoxAlarmOutDelay.Name = "comboBoxAlarmOutDelay";
            this.comboBoxAlarmOutDelay.Size = new System.Drawing.Size(121, 20);
            this.comboBoxAlarmOutDelay.TabIndex = 9;
            // 
            // btnSetAlarmOut
            // 
            this.btnSetAlarmOut.Location = new System.Drawing.Point(37, 150);
            this.btnSetAlarmOut.Name = "btnSetAlarmOut";
            this.btnSetAlarmOut.Size = new System.Drawing.Size(116, 23);
            this.btnSetAlarmOut.TabIndex = 10;
            this.btnSetAlarmOut.Text = "set alarmout ";
            this.btnSetAlarmOut.UseVisualStyleBackColor = true;
            this.btnSetAlarmOut.Click += new System.EventHandler(this.btnSetAlarmOut_Click);
            // 
            // comboBoxAlarmOutStatic
            // 
            this.comboBoxAlarmOutStatic.FormattingEnabled = true;
            this.comboBoxAlarmOutStatic.Items.AddRange(new object[] {
            "Stop output",
            "Output"});
            this.comboBoxAlarmOutStatic.Location = new System.Drawing.Point(169, 152);
            this.comboBoxAlarmOutStatic.Name = "comboBoxAlarmOutStatic";
            this.comboBoxAlarmOutStatic.Size = new System.Drawing.Size(121, 20);
            this.comboBoxAlarmOutStatic.TabIndex = 11;
            // 
            // AlarmConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(689, 424);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "AlarmConfig";
            this.Text = "Alarm Setup";
            this.Load += new System.EventHandler(this.AlarmConfig_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnAlarmOutCfg;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnAlarmInCfg;
        private System.Windows.Forms.ComboBox comboBoxAlarmIn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxAlarmOut;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxAlarmInName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBoxAlarmType;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBoxAlarmOutName;
        private System.Windows.Forms.ComboBox comboBoxAlarmOutDelay;
        private System.Windows.Forms.Button btnSetAlarmOut;
        private System.Windows.Forms.ComboBox comboBoxAlarmOutStatic;
    }
}