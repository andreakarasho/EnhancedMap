namespace EnhancedMap.GUI.MapMakerSteps
{
    partial class OriginalMapStep
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
            this.customButtonGenerate = new EnhancedMap.GUI.CustomButton();
            this.customButtonDownload = new EnhancedMap.GUI.CustomButton();
            this.customFlatButtonBack = new EnhancedMap.GUI.CustomFlatButton();
            this.SuspendLayout();
            // 
            // customButtonGenerate
            // 
            this.customButtonGenerate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(49)))), ((int)(((byte)(49)))));
            this.customButtonGenerate.ForceHover = false;
            this.customButtonGenerate.IsHover = false;
            this.customButtonGenerate.IsPressed = false;
            this.customButtonGenerate.Location = new System.Drawing.Point(12, 125);
            this.customButtonGenerate.MouseState = EnhancedMap.GUI.MouseState.HOVER;
            this.customButtonGenerate.Name = "customButtonGenerate";
            this.customButtonGenerate.Size = new System.Drawing.Size(437, 90);
            this.customButtonGenerate.TabIndex = 2;
            this.customButtonGenerate.Text = "GENERATE";
            this.customButtonGenerate.UseVisualStyleBackColor = false;
            // 
            // customButtonDownload
            // 
            this.customButtonDownload.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(49)))), ((int)(((byte)(49)))));
            this.customButtonDownload.ForceHover = false;
            this.customButtonDownload.IsHover = false;
            this.customButtonDownload.IsPressed = false;
            this.customButtonDownload.Location = new System.Drawing.Point(11, 29);
            this.customButtonDownload.MouseState = EnhancedMap.GUI.MouseState.HOVER;
            this.customButtonDownload.Name = "customButtonDownload";
            this.customButtonDownload.Size = new System.Drawing.Size(438, 90);
            this.customButtonDownload.TabIndex = 1;
            this.customButtonDownload.Text = "DOWNLOAD";
            this.customButtonDownload.UseVisualStyleBackColor = false;
            // 
            // customFlatButtonBack
            // 
            this.customFlatButtonBack.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.customFlatButtonBack.ForceHover = false;
            this.customFlatButtonBack.Icon = null;
            this.customFlatButtonBack.IsHover = true;
            this.customFlatButtonBack.IsPressed = false;
            this.customFlatButtonBack.IsSelected = false;
            this.customFlatButtonBack.Location = new System.Drawing.Point(10, 299);
            this.customFlatButtonBack.Margin = new System.Windows.Forms.Padding(5, 7, 5, 7);
            this.customFlatButtonBack.MouseState = EnhancedMap.GUI.MouseState.HOVER;
            this.customFlatButtonBack.Name = "customFlatButtonBack";
            this.customFlatButtonBack.Size = new System.Drawing.Size(75, 27);
            this.customFlatButtonBack.TabIndex = 0;
            this.customFlatButtonBack.Text = "Back";
            this.customFlatButtonBack.UseVisualStyleBackColor = true;
            // 
            // OriginalMapStep
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.customButtonGenerate);
            this.Controls.Add(this.customButtonDownload);
            this.Controls.Add(this.customFlatButtonBack);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "OriginalMapStep";
            this.Size = new System.Drawing.Size(461, 333);
            this.ResumeLayout(false);

        }

        #endregion

        private CustomFlatButton customFlatButtonBack;
        private CustomButton customButtonDownload;
        private CustomButton customButtonGenerate;
    }
}
