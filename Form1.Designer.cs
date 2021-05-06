
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageDemo = new System.Windows.Forms.TabPage();
            this.tabPageClient = new System.Windows.Forms.TabPage();
            this.textBoxKorpusOrdner = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnDurchsuchen = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxAusgabeOrdner = new System.Windows.Forms.TextBox();
            this.btnDurchsuchen2 = new System.Windows.Forms.Button();
            this.checkBoxIBM = new System.Windows.Forms.CheckBox();
            this.checkBoxMicrosoft = new System.Windows.Forms.CheckBox();
            this.checkBoxGoogle = new System.Windows.Forms.CheckBox();
            this.checkBoxAWS = new System.Windows.Forms.CheckBox();
            this.labelServiceAuswahl = new System.Windows.Forms.Label();
            this.rtbClientLog = new System.Windows.Forms.RichTextBox();
            this.labelClientLog = new System.Windows.Forms.Label();
            this.tabAuswertung = new System.Windows.Forms.TabPage();
            this.tabControl1.SuspendLayout();
            this.tabPageDemo.SuspendLayout();
            this.tabPageClient.SuspendLayout();
            this.SuspendLayout();
            // 
            // richTextBoxConsole
            // 
            this.richTextBoxConsole.Location = new System.Drawing.Point(369, 33);
            this.richTextBoxConsole.Name = "richTextBoxConsole";
            this.richTextBoxConsole.ReadOnly = true;
            this.richTextBoxConsole.Size = new System.Drawing.Size(345, 374);
            this.richTextBoxConsole.TabIndex = 0;
            this.richTextBoxConsole.Text = "";
            // 
            // btnIBMTest
            // 
            this.btnIBMTest.Location = new System.Drawing.Point(7, 408);
            this.btnIBMTest.Name = "btnIBMTest";
            this.btnIBMTest.Size = new System.Drawing.Size(75, 40);
            this.btnIBMTest.TabIndex = 1;
            this.btnIBMTest.Text = "IBM";
            this.btnIBMTest.UseVisualStyleBackColor = true;
            this.btnIBMTest.Click += new System.EventHandler(this.btnIBMTest_Click);
            // 
            // richTextBoxInput
            // 
            this.richTextBoxInput.Location = new System.Drawing.Point(7, 33);
            this.richTextBoxInput.Name = "richTextBoxInput";
            this.richTextBoxInput.Size = new System.Drawing.Size(356, 374);
            this.richTextBoxInput.TabIndex = 2;
            this.richTextBoxInput.Text = resources.GetString("richTextBoxInput.Text");
            // 
            // labelOutput
            // 
            this.labelOutput.AutoSize = true;
            this.labelOutput.Location = new System.Drawing.Point(366, 11);
            this.labelOutput.Name = "labelOutput";
            this.labelOutput.Size = new System.Drawing.Size(39, 13);
            this.labelOutput.TabIndex = 3;
            this.labelOutput.Text = "Output";
            // 
            // labelInput
            // 
            this.labelInput.AutoSize = true;
            this.labelInput.Location = new System.Drawing.Point(9, 11);
            this.labelInput.Name = "labelInput";
            this.labelInput.Size = new System.Drawing.Size(31, 13);
            this.labelInput.TabIndex = 4;
            this.labelInput.Text = "Input";
            // 
            // btnGoogleTest
            // 
            this.btnGoogleTest.Location = new System.Drawing.Point(88, 408);
            this.btnGoogleTest.Name = "btnGoogleTest";
            this.btnGoogleTest.Size = new System.Drawing.Size(75, 40);
            this.btnGoogleTest.TabIndex = 5;
            this.btnGoogleTest.Text = "GOOGLE";
            this.btnGoogleTest.UseVisualStyleBackColor = true;
            this.btnGoogleTest.Click += new System.EventHandler(this.btnGoogleTest_Click);
            // 
            // btn_Clear
            // 
            this.btn_Clear.Location = new System.Drawing.Point(639, 417);
            this.btn_Clear.Name = "btn_Clear";
            this.btn_Clear.Size = new System.Drawing.Size(75, 23);
            this.btn_Clear.TabIndex = 6;
            this.btn_Clear.Text = "Clear";
            this.btn_Clear.UseVisualStyleBackColor = true;
            this.btn_Clear.Click += new System.EventHandler(this.btn_Clear_Click);
            // 
            // btnMicrosoftTest
            // 
            this.btnMicrosoftTest.Location = new System.Drawing.Point(169, 408);
            this.btnMicrosoftTest.Name = "btnMicrosoftTest";
            this.btnMicrosoftTest.Size = new System.Drawing.Size(94, 40);
            this.btnMicrosoftTest.TabIndex = 7;
            this.btnMicrosoftTest.Text = "MICROSOFT";
            this.btnMicrosoftTest.UseVisualStyleBackColor = true;
            this.btnMicrosoftTest.Click += new System.EventHandler(this.btnMicrosoftTest_Click);
            // 
            // btnAmazonTest
            // 
            this.btnAmazonTest.Location = new System.Drawing.Point(269, 408);
            this.btnAmazonTest.Name = "btnAmazonTest";
            this.btnAmazonTest.Size = new System.Drawing.Size(94, 40);
            this.btnAmazonTest.TabIndex = 8;
            this.btnAmazonTest.Text = "AWS";
            this.btnAmazonTest.UseVisualStyleBackColor = true;
            this.btnAmazonTest.Click += new System.EventHandler(this.btnAmazonTest_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(558, 417);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 9;
            this.button1.Text = "Save";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // richTextBoxLog
            // 
            this.richTextBoxLog.Location = new System.Drawing.Point(720, 33);
            this.richTextBoxLog.Name = "richTextBoxLog";
            this.richTextBoxLog.ReadOnly = true;
            this.richTextBoxLog.Size = new System.Drawing.Size(285, 374);
            this.richTextBoxLog.TabIndex = 10;
            this.richTextBoxLog.Text = "";
            // 
            // btnClearLog
            // 
            this.btnClearLog.Location = new System.Drawing.Point(930, 417);
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
            this.label1.Location = new System.Drawing.Point(717, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(25, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Log";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageClient);
            this.tabControl1.Controls.Add(this.tabPageDemo);
            this.tabControl1.Controls.Add(this.tabAuswertung);
            this.tabControl1.Location = new System.Drawing.Point(1, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1052, 485);
            this.tabControl1.TabIndex = 13;
            // 
            // tabPageDemo
            // 
            this.tabPageDemo.Controls.Add(this.label1);
            this.tabPageDemo.Controls.Add(this.richTextBoxInput);
            this.tabPageDemo.Controls.Add(this.btnClearLog);
            this.tabPageDemo.Controls.Add(this.richTextBoxConsole);
            this.tabPageDemo.Controls.Add(this.richTextBoxLog);
            this.tabPageDemo.Controls.Add(this.btnIBMTest);
            this.tabPageDemo.Controls.Add(this.button1);
            this.tabPageDemo.Controls.Add(this.labelOutput);
            this.tabPageDemo.Controls.Add(this.btnAmazonTest);
            this.tabPageDemo.Controls.Add(this.labelInput);
            this.tabPageDemo.Controls.Add(this.btnMicrosoftTest);
            this.tabPageDemo.Controls.Add(this.btnGoogleTest);
            this.tabPageDemo.Controls.Add(this.btn_Clear);
            this.tabPageDemo.Location = new System.Drawing.Point(4, 22);
            this.tabPageDemo.Name = "tabPageDemo";
            this.tabPageDemo.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDemo.Size = new System.Drawing.Size(1044, 459);
            this.tabPageDemo.TabIndex = 0;
            this.tabPageDemo.Text = "Demo";
            this.tabPageDemo.UseVisualStyleBackColor = true;
            // 
            // tabPageClient
            // 
            this.tabPageClient.Controls.Add(this.labelClientLog);
            this.tabPageClient.Controls.Add(this.rtbClientLog);
            this.tabPageClient.Controls.Add(this.labelServiceAuswahl);
            this.tabPageClient.Controls.Add(this.checkBoxAWS);
            this.tabPageClient.Controls.Add(this.checkBoxGoogle);
            this.tabPageClient.Controls.Add(this.checkBoxMicrosoft);
            this.tabPageClient.Controls.Add(this.checkBoxIBM);
            this.tabPageClient.Controls.Add(this.btnDurchsuchen2);
            this.tabPageClient.Controls.Add(this.textBoxAusgabeOrdner);
            this.tabPageClient.Controls.Add(this.label3);
            this.tabPageClient.Controls.Add(this.btnDurchsuchen);
            this.tabPageClient.Controls.Add(this.label2);
            this.tabPageClient.Controls.Add(this.textBoxKorpusOrdner);
            this.tabPageClient.Location = new System.Drawing.Point(4, 22);
            this.tabPageClient.Name = "tabPageClient";
            this.tabPageClient.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageClient.Size = new System.Drawing.Size(1044, 459);
            this.tabPageClient.TabIndex = 1;
            this.tabPageClient.Text = "Client";
            this.tabPageClient.UseVisualStyleBackColor = true;
            // 
            // textBoxKorpusOrdner
            // 
            this.textBoxKorpusOrdner.Location = new System.Drawing.Point(6, 23);
            this.textBoxKorpusOrdner.Name = "textBoxKorpusOrdner";
            this.textBoxKorpusOrdner.ReadOnly = true;
            this.textBoxKorpusOrdner.Size = new System.Drawing.Size(545, 20);
            this.textBoxKorpusOrdner.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(115, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Korpus-Ordner wählen:";
            // 
            // btnDurchsuchen
            // 
            this.btnDurchsuchen.Location = new System.Drawing.Point(468, 49);
            this.btnDurchsuchen.Name = "btnDurchsuchen";
            this.btnDurchsuchen.Size = new System.Drawing.Size(83, 23);
            this.btnDurchsuchen.TabIndex = 2;
            this.btnDurchsuchen.Text = "Durchsuchen";
            this.btnDurchsuchen.UseVisualStyleBackColor = true;
            this.btnDurchsuchen.Click += new System.EventHandler(this.btnDurchsuchen_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 89);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(119, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Ausgabeordner wählen:";
            // 
            // textBoxAusgabeOrdner
            // 
            this.textBoxAusgabeOrdner.Location = new System.Drawing.Point(6, 105);
            this.textBoxAusgabeOrdner.Name = "textBoxAusgabeOrdner";
            this.textBoxAusgabeOrdner.ReadOnly = true;
            this.textBoxAusgabeOrdner.Size = new System.Drawing.Size(545, 20);
            this.textBoxAusgabeOrdner.TabIndex = 4;
            // 
            // btnDurchsuchen2
            // 
            this.btnDurchsuchen2.Location = new System.Drawing.Point(468, 131);
            this.btnDurchsuchen2.Name = "btnDurchsuchen2";
            this.btnDurchsuchen2.Size = new System.Drawing.Size(83, 23);
            this.btnDurchsuchen2.TabIndex = 5;
            this.btnDurchsuchen2.Text = "Durchsuchen";
            this.btnDurchsuchen2.UseVisualStyleBackColor = true;
            this.btnDurchsuchen2.Click += new System.EventHandler(this.btnDurchsuchen2_Click);
            // 
            // checkBoxIBM
            // 
            this.checkBoxIBM.AutoSize = true;
            this.checkBoxIBM.Location = new System.Drawing.Point(11, 214);
            this.checkBoxIBM.Name = "checkBoxIBM";
            this.checkBoxIBM.Size = new System.Drawing.Size(45, 17);
            this.checkBoxIBM.TabIndex = 6;
            this.checkBoxIBM.Text = "IBM";
            this.checkBoxIBM.UseVisualStyleBackColor = true;
            // 
            // checkBoxMicrosoft
            // 
            this.checkBoxMicrosoft.AutoSize = true;
            this.checkBoxMicrosoft.Location = new System.Drawing.Point(11, 260);
            this.checkBoxMicrosoft.Name = "checkBoxMicrosoft";
            this.checkBoxMicrosoft.Size = new System.Drawing.Size(69, 17);
            this.checkBoxMicrosoft.TabIndex = 7;
            this.checkBoxMicrosoft.Text = "Microsoft";
            this.checkBoxMicrosoft.UseVisualStyleBackColor = true;
            // 
            // checkBoxGoogle
            // 
            this.checkBoxGoogle.AutoSize = true;
            this.checkBoxGoogle.Location = new System.Drawing.Point(11, 237);
            this.checkBoxGoogle.Name = "checkBoxGoogle";
            this.checkBoxGoogle.Size = new System.Drawing.Size(60, 17);
            this.checkBoxGoogle.TabIndex = 8;
            this.checkBoxGoogle.Text = "Google";
            this.checkBoxGoogle.UseVisualStyleBackColor = true;
            // 
            // checkBoxAWS
            // 
            this.checkBoxAWS.AutoSize = true;
            this.checkBoxAWS.Location = new System.Drawing.Point(11, 283);
            this.checkBoxAWS.Name = "checkBoxAWS";
            this.checkBoxAWS.Size = new System.Drawing.Size(51, 17);
            this.checkBoxAWS.TabIndex = 9;
            this.checkBoxAWS.Text = "AWS";
            this.checkBoxAWS.UseVisualStyleBackColor = true;
            // 
            // labelServiceAuswahl
            // 
            this.labelServiceAuswahl.AutoSize = true;
            this.labelServiceAuswahl.Location = new System.Drawing.Point(8, 196);
            this.labelServiceAuswahl.Name = "labelServiceAuswahl";
            this.labelServiceAuswahl.Size = new System.Drawing.Size(111, 13);
            this.labelServiceAuswahl.TabIndex = 10;
            this.labelServiceAuswahl.Text = "Service(s) auswählen:";
            // 
            // rtbClientLog
            // 
            this.rtbClientLog.Location = new System.Drawing.Point(625, 49);
            this.rtbClientLog.Name = "rtbClientLog";
            this.rtbClientLog.ReadOnly = true;
            this.rtbClientLog.Size = new System.Drawing.Size(370, 371);
            this.rtbClientLog.TabIndex = 11;
            this.rtbClientLog.Text = "";
            // 
            // labelClientLog
            // 
            this.labelClientLog.AutoSize = true;
            this.labelClientLog.Location = new System.Drawing.Point(622, 26);
            this.labelClientLog.Name = "labelClientLog";
            this.labelClientLog.Size = new System.Drawing.Size(28, 13);
            this.labelClientLog.TabIndex = 12;
            this.labelClientLog.Text = "Log:";
            // 
            // tabAuswertung
            // 
            this.tabAuswertung.Location = new System.Drawing.Point(4, 22);
            this.tabAuswertung.Name = "tabAuswertung";
            this.tabAuswertung.Padding = new System.Windows.Forms.Padding(3);
            this.tabAuswertung.Size = new System.Drawing.Size(1044, 459);
            this.tabAuswertung.TabIndex = 2;
            this.tabAuswertung.Text = "Auswertung";
            this.tabAuswertung.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1056, 491);
            this.Controls.Add(this.tabControl1);
            this.Name = "Form1";
            this.ShowIcon = false;
            this.Text = "NLP Service Endpoint";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPageDemo.ResumeLayout(false);
            this.tabPageDemo.PerformLayout();
            this.tabPageClient.ResumeLayout(false);
            this.tabPageClient.PerformLayout();
            this.ResumeLayout(false);

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
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageClient;
        private System.Windows.Forms.TabPage tabPageDemo;
        private System.Windows.Forms.Label labelServiceAuswahl;
        private System.Windows.Forms.CheckBox checkBoxAWS;
        private System.Windows.Forms.CheckBox checkBoxGoogle;
        private System.Windows.Forms.CheckBox checkBoxMicrosoft;
        private System.Windows.Forms.CheckBox checkBoxIBM;
        private System.Windows.Forms.Button btnDurchsuchen2;
        private System.Windows.Forms.TextBox textBoxAusgabeOrdner;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnDurchsuchen;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxKorpusOrdner;
        private System.Windows.Forms.Label labelClientLog;
        private System.Windows.Forms.RichTextBox rtbClientLog;
        private System.Windows.Forms.TabPage tabAuswertung;
    }
}

