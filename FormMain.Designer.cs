namespace WinFormsAppDbDemo
{
    partial class FormMain
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
            listView1 = new ListView();
            btnAdd = new Button();
            btnDel = new Button();
            dateTimePicker1 = new DateTimePicker();
            btnTest = new Button();
            btnClear = new Button();
            SuspendLayout();
            // 
            // listView1
            // 
            listView1.Location = new Point(12, 12);
            listView1.Name = "listView1";
            listView1.Size = new Size(1474, 720);
            listView1.TabIndex = 0;
            listView1.UseCompatibleStateImageBehavior = false;
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(272, 738);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(150, 46);
            btnAdd.TabIndex = 1;
            btnAdd.Text = "生成";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // btnDel
            // 
            btnDel.Location = new Point(428, 738);
            btnDel.Name = "btnDel";
            btnDel.Size = new Size(150, 46);
            btnDel.TabIndex = 2;
            btnDel.Text = "删除";
            btnDel.UseVisualStyleBackColor = true;
            btnDel.Click += btnDel_Click;
            // 
            // dateTimePicker1
            // 
            dateTimePicker1.Location = new Point(12, 742);
            dateTimePicker1.Name = "dateTimePicker1";
            dateTimePicker1.Size = new Size(226, 38);
            dateTimePicker1.TabIndex = 3;
            dateTimePicker1.ValueChanged += dateTimePicker1_ValueChanged;
            // 
            // btnTest
            // 
            btnTest.Location = new Point(584, 738);
            btnTest.Name = "btnTest";
            btnTest.Size = new Size(150, 46);
            btnTest.TabIndex = 4;
            btnTest.Text = "并发";
            btnTest.UseVisualStyleBackColor = true;
            btnTest.Click += btnTest_Click;
            // 
            // btnClear
            // 
            btnClear.Location = new Point(740, 740);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(150, 46);
            btnClear.TabIndex = 5;
            btnClear.Text = "清空";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(14F, 31F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1498, 915);
            Controls.Add(btnClear);
            Controls.Add(btnTest);
            Controls.Add(dateTimePicker1);
            Controls.Add(btnDel);
            Controls.Add(btnAdd);
            Controls.Add(listView1);
            Name = "FormMain";
            Text = "数据库并发测试";
            Load += Form1_Load;
            ResumeLayout(false);
        }

        #endregion

        private ListView listView1;
        private Button btnAdd;
        private Button btnDel;
        private DateTimePicker dateTimePicker1;
        private Button btnTest;
        private Button btnClear;
    }
}
