using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IBM.Cloud.SDK.Core.Authentication.Iam;
using IBM.Watson.NaturalLanguageUnderstanding.v1;
using IBM.Watson.NaturalLanguageUnderstanding.v1.Model;
using Google.Cloud.Language.V1;
using System.Collections;
using Azure;
using System.Globalization;
using Azure.AI.TextAnalytics;
using Amazon.Comprehend;
using Amazon.Comprehend.Model;


namespace NLPServiceEndpoint
{
    public partial class Form1 : Form
    {
        private static readonly AzureKeyCredential credentials = new AzureKeyCredential(Properties.Settings.Default.microsoft_api_key);
        private static readonly Uri endpoint = new Uri(Properties.Settings.Default.microsoft_endpoint);
        public Form1()
        {
            InitializeComponent();
        }

        private void btnIBMTest_Click(object sender, EventArgs e)
        {
            disableButtons();

            IamAuthenticator authenticator = new IamAuthenticator(
            apikey: Properties.Settings.Default.ibm_api_key
            );

            NaturalLanguageUnderstandingService naturalLanguageUnderstanding = new NaturalLanguageUnderstandingService("2020-08-01", authenticator);
            naturalLanguageUnderstanding.SetServiceUrl(Properties.Settings.Default.ibm_service_url);

            var features = new Features()
            {
                //Keywords = new KeywordsOptions()
                //{
                //    Limit = 2,
                //    Sentiment = true,
                //    Emotion = true
                //},
                Entities = new EntitiesOptions()
                {
                    Sentiment = true,
                    Limit = 5
                }
            };

            var result = naturalLanguageUnderstanding.Analyze(
                features: features,
                text: richTextBoxInput.Text
                );
            //IBM is an American multinational technology company headquartered in Armonk, New York, United States, with operations in over 170 countries.
            //Console.WriteLine(result.Response);

            richTextBoxConsole.Text = result.Response;
            richTextBoxLog.Text += "IBM - Done" + "\n";

            enableButtons();

            return;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void WriteEntities(IEnumerable<Google.Cloud.Language.V1.Entity> entities)
        {
            Console.WriteLine("Entities:");
            foreach (var entity in entities)
            {
                richTextBoxConsole.Text += "\n" + $"\tName: {entity.Name}";
                richTextBoxConsole.Text += "\n" + $"\tType: {entity.Type}";
                richTextBoxConsole.Text += "\n" + $"\tSalience: {entity.Salience}";
                richTextBoxConsole.Text += "\n" + "\tMentions:";
                foreach (var mention in entity.Mentions)
                    richTextBoxConsole.Text += "\n" + $"\t\t{mention.Text.BeginOffset}: {mention.Text.Content}";
                richTextBoxConsole.Text += "\n" + "\tMetadata:";
                foreach (var keyval in entity.Metadata)
                {
                    richTextBoxConsole.Text += "\n" + $"\t\t{keyval.Key}: {keyval.Value}";
                }

                Console.WriteLine($"\tName: {entity.Name}");
                Console.WriteLine($"\tType: {entity.Type}");
                Console.WriteLine($"\tSalience: {entity.Salience}");
                Console.WriteLine("\tMentions:");
                foreach (var mention in entity.Mentions)
                    Console.WriteLine($"\t\t{mention.Text.BeginOffset}: {mention.Text.Content}");
                Console.WriteLine("\tMetadata:");
                foreach (var keyval in entity.Metadata)
                {
                    Console.WriteLine($"\t\t{keyval.Key}: {keyval.Value}");
                }
            }
        }

        private void btnGoogleTest_Click(object sender, EventArgs e)
        {
            disableButtons();

            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Properties.Settings.Default.google_api_path);

            var client = LanguageServiceClient.Create();
            var response = client.AnalyzeEntities(new Document()
            {
                Content = richTextBoxInput.Text,
                Type = Document.Types.Type.PlainText
            });
            WriteEntities(response.Entities);

            richTextBoxLog.Text += "Google - Done" + "\n";

            enableButtons();
        }

        private void disableButtons()
        {
            btnAmazonTest.Enabled = false;
            btnIBMTest.Enabled = false;
            btnMicrosoftTest.Enabled = false;
            btnGoogleTest.Enabled = false;
        }
        private void enableButtons()
        {
            btnAmazonTest.Enabled = true;
            btnIBMTest.Enabled = true;
            btnMicrosoftTest.Enabled = true;
            btnGoogleTest.Enabled = true;
        }

        private void btn_Clear_Click(object sender, EventArgs e)
        {
            richTextBoxConsole.Text = "";
        }

        private void btnMicrosoftTest_Click(object sender, EventArgs e)
        {
            disableButtons();

            var client = new TextAnalyticsClient(endpoint, credentials);
            // You will implement these methods later in the quickstart.
            //SentimentAnalysisExample(client);
            //SentimentAnalysisWithOpinionMiningExample(client);
            //LanguageDetectionExample(client);
            EntityRecognitionExample(client);
            //EntityLinkingExample(client);
            //RecognizePIIExample(client);
            //KeyPhraseExtractionExample(client);
            richTextBoxLog.Text += "Microsoft - Done" + "\n";
            enableButtons();
        }

        private void EntityRecognitionExample(TextAnalyticsClient client)
        {
            var response = client.RecognizeEntities(richTextBoxInput.Text);
            Console.WriteLine("Named Entities:");
            richTextBoxConsole.Text += "Named Entities:\n";
            foreach (var entity in response.Value)
            {
                richTextBoxConsole.Text += $"\tText: {entity.Text},\tCategory: {entity.Category},\tSub-Category: {entity.SubCategory}" + "\n";
                richTextBoxConsole.Text += $"\t\tScore: {entity.ConfidenceScore:F2},\tLength: {entity.Length},\tOffset: {entity.Offset}\n"+"\n";

                Console.WriteLine($"\tText: {entity.Text},\tCategory: {entity.Category},\tSub-Category: {entity.SubCategory}");
                Console.WriteLine($"\t\tScore: {entity.ConfidenceScore:F2},\tLength: {entity.Length},\tOffset: {entity.Offset}\n");
            }
        }

        private void btnAmazonTest_Click(object sender, EventArgs e)
        {
            disableButtons();
            string text = richTextBoxInput.Text;

            AmazonComprehendClient comprehendClient = new AmazonComprehendClient(Amazon.RegionEndpoint.EUCentral1);

            // Call DetectEntities API
            richTextBoxConsole.Text += "" + "\n";
            Console.WriteLine("Calling DetectEntities\n");
            DetectEntitiesRequest detectEntitiesRequest = new DetectEntitiesRequest()
            {
                Text = text,
                LanguageCode = "en"
            };
            DetectEntitiesResponse detectEntitiesResponse = comprehendClient.DetectEntities(detectEntitiesRequest);
            foreach (Amazon.Comprehend.Model.Entity ent in detectEntitiesResponse.Entities)
            {
                richTextBoxConsole.Text += ("Text: { 1}, Type: { 1}, Score: { 2}, BeginOffset: { 3}, EndOffset: { 4}",
                        ent.Text, ent.Type, ent.Score, ent.BeginOffset, ent.EndOffset) + "\n";
                Console.WriteLine("Text: {1}, Type: {1}, Score: {2}, BeginOffset: {3}, EndOffset: {4}",
                        ent.Text, ent.Type, ent.Score, ent.BeginOffset, ent.EndOffset);
            }
            richTextBoxLog.Text += "AWS - Done" + "\n";
            enableButtons();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog SaveDia = new SaveFileDialog();
            if(SaveDia.ShowDialog() == DialogResult.OK)
            {

            }
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            richTextBoxLog.Text = "";
        }
    }
}
