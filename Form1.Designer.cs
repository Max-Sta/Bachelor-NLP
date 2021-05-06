
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
            this.button1 = new System.Windows.Forms.Button();
            this.richTextBoxLog = new System.Windows.Forms.RichTextBox();
            this.btnClearLog = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // richTextBoxConsole
            // 
            this.richTextBoxConsole.Location = new System.Drawing.Point(372, 31);
            this.richTextBoxConsole.Name = "richTextBoxConsole";
            this.richTextBoxConsole.Size = new System.Drawing.Size(345, 374);
            this.richTextBoxConsole.TabIndex = 0;
            this.richTextBoxConsole.Text = "";
            // 
            // btnIBMTest
            // 
            this.btnIBMTest.Location = new System.Drawing.Point(10, 406);
            this.btnIBMTest.Name = "btnIBMTest";
            this.btnIBMTest.Size = new System.Drawing.Size(75, 40);
            this.btnIBMTest.TabIndex = 1;
            this.btnIBMTest.Text = "IBM";
            this.btnIBMTest.UseVisualStyleBackColor = true;
            this.btnIBMTest.Click += new System.EventHandler(this.btnIBMTest_Click);
            // 
            // richTextBoxInput
            // 
            this.richTextBoxInput.Location = new System.Drawing.Point(10, 31);
            this.richTextBoxInput.Name = "richTextBoxInput";
            this.richTextBoxInput.Size = new System.Drawing.Size(356, 374);
            this.richTextBoxInput.TabIndex = 2;
            this.richTextBoxInput.Text = resources.GetString("richTextBoxInput.Text");
            // 
            // labelOutput
            // 
            this.labelOutput.AutoSize = true;
            this.labelOutput.Location = new System.Drawing.Point(369, 9);
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
            this.btnGoogleTest.Location = new System.Drawing.Point(91, 406);
            this.btnGoogleTest.Name = "btnGoogleTest";
            this.btnGoogleTest.Size = new System.Drawing.Size(75, 40);
            this.btnGoogleTest.TabIndex = 5;
            this.btnGoogleTest.Text = "GOOGLE";
            this.btnGoogleTest.UseVisualStyleBackColor = true;
            this.btnGoogleTest.Click += new System.EventHandler(this.btnGoogleTest_Click);
            // 
            // btn_Clear
            // 
            this.btn_Clear.Location = new System.Drawing.Point(642, 415);
            this.btn_Clear.Name = "btn_Clear";
            this.btn_Clear.Size = new System.Drawing.Size(75, 23);
            this.btn_Clear.TabIndex = 6;
            this.btn_Clear.Text = "Clear";
            this.btn_Clear.UseVisualStyleBackColor = true;
            this.btn_Clear.Click += new System.EventHandler(this.btn_Clear_Click);
            // 
            // btnMicrosoftTest
            // 
            this.btnMicrosoftTest.Location = new System.Drawing.Point(172, 406);
            this.btnMicrosoftTest.Name = "btnMicrosoftTest";
            this.btnMicrosoftTest.Size = new System.Drawing.Size(94, 40);
            this.btnMicrosoftTest.TabIndex = 7;
            this.btnMicrosoftTest.Text = "MICROSOFT";
            this.btnMicrosoftTest.UseVisualStyleBackColor = true;
            this.btnMicrosoftTest.Click += new System.EventHandler(this.btnMicrosoftTest_Click);
            // 
            // btnAmazonTest
            // 
            this.btnAmazonTest.Location = new System.Drawing.Point(272, 406);
            this.btnAmazonTest.Name = "btnAmazonTest";
            this.btnAmazonTest.Size = new System.Drawing.Size(94, 40);
            this.btnAmazonTest.TabIndex = 8;
            this.btnAmazonTest.Text = "AWS";
            this.btnAmazonTest.UseVisualStyleBackColor = true;
            this.btnAmazonTest.Click += new System.EventHandler(this.btnAmazonTest_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(561, 415);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 9;
            this.button1.Text = "Save";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // richTextBoxLog
            // 
            this.richTextBoxLog.Location = new System.Drawing.Point(723, 31);
            this.richTextBoxLog.Name = "richTextBoxLog";
            this.richTextBoxLog.Size = new System.Drawing.Size(285, 374);
            this.richTextBoxLog.TabIndex = 10;
            this.richTextBoxLog.Text = "";
            // 
            // btnClearLog
            // 
            this.btnClearLog.Location = new System.Drawing.Point(933, 415);
            this.btnClearLog.Name = "btnClearLog";
            this.btnClearLog.Size = new System.Drawing.Size(75, 23);
            this.btnClearLog.TabIndex = 11;
            this.btnClearLog.Text = "Clear Log";
            this.btnClearLog.UseVisualStyleBackColor = true;
            this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(720, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(25, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Log";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1020, 450);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnClearLog);
            this.Controls.Add(this.richTextBoxLog);
            this.Controls.Add(this.button1);
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
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.RichTextBox richTextBoxLog;
        private System.Windows.Forms.Button btnClearLog;
        private System.Windows.Forms.Label label1;
    }
}

