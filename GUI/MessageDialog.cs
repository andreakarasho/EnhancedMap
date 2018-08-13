using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace EnhancedMap.GUI
{
    public class MessageDialog : Form
    {
        /// <summary>
        ///     Required designer variable.
        /// </summary>
        private readonly Container _components = null;

        private readonly bool _mCanIgnore;
        private readonly string _mMessage;

        private readonly string _mTitle;
        private TextBox _message;
        private Button _okay;

        public MessageDialog(string title, string message) : this(title, false, message)
        {
        }

        public MessageDialog(string title, bool ignorable, string message, params object[] msgArgs) : this(title, ignorable, string.Format(message, msgArgs))
        {
        }

        public MessageDialog(string title, bool ignorable, string message)
        {
            _mTitle = title;
            _mMessage = message;
            _mCanIgnore = ignorable;
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        /// <summary>
        ///     Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_components != null) _components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///     Required method for Designer support - do not modify
        ///     the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._message = new System.Windows.Forms.TextBox();
            this._okay = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // _message
            //
            this._message.Location = new System.Drawing.Point(8, 8);
            this._message.Multiline = true;
            this._message.Name = "_message";
            this._message.ReadOnly = true;
            this._message.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this._message.Size = new System.Drawing.Size(552, 320);
            this._message.TabIndex = 0;
            //
            // _okay
            //
            this._okay.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okay.Location = new System.Drawing.Point(480, 334);
            this._okay.Name = "_okay";
            this._okay.Size = new System.Drawing.Size(80, 24);
            this._okay.TabIndex = 1;
            this._okay.Text = "OK";
            this._okay.Click += new System.EventHandler(this.okay_Click);
            //
            // MessageDialog
            //
            this.AcceptButton = this._okay;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(572, 365);
            this.ControlBox = false;
            this.Controls.Add(this._okay);
            this.Controls.Add(this._message);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "MessageDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Message";
            this.Load += new System.EventHandler(this.MessageDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion Windows Form Designer generated code

        private void MessageDialog_Load(object sender, EventArgs e)
        {
            Text = _mTitle;
            _message.Text = _mMessage;
            _message.Select(0, 0);
            BringToFront();

            if (_mCanIgnore)
                _okay.Text = "&Ignore";
        }

        private void okay_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}