﻿using System;
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
        private static string datenschutzerkl = "";
        private static List<string> ibmOrgaBadWords = new List<string>(new string[] { "DSGVO", "EU", "TLS", "IP", "GOOGLE" });

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
        private static TIL AnalyseIBMEntityResponse(IBMEntity result)
        {
            TIL ibm_til = new TIL();
            string earliestOrga = "";
            string mostMentionedOrga = "";
            int earliestOrgaStart = int.MaxValue;
            int mostMentions = 0;
            int newMentions = 0;
            int mostWe = 0;
            int newWe = 0;
            string mostWeOrga = "";
            IBMEntity.entity mostweOrgaEntity = new IBMEntity.entity();
            foreach (var entity in result.entities)
            {
                #region Organizations
                if (String.Compare(entity.type, "Organization") == 0 || String.Compare(entity.type, "Company") == 0)
                {
                    if (ibmOrgaBadWords.Where(item => entity.text.ToLower().StartsWith(item.ToLower())).ToList().Count > 0)
                    {
                        Console.WriteLine("Filtered Organization Entity \""+entity.text+"\"."); //debug
                        continue;
                    }
                    if (ibmOrgaBadWords.Where(item => entity.text.ToLower().EndsWith(item.ToLower())).ToList().Count > 0)
                    {
                        Console.WriteLine("Filtered Organization Entity \"" + entity.text + "\"."); //debug
                        continue;
                    }
                    newMentions = 0;
                    newWe = 0;
                    foreach (var mention in entity.mentions)
                    {
                        newMentions++;
                        if (earliestOrgaStart > mention.location[0]/* && entity.mentions.Count > 3*/)
                        {
                            earliestOrgaStart = mention.location[0];
                            earliestOrga = entity.text;
                        }
                        if (IsThisCloseTo(" us ", mention.location[0], 20) || IsThisCloseTo(" we ", mention.location[0], 20)
                            || IsThisCloseTo(" uns ", mention.location[0], 20) || IsThisCloseTo(" wir ", mention.location[0], 20)
                            || IsThisCloseTo(" us,", mention.location[0], 20) || IsThisCloseTo(" we,", mention.location[0], 20)
                            || IsThisCloseTo(" uns,", mention.location[0], 20) || IsThisCloseTo(" wir,", mention.location[0], 20))
                        {
                            newWe++;
                            //Console.WriteLine("The organizational/company Entity \""+entity.text+"\" has been found " +
                            //    "within range of at least one of the words \"us\", \"we\", \"uns\" and \"wir\".");
                        }
                    }
                    if (newMentions>mostMentions)
                    {
                        mostMentions = newMentions;
                        mostMentionedOrga = entity.text;
                    }
                    if (newWe > mostWe)
                    {
                        mostWeOrga = entity.text;
                        mostWe = newWe;
                        mostweOrgaEntity = entity;
                    }
                }
                #endregion

            }
            //Console.WriteLine("The earliest organization is: " + earliestOrga);
            //Console.WriteLine("The most mentioned organization is: " + mostMentionedOrga);
            Console.WriteLine("The organization/company with the most WeUsWirUns context mentions is: " + mostWeOrga);
            string typeToCheck = "Person";
            Console.WriteLine("The closest Entity of type \""+typeToCheck+"\" to that is: " + GetOverallClosestOfType(result, typeToCheck, mostweOrgaEntity).text);
            //Console.WriteLine("These results do not include Organizations that were removed due to a filter.");

            ibm_til.meta.language = result.language;
            ibm_til.controller.name = mostWeOrga;
            //ibm_til.controller.representative.name = "bob";


            return ibm_til;
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
            string read = "";

            for (int i = 0; i < 100000; i++)
            {

                Console.WriteLine("Please choose (A)WS, (M)icrosoft, (G)oogle, (I)BM or (q)uit\n");
                read = Console.ReadLine();
                inputLine = "";
                datenschutzerkl = "";
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
                            datenschutzerkl = File.ReadAllText(inputLine);
                            var response = AwsEntityRecognize(datenschutzerkl);
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
                            datenschutzerkl = File.ReadAllText(inputLine);
                            var response = MicrosoftEntityRecognize(datenschutzerkl);
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
                            datenschutzerkl = File.ReadAllText(inputLine);
                            var response = GoogleEntityRecognize(datenschutzerkl);
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
                        if (String.IsNullOrEmpty(inputLine))
                        {
                            Console.WriteLine("Error - Input empty.");
                            continue;
                        }
                        if (File.Exists(inputLine))
                        {
                            datenschutzerkl = File.ReadAllText(inputLine);
                            Console.WriteLine("IBM - Beginning recognition...");
                            IBMEntity responseEntity = IBMCompleteEntityRecognition(datenschutzerkl);
                            //string response = IBMEntityRecognize(datenschutzerkl);
                            Console.WriteLine("IBM - Recognition finished.");

                            if (DeMode == "a")
                            {
                                Console.WriteLine("IBM - Beginning processing...");

                                AnalyseIBMEntityResponse(responseEntity);
                                Console.WriteLine("IBM - Processing finished.");

                            }
                            //if (String.IsNullOrEmpty(response)) { Console.Write("Result empty"); }
                            //else
                            //{
                            //    //Console.WriteLine(response);
                            //    Console.WriteLine("IBM - Recognition finished.");

                            //    if (DeMode == "a")
                            //    {
                            //        Console.WriteLine("IBM - Beginning processing...");

                            //        AnalyseIBMEntityResponse(responseEntity);
                            //        Console.WriteLine("IBM - Processing finished.");

                            //    }
                            //}
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
                return result;
            }
            catch (Exception)
            {
                Console.WriteLine("Error parsing response JSON");
            }
            return null;
        }
        /// <summary>
        /// Returns whether or not a specified string can be found
        /// in the given range around the start offset of another.
        /// Checks in the string "datenschutzerkl".
        /// </summary>
        /// <param name="text"></param>
        /// <param name="start"></param>
        /// <param name="range"></param>
        /// <returns>boolean result of Search within range</returns>
        private static Boolean IsThisCloseTo(string text, int start, int range = 20) {
            int datLen = datenschutzerkl.Length;
            if (datLen-start <= range && start <=range)
            {
                return datenschutzerkl.Substring(0, datLen).Contains(text);
            }
            else if (start <=range)
            {
                return datenschutzerkl.Substring(0, range+start).Contains(text);
            }
            else if (datLen-start>=range)
            {
                return datenschutzerkl.Substring(start - range, datLen-start+range).Contains(text);
            }
            else
            {
                return datenschutzerkl.Substring(start-range, start + 2*range).Contains(text);
            }
        }
        private static string GetTextAroundHere(int start, int leftrange, int rightrange = -1)
        {
            int datLen = datenschutzerkl.Length;
            if (rightrange == -1)
            { rightrange = leftrange; }

            if (datLen - 1 < start + rightrange && start < leftrange)
            {
                return datenschutzerkl.Substring(0, datLen);
            }
            else if (datLen - 1 <= start + rightrange)
            {
                return datenschutzerkl.Substring(start - leftrange, leftrange + datLen - start);
            }
            else if (start <= leftrange)
            {
                return datenschutzerkl.Substring(0, start + rightrange);
            }
            else return datenschutzerkl.Substring(start-leftrange, leftrange+rightrange);
        }
        private static string[] SplitDatenschutz(string input, int size = 5000)
        {
            string[] res = new string[(int)Math.Ceiling((double)input.Length/size)];
            string partRes = "";
            int count = 0;
            int placeInArray = 0;
            while (count < input.Length)
            {
                for (int j = 0; j < size; j++)
                {
                    if (count < input.Length)
                    {
                        partRes += input[count];
                        count++;
                    }
                }
                res[placeInArray] = partRes;
                placeInArray++;
                partRes = "";
            }
            return res;
        }
        private static IBMEntity IBMCompleteEntityRecognition(string input)
        {
            string[] datenSplit = SplitDatenschutz(input);
            string[] entResults = new string[datenSplit.Length];
            for(int i = 0; i<datenSplit.Length; i++)
            {
                entResults[i] = IBMEntityRecognize(datenSplit[i]);
                //Console.WriteLine(entResults[i]);
            }
            Console.WriteLine("ibm rec done, combining results...");
            IBMEntity ibmResEnt = CombineIBMEntities(entResults);
            Console.WriteLine("ibm results successfully combined.");

            return ibmResEnt;
        }
        private static IBMEntity CombineIBMEntities(string[] input)
        {
            if (input.Length == 0)
            {
                return null;
            }
            IBMEntity resEntity = ConvertIBMJson(input[0]);
            for (int i = 1; i < input.Length; i++)
            {
                IBMEntity midEntity = ConvertIBMJson(input[i]);
                resEntity += midEntity;
            }
            foreach (IBMEntity.entity item in resEntity.entities)
            {
                Console.WriteLine("Found Entity: " + item.text + " of type "+ item.type);
            }
            return resEntity;
        }
        private static IBMEntity.entity FindClosestEntityOfType(IBMEntity root, string type, int start, string[] ignoreList = null)
        {
            IBMEntity.entity closestEntity = new IBMEntity.entity();
            closestEntity.text = "[No entity of type \""+type+"\" found.]";
            int closestDistance = int.MaxValue;

            foreach (IBMEntity.entity entity in root.entities)
            {
                if (String.Compare(entity.type, type) != 0)
                {
                    continue;
                }
                
                if (ignoreList != null)
                {
                    bool foundInIgnore = false;

                    if (ignoreList.Where(item => entity.text.ToLower().StartsWith(item.ToLower())).ToList().Count > 0)
                    {   foundInIgnore = true;   }
                    if (ignoreList.Where(item => entity.text.ToLower().EndsWith(item.ToLower())).ToList().Count > 0)
                    {   foundInIgnore = true;   }

                    if (foundInIgnore)
                    {
                        continue;
                    }
                }
                foreach (IBMEntity.mention mention in entity.mentions)
                {
                    if (Math.Abs(mention.location[0]-start)<closestDistance)
                    {
                        closestDistance = Math.Abs(mention.location[0] - start);
                        closestEntity = entity;
                    }
                }
            }
            return closestEntity;
        }
        private static int GetEarliestMention(IBMEntity.entity entity)
        {
            int earliest = int.MaxValue;
            foreach (IBMEntity.mention mention in entity.mentions)
            {
                if (mention.location[0] < earliest)
                {
                    earliest = mention.location[0];
                }
            }
            return earliest;
        }
        private static IBMEntity.entity GetOverallClosestOfType(IBMEntity root, string type, IBMEntity.entity entity, string[] ignoreList = null)
        {
            IBMEntity.entity closestEntity = new IBMEntity.entity();
            closestEntity.text = "[No entity of type \"" + type + "\" found.]";
            int closestDistance = int.MaxValue;
            foreach (IBMEntity.entity item in root.entities)
            {
                if (String.Compare(item.text, entity.text) == 0 || String.Compare(type, item.type) != 0)
                {   continue;   }
                if (ignoreList != null)
                {
                    bool ignoreFound = false;

                    if (ignoreList.Where(something => item.text.ToLower().StartsWith(something.ToLower())).ToList().Count > 0)
                        { ignoreFound = true; }
                    if (ignoreList.Where(something => item.text.ToLower().EndsWith(something.ToLower())).ToList().Count > 0)
                        { ignoreFound = true; }

                    if (ignoreFound)
                    {   continue;   }
                }
                foreach (IBMEntity.mention entityMention in entity.mentions)
                {
                    foreach (IBMEntity.mention itemMention in item.mentions)
                    {
                        if (Math.Abs(itemMention.location[0]-entityMention.location[0]) < closestDistance)
                        {
                            closestDistance = Math.Abs(itemMention.location[0] - entityMention.location[0]);
                            closestEntity = item;
                        }
                    }
                }
            }
            return closestEntity;
        }
    }
}