namespace Quasi
{
    partial class Form6
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form6));
            this.numberdellabel = new System.Windows.Forms.Label();
            this.num_del = new System.Windows.Forms.TextBox();
            this.cont_del_button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // numberdellabel
            // 
            this.numberdellabel.AutoSize = true;
            this.numberdellabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.numberdellabel.Location = new System.Drawing.Point(82, 5);
            this.numberdellabel.Name = "numberdellabel";
            this.numberdellabel.Size = new System.Drawing.Size(210, 20);
            this.numberdellabel.TabIndex = 0;
            this.numberdellabel.Text = "Введите номер телефона";
            // 
            // num_del
            // 
            this.num_del.Location = new System.Drawing.Point(68, 32);
            this.num_del.MaxLength = 11;
            this.num_del.Name = "num_del";
            this.num_del.Size = new System.Drawing.Size(237, 20);
            this.num_del.TabIndex = 1;
            // 
            // cont_del_button
            // 
            this.cont_del_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cont_del_button.Location = new System.Drawing.Point(48, 58);
            this.cont_del_button.Name = "cont_del_button";
            this.cont_del_button.Size = new System.Drawing.Size(275, 33);
            this.cont_del_button.TabIndex = 2;
            this.cont_del_button.Text = "Удалить контакт";
            this.cont_del_button.UseVisualStyleBackColor = true;
            this.cont_del_button.Click += new System.EventHandler(this.cont_del_button_Click);
            // 
            // Form6
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(372, 102);
            this.Controls.Add(this.cont_del_button);
            this.Controls.Add(this.num_del);
            this.Controls.Add(this.numberdellabel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form6";
            this.Text = "Удалить запись из справочника";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label numberdellabel;
        private System.Windows.Forms.TextBox num_del;
        private System.Windows.Forms.Button cont_del_button;

    }
}