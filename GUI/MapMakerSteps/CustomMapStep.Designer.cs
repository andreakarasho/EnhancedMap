namespace EnhancedMap.GUI.MapMakerSteps
{
    partial class CustomMapStep
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
            this.customFlatButtonBack = new EnhancedMap.GUI.CustomFlatButton();
            this.listViewMaps = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.customButtonRemove = new EnhancedMap.GUI.CustomButton();
            this.customButtonAdd = new EnhancedMap.GUI.CustomButton();
            this.numericUpDownHeight = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownWidth = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDownIndex = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.customButtonGenerate = new EnhancedMap.GUI.CustomButton();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownIndex)).BeginInit();
            this.SuspendLayout();
            // 
            // customFlatButtonBack
            // 
            this.customFlatButtonBack.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.customFlatButtonBack.ForceHover = false;
            this.customFlatButtonBack.ForeColor = System.Drawing.Color.White;
            this.customFlatButtonBack.Icon = null;
            this.customFlatButtonBack.IsHover = true;
            this.customFlatButtonBack.IsPressed = false;
            this.customFlatButtonBack.IsSelected = false;
            this.customFlatButtonBack.Location = new System.Drawing.Point(5, 299);
            this.customFlatButtonBack.Margin = new System.Windows.Forms.Padding(5, 7, 5, 7);
            this.customFlatButtonBack.MouseState = EnhancedMap.GUI.MouseState.HOVER;
            this.customFlatButtonBack.Name = "customFlatButtonBack";
            this.customFlatButtonBack.Size = new System.Drawing.Size(75, 27);
            this.customFlatButtonBack.TabIndex = 1;
            this.customFlatButtonBack.Text = "Back";
            this.customFlatButtonBack.UseVisualStyleBackColor = true;
            // 
            // listViewMaps
            // 
            this.listViewMaps.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.listViewMaps.FullRowSelect = true;
            this.listViewMaps.GridLines = true;
            this.listViewMaps.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewMaps.Location = new System.Drawing.Point(5, 135);
            this.listViewMaps.MultiSelect = false;
            this.listViewMaps.Name = "listViewMaps";
            this.listViewMaps.Size = new System.Drawing.Size(456, 113);
            this.listViewMaps.TabIndex = 12;
            this.listViewMaps.UseCompatibleStateImageBehavior = false;
            this.listViewMaps.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Index";
            this.columnHeader1.Width = 54;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Name";
            this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader2.Width = 212;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Width";
            this.columnHeader3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader3.Width = 88;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Height";
            this.columnHeader4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader4.Width = 82;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBoxName);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.customButtonRemove);
            this.groupBox1.Controls.Add(this.customButtonAdd);
            this.groupBox1.Controls.Add(this.numericUpDownHeight);
            this.groupBox1.Controls.Add(this.numericUpDownWidth);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.numericUpDownIndex);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(456, 126);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Setup";
            // 
            // textBoxName
            // 
            this.textBoxName.Location = new System.Drawing.Point(70, 24);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(146, 21);
            this.textBoxName.TabIndex = 18;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(20, 27);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 15);
            this.label3.TabIndex = 17;
            this.label3.Text = "Name:";
            // 
            // customButtonRemove
            // 
            this.customButtonRemove.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(49)))), ((int)(((byte)(49)))));
            this.customButtonRemove.ForceHover = false;
            this.customButtonRemove.IsHover = false;
            this.customButtonRemove.IsPressed = false;
            this.customButtonRemove.Location = new System.Drawing.Point(364, 93);
            this.customButtonRemove.MouseState = EnhancedMap.GUI.MouseState.HOVER;
            this.customButtonRemove.Name = "customButtonRemove";
            this.customButtonRemove.Size = new System.Drawing.Size(86, 27);
            this.customButtonRemove.TabIndex = 16;
            this.customButtonRemove.Text = "Remove";
            this.customButtonRemove.UseVisualStyleBackColor = false;
            // 
            // customButtonAdd
            // 
            this.customButtonAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(49)))), ((int)(((byte)(49)))));
            this.customButtonAdd.ForceHover = false;
            this.customButtonAdd.IsHover = false;
            this.customButtonAdd.IsPressed = false;
            this.customButtonAdd.Location = new System.Drawing.Point(273, 93);
            this.customButtonAdd.MouseState = EnhancedMap.GUI.MouseState.HOVER;
            this.customButtonAdd.Name = "customButtonAdd";
            this.customButtonAdd.Size = new System.Drawing.Size(86, 27);
            this.customButtonAdd.TabIndex = 15;
            this.customButtonAdd.Text = "Add";
            this.customButtonAdd.UseVisualStyleBackColor = false;
            // 
            // numericUpDownHeight
            // 
            this.numericUpDownHeight.Location = new System.Drawing.Point(149, 61);
            this.numericUpDownHeight.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownHeight.Name = "numericUpDownHeight";
            this.numericUpDownHeight.Size = new System.Drawing.Size(73, 21);
            this.numericUpDownHeight.TabIndex = 14;
            // 
            // numericUpDownWidth
            // 
            this.numericUpDownWidth.Location = new System.Drawing.Point(70, 61);
            this.numericUpDownWidth.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownWidth.Name = "numericUpDownWidth";
            this.numericUpDownWidth.Size = new System.Drawing.Size(73, 21);
            this.numericUpDownWidth.TabIndex = 13;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(24, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 15);
            this.label2.TabIndex = 11;
            this.label2.Text = "Size:";
            // 
            // numericUpDownIndex
            // 
            this.numericUpDownIndex.Location = new System.Drawing.Point(345, 25);
            this.numericUpDownIndex.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.numericUpDownIndex.Name = "numericUpDownIndex";
            this.numericUpDownIndex.Size = new System.Drawing.Size(66, 21);
            this.numericUpDownIndex.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(299, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Index:";
            // 
            // customButtonGenerate
            // 
            this.customButtonGenerate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(49)))), ((int)(((byte)(49)))));
            this.customButtonGenerate.ForceHover = false;
            this.customButtonGenerate.IsHover = false;
            this.customButtonGenerate.IsPressed = false;
            this.customButtonGenerate.Location = new System.Drawing.Point(5, 254);
            this.customButtonGenerate.MouseState = EnhancedMap.GUI.MouseState.HOVER;
            this.customButtonGenerate.Name = "customButtonGenerate";
            this.customButtonGenerate.Size = new System.Drawing.Size(453, 35);
            this.customButtonGenerate.TabIndex = 14;
            this.customButtonGenerate.Text = "GENERATE";
            this.customButtonGenerate.UseVisualStyleBackColor = false;
            // 
            // CustomMapStep
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.customButtonGenerate);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.listViewMaps);
            this.Controls.Add(this.customFlatButtonBack);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "CustomMapStep";
            this.Size = new System.Drawing.Size(461, 333);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownIndex)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private CustomFlatButton customFlatButtonBack;
        private System.Windows.Forms.ListView listViewMaps;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDownIndex;
        private System.Windows.Forms.NumericUpDown numericUpDownWidth;
        private CustomButton customButtonRemove;
        private CustomButton customButtonAdd;
        private System.Windows.Forms.NumericUpDown numericUpDownHeight;
        private CustomButton customButtonGenerate;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Label label3;
    }
}
