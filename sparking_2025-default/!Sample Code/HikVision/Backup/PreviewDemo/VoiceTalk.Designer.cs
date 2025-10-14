namespace PreviewDemo
{
    partial class VoiceTalk
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        /// 
        private CHCNetSDK.VOICEDATACALLBACKV30 m_fVoiceData = null;
        public int m_lUserID = -1;
        public uint m_dwAudioChanNum = 0;
        public int m_lVoiceTalkHandle = -1;
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
            this.btnStartVoiceTale = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxVoiceChan = new System.Windows.Forms.ComboBox();
            this.btnStopVoiceTale = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxCallBack = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // btnStartVoiceTale
            // 
            this.btnStartVoiceTale.Location = new System.Drawing.Point(42, 87);
            this.btnStartVoiceTale.Name = "btnStartVoiceTale";
            this.btnStartVoiceTale.Size = new System.Drawing.Size(75, 23);
            this.btnStartVoiceTale.TabIndex = 0;
            this.btnStartVoiceTale.Text = "StartVoiceTale";
            this.btnStartVoiceTale.UseVisualStyleBackColor = true;
            this.btnStartVoiceTale.Click += new System.EventHandler(this.btnStartVoiceTale_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(40, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "VoiceTaleChannel";
            // 
            // comboBoxVoiceChan
            // 
            this.comboBoxVoiceChan.FormattingEnabled = true;
            this.comboBoxVoiceChan.Location = new System.Drawing.Point(128, 24);
            this.comboBoxVoiceChan.Name = "comboBoxVoiceChan";
            this.comboBoxVoiceChan.Size = new System.Drawing.Size(121, 20);
            this.comboBoxVoiceChan.TabIndex = 2;
            // 
            // btnStopVoiceTale
            // 
            this.btnStopVoiceTale.Location = new System.Drawing.Point(174, 86);
            this.btnStopVoiceTale.Name = "btnStopVoiceTale";
            this.btnStopVoiceTale.Size = new System.Drawing.Size(75, 23);
            this.btnStopVoiceTale.TabIndex = 3;
            this.btnStopVoiceTale.Text = "StopVoiceTale";
            this.btnStopVoiceTale.UseVisualStyleBackColor = true;
            this.btnStopVoiceTale.Click += new System.EventHandler(this.btnStopVoiceTale_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(40, 61);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "Callback";
            // 
            // comboBoxCallBack
            // 
            this.comboBoxCallBack.FormattingEnabled = true;
            this.comboBoxCallBack.Items.AddRange(new object[] {
            "Encode data",
            "PCM"});
            this.comboBoxCallBack.Location = new System.Drawing.Point(128, 53);
            this.comboBoxCallBack.Name = "comboBoxCallBack";
            this.comboBoxCallBack.Size = new System.Drawing.Size(121, 20);
            this.comboBoxCallBack.TabIndex = 5;
            // 
            // VoiceTalk
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(311, 122);
            this.Controls.Add(this.comboBoxCallBack);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnStopVoiceTale);
            this.Controls.Add(this.comboBoxVoiceChan);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnStartVoiceTale);
            this.Name = "VoiceTalk";
            this.Text = "voice talk";
            this.Load += new System.EventHandler(this.VoiceTalk_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStartVoiceTale;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxVoiceChan;
        private System.Windows.Forms.Button btnStopVoiceTale;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxCallBack;
    }
}