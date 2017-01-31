namespace EpiInfoProjectMetadataAdmin
{
    partial class ViewMetaDataResponse
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
            this.txtVewMetadata = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // txtVewMetadata
            // 
            this.txtVewMetadata.Location = new System.Drawing.Point(27, 31);
            this.txtVewMetadata.Name = "txtVewMetadata";
            this.txtVewMetadata.ReadOnly = true;
            this.txtVewMetadata.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtVewMetadata.Size = new System.Drawing.Size(615, 534);
            this.txtVewMetadata.TabIndex = 0;
            this.txtVewMetadata.Text = "";
            // 
            // ViewMetaDataResponse
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(682, 590);
            this.Controls.Add(this.txtVewMetadata);
            this.Name = "ViewMetaDataResponse";
            this.Text = "ViewMetaDataResponse";
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.RichTextBox txtVewMetadata;
    }
}