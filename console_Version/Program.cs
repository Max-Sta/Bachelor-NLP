using System;
using System.Collections;
using IBM.Cloud.SDK.Core.Authentication.Iam;
using IBM.Watson.NaturalLanguageUnderstanding.v1;
using IBM.Watson.NaturalLanguageUnderstanding.v1.Model;
using Google.Cloud.Language.V1;
using Azure;
using Azure.AI.TextAnalytics;
using System.Globalization;
using Amazon;
using Amazon.Comprehend;
using Amazon.Comprehend.Model;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Newtonsoft.Json;

namespace NLPServiceEndpoint_Console_Ver
{
    public class Program
    {
        private static AzureKeyCredential azureCredentials;
        private static Uri azureEndpoint;
        private static string microsoft_standort;
        private static string ibm_api_key;
        private static string ibm_service_url;
        private static string google_api_path;
        static void Main(string[] args)
        {
            if (LoadCredentials() == 0)
            {
                for (int i = 0; i < 100000; i++)
                {
                    Console.WriteLine("Please choose (d)emo, (a)nalyseDemo, (f)ullRun or (q)uit\n");
                    string read = Console.ReadLine();
                    switch (read)
                    {
                        case "d":
                            DemoRun("d");
                            break;
                        case "a":
                            DemoRun("a");
                            break;
                        case "f":
                            FullRun();
                            break;
                        case "q":
                            i += 1000000;
                            break;
                        case "jsonTest":
                            ConvertIBMJson(File.ReadAllText("C:\\MAX\\$INFORMATIK\\C - Bachelor\\Datenschutzerklärungen\\bvg.de_short - ResponseIBM.json"));
                            break;
                        default:
                            Console.WriteLine("Command not recognized");
                            break;
                    }
                }
            }
        }
        public static int LoadCredentials()
        {
            if (File.Exists("nlpConfig.config"))
            {
                var fileName = "nlpConfig.config";
                using var sr = new StreamReader(fileName);
                string[] lines = File.ReadAllLines(fileName);
                ibm_api_key = lines[0];
                ibm_service_url = lines[1];
                google_api_path = lines[2];
                azureCredentials = new AzureKeyCredential(lines[3]);
                azureEndpoint = new Uri(lines[4]);
                microsoft_standort = lines[5];
                Console.WriteLine("Config File Found");
                return 0;
            }
            Console.WriteLine("Config file not Found, please put a file by the name of nlpConfig.config in the same folder as the program.");
            return 1;
        }
        public static CategorizedEntityCollection MicrosoftEntityRecognize(string input)
        {
            var client = new TextAnalyticsClient(azureEndpoint, azureCredentials);

            var response = client.RecognizeEntities(input);
            return response.Value;
        }
        public static void PrintMicrosoftEntities(CategorizedEntityCollection response)
        {
            string result = "";
            foreach (var entity in response)
            {
                result += $"\tText: {entity.Text},\tPosition: {entity.Offset}-{entity.Offset+entity.Length},\tCategory: {entity.Category},\tSub-Category: {entity.SubCategory}" + "\n";
            }
            if (String.IsNullOrEmpty(result)) { Console.Write("Result empty"); }
            Console.WriteLine(result);
            Console.WriteLine("Microsoft - Done");
            return;
        }
        public static string IBMEntityRecognize(string input)
        {
            IamAuthenticator authenticator = new IamAuthenticator(
           apikey: ibm_api_key
           );

            NaturalLanguageUnderstandingService naturalLanguageUnderstanding = new NaturalLanguageUnderstandingService("2020-08-01", authenticator);
            naturalLanguageUnderstanding.SetServiceUrl(ibm_service_url);

            var result = naturalLanguageUnderstanding.Analyze(
                text: input,
                features: new Features()
                {
                    Entities = new EntitiesOptions()
                    {
                        Sentiment = true,
                        Limit = 50,
                        Mentions = true
                    }
                }
            ); 
            return result.Response;
        }
        public static AnalyzeEntitiesResponse GoogleEntityRecognize(string input)
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", google_api_path);

            var client = LanguageServiceClient.Create();
            var response = client.AnalyzeEntities(new Document()
            {
                Content = input,
                Type = Document.Types.Type.PlainText
            });
            return response;
        }
        private static void GoogleWriteEntities(AnalyzeEntitiesResponse response)
        {
            var entities = response.Entities;
            Console.WriteLine("Entities:");
            foreach (var entity in entities)
            {
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
            Console.WriteLine("Google - Done");
        }
        public static DetectEntitiesResponse AwsEntityRecognize(string inputText)
        {

            var client = new AmazonComprehendClient(Amazon.RegionEndpoint.EUCentral1);
            DetectEntitiesRequest detectEntitiesRequest = new DetectEntitiesRequest
            {
                Text = inputText,
                LanguageCode = "en"
            };
            return client.DetectEntitiesAsync(detectEntitiesRequest).Result;
        }

        public static void PrintAWSEntityResponse(DetectEntitiesResponse response)
        {
            foreach (var e in response.Entities.ToList())
            {
                Console.WriteLine("Text: {0}, Type: {1}, Score: {2}, BeginOffset: {3}, EndOffset: {4}",
                    e.Text, e.Type, e.Score, e.BeginOffset, e.EndOffset);
            }

            Console.WriteLine("Done");
        }
        private static void AnalyseAWSEntityResponse(DetectEntitiesResponse response)
        {
            return;
        }
        private static void AnalyseIBMEntityResponse(string response)
        {
            TIL ibm_til = new TIL();
            try
            {
                IBMEntity result = JsonConvert.DeserializeObject<IBMEntity>(response);
                string earliestOrga = "";
                int earliestOrgaStart = int.MaxValue;
                foreach (var entity in result.entities)
                {
                    if (String.Compare(entity.type, "Organization") == 0)
                    {
                        foreach (var mention in entity.mentions)
                        {
                            if (earliestOrgaStart > mention.location[0])
                            {
                                earliestOrgaStart = mention.location[0];
                                earliestOrga = entity.text;
                            }
                        }
                    }
                }
                Console.WriteLine("The earliest Organization is: " + earliestOrga);
            }
            catch (Exception)
            {
                Console.WriteLine("Error parsing response JSON");
            }
            return;
        }
        private static void AnalyseMicrosoftEntityResponse(CategorizedEntityCollection response)
        {
            return;
        }
        private static void AnalyseGoogleEntityResponse(AnalyzeEntitiesResponse response)
        {
            return;
        }

        public static void DemoRun(string DeMode)
        {
            string inputLine = "";
            string fileContent = "";
            string read = "";

            for (int i = 0; i < 100000; i++)
            {

                Console.WriteLine("Please choose (A)WS, (M)icrosoft, (G)oogle, (I)BM or (q)uit\n");
                read = Console.ReadLine();
                inputLine = "";
                fileContent = "";
                switch (read)
                {
                    case "A":   //CASE AWS
                        if (DeMode == "a")
                        {
                            Console.WriteLine("Please insert the filename of a text to be analysed\n");
                        }
                        else
                        {
                            Console.WriteLine("Please insert text or filename of a text to be analysed\n");
                        }
                        inputLine = Console.ReadLine();
                        if (File.Exists(inputLine))
                        {
                            fileContent = File.ReadAllText(inputLine);
                            var response = AwsEntityRecognize(fileContent);
                            PrintAWSEntityResponse(response);
                            if (DeMode == "a")
                            {
                                AnalyseAWSEntityResponse(response);
                            }
                        }
                        else
                        {
                            var response = AwsEntityRecognize(inputLine);
                            PrintAWSEntityResponse(response);
                            if (DeMode == "a")
                            {
                                Console.WriteLine("Note that the response wasn't analysed due to no file with the given name having been found.");
                            }
                        }
                        //Console.WriteLine("Would you like to save the answer to a file (y/n)?");
                        //read = Console.ReadLine();
                        return;

                    case "M":   //CASE MICROSOFT AZURE
                        if (DeMode == "a")
                        {
                            Console.WriteLine("Please insert the filename of a text to be analysed\n");
                        }
                        else
                        {
                            Console.WriteLine("Please insert text or filename of a text to be analysed\n");
                        }
                        inputLine = Console.ReadLine();
                        if (File.Exists(inputLine))
                        {
                            fileContent = File.ReadAllText(inputLine);
                            var response = MicrosoftEntityRecognize(fileContent);
                            PrintMicrosoftEntities(response);
                            if (DeMode == "a")
                            {
                                AnalyseMicrosoftEntityResponse(response);
                            }
                        }
                        else
                        {
                            var response = MicrosoftEntityRecognize(inputLine); 
                            PrintMicrosoftEntities(response);
                            if (DeMode == "a")
                            {
                                Console.WriteLine("Note that the response wasn't analysed due to no file with the given name having been found.");
                            }
                        }
                        return;

                    case "G":   //CASE GOOGLE
                        if (DeMode == "a")
                        {
                            Console.WriteLine("Please insert the filename of a text to be analysed\n");
                        }
                        else
                        {
                            Console.WriteLine("Please insert text or filename of a text to be analysed\n");
                        }
                        inputLine = Console.ReadLine();
                        if (File.Exists(inputLine))
                        {
                            fileContent = File.ReadAllText(inputLine);
                            var response = GoogleEntityRecognize(fileContent);
                            GoogleWriteEntities(response);
                            if (DeMode == "a")
                            {
                                AnalyseGoogleEntityResponse(response);
                            }
                        }
                        else
                        {
                            var response = GoogleEntityRecognize(inputLine);
                            GoogleWriteEntities(response);
                            if (DeMode == "a")
                            {
                                Console.WriteLine("Note that the response wasn't analysed due to no file with the given name having been found.");
                            }
                        }
                        return;

                    case "I":   //CASE IBM
                        if (DeMode == "a")
                        {
                            Console.WriteLine("Please insert the filename of a text to be analysed\n");
                        }
                        else
                        {
                            Console.WriteLine("Please insert text or filename of a text to be analysed\n");
                        }
                        inputLine = Console.ReadLine();
                        if (File.Exists(inputLine))
                        {
                            fileContent = File.ReadAllText(inputLine);
                            string response = IBMEntityRecognize(fileContent);
                            if (String.IsNullOrEmpty(response)) { Console.Write("Result empty"); }
                            else
                            {
                                Console.WriteLine(response);
                                Console.WriteLine("IBM - Done");

                                if (DeMode == "a")
                                {
                                    AnalyseIBMEntityResponse(response);
                                }
                            }
                        }
                        else
                        {
                            string response = IBMEntityRecognize(inputLine);
                            if (String.IsNullOrEmpty(response)) { Console.Write("Result empty"); }
                            else
                            {
                                Console.WriteLine(response);
                                Console.WriteLine("IBM - Done");

                                if (DeMode == "a")
                                {
                                    Console.WriteLine("Note that the response wasn't analysed due to no file with the given name having been found.");
                                }
                            }
                        }
                        return;
                    case "q":
                        return;
                    default:
                        Console.WriteLine("Command not recognized");
                        break;
                }
            }
            return;
        }
        public static void FullRun()
        {
            Console.WriteLine("Not yet implemented");
            return;
        }
        private static IBMEntity ConvertIBMJson(string jsonString)
        {
            try
            {
                IBMEntity result = JsonConvert.DeserializeObject<IBMEntity>(jsonString);
                //Console.WriteLine(result.ToString());
                foreach (var entity in result.entities)
                {
                    Console.WriteLine("text: " + entity.text);
                }

                return result;
            }
            catch (Exception)
            {
                Console.WriteLine("Error parsing response JSON");
            }

            return null;
        }

    }
}
