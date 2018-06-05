namespace EnhancedMap.GUI
{
    partial class SharedLabelF
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
            this.numericTextBoxX = new Aga.Controls.NumericTextBox();
            this.numericTextBoxY = new Aga.Controls.NumericTextBox();
            this.comboBoxMap = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxDescription = new System.Windows.Forms.TextBox();
            this.customButtonSend = new EnhancedMap.GUI.CustomButton();
            this.SuspendLayout();
            // 
            // numericTextBoxX
            // 
            this.numericTextBoxX.AllowDecimalSeparator = false;
            this.numericTextBoxX.AllowNegativeSign = false;
            this.numericTextBoxX.Location = new System.Drawing.Point(115, 38);
            this.numericTextBoxX.Name = "numericTextBoxX";
            this.numericTextBoxX.Size = new System.Drawing.Size(116, 21);
            this.numericTextBoxX.TabIndex = 0;
            this.numericTextBoxX.Text = "0";
            // 
            // numericTextBoxY
            // 
            this.numericTextBoxY.AllowDecimalSeparator = false;
            this.numericTextBoxY.AllowNegativeSign = false;
            this.numericTextBoxY.Location = new System.Drawing.Point(115, 69);
            this.numericTextBoxY.Name = "numericTextBoxY";
            this.numericTextBoxY.Size = new System.Drawing.Size(116, 21);
            this.numericTextBoxY.TabIndex = 1;
            this.numericTextBoxY.Text = "0";
            // 
            // comboBoxMap
            // 
            this.comboBoxMap.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMap.FormattingEnabled = true;
            this.comboBoxMap.Location = new System.Drawing.Point(115, 105);
            this.comboBoxMap.Name = "comboBoxMap";
            this.comboBoxMap.Size = new System.Drawing.Size(140, 23);
            this.comboBoxMap.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(12, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(18, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "X:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(12, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "Y:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(12, 108);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 15);
            this.label3.TabIndex = 5;
            this.label3.Text = "Map:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(12, 139);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 15);
            this.label4.TabIndex = 6;
            this.label4.Text = "Description:";
            // 
            // textBoxDescription
            // 
            this.textBoxDescription.Location = new System.Drawing.Point(115, 136);
            this.textBoxDescription.MaxLength = 20;
            this.textBoxDescription.Name = "textBoxDescription";
            this.textBoxDescription.Size = new System.Drawing.Size(208, 21);
            this.textBoxDescription.TabIndex = 7;
            // 
            // customButtonSend
            // 
            this.customButtonSend.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(49)))), ((int)(((byte)(49)))));
            this.customButtonSend.ForceHover = false;
            this.customButtonSend.IsHover = false;
            this.customButtonSend.IsPressed = false;
            this.customButtonSend.Location = new System.Drawing.Point(115, 192);
            this.customButtonSend.MouseState = EnhancedMap.GUI.MouseState.HOVER;
            this.customButtonSend.Name = "customButtonSend";
            this.customButtonSend.Size = new System.Drawing.Size(100, 26);
            this.customButtonSend.TabIndex = 8;
            this.customButtonSend.Text = "Send";
            this.customButtonSend.UseVisualStyleBackColor = false;
            // 
            // SharedLabelF
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(335, 230);
            this.Controls.Add(this.customButtonSend);
            this.Controls.Add(this.textBoxDescription);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBoxMap);
            this.Controls.Add(this.numericTextBoxY);
            this.Controls.Add(this.numericTextBoxX);
            this.Cursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(204, 29);
            this.Name = "SharedLabelF";
            this.Text = "SharedLabelF";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Aga.Controls.NumericTextBox numericTextBoxX;
        private Aga.Controls.NumericTextBox numericTextBoxY;
        private System.Windows.Forms.ComboBox comboBoxMap;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxDescription;
        private CustomButton customButtonSend;
    }
}