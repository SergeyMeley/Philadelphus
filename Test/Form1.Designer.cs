namespace Test
{
    partial class Form1
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
            NewPathTextBox = new TextBox();
            LoadButton = new Button();
            button2 = new Button();
            PathesListBox = new ListBox();
            TestButton = new Button();
            SuspendLayout();
            // 
            // NewPathTextBox
            // 
            NewPathTextBox.Location = new Point(12, 12);
            NewPathTextBox.Multiline = true;
            NewPathTextBox.Name = "NewPathTextBox";
            NewPathTextBox.Size = new Size(392, 46);
            NewPathTextBox.TabIndex = 0;
            NewPathTextBox.TextChanged += NewPathTextBox_TextChanged;
            // 
            // LoadButton
            // 
            LoadButton.Location = new Point(12, 106);
            LoadButton.Name = "LoadButton";
            LoadButton.Size = new Size(392, 36);
            LoadButton.TabIndex = 1;
            LoadButton.Text = "Загрузить список";
            LoadButton.UseVisualStyleBackColor = true;
            LoadButton.Click += LoadButton_Click;
            // 
            // button2
            // 
            button2.Location = new Point(12, 64);
            button2.Name = "button2";
            button2.Size = new Size(392, 36);
            button2.TabIndex = 1;
            button2.Text = "Добавить путь";
            button2.UseVisualStyleBackColor = true;
            button2.Click += AddButton_Click;
            // 
            // PathesListBox
            // 
            PathesListBox.FormattingEnabled = true;
            PathesListBox.ItemHeight = 15;
            PathesListBox.Location = new Point(12, 148);
            PathesListBox.Name = "PathesListBox";
            PathesListBox.Size = new Size(392, 214);
            PathesListBox.TabIndex = 2;
            PathesListBox.SelectedIndexChanged += PathesListBox_SelectedIndexChanged;
            // 
            // TestButton
            // 
            TestButton.Location = new Point(88, 403);
            TestButton.Name = "TestButton";
            TestButton.Size = new Size(75, 23);
            TestButton.TabIndex = 3;
            TestButton.Text = "TEST";
            TestButton.UseVisualStyleBackColor = true;
            TestButton.Click += TestButton_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(416, 453);
            Controls.Add(TestButton);
            Controls.Add(PathesListBox);
            Controls.Add(button2);
            Controls.Add(LoadButton);
            Controls.Add(NewPathTextBox);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox NewPathTextBox;
        private Button LoadButton;
        private Button button2;
        private ListBox PathesListBox;
        private Button TestButton;
    }
}
