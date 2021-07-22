
namespace Firestar
{
    partial class Status
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.NbBlockedIps = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(84, 65);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(206, 30);
            this.label1.TabIndex = 0;
            this.label1.Text = "Blocked IP addresses";
            // 
            // NbBlockedIps
            // 
            this.NbBlockedIps.AutoSize = true;
            this.NbBlockedIps.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.NbBlockedIps.Location = new System.Drawing.Point(84, 111);
            this.NbBlockedIps.Name = "NbBlockedIps";
            this.NbBlockedIps.Size = new System.Drawing.Size(225, 62);
            this.NbBlockedIps.TabIndex = 1;
            this.NbBlockedIps.Text = "Unknown";
            // 
            // Status
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 30F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(414, 247);
            this.Controls.Add(this.NbBlockedIps);
            this.Controls.Add(this.label1);
            this.Name = "Status";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label NbBlockedIps;
    }
}

