namespace EnhancedMap.GUI.SettingsLayouts
{
    partial class DiagnosticLayout
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.richTextBox = new System.Windows.Forms.RichTextBox();
            this.checkBoxAutoScroll = new System.Windows.Forms.CheckBox();
            this.labelData = new System.Windows.Forms.Label();
            this.customButtonSaveLog = new EnhancedMap.GUI.CustomButton();
            this.SuspendLayout();
            // 
            // richTextBox
            // 
            this.richTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(49)))), ((int)(((byte)(49)))));
            this.richTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.richTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBox.Location = new System.Drawing.Point(0, 0);
            this.richTextBox.Name = "richTextBox";
            this.richTextBox.ReadOnly = true;
            this.richTextBox.Size = new System.Drawing.Size(392, 485);
            this.richTextBox.TabIndex = 0;
            this.richTextBox.Text = "";
            // 
            // checkBoxAutoScroll
            // 
            this.checkBoxAutoScroll.AutoSize = true;
            this.checkBoxAutoScroll.Checked = true;
            this.checkBoxAutoScroll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutoScroll.Location = new System.Drawing.Point(415, 85);
            this.checkBoxAutoScroll.Name = "checkBoxAutoScroll";
            this.checkBoxAutoScroll.Size = new System.Drawing.Size(81, 19);
            this.checkBoxAutoScroll.TabIndex = 3;
            this.checkBoxAutoScroll.Text = "AutoScroll";
            this.checkBoxAutoScroll.UseVisualStyleBackColor = true;
            // 
            // labelData
            // 
            this.labelData.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelData.Location = new System.Drawing.Point(395, 396);
            this.labelData.Name = "labelData";
            this.labelData.Size = new System.Drawing.Size(126, 77);
            this.labelData.TabIndex = 4;
            this.labelData.Text = "Data:\r\n\r\n- IN: 0 Kb/s\r\n- OUT: 0 Kb/s";
            // 
            // customButtonSaveLog
            // 
            this.customButtonSaveLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(49)))), ((int)(((byte)(49)))));
            this.customButtonSaveLog.ForceHover = false;
            this.customButtonSaveLog.IsHover = false;
            this.customButtonSaveLog.IsPressed = false;
            this.customButtonSaveLog.Location = new System.Drawing.Point(398, 3);
            this.customButtonSaveLog.MouseState = EnhancedMap.GUI.MouseState.HOVER;
            this.customButtonSaveLog.Name = "customButtonSaveLog";
            this.customButtonSaveLog.Size = new System.Drawing.Size(123, 23);
            this.customButtonSaveLog.TabIndex = 1;
            this.customButtonSaveLog.Text = "Save Log";
            this.customButtonSaveLog.UseVisualStyleBackColor = false;
            // 
            // DiagnosticLayout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelData);
            this.Controls.Add(this.checkBoxAutoScroll);
            this.Controls.Add(this.customButtonSaveLog);
            this.Controls.Add(this.richTextBox);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "DiagnosticLayout";
            this.Size = new System.Drawing.Size(524, 485);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox;
        private CustomButton customButtonSaveLog;
        private System.Windows.Forms.CheckBox checkBoxAutoScroll;
        private System.Windows.Forms.Label labelData;
    }
}
