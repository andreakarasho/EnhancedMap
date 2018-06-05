namespace EnhancedMap.GUI.MapMakerSteps
{
    partial class KindClientStep
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
            this.pictureBoxClassic = new System.Windows.Forms.PictureBox();
            this.pictureBoxCustom = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxClassic)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCustom)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBoxClassic
            // 
            this.pictureBoxClassic.Image = global::EnhancedMap.Properties.Resources.uoclassic;
            this.pictureBoxClassic.Location = new System.Drawing.Point(3, 3);
            this.pictureBoxClassic.Name = "pictureBoxClassic";
            this.pictureBoxClassic.Size = new System.Drawing.Size(217, 384);
            this.pictureBoxClassic.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxClassic.TabIndex = 0;
            this.pictureBoxClassic.TabStop = false;
            // 
            // pictureBoxCustom
            // 
            this.pictureBoxCustom.Image = global::EnhancedMap.Properties.Resources.uoenhanced;
            this.pictureBoxCustom.Location = new System.Drawing.Point(241, 3);
            this.pictureBoxCustom.Name = "pictureBoxCustom";
            this.pictureBoxCustom.Size = new System.Drawing.Size(217, 384);
            this.pictureBoxCustom.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxCustom.TabIndex = 1;
            this.pictureBoxCustom.TabStop = false;
            // 
            // KindClientStep
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pictureBoxCustom);
            this.Controls.Add(this.pictureBoxClassic);
            this.Name = "KindClientStep";
            this.Size = new System.Drawing.Size(461, 390);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxClassic)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCustom)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxClassic;
        private System.Windows.Forms.PictureBox pictureBoxCustom;
    }
}
