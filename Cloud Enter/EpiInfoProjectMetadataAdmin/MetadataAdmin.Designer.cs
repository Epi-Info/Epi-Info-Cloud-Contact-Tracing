namespace Epi.Cloud.EpiInfoProjectMetadataAdmin
{
    partial class MetadataAdmin
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
            this.uploadBlob = new System.Windows.Forms.Button();
            this.deleteBlob = new System.Windows.Forms.Button();
            this.lblEnvironment = new System.Windows.Forms.Label();
            this.comboEnvironment = new System.Windows.Forms.ComboBox();
            this.btnViewBlob = new System.Windows.Forms.Button();
            this.lstBlob = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // uploadBlob
            // 
            this.uploadBlob.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.uploadBlob.Font = new System.Drawing.Font("Calibri", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.uploadBlob.Location = new System.Drawing.Point(500, 235);
            this.uploadBlob.Name = "uploadBlob";
            this.uploadBlob.Size = new System.Drawing.Size(142, 31);
            this.uploadBlob.TabIndex = 7;
            this.uploadBlob.Text = "Upload Blob";
            this.uploadBlob.UseVisualStyleBackColor = false;
            this.uploadBlob.Click += new System.EventHandler(this.uploadBlob_Click);
            // 
            // deleteBlob
            // 
            this.deleteBlob.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.deleteBlob.Enabled = false;
            this.deleteBlob.Font = new System.Drawing.Font("Calibri", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.deleteBlob.Location = new System.Drawing.Point(339, 235);
            this.deleteBlob.Name = "deleteBlob";
            this.deleteBlob.Size = new System.Drawing.Size(132, 31);
            this.deleteBlob.TabIndex = 6;
            this.deleteBlob.Text = "Delete Blob";
            this.deleteBlob.UseVisualStyleBackColor = false;
            this.deleteBlob.Click += new System.EventHandler(this.deleteBlob_Click);
            // 
            // lblEnvironment
            // 
            this.lblEnvironment.AutoSize = true;
            this.lblEnvironment.Font = new System.Drawing.Font("Calibri", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEnvironment.Location = new System.Drawing.Point(266, 31);
            this.lblEnvironment.Name = "lblEnvironment";
            this.lblEnvironment.Size = new System.Drawing.Size(88, 18);
            this.lblEnvironment.TabIndex = 5;
            this.lblEnvironment.Text = "Environment";
            // 
            // comboEnvironment
            // 
            this.comboEnvironment.Font = new System.Drawing.Font("Calibri", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboEnvironment.FormattingEnabled = true;
            this.comboEnvironment.Items.AddRange(new object[] {
            "CDCDev",
            "CDCQA",
            "Ananth",
            "Garry"});
            this.comboEnvironment.Location = new System.Drawing.Point(383, 23);
            this.comboEnvironment.Name = "comboEnvironment";
            this.comboEnvironment.Size = new System.Drawing.Size(196, 26);
            this.comboEnvironment.TabIndex = 4;
            this.comboEnvironment.Text = "Select";
            this.comboEnvironment.SelectedIndexChanged += new System.EventHandler(this.comboEnvironment_SelectedIndexChanged);
            // 
            // btnViewBlob
            // 
            this.btnViewBlob.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnViewBlob.Enabled = false;
            this.btnViewBlob.Font = new System.Drawing.Font("Calibri", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnViewBlob.Location = new System.Drawing.Point(189, 235);
            this.btnViewBlob.Name = "btnViewBlob";
            this.btnViewBlob.Size = new System.Drawing.Size(132, 31);
            this.btnViewBlob.TabIndex = 9;
            this.btnViewBlob.Text = "View Blob";
            this.btnViewBlob.UseVisualStyleBackColor = false;
            this.btnViewBlob.Click += new System.EventHandler(this.btnViewBlob_Click);
            // 
            // lstBlob
            // 
            this.lstBlob.FormattingEnabled = true;
            this.lstBlob.HorizontalScrollbar = true;
            this.lstBlob.Location = new System.Drawing.Point(156, 78);
            this.lstBlob.Name = "lstBlob";
            this.lstBlob.Size = new System.Drawing.Size(629, 95);
            this.lstBlob.TabIndex = 10;
            this.lstBlob.SelectedIndexChanged += new System.EventHandler(this.lstBlob_SelectedIndexChanged);
            // 
            // MetadataAdmin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(832, 303);
            this.Controls.Add(this.lstBlob);
            this.Controls.Add(this.btnViewBlob);
            this.Controls.Add(this.uploadBlob);
            this.Controls.Add(this.deleteBlob);
            this.Controls.Add(this.lblEnvironment);
            this.Controls.Add(this.comboEnvironment);
            this.Name = "MetadataAdmin";
            this.Text = "Metadata Admin";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button uploadBlob;
        private System.Windows.Forms.Button deleteBlob;
        private System.Windows.Forms.Label lblEnvironment;
        private System.Windows.Forms.ComboBox comboEnvironment;
        private System.Windows.Forms.Button btnViewBlob;
        private System.Windows.Forms.ListBox lstBlob;
    }
}

