namespace WinformsExample
{
    partial class Min_max_setting
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
            this.button7 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.comboBox14 = new System.Windows.Forms.ComboBox();
            this.comboBox13 = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(226, 39);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(45, 23);
            this.button7.TabIndex = 37;
            this.button7.Text = "FIND";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(226, 12);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(45, 23);
            this.button6.TabIndex = 36;
            this.button6.Text = "FIND";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // comboBox14
            // 
            this.comboBox14.FormattingEnabled = true;
            this.comboBox14.Location = new System.Drawing.Point(56, 39);
            this.comboBox14.Name = "comboBox14";
            this.comboBox14.Size = new System.Drawing.Size(164, 21);
            this.comboBox14.TabIndex = 35;
            // 
            // comboBox13
            // 
            this.comboBox13.FormattingEnabled = true;
            this.comboBox13.Location = new System.Drawing.Point(56, 12);
            this.comboBox13.Name = "comboBox13";
            this.comboBox13.Size = new System.Drawing.Size(164, 21);
            this.comboBox13.TabIndex = 34;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(20, 44);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(27, 13);
            this.label9.TabIndex = 33;
            this.label9.Text = "MIN";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(20, 15);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(30, 13);
            this.label8.TabIndex = 32;
            this.label8.Text = "MAX";
            // 
            // Min_max_setting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(297, 76);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.comboBox14);
            this.Controls.Add(this.comboBox13);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Name = "Min_max_setting";
            this.Text = "Min_max_setting";
            this.Load += new System.EventHandler(this.Min_max_setting_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.ComboBox comboBox14;
        private System.Windows.Forms.ComboBox comboBox13;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
    }
}