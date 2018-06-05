namespace EnhancedMap.GUI
{
    partial class CoordsConverterF
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
            this.customFlatButtonSwitch = new EnhancedMap.GUI.CustomFlatButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.customButtonReset = new EnhancedMap.GUI.CustomButton();
            this.customButtonGo = new EnhancedMap.GUI.CustomButton();
            this.SuspendLayout();
            // 
            // customFlatButtonSwitch
            // 
            this.customFlatButtonSwitch.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.customFlatButtonSwitch.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.customFlatButtonSwitch.ForceHover = false;
            this.customFlatButtonSwitch.ForeColor = System.Drawing.Color.White;
            this.customFlatButtonSwitch.Icon = null;
            this.customFlatButtonSwitch.IsHover = true;
            this.customFlatButtonSwitch.IsPressed = false;
            this.customFlatButtonSwitch.IsSelected = false;
            this.customFlatButtonSwitch.Location = new System.Drawing.Point(14, 147);
            this.customFlatButtonSwitch.Margin = new System.Windows.Forms.Padding(5, 7, 5, 7);
            this.customFlatButtonSwitch.MouseState = EnhancedMap.GUI.MouseState.HOVER;
            this.customFlatButtonSwitch.Name = "customFlatButtonSwitch";
            this.customFlatButtonSwitch.Size = new System.Drawing.Size(104, 29);
            this.customFlatButtonSwitch.TabIndex = 0;
            this.customFlatButtonSwitch.Text = "To Lat/Long";
            this.customFlatButtonSwitch.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(12, 28);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(299, 109);
            this.panel1.TabIndex = 1;
            // 
            // customButtonReset
            // 
            this.customButtonReset.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(49)))), ((int)(((byte)(49)))));
            this.customButtonReset.ForceHover = false;
            this.customButtonReset.IsHover = false;
            this.customButtonReset.IsPressed = false;
            this.customButtonReset.Location = new System.Drawing.Point(189, 147);
            this.customButtonReset.MouseState = EnhancedMap.GUI.MouseState.HOVER;
            this.customButtonReset.Name = "customButtonReset";
            this.customButtonReset.Size = new System.Drawing.Size(58, 29);
            this.customButtonReset.TabIndex = 2;
            this.customButtonReset.Text = "Reset";
            this.customButtonReset.UseVisualStyleBackColor = false;
            // 
            // customButtonGo
            // 
            this.customButtonGo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(49)))), ((int)(((byte)(49)))));
            this.customButtonGo.ForceHover = false;
            this.customButtonGo.IsHover = false;
            this.customButtonGo.IsPressed = false;
            this.customButtonGo.Location = new System.Drawing.Point(253, 147);
            this.customButtonGo.MouseState = EnhancedMap.GUI.MouseState.HOVER;
            this.customButtonGo.Name = "customButtonGo";
            this.customButtonGo.Size = new System.Drawing.Size(58, 29);
            this.customButtonGo.TabIndex = 3;
            this.customButtonGo.Text = "Go";
            this.customButtonGo.UseVisualStyleBackColor = false;
            // 
            // CoordsConverterF
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(323, 183);
            this.Controls.Add(this.customButtonGo);
            this.Controls.Add(this.customButtonReset);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.customFlatButtonSwitch);
            this.Cursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(204, 29);
            this.Name = "CoordsConverterF";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Coords Converter";
            this.ResumeLayout(false);

        }

        #endregion

        private CustomFlatButton customFlatButtonSwitch;
        private System.Windows.Forms.Panel panel1;
        private CustomButton customButtonReset;
        private CustomButton customButtonGo;
    }
}