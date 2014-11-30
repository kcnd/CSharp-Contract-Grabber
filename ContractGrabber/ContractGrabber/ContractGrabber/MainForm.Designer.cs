namespace ContractGrabber
{
    partial class MainForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.buttonChocose = new System.Windows.Forms.Button();
            this.buttonGrab = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(23, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(348, 26);
            this.label1.TabIndex = 0;
            this.label1.Text = "Output directory:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonChocose
            // 
            this.buttonChocose.Location = new System.Drawing.Point(393, 26);
            this.buttonChocose.Name = "buttonChocose";
            this.buttonChocose.Size = new System.Drawing.Size(93, 26);
            this.buttonChocose.TabIndex = 1;
            this.buttonChocose.Text = "Choose";
            this.buttonChocose.UseVisualStyleBackColor = true;
            this.buttonChocose.Click += new System.EventHandler(this.buttonChoose_Click);
            // 
            // buttonGrab
            // 
            this.buttonGrab.Location = new System.Drawing.Point(393, 67);
            this.buttonGrab.Name = "buttonGrab";
            this.buttonGrab.Size = new System.Drawing.Size(93, 26);
            this.buttonGrab.TabIndex = 2;
            this.buttonGrab.Text = "Grab";
            this.buttonGrab.UseVisualStyleBackColor = true;
            this.buttonGrab.Click += new System.EventHandler(this.buttonGrab_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(510, 135);
            this.Controls.Add(this.buttonGrab);
            this.Controls.Add(this.buttonChocose);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Contract Grabber";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonChocose;
        private System.Windows.Forms.Button buttonGrab;
    }
}

