namespace Quasi
{
    partial class Form7
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form7));
            this.num_search_label = new System.Windows.Forms.Label();
            this.num_search = new System.Windows.Forms.TextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.search_button = new System.Windows.Forms.Button();
            this.edit_post_label = new System.Windows.Forms.Label();
            this.edit_name_label = new System.Windows.Forms.Label();
            this.edit_num_label = new System.Windows.Forms.Label();
            this.edit_post = new System.Windows.Forms.TextBox();
            this.edit_namecont = new System.Windows.Forms.TextBox();
            this.edit_numphon = new System.Windows.Forms.TextBox();
            this.edit_button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // num_search_label
            // 
            this.num_search_label.AutoSize = true;
            this.num_search_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.num_search_label.Location = new System.Drawing.Point(71, 10);
            this.num_search_label.Name = "num_search_label";
            this.num_search_label.Size = new System.Drawing.Size(178, 17);
            this.num_search_label.TabIndex = 0;
            this.num_search_label.Text = "Введите номер телефона";
            this.toolTip1.SetToolTip(this.num_search_label, "Введите номер телефона контакта, который нужно редактировать");
            // 
            // num_search
            // 
            this.num_search.Location = new System.Drawing.Point(39, 33);
            this.num_search.MaxLength = 11;
            this.num_search.Name = "num_search";
            this.num_search.Size = new System.Drawing.Size(251, 20);
            this.num_search.TabIndex = 1;
            this.toolTip1.SetToolTip(this.num_search, "Введите номер телефона контакта, который нужно редактировать");
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 3000;
            this.toolTip1.BackColor = System.Drawing.Color.DarkOrange;
            this.toolTip1.InitialDelay = 200;
            this.toolTip1.IsBalloon = true;
            this.toolTip1.ReshowDelay = 100;
            // 
            // search_button
            // 
            this.search_button.Location = new System.Drawing.Point(83, 61);
            this.search_button.Name = "search_button";
            this.search_button.Size = new System.Drawing.Size(151, 24);
            this.search_button.TabIndex = 2;
            this.search_button.Text = "Найти контакт";
            this.search_button.UseVisualStyleBackColor = true;
            this.search_button.Click += new System.EventHandler(this.search_button_Click);
            // 
            // edit_post_label
            // 
            this.edit_post_label.AutoSize = true;
            this.edit_post_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.edit_post_label.Location = new System.Drawing.Point(23, 101);
            this.edit_post_label.Name = "edit_post_label";
            this.edit_post_label.Size = new System.Drawing.Size(85, 17);
            this.edit_post_label.TabIndex = 15;
            this.edit_post_label.Text = "Должность:";
            // 
            // edit_name_label
            // 
            this.edit_name_label.AutoSize = true;
            this.edit_name_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.edit_name_label.Location = new System.Drawing.Point(23, 57);
            this.edit_name_label.Name = "edit_name_label";
            this.edit_name_label.Size = new System.Drawing.Size(103, 17);
            this.edit_name_label.TabIndex = 14;
            this.edit_name_label.Text = "Имя контакта:";
            // 
            // edit_num_label
            // 
            this.edit_num_label.AutoSize = true;
            this.edit_num_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.edit_num_label.Location = new System.Drawing.Point(23, 11);
            this.edit_num_label.Name = "edit_num_label";
            this.edit_num_label.Size = new System.Drawing.Size(125, 17);
            this.edit_num_label.TabIndex = 13;
            this.edit_num_label.Text = "Номер телефона:";
            // 
            // edit_post
            // 
            this.edit_post.Location = new System.Drawing.Point(23, 121);
            this.edit_post.MaxLength = 20;
            this.edit_post.Name = "edit_post";
            this.edit_post.Size = new System.Drawing.Size(241, 20);
            this.edit_post.TabIndex = 12;
            // 
            // edit_namecont
            // 
            this.edit_namecont.Location = new System.Drawing.Point(23, 77);
            this.edit_namecont.MaxLength = 250;
            this.edit_namecont.Name = "edit_namecont";
            this.edit_namecont.Size = new System.Drawing.Size(241, 20);
            this.edit_namecont.TabIndex = 11;
            // 
            // edit_numphon
            // 
            this.edit_numphon.Location = new System.Drawing.Point(23, 33);
            this.edit_numphon.MaxLength = 11;
            this.edit_numphon.Name = "edit_numphon";
            this.edit_numphon.Size = new System.Drawing.Size(241, 20);
            this.edit_numphon.TabIndex = 10;
            // 
            // edit_button
            // 
            this.edit_button.Location = new System.Drawing.Point(63, 154);
            this.edit_button.Name = "edit_button";
            this.edit_button.Size = new System.Drawing.Size(170, 31);
            this.edit_button.TabIndex = 16;
            this.edit_button.Text = "Редактировать";
            this.edit_button.UseVisualStyleBackColor = true;
            this.edit_button.Click += new System.EventHandler(this.edit_button_Click);
            // 
            // Form7
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(322, 203);
            this.Controls.Add(this.edit_button);
            this.Controls.Add(this.edit_post_label);
            this.Controls.Add(this.edit_name_label);
            this.Controls.Add(this.edit_num_label);
            this.Controls.Add(this.edit_post);
            this.Controls.Add(this.edit_namecont);
            this.Controls.Add(this.edit_numphon);
            this.Controls.Add(this.search_button);
            this.Controls.Add(this.num_search_label);
            this.Controls.Add(this.num_search);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form7";
            this.Text = "Редактировать запись";
            this.Shown += new System.EventHandler(this.Form7_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label num_search_label;
        private System.Windows.Forms.TextBox num_search;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button search_button;
        private System.Windows.Forms.Label edit_post_label;
        private System.Windows.Forms.Label edit_name_label;
        private System.Windows.Forms.Label edit_num_label;
        public System.Windows.Forms.TextBox edit_post;
        public System.Windows.Forms.TextBox edit_namecont;
        public System.Windows.Forms.TextBox edit_numphon;
        private System.Windows.Forms.Button edit_button;

    }
}