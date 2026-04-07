namespace GravadorCensura
{
    partial class Form1
    {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Windows Form Designer

        /// <summary>
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.button1 = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lstLog = new System.Windows.Forms.ListBox();
            this.pbDisco = new System.Windows.Forms.ProgressBar();
            this.lblDiscoPorcentagem = new System.Windows.Forms.Label();
            this.lblCPU = new System.Windows.Forms.Label();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button1.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.button1.Location = new System.Drawing.Point(122, 372);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 100);
            this.button1.TabIndex = 0;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 528);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(353, 22);
            this.statusStrip1.TabIndex = 0;
            // 
            // lblStatus
            // 
            this.lblStatus.BackColor = System.Drawing.SystemColors.Control;
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(118, 17);
            this.lblStatus.Text = "toolStripStatusLabel1";
            // 
            // picPreview
            // 
            this.picPreview.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.picPreview.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("picPreview.BackgroundImage")));
            this.picPreview.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.picPreview.Location = new System.Drawing.Point(3, 3);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(323, 188);
            this.picPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picPreview.TabIndex = 1;
            this.picPreview.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.panel1.Controls.Add(this.picPreview);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(329, 194);
            this.panel1.TabIndex = 2;
            // 
            // lstLog
            // 
            this.lstLog.BackColor = System.Drawing.Color.Black;
            this.lstLog.ForeColor = System.Drawing.Color.MintCream;
            this.lstLog.FormattingEnabled = true;
            this.lstLog.Location = new System.Drawing.Point(12, 212);
            this.lstLog.Name = "lstLog";
            this.lstLog.Size = new System.Drawing.Size(329, 69);
            this.lstLog.TabIndex = 3;
            // 
            // pbDisco
            // 
            this.pbDisco.Location = new System.Drawing.Point(12, 310);
            this.pbDisco.Name = "pbDisco";
            this.pbDisco.Size = new System.Drawing.Size(326, 23);
            this.pbDisco.TabIndex = 4;
            // 
            // lblDiscoPorcentagem
            // 
            this.lblDiscoPorcentagem.AutoSize = true;
            this.lblDiscoPorcentagem.BackColor = System.Drawing.Color.Navy;
            this.lblDiscoPorcentagem.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblDiscoPorcentagem.Location = new System.Drawing.Point(15, 288);
            this.lblDiscoPorcentagem.Name = "lblDiscoPorcentagem";
            this.lblDiscoPorcentagem.Size = new System.Drawing.Size(35, 13);
            this.lblDiscoPorcentagem.TabIndex = 5;
            this.lblDiscoPorcentagem.Text = "label1";
            // 
            // lblCPU
            // 
            this.lblCPU.AutoSize = true;
            this.lblCPU.BackColor = System.Drawing.Color.Navy;
            this.lblCPU.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblCPU.Location = new System.Drawing.Point(216, 288);
            this.lblCPU.Name = "lblCPU";
            this.lblCPU.Size = new System.Drawing.Size(35, 13);
            this.lblCPU.TabIndex = 6;
            this.lblCPU.Text = "label1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(3)))), ((int)(((byte)(3)))));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(353, 550);
            this.Controls.Add(this.lblCPU);
            this.Controls.Add(this.lblDiscoPorcentagem);
            this.Controls.Add(this.pbDisco);
            this.Controls.Add(this.lstLog);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.button1);
            this.DoubleBuffered = true;
            this.Name = "Form1";
            this.Text = "Gravador de Censura";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.PictureBox picPreview;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListBox lstLog;
        private System.Windows.Forms.ProgressBar pbDisco;
        private System.Windows.Forms.Label lblDiscoPorcentagem;
        private System.Windows.Forms.Label lblCPU;
    }
}

