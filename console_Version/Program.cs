using Amazon.Comprehend;
using Amazon.Comprehend.Model;
using Azure;
using Azure.AI.TextAnalytics;
using Google.Cloud.Language.V1;
using IBM.Cloud.SDK.Core.Authentication.Iam;
using IBM.Watson.NaturalLanguageUnderstanding.v1;
using IBM.Watson.NaturalLanguageUnderstanding.v1.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        private static int splitsize = 4500;
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

        #region Microsoft

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
                result += $"\tText: {entity.Text},\tPosition: {entity.Offset}-{entity.Offset + entity.Length},\tCategory: {entity.Category},\tSub-Category: {entity.SubCategory}" + "\n";
            }
            if (String.IsNullOrEmpty(result)) { Console.Write("Result empty"); }
            Console.WriteLine(result);
            Console.WriteLine("Microsoft - Done");
            return;
        }
        private static void AnalyseMicrosoftEntityResponse(UniEntity response)
        {
            AnalyseIBMEntityResponse(response);
            return;
        }
        private static UniEntity MicrosoftCompleteEntityRecognition(string input)
        {
            string[] datenSplit = SplitDatenschutz(input);

            UniEntity[] entResultEntities = new UniEntity[datenSplit.Length];
            for (int i = 0; i < datenSplit.Length; i++)
            {
                entResultEntities[i] = ConvertMicrosoftToUniEntity(MicrosoftEntityRecognize(datenSplit[i]));
                //Console.WriteLine(entResults[i]);
            }
            Console.WriteLine("Microsoft rec. done, combining results...");
            UniEntity microsoftResEnt = CombineUniEntities(entResultEntities);
            Console.WriteLine("Microsoft results successfully combined.");

            return microsoftResEnt;
        }
        private static UniEntity ConvertMicrosoftToUniEntity(CategorizedEntityCollection microsoftResponse)
        {
            UniEntity result = new UniEntity();
            result.entities = new List<UniEntity.entity>();
            result.language = "no language code found";     //TODO
            Boolean EntityAlreadyExists;
            foreach (var microsoftEntity in microsoftResponse)
            {
                EntityAlreadyExists = false;

                if (result.entities.Count > 0)
                {
                    foreach (var resultEntity in result.entities) //Check für gleiche Entities
                    {
                        if (String.Compare(resultEntity.text, microsoftEntity.Text) == 0 && String.Compare(resultEntity.type.ToLower(), microsoftEntity.Category.ToString().ToLower()) == 0)
                        {
                            //Combining existing entities. Should mainly be adding mentions.

                            UniEntity.mention newMention = new UniEntity.mention();
                            newMention.confidence = (float)microsoftEntity.ConfidenceScore;

                            newMention.text = microsoftEntity.Text;
                            newMention.location = new List<int>();
                            newMention.location.Add(microsoftEntity.Offset);
                            newMention.location.Add(microsoftEntity.Offset + microsoftEntity.Length);
                            resultEntity.mentions.Add(newMention);
                            EntityAlreadyExists = true;

                        }
                    }
                }

                if (!EntityAlreadyExists)
                {
                    //Add new Entity
                    UniEntity.entity resEnt = new UniEntity.entity();
                    resEnt.type = microsoftEntity.Category.ToString();
                    resEnt.type = char.ToUpper(resEnt.type[0]) + resEnt.type[1..].ToLower();
                    if (microsoftEntity.SubCategory != null)
                    {
                        resEnt.subcategory = microsoftEntity.SubCategory;
                        resEnt.subcategory = char.ToUpper(resEnt.subcategory[0]) + resEnt.subcategory[1..].ToLower();

                    }
                    else { resEnt.subcategory = ""; }

                    resEnt.text = microsoftEntity.Text;
                    resEnt.sentiment = new UniEntity.sentim();
                    resEnt.confidence = (float)microsoftEntity.ConfidenceScore;
                    //if (microsoftEntity. != null)
                    //{
                    //    resEnt.sentiment.score = microsoftEntity.Sentiment.Score;
                    //    resEnt.sentiment.label = microsoftEntity.Sentiment.Magnitude.ToString();    //ACHTUNG nongleich
                    //}
                    //resEnt.relevance = microsoftEntity.Salience;

                    resEnt.mentions = new List<UniEntity.mention>();

                    UniEntity.mention newMention = new UniEntity.mention();

                    newMention.confidence = (float)microsoftEntity.ConfidenceScore;
                    newMention.text = microsoftEntity.Text;
                    newMention.location = new List<int>();
                    newMention.location.Add(microsoftEntity.Offset);
                    newMention.location.Add(microsoftEntity.Offset + microsoftEntity.Length);
                    resEnt.mentions.Add(newMention);

                    //resEnt.Metadata = new Dictionary<string, string>();
                    //foreach (var meta in microsoftEntity.Metadata)
                    //{
                    //    resEnt.Metadata.Add(meta.Key, meta.Value);
                    //}

                    result.entities.Add(resEnt);
                }
            }
            return result;
        }
        #endregion

        #region Google

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
        private static UniEntity ConvertGoogleToUniEntity(AnalyzeEntitiesResponse googleResponse)
        {
            UniEntity result = new UniEntity();
            result.entities = new List<UniEntity.entity>();
            result.language = googleResponse.Language;
            Boolean EntityAlreadyExists = false;
            foreach (var googleEntity in googleResponse.Entities)
            {
                EntityAlreadyExists = false;
                if (result.entities.Count > 0)
                {
                    foreach (var resultEntity in result.entities) //Check für gleiche Entities
                    {
                        if (String.Compare(resultEntity.text, googleEntity.Name) == 0 && String.Compare(resultEntity.type.ToLower(), googleEntity.Type.ToString().ToLower()) == 0)
                        {
                            //Combining existing entities. Should mainly be adding mentions.
                            foreach (var responseMention in googleEntity.Mentions)
                            {
                                UniEntity.mention newMention = new UniEntity.mention();
                                //resMention.confidence = responseMention.N/A;
                                newMention.text = responseMention.Text.Content;
                                newMention.location = new List<int>();
                                newMention.location.Add(responseMention.Text.BeginOffset);
                                newMention.location.Add(responseMention.Text.BeginOffset + responseMention.Text.Content.Length);
                                resultEntity.mentions.Add(newMention);
                            }
                            EntityAlreadyExists = true;

                        }
                    }
                }

                if (!EntityAlreadyExists)
                {
                    //Add new Entity
                    UniEntity.entity resEnt = new UniEntity.entity();
                    resEnt.type = googleEntity.Type.ToString();
                    resEnt.type = char.ToUpper(resEnt.type[0]) + resEnt.type[1..].ToLower();
                    resEnt.text = googleEntity.Name;
                    resEnt.sentiment = new UniEntity.sentim();
                    if (googleEntity.Sentiment != null)
                    {
                        resEnt.sentiment.score = googleEntity.Sentiment.Score;
                        resEnt.sentiment.label = googleEntity.Sentiment.Magnitude.ToString();    //ACHTUNG nongleich
                    }
                    resEnt.relevance = googleEntity.Salience;
                    resEnt.mentions = new List<UniEntity.mention>();
                    foreach (var responseMention in googleEntity.Mentions)
                    {
                        UniEntity.mention newMention = new UniEntity.mention();
                        //resMention.confidence = responseMention.N/A;
                        newMention.text = responseMention.Text.Content;
                        newMention.location = new List<int>();
                        newMention.location.Add(responseMention.Text.BeginOffset);
                        newMention.location.Add(responseMention.Text.BeginOffset + responseMention.Text.Content.Length);
                        resEnt.mentions.Add(newMention);
                    }
                    resEnt.Metadata = new Dictionary<string, string>();
                    foreach (var meta in googleEntity.Metadata)
                    {
                        resEnt.Metadata.Add(meta.Key, meta.Value);
                    }
                    result.entities.Add(resEnt);
                }
            }
            return result;
        }
        private static UniEntity GoogleCompleteEntityRecognition(string input)
        {
            string[] datenSplit = SplitDatenschutz(input);
            //AnalyzeEntitiesResponse[] entResults = new AnalyzeEntitiesResponse[datenSplit.Length];
            UniEntity[] entResultEntities = new UniEntity[datenSplit.Length];
            for (int i = 0; i < datenSplit.Length; i++)
            {
                entResultEntities[i] = ConvertGoogleToUniEntity(GoogleEntityRecognize(datenSplit[i]));
                //Console.WriteLine(entResults[i]);
            }
            Console.WriteLine("Google rec. done, combining results...");
            UniEntity googleResEnt = CombineUniEntities(entResultEntities);
            Console.WriteLine("Google results successfully combined.");

            return googleResEnt;
        }
        private static void AnalyseGoogleEntityResponse(UniEntity response)
        {
            AnalyseIBMEntityResponse(response);
            return;
        }

        #endregion

        #region IBM

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
        private static UniEntity IBMCompleteEntityRecognition(string input)
        {
            string[] datenSplit = SplitDatenschutz(input);
            UniEntity[] entResultEntities = new UniEntity[datenSplit.Length];
            for (int i = 0; i < datenSplit.Length; i++)
            {
                entResultEntities[i] = ConvertIBMJson(IBMEntityRecognize(datenSplit[i]));
            }
            //TODO umformen in unientities
            Console.WriteLine("ibm rec. done, combining results...");
            UniEntity ibmResEnt = CombineUniEntities(entResultEntities);
            Console.WriteLine("ibm results successfully combined.");

            return ibmResEnt;
        }
        private static UniEntity ConvertIBMJson(string jsonString)
        {
            try
            {
                UniEntity result = JsonConvert.DeserializeObject<UniEntity>(jsonString);
                return result;
            }
            catch (Exception)
            {
                Console.WriteLine("Error parsing response JSON");
            }
            return null;
        }
        private static TIL AnalyseIBMEntityResponse(UniEntity result)
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
            UniEntity.entity mostweOrgaEntity = new UniEntity.entity();
            result.entities = result.entities.OrderBy(ent => ent.type).ToList();
            foreach (var ent1 in result.entities)
            {
                foreach (var ent2 in result.entities)
                {
                    if (String.Compare(ent1.type, ent2.type) == 0 && (ent1.text.Contains(" " + ent2.text) || ent1.text.Contains(ent2.text + " ")))
                    {
                        foreach (var ment in ent1.mentions)
                        {
                            ent2.mentions.Add(ment);
                        }
                    }
                }
            }
            foreach (var entity in result.entities)
            {
                if (!entity.type.Contains("IP") && String.Compare(entity.type, "Number") != 0/* && item.mentions.Count>1*/)
                {
                    Console.WriteLine("Found this \"" + entity.type + "\" " + entity.mentions.Count + " times: \"" + entity.text + "\"");
                }
                //Console.WriteLine("Found Entity " + entity.text + " of type " + entity.type);

                #region Organizations
                if (String.Compare(entity.type, "Organization") == 0 || String.Compare(entity.type, "Company") == 0)
                {
                    if (ibmOrgaBadWords.Where(item => entity.text.ToLower().StartsWith(item.ToLower())).ToList().Count > 0)
                    {
                        //Console.WriteLine("Filtered Organization Entity \"" + entity.text + "\"."); //debug
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
                    if (newMentions > mostMentions)
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

            string[] typesToCheck = { "Location", "Facility" };
            UniEntity.mention firstClosestMention = FindClosestMentionOfType(result, typesToCheck, GetEarliestMention(mostweOrgaEntity));
            UniEntity.mention closestMention = GetOverallClosestMentionOfType(result, typesToCheck, mostweOrgaEntity);
            UniEntity.mention overallClosestFollowingMention = GetOverallClosestMentionOfType(result, typesToCheck, mostweOrgaEntity, "after");

            //Console.WriteLine("The earliest organization is: " + earliestOrga);
            //Console.WriteLine("The most mentioned organization is: " + mostMentionedOrga);
            Console.WriteLine("\nThe organization/company with the most WeUsWirUns context mentions is: " + mostWeOrga);
            Console.WriteLine("\nThe first closest Entity of types including \"" + typesToCheck[0] + "\" to that is: " + firstClosestMention.text);
            Console.WriteLine("The text around this entity is as follows:\n" + GetTextAroundHere(firstClosestMention.location[0], 40, 30));
            Console.WriteLine("\nThe overall closest Entity of types including  \"" + typesToCheck[0] + "\" to that is: " + closestMention.text);
            Console.WriteLine("The text around this entity is as follows:\n" + GetTextAroundHere(closestMention.location[0], 40, 30));
            Console.WriteLine("\nOverall closest following Mention of type Location/Facility: " + overallClosestFollowingMention.text);
            Console.WriteLine("The text around that is:\n" + GetTextAroundHere(overallClosestFollowingMention.location[0], 40, 30) + "\n");
            //Console.WriteLine("These results do not include Organizations that were removed due to a filter.");

            ibm_til.meta.language = result.language;
            ibm_til.controller.name = mostWeOrga;
            ibm_til.controller.address = GetTextAroundHere(closestMention.location[0], 40, 30);
            string[] typesToCheck2 = { "Facility" }; //TODO Achtung IBM only..

            UniEntity.mention help = FindClosestMentionOfType(result, typesToCheck2, firstClosestMention.location[0]);
            if (!help.text.Contains("[No entity"))
            {
                ibm_til.controller.division = GetTextAroundHere(help.location[0], 40, 30);
            }
            printTILResult(ibm_til);

            //Console.WriteLine("Result TIL: meta.language: " + ibm_til.meta.language);
            //Console.WriteLine("Result TIL: controller.name: " + ibm_til.controller.name);
            //Console.WriteLine("Result TIL: controller.address: " + ibm_til.controller.address);
            //Console.WriteLine("Result TIL: controller.division: " + ibm_til.controller.division);

            //Console.WriteLine("Result TIL: controller.country: " + ibm_til.controller.country);
            //Console.WriteLine("Result TIL: controller.representative.name: " + ibm_til.controller.representative.name);
            //Console.WriteLine("Result TIL: controller.representative.email: " + ibm_til.controller.representative.email);
            //Console.WriteLine("Result TIL: controller.representative.phone: " + ibm_til.controller.representative.phone);
            //Console.WriteLine("Result TIL: " + ibm_til);
            //Console.WriteLine("Result TIL: " + ibm_til);
            //Console.WriteLine("Result TIL: " + ibm_til);
            //Console.WriteLine("Result TIL: " + ibm_til);
            //ibm_til.controller.representative.name = "bob";

            //evtl bei facility eine Mindestlänge fordern

            return ibm_til;
        }


        #endregion
        
        #region AWS

        public static DetectEntitiesResponse AWSEntityRecognize(string inputText)
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
            foreach (var item in response.ResponseMetadata.Metadata)
            {
                Console.WriteLine("key: "+item.Key+" value: "+item.Value+"");
            }

            Console.WriteLine("Done");
        }
        private static void AnalyseAWSEntityResponse(UniEntity response)
        {
            AnalyseIBMEntityResponse(response);
            return;
        }
        private static UniEntity AWSCompleteEntityRecognition(string input)
        {
            string[] datenSplit = SplitDatenschutz(input);
            UniEntity[] entResultEntities = new UniEntity[datenSplit.Length];
            for (int i = 0; i < datenSplit.Length; i++)
            {
                entResultEntities[i] = ConvertAWSToUniEntity(AWSEntityRecognize(datenSplit[i]));
            }
            Console.WriteLine("Google rec. done, combining results...");
            UniEntity googleResEnt = CombineUniEntities(entResultEntities);
            Console.WriteLine("Google results successfully combined.");

            return googleResEnt;
        }
        private static UniEntity ConvertAWSToUniEntity(DetectEntitiesResponse awsResponse)
        {

            UniEntity result = new UniEntity();
            result.entities = new List<UniEntity.entity>();
            result.language = "";
            Boolean EntityAlreadyExists;
            foreach (var awsEntity in awsResponse.Entities)
            {
                EntityAlreadyExists = false;

                if (result.entities.Count > 0)
                {
                    foreach (var resultEntity in result.entities) //Check für gleiche Entities
                    {
                        if (String.Compare(resultEntity.text, awsEntity.Text) == 0 && String.Compare(resultEntity.type.ToLower(), awsEntity.Type.ToString().ToLower()) == 0)
                        {
                            //Combining existing entities. Should mainly be adding mentions.

                            UniEntity.mention newMention = new UniEntity.mention();
                            newMention.confidence = awsEntity.Score;
                            newMention.text = awsEntity.Text;
                            newMention.location = new List<int>();
                            newMention.location.Add(awsEntity.BeginOffset);
                            newMention.location.Add(awsEntity.EndOffset);

                            resultEntity.mentions.Add(newMention);

                            EntityAlreadyExists = true;
                        }
                    }
                }

                if (!EntityAlreadyExists)
                {
                    //Add new Entity
                    UniEntity.entity resEnt = new UniEntity.entity();
                    resEnt.type = awsEntity.Type.ToString();
                    resEnt.type = resEnt.type[0] + resEnt.type[1..].ToLower();
                    resEnt.text = awsEntity.Text;
                    resEnt.confidence = awsEntity.Score;
                    resEnt.relevance = 0;
                    resEnt.mentions = new List<UniEntity.mention>();

                    UniEntity.mention newMention = new UniEntity.mention();

                    //adding the mention
                    newMention.text = awsEntity.Text;
                    newMention.location = new List<int>();
                    newMention.location.Add(awsEntity.BeginOffset);
                    newMention.location.Add(awsEntity.EndOffset);
                    resEnt.mentions.Add(newMention);

                    //resEnt.Metadata = new Dictionary<string, string>();
                    //foreach (var meta in awsResponse.ResponseMetadata.Metadata)
                    //{
                    //    resEnt.Metadata.Add(meta.Key, meta.Value);
                    //}
                    result.entities.Add(resEnt);
                }
            }
            return result;
        }

        #endregion

        #region Hilfsfunktionen Analysis

        private static Boolean IsThisCloseTo(string text, int start, int range = 30)
        {
            int datLen = datenschutzerkl.Length;
            if (datLen - start <= range && start <= range)
            {
                return datenschutzerkl.Substring(0, datLen).Contains(text);
            }
            else if (start <= range)
            {
                return datenschutzerkl.Substring(0, range + start).Contains(text);
            }
            else if (datLen - start >= range)
            {
                return datenschutzerkl.Substring(start - range, datLen - start + range).Contains(text);
            }
            else
            {
                return datenschutzerkl.Substring(start - range, start + 2 * range).Contains(text);
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
            else return datenschutzerkl.Substring(start - leftrange, leftrange + rightrange);
        }
        private static UniEntity.mention FindClosestMentionOfType(UniEntity root, string[] types, int start, string[] ignoreList = null)
        {
            UniEntity.entity closestEntity = new UniEntity.entity();
            UniEntity.mention closestMention = new UniEntity.mention();
            closestEntity.text = "[No entity of type \"" + types + "\" found.]";
            closestMention.text = "[No entity of type \"" + types + "\" found.]";
            int closestDistance = int.MaxValue;
            bool isOfCorrectType = false;
            bool foundInIgnore = false;

            foreach (UniEntity.entity entity in root.entities)
            {
                isOfCorrectType = false;
                for (int i = 0; i < types.Length; i++)
                {
                    if (String.Compare(entity.type, types[i]) == 0)
                    {
                        isOfCorrectType = true;
                    }
                }
                if (!isOfCorrectType)
                { continue; }
                if (ignoreList != null)
                {
                    foundInIgnore = false;

                    if (ignoreList.Where(item => entity.text.ToLower().StartsWith(item.ToLower())).ToList().Count > 0)
                    { foundInIgnore = true; }
                    if (ignoreList.Where(item => entity.text.ToLower().EndsWith(item.ToLower())).ToList().Count > 0)
                    { foundInIgnore = true; }

                    if (foundInIgnore)
                    { continue; }
                }
                foreach (UniEntity.mention mention in entity.mentions)
                {
                    if (Math.Abs(mention.location[0] - start) < closestDistance)
                    {
                        closestDistance = Math.Abs(mention.location[0] - start);
                        closestEntity = entity;
                        closestMention = mention;
                    }
                }
            }
            return closestMention;
        }
        private static int GetEarliestMention(UniEntity.entity entity)
        {
            int earliest = int.MaxValue;
            if (entity.mentions != null)
            {
                foreach (UniEntity.mention mention in entity.mentions)
                {
                    if (mention.location[0] < earliest)
                    {
                        earliest = mention.location[0];
                    }
                }
            }
            return earliest;
        }
        private static UniEntity.mention GetOverallClosestMentionOfType(UniEntity root, string[] types, UniEntity.entity entity, string mode = "normal", string[] ignoreList = null)
        {
            //IBMEntity.entity closestEntity = new IBMEntity.entity();
            UniEntity.mention closestMention = new UniEntity.mention();
            //closestEntity.text = "[No entity of type \"" + types + "\" found.]";
            //closestMention.location = new List<int>; //TODO watch out for empty locations
            //closestMention.location.Add(0);
            //closestMention.location.Add(0);
            int closestDistance = int.MaxValue;
            bool isOfCorrectType = false;
            foreach (UniEntity.entity item in root.entities)
            {
                isOfCorrectType = false;
                for (int i = 0; i < types.Length; i++)
                {
                    if (String.Compare(item.type, types[i]) == 0)
                    {
                        isOfCorrectType = true;
                    }
                }
                if (!isOfCorrectType || String.Compare(item.text, entity.text) == 0)
                { continue; }

                if (ignoreList != null)
                {
                    bool ignoreFound = false;

                    if (ignoreList.Where(something => item.text.ToLower().StartsWith(something.ToLower())).ToList().Count > 0)
                    { ignoreFound = true; }
                    if (ignoreList.Where(something => item.text.ToLower().EndsWith(something.ToLower())).ToList().Count > 0)
                    { ignoreFound = true; }

                    if (ignoreFound)
                    { continue; }
                }
                foreach (UniEntity.mention entityMention in entity.mentions)
                {
                    foreach (UniEntity.mention itemMention in item.mentions)
                    {
                        if (String.Compare(mode, "after") == 0)
                        {
                            if (itemMention.location[0] > entityMention.location[1] && Math.Abs(itemMention.location[0] - ((entityMention.location[1] + entityMention.location[0]) / 2)) < closestDistance)
                            {
                                closestDistance = Math.Abs(itemMention.location[0] - ((entityMention.location[1] + entityMention.location[0]) / 2));
                                //closestEntity = item;
                                closestMention = itemMention;
                            }
                            continue;
                        }
                        else if (String.Compare(mode, "before") == 0)
                        {
                            if (itemMention.location[0] < entityMention.location[1] && Math.Abs(itemMention.location[0] - ((entityMention.location[1] + entityMention.location[0]) / 2)) < closestDistance)
                            {
                                closestDistance = Math.Abs(itemMention.location[0] - ((entityMention.location[1] + entityMention.location[0]) / 2));
                                //closestEntity = item;
                                closestMention = itemMention;
                            }
                            continue;
                        }
                        else if (Math.Abs(itemMention.location[0] - ((entityMention.location[1] + entityMention.location[0]) / 2)) < closestDistance)
                        {
                            closestDistance = Math.Abs(itemMention.location[0] - ((entityMention.location[1] + entityMention.location[0]) / 2));
                            //closestEntity = item;
                            closestMention = itemMention;
                            continue;
                        }
                    }
                }
            }
            return closestMention;
        }

        #endregion

        public static void DemoRun(string deMode)
        {
            string read;

            for (int i = 0; i < 100000; i++)
            {
                Console.WriteLine("Please choose (A)WS, (M)icrosoft, (G)oogle, (I)BM or (q)uit\n");
                read = Console.ReadLine();
                datenschutzerkl = "";
                switch (read)
                {
                    case "A":   //CASE AWS
                        entityDemoExecute(deMode, "A");
                        return;
                    case "M":   //CASE MICROSOFT AZURE
                        entityDemoExecute(deMode, "M");
                        return;
                    case "G":   //CASE GOOGLE
                        entityDemoExecute(deMode, "G");
                        return;
                    case "I":   //CASE IBM
                        entityDemoExecute(deMode, "I");
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
        private static void entityDemoExecute(string deMode, string serviceCode)
        {
            //if (deMode == "a")
            //{ Console.WriteLine("Please insert the filename of a text to be analysed\n"); }
            //else
            //{ Console.WriteLine("Please insert text or filename of a text to be analysed\n"); }

            Console.WriteLine("Please insert text or filename of a text to be analysed\n");
            string inputLine = Console.ReadLine();

            if (String.IsNullOrEmpty(inputLine))
            {
                Console.WriteLine("Error - Input empty.");
                return;
            }
            if (File.Exists(inputLine))
            {
                datenschutzerkl = File.ReadAllText(inputLine); //TODO eher jedes Mal wenn nötig einlesen.
                UniEntity responseEntity = new UniEntity();
                switch (serviceCode)
                {
                    case "I":
                        Console.WriteLine("IBM - Beginning recognition...");
                        responseEntity = IBMCompleteEntityRecognition(datenschutzerkl);
                        Console.Write("IBM");
                        break;
                    case "A":
                        Console.WriteLine("AWS - Beginning recognition...");
                        responseEntity = AWSCompleteEntityRecognition(datenschutzerkl);
                        Console.Write("AWS");
                        break;
                    case "M":
                        Console.WriteLine("Microsoft - Beginning recognition...");
                        responseEntity = MicrosoftCompleteEntityRecognition(datenschutzerkl);
                        Console.Write("Microsoft");
                        break;
                    case "G":
                        Console.WriteLine("Google - Beginning recognition...");
                        responseEntity = GoogleCompleteEntityRecognition(datenschutzerkl);
                        Console.Write("Google");
                        break;
                    default:
                        return;
                }
                Console.WriteLine(" - Recognition finished.");

                if (deMode == "a")
                {
                    switch (serviceCode)
                    {
                        case "I":
                            Console.WriteLine("IBM - Beginning processing...");
                            AnalyseIBMEntityResponse(responseEntity);
                            Console.Write("IBM");
                            break;
                        case "A":
                            Console.WriteLine("AWS - Beginning processing...");
                            AnalyseAWSEntityResponse(responseEntity);
                            Console.Write("AWS");
                            break;
                        case "M":
                            Console.WriteLine("Microsoft - Beginning processing...");
                            AnalyseMicrosoftEntityResponse(responseEntity);
                            Console.Write("Microsoft");
                            break;
                        case "G":
                            Console.WriteLine("Google - Beginning processing...");
                            AnalyseGoogleEntityResponse(responseEntity);
                            Console.Write("Google");
                            break;
                        default:
                            return;
                    }
                    Console.WriteLine(" - Processing finished.");
                }
                else
                {
                    foreach (var entity in responseEntity.entities)
                    {
                        Console.WriteLine("Text: " + entity.text + ", type: " + entity.type + ", mentions:");
                        foreach (var mention in entity.mentions)
                        {
                            Console.WriteLine("\t"+mention.text);
                            Console.WriteLine("\t"+mention.location[0]);
                            Console.WriteLine("\t"+mention.location[1]);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Sorry, no file with that path/name could be found."); //Text input not used anymore
                //switch (serviceCode)
                //{
                //    case "I":
                //        Console.WriteLine("IBM - Beginning recognition...");
                //        responseEntity = IBMCompleteEntityRecognition(datenschutzerkl);
                //        Console.Write("IBM");
                //        break;
                //    case "A":
                //        Console.WriteLine("AWS - Beginning recognition...");
                //        responseEntity = AWSCompleteEntityRecognition(datenschutzerkl);
                //        Console.Write("AWS");
                //        break;
                //    case "M":
                //        Console.WriteLine("Microsoft - Beginning recognition...");
                //        responseEntity = MicrosoftCompleteEntityRecognition(datenschutzerkl);
                //        Console.Write("Microsoft");
                //        break;
                //    case "G":
                //        Console.WriteLine("Google - Beginning recognition...");
                //        responseEntity = GoogleCompleteEntityRecognition(datenschutzerkl);
                //        Console.Write("Google");
                //        break;
                //    default:
                //        return;
                //}
                //string response = IBMEntityRecognize(inputLine);
                //if (String.IsNullOrEmpty(response)) { Console.Write("Result empty"); }
                //else
                //{
                //    Console.WriteLine(response);
                //    Console.WriteLine("IBM - Done");

                //    if (deMode == "a")
                //    {
                //        Console.WriteLine("Note that the response wasn't analysed " +
                //        "due to no file with the given name having been found.");
                //    }
                //}
            }
            return;
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
        private static string[] SplitDatenschutz(string input, int size = -1)
        {
            if (size == -1)
            {
                size = splitsize;
            }
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
        private static UniEntity CombineUniEntities(UniEntity[] input)
        {
            if (input.Length == 0)
            {
                return null;
            }
            UniEntity resEntity = input[0];
            //UniEntity resEntity = ConvertIBMJson(input[0]);
            for (int i = 1; i < input.Length; i++)
            {
                UniEntity midEntity = input[i];
                foreach (UniEntity.entity entity in midEntity.entities)
                {
                    foreach (UniEntity.mention mention in entity.mentions)
                    {
                        mention.location[0] += splitsize * i;
                        mention.location[1] += splitsize * i;
                    }
                }
                resEntity += midEntity;
            }
            //foreach (IBMEntity.entity item in resEntity.entities)
            //{
            //    if (!item.type.Contains("IP") && String.Compare(item.type, "Number") != 0/* && item.mentions.Count>1*/)
            //    {
            //        Console.WriteLine("Found the following Entity "+item.mentions.Count+" times: " + item.text + " of type " + item.type);
            //    }
            //}
            return resEntity;
        }
        private static void printTILResult(TIL resultTIL)
        {
            Console.WriteLine(JsonConvert.SerializeObject(resultTIL));
            ////Meta
            //Console.WriteLine("meta: ");
            //Console.WriteLine("\tmeta._id: "+resultTIL.meta._id);
            //Console.WriteLine("\tmeta.name: "+resultTIL.meta.name);
            //Console.WriteLine("\tmeta.created: "+resultTIL.meta.created);
            //Console.WriteLine("\tmeta.modified: "+resultTIL.meta.modified);
            //Console.WriteLine("\tmeta.version: "+resultTIL.meta.version);
            //Console.WriteLine("\tmete.language: "+resultTIL.meta.language);
            //Console.WriteLine("\tmeta.status: "+resultTIL.meta.status);
            //Console.WriteLine("\tmeta.url: "+resultTIL.meta.url);
            //Console.WriteLine("\tmeta.hash: "+resultTIL.meta._hash);

            ////Controller
            //Console.WriteLine("controller: ");
            //Console.WriteLine("\tcontroller.name: "+resultTIL.controller.name);
            //Console.WriteLine("\tcontroller.division: "+resultTIL.controller.division);
            //Console.WriteLine("\tcontroller.address: "+resultTIL.controller.address);
            //Console.WriteLine("\tcontroller.country: "+resultTIL.controller.country);
            //Console.WriteLine("\tcontroller.representative: ");
            //Console.WriteLine("\t\tcontroller.representative.name: "+resultTIL.controller.representative.name);
            //Console.WriteLine("\t\tcontroller.representative.email: "+resultTIL.controller.representative.email);
            //Console.WriteLine("\t\tcontroller.representative.phone: "+resultTIL.controller.representative.phone);

            ////DataProtectionOfficer
            //Console.WriteLine("dataProtectionOfficer: ");
            //Console.WriteLine("\tdataProtectionOfficer.name: ");
            //Console.WriteLine("\tdataProtectionOfficer.address: ");
            //Console.WriteLine("\tdataProtectionOfficer.country: ");
            //Console.WriteLine("\tdataProtectionOfficer.email: ");
            //Console.WriteLine("\tdataProtectionOfficer.phone: ");
            return;
        }
    }
}

//NOTIZEN

    //TODO:
    //  Evtl Bei Google Entities mit den gleichen Metadaten (zB Wikieinträge) zu einem kombinieren.
    //  AWS noch enablen
    //  Microsoft noch enablen.
    // maybe für die wo es pro mention nen confidence value gibt diese der mention hinzufügen. confidence schon a thing in mentions.