namespace EnhancedMap.GUI.MapMakerSteps
{
    partial class CreationMapsStep
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.customProgressBar1 = new EnhancedMap.GUI.CustomProgressBar();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(5, 79);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(451, 274);
            this.textBox1.TabIndex = 1;
            this.textBox1.WordWrap = false;
            // 
            // customProgressBar1
            // 
            this.customProgressBar1.Location = new System.Drawing.Point(5, 45);
            this.customProgressBar1.Name = "customProgressBar1";
            this.customProgressBar1.Size = new System.Drawing.Size(451, 10);
            this.customProgressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.customProgressBar1.TabIndex = 0;
            // 
            // CreationMapsStep
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.customProgressBar1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "CreationMapsStep";
            this.Size = new System.Drawing.Size(461, 356);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private CustomProgressBar customProgressBar1;
        private System.Windows.Forms.TextBox textBox1;
    }
}
