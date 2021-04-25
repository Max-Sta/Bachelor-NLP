
namespace NLPServiceEndpoint
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.richTextBoxConsole = new System.Windows.Forms.RichTextBox();
            this.btnIBMTest = new System.Windows.Forms.Button();
            this.richTextBoxInput = new System.Windows.Forms.RichTextBox();
            this.labelOutput = new System.Windows.Forms.Label();
            this.labelInput = new System.Windows.Forms.Label();
            this.btnGoogleTest = new System.Windows.Forms.Button();
            this.btn_Clear = new System.Windows.Forms.Button();
            this.btnMicrosoftTest = new System.Windows.Forms.Button();
            this.btnAmazonTest = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // richTextBoxConsole
            // 
            this.richTextBoxConsole.Location = new System.Drawing.Point(352, 31);
            this.richTextBoxConsole.Name = "richTextBoxConsole";
            this.richTextBoxConsole.Size = new System.Drawing.Size(336, 407);
            this.richTextBoxConsole.TabIndex = 0;
            this.richTextBoxConsole.Text = "";
            // 
            // btnIBMTest
            // 
            this.btnIBMTest.Location = new System.Drawing.Point(713, 31);
            this.btnIBMTest.Name = "btnIBMTest";
            this.btnIBMTest.Size = new System.Drawing.Size(75, 23);
            this.btnIBMTest.TabIndex = 1;
            this.btnIBMTest.Text = "IBM TEST";
            this.btnIBMTest.UseVisualStyleBackColor = true;
            this.btnIBMTest.Click += new System.EventHandler(this.btnIBMTest_Click);
            // 
            // richTextBoxInput
            // 
            this.richTextBoxInput.Location = new System.Drawing.Point(10, 31);
            this.richTextBoxInput.Name = "richTextBoxInput";
            this.richTextBoxInput.Size = new System.Drawing.Size(336, 407);
            this.richTextBoxInput.TabIndex = 2;
            this.richTextBoxInput.Text = resources.GetString("richTextBoxInput.Text");
            // 
            // labelOutput
            // 
            this.labelOutput.AutoSize = true;
            this.labelOutput.Location = new System.Drawing.Point(352, 12);
            this.labelOutput.Name = "labelOutput";
            this.labelOutput.Size = new System.Drawing.Size(39, 13);
            this.labelOutput.TabIndex = 3;
            this.labelOutput.Text = "Output";
            // 
            // labelInput
            // 
            this.labelInput.AutoSize = true;
            this.labelInput.Location = new System.Drawing.Point(12, 9);
            this.labelInput.Name = "labelInput";
            this.labelInput.Size = new System.Drawing.Size(31, 13);
            this.labelInput.TabIndex = 4;
            this.labelInput.Text = "Input";
            // 
            // btnGoogleTest
            // 
            this.btnGoogleTest.Location = new System.Drawing.Point(713, 73);
            this.btnGoogleTest.Name = "btnGoogleTest";
            this.btnGoogleTest.Size = new System.Drawing.Size(75, 39);
            this.btnGoogleTest.TabIndex = 5;
            this.btnGoogleTest.Text = "GOOGLE TEST";
            this.btnGoogleTest.UseVisualStyleBackColor = true;
            this.btnGoogleTest.Click += new System.EventHandler(this.btnGoogleTest_Click);
            // 
            // btn_Clear
            // 
            this.btn_Clear.Location = new System.Drawing.Point(713, 415);
            this.btn_Clear.Name = "btn_Clear";
            this.btn_Clear.Size = new System.Drawing.Size(75, 23);
            this.btn_Clear.TabIndex = 6;
            this.btn_Clear.Text = "Clear";
            this.btn_Clear.UseVisualStyleBackColor = true;
            this.btn_Clear.Click += new System.EventHandler(this.btn_Clear_Click);
            // 
            // btnMicrosoftTest
            // 
            this.btnMicrosoftTest.Location = new System.Drawing.Point(694, 118);
            this.btnMicrosoftTest.Name = "btnMicrosoftTest";
            this.btnMicrosoftTest.Size = new System.Drawing.Size(94, 40);
            this.btnMicrosoftTest.TabIndex = 7;
            this.btnMicrosoftTest.Text = "MICROSOFT TEST";
            this.btnMicrosoftTest.UseVisualStyleBackColor = true;
            this.btnMicrosoftTest.Click += new System.EventHandler(this.btnMicrosoftTest_Click);
            // 
            // btnAmazonTest
            // 
            this.btnAmazonTest.Location = new System.Drawing.Point(694, 164);
            this.btnAmazonTest.Name = "btnAmazonTest";
            this.btnAmazonTest.Size = new System.Drawing.Size(94, 40);
            this.btnAmazonTest.TabIndex = 8;
            this.btnAmazonTest.Text = "AWS TEST";
            this.btnAmazonTest.UseVisualStyleBackColor = true;
            this.btnAmazonTest.Click += new System.EventHandler(this.btnAmazonTest_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnAmazonTest);
            this.Controls.Add(this.btnMicrosoftTest);
            this.Controls.Add(this.btn_Clear);
            this.Controls.Add(this.btnGoogleTest);
            this.Controls.Add(this.labelInput);
            this.Controls.Add(this.labelOutput);
            this.Controls.Add(this.richTextBoxInput);
            this.Controls.Add(this.btnIBMTest);
            this.Controls.Add(this.richTextBoxConsole);
            this.Name = "Form1";
            this.ShowIcon = false;
            this.Text = "NLP Service Endpoint";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBoxConsole;
        private System.Windows.Forms.Button btnIBMTest;
        private System.Windows.Forms.RichTextBox richTextBoxInput;
        private System.Windows.Forms.Label labelOutput;
        private System.Windows.Forms.Label labelInput;
        private System.Windows.Forms.Button btnGoogleTest;
        private System.Windows.Forms.Button btn_Clear;
        private System.Windows.Forms.Button btnMicrosoftTest;
        private System.Windows.Forms.Button btnAmazonTest;
    }
}

