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
        private static TIL AnalyseMicrosoftEntityResponse(UniEntity response)
        {
            return AnalyseIBMEntityResponse(response);
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
        private static TIL AnalyseGoogleEntityResponse(UniEntity response)
        {
            return AnalyseIBMEntityResponse(response);
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
            
            //Relations Analysis

            //var result2 = naturalLanguageUnderstanding.Analyze(   
            //    text: input,
            //    features: new Features()
            //    {
            //        Relations = new RelationsOptions()
            //    }
            //    );
            //Console.WriteLine(result2.Response);
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
            Console.WriteLine("\n##########################################################################################\n");

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
            UniEntity.entity mostMentionedOrgaEntity = new UniEntity.entity();
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
                if (true)//!entity.type.Contains("IP") && String.Compare(entity.type, "Number") != 0/* && item.mentions.Count>1*/)
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
                        }
                    }
                    if (newMentions > mostMentions)
                    {
                        mostMentions = newMentions;
                        mostMentionedOrga = entity.text;
                        mostMentionedOrgaEntity = entity;
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
            Console.WriteLine("\n##########################################################################################\n");

            string[] typesToCheck = { "Location", "Facility", "Company" };
            //UniEntity.mention firstClosestMention = FindClosestMentionOfType(result, "", GetEarliestMention(mostweOrgaEntity), typesToCheck);
            //UniEntity.mention closestLocationMention = GetOverallClosestMentionOfType(result, "", mostweOrgaEntity, "normal", typesToCheck);
            //UniEntity.mention overallClosestFollowingMention = GetOverallClosestMentionOfType(result, "", mostweOrgaEntity, "after", typesToCheck);

            UniEntity.mention firstClosestMention = FindClosestMentionOfType(result, "", GetEarliestMention(mostMentionedOrgaEntity).location[0], typesToCheck);
            UniEntity.mention closestLocationMention = GetOverallClosestMentionOfType(result, "", mostMentionedOrgaEntity, "normal", typesToCheck);
            UniEntity.mention overallClosestFollowingMention = GetOverallClosestMentionOfType(result, "", mostMentionedOrgaEntity, "after", typesToCheck);

            //Console.WriteLine("The earliest organization is: " + earliestOrga);
            Console.WriteLine("The most mentioned organization is: " + mostMentionedOrga);
            Console.WriteLine("Earliest mention of that found at: " + GetEarliestMention(mostMentionedOrgaEntity).location[0]);
            //Console.WriteLine("\nThe organization/company with the most WeUsWirUns context mentions is: " + mostWeOrga);
            Console.WriteLine("\nThe first closest Entity of types including \"" + typesToCheck[0] + "\" to that is: " + firstClosestMention.text);
            if (!firstClosestMention.text.Contains("[No")){
                Console.WriteLine("The text around this entity is as follows:\n" + GetTextAroundHere(firstClosestMention.location[0], 40, 30));
            }
            Console.WriteLine("\nThe overall closest Entity of types including  \"" + typesToCheck[0] + "\" to that is: " + closestLocationMention.text);
            Console.WriteLine("The text around this entity is as follows:\n" + GetTextAroundHere(closestLocationMention.location[0], 40, 30));
            Console.WriteLine("\nOverall closest following Mention of type Location/Facility: " + overallClosestFollowingMention.text);
            Console.WriteLine("The text around that is:\n" + GetTextAroundHere(overallClosestFollowingMention.location[0], 40, 30) + "\n");

            if (IsThisCloseTo("erantwortlich", closestLocationMention.location[0], 50)
                || IsThisCloseTo("epresentative", closestLocationMention.location[0], 50)
                || IsThisCloseTo("orsitzend", closestLocationMention.location[0], 50))
            {
                UniEntity.mention reprMailMention = FindClosestMentionOfType(result, "EmailAddress", closestLocationMention.location[0]);
                Console.WriteLine("reprMailMention: " + reprMailMention.text);
                UniEntity.mention reprNameMention = FindClosestMentionOfType(result, "Person", closestLocationMention.location[0]);
                Console.WriteLine("reprNameMention: " + reprNameMention.text);
                UniEntity.mention reprPhoneMention = FindClosestMentionOfType(result, "PhoneNumber", closestLocationMention.location[0]);
                Console.WriteLine("reprPhoneMention: " + reprPhoneMention.text);
                ibm_til.controller.representative.email = reprMailMention.text;
                ibm_til.controller.representative.name = reprNameMention.text;
                ibm_til.controller.representative.phone = reprPhoneMention.text;
            }

            //dataProt might also work with just FindClosestMention after the first result.

        #region dataProtectionOfficer
            string dataProtMode = "before";
            string deutschDatenschutz = "atenschutzbeauf";

            //DataProtectionOfficer.address

            UniEntity.mention closestDatenschutzbeaufMention = GetOverallClosestMentionOfTypeToString(result, "Location", deutschDatenschutz, dataProtMode, typesToCheck);
            int closestDatenschutzbeaufDistanz = HowCloseTo(closestDatenschutzbeaufMention.location[0], deutschDatenschutz, 500);

            UniEntity.mention closestOfficerMention = GetOverallClosestMentionOfTypeToString(result, "Location", "fficer", dataProtMode, typesToCheck);
            int closestOfficerDistanz = HowCloseTo(closestOfficerMention.location[0], "fficer", 500);

            if (closestOfficerDistanz < 500 || closestDatenschutzbeaufDistanz < 500)
            {
                if (closestOfficerDistanz < closestDatenschutzbeaufDistanz)
                {
                    Console.WriteLine("Closest Location/Facility to \"fficer\": " + closestOfficerMention.text);
                    ibm_til.dataProtectionOfficer.address = GetTextAroundHere(closestOfficerMention.location[0], 30);

                }
                else
                {
                    Console.WriteLine("Closest Location/Facility to \"atenschutzbeauf\": " + closestDatenschutzbeaufMention.text);
                    ibm_til.dataProtectionOfficer.address = GetTextAroundHere(closestDatenschutzbeaufMention.location[0], 30);
                }
            }

            //DataProtectionOfficer.name

            UniEntity.mention closestDatenschutzbeaufPersonMention = GetOverallClosestMentionOfTypeToString(result, "Person", deutschDatenschutz, dataProtMode);
            int closestDatenschutzbeaufPersonDistanz = HowCloseTo(closestDatenschutzbeaufPersonMention.location[0], deutschDatenschutz, 250);

            UniEntity.mention closestOfficerPersonMention = GetOverallClosestMentionOfTypeToString(result, "Person", "fficer", dataProtMode);
            int closestOfficerPersonDistanz = HowCloseTo(closestOfficerPersonMention.location[0], "fficer", 250);

            if (closestOfficerDistanz < 250 || closestDatenschutzbeaufDistanz < 250)
            {
                if (closestOfficerPersonDistanz < closestDatenschutzbeaufPersonDistanz)
                {
                    Console.WriteLine("Closest Person to \"fficer\": " + closestOfficerPersonMention.text);
                    ibm_til.dataProtectionOfficer.name = GetTextAroundHere(closestOfficerPersonMention.location[0], 30);

                }
                else
                {
                    Console.WriteLine("Closest Person to \"atenschutzbeauf\": " + closestDatenschutzbeaufPersonMention.text);
                    ibm_til.dataProtectionOfficer.name = GetTextAroundHere(closestDatenschutzbeaufPersonMention.location[0], 30);
                }
            }

            //DataProtectionOfficer.email

            UniEntity.mention datProtEmailMentionDE = GetOverallClosestMentionOfTypeToString(result, "EmailAddress", deutschDatenschutz, dataProtMode);
            int datProtEmailDistanzDE = HowCloseTo(datProtEmailMentionDE.location[0], deutschDatenschutz, 250);

            UniEntity.mention datProtEmailMentionEN = GetOverallClosestMentionOfTypeToString(result, "EmailAddress", "fficer", dataProtMode);
            int datProtEmailDistanzEN = HowCloseTo(datProtEmailMentionEN.location[0], "fficer", 250);

            if(datProtEmailDistanzDE < 250 || datProtEmailDistanzEN < 250)
            {
                if (datProtEmailDistanzDE < datProtEmailDistanzEN)
                {
                    Console.WriteLine("dataProtectionEmail: " + datProtEmailMentionDE.text);
                    ibm_til.dataProtectionOfficer.email = datProtEmailMentionDE.text;
                }
                else
                {
                    Console.WriteLine("dataProtectionEmail: " + datProtEmailMentionEN.text);
                    ibm_til.dataProtectionOfficer.email = datProtEmailMentionEN.text;

                }
            }

            //DataProtectionOfficer.phone

            UniEntity.mention datProtPhoneMentionDE = GetOverallClosestMentionOfTypeToString(result, "PhoneNumber", deutschDatenschutz, dataProtMode);
            int datProtPhoneDistanzDE = HowCloseTo(datProtPhoneMentionDE.location[0], deutschDatenschutz, 250);

            UniEntity.mention datProtPhoneMentionEN = GetOverallClosestMentionOfTypeToString(result, "PhoneNumber", "fficer", dataProtMode);
            int datProtPhoneDistanzEN = HowCloseTo(datProtPhoneMentionEN.location[0], "fficer", 250);

            if (datProtPhoneDistanzDE < 250 || datProtPhoneDistanzEN < 250)
            {
                if (datProtPhoneDistanzDE < datProtPhoneDistanzEN)
                {
                    Console.WriteLine("dataProtectionPhone: " + datProtPhoneMentionDE.text);
                    ibm_til.dataProtectionOfficer.phone = GetTextAroundHere(datProtPhoneMentionDE.location[0], 30);
                }
                else
                {
                    Console.WriteLine("dataProtectionPhone: " + datProtPhoneMentionEN.text);
                    ibm_til.dataProtectionOfficer.phone = GetTextAroundHere(datProtPhoneMentionEN.location[0], 30);
                }
            }
            #endregion

        #region dataDisclosed TODO

            foreach (var resultEntity in result.entities)
            {
                if (String.Compare(resultEntity.type, "Ordinal") == 0 && (resultEntity.text.Length > 15))
                {
                    TIL.DataDisclosed newDataDisclosed = new TIL.DataDisclosed();
                    newDataDisclosed.category = resultEntity.text;



                    foreach (var innerEntity in result.entities)
                    {
                        //todo fill datadisclosed elem
                    }

                    ibm_til.dataDisclosed.Add(newDataDisclosed);

                }
            }

            #endregion

        #region thirdCountryTransfers TODO

            #endregion

        #region accessAndDataPortability TODO

            #endregion

        #region sources TODO

            #endregion

        #region rightsTo
            //TODO maybe combined location, one right location has to also be close to another right location (or all)
            int closestDistance = int.MaxValue;
            int closestRightLocation = int.MaxValue;
            int rightRelevantLocation = int.MaxValue;
            string rightRelevantEN = "";
            string rightRelevantDE = "";
            string rightRelevantENControl = "";
            string rightRelevantDEControl = "";
            List<int> rightLocations = AllOccurrancesOfText("right");
            foreach (var item in AllOccurrancesOfText("Recht"))
            {
                rightLocations.Add(item);
            }

            #region rightToInformation todo
            closestDistance = int.MaxValue;
            closestRightLocation = int.MaxValue;
            rightRelevantLocation = int.MaxValue;

            rightRelevantEN = "informat";
            rightRelevantDE = "nformation";

            rightRelevantENControl = "delet";
            rightRelevantDEControl = "ösch";

            int[] rightToInformationResults = CheckForRights(rightLocations, rightRelevantEN, rightRelevantDE, rightRelevantENControl, rightRelevantDEControl);
            closestDistance = rightToInformationResults[0];
            closestRightLocation = rightToInformationResults[1];
            rightRelevantLocation = rightToInformationResults[2];

            if (closestDistance != int.MaxValue)
            {
                ibm_til.rightToInformation.available = true;
                ibm_til.rightToInformation.description = GetTextAroundHere(rightRelevantLocation, 50, 80);
                ibm_til.rightToInformation.email = FindClosestMentionOfType(result, "EmailAddress", rightRelevantLocation).text;
                ibm_til.rightToInformation.url = FindClosestMentionOfType(result, "URL", rightRelevantLocation).text;
            }
            #endregion

            #region rightToRectificationOrDeletion todo
            closestDistance = int.MaxValue;
            closestRightLocation = int.MaxValue;
            rightRelevantLocation = int.MaxValue;

            rightRelevantEN = "delet";
            rightRelevantDE = "ösch";

            rightRelevantENControl = "complain";
            rightRelevantDEControl = "atenschutzaufsichtsbehörde";

            int[] rightToRectificationResults = CheckForRights(rightLocations, rightRelevantEN, rightRelevantDE, rightRelevantENControl, rightRelevantDEControl);
            closestDistance = rightToRectificationResults[0];
            closestRightLocation = rightToRectificationResults[1];
            rightRelevantLocation = rightToRectificationResults[2];

            if (closestDistance != int.MaxValue)
            {
                ibm_til.rightToRectificationOrDeletion.available = true;
                ibm_til.rightToRectificationOrDeletion.description = GetTextAroundHere(rightRelevantLocation, 50, 80);
                ibm_til.rightToRectificationOrDeletion.email = FindClosestMentionOfType(result, "EmailAddress", rightRelevantLocation).text;
                ibm_til.rightToRectificationOrDeletion.url = FindClosestMentionOfType(result, "URL", rightRelevantLocation).text;

            }
            #endregion

            #region rightToDataPortability
            closestDistance = int.MaxValue;
            closestRightLocation = int.MaxValue;
            rightRelevantLocation = int.MaxValue;

            rightRelevantEN = " transfer ";
            rightRelevantDE = " über";

            rightRelevantENControl = "delet";
            rightRelevantDEControl = "ösch";

            int[] rightToDataPortabilityResults = CheckForRights(rightLocations, rightRelevantEN, rightRelevantDE, rightRelevantENControl, rightRelevantDEControl);
            closestDistance = rightToDataPortabilityResults[0];
            closestRightLocation = rightToDataPortabilityResults[1];
            rightRelevantLocation = rightToDataPortabilityResults[2];

            if (closestDistance != int.MaxValue)
            {
                ibm_til.rightToDataPortability.available = true;
                ibm_til.rightToDataPortability.description = GetTextAroundHere(rightRelevantLocation, 50, 80);
                ibm_til.rightToDataPortability.email = FindClosestMentionOfType(result, "EmailAddress", rightRelevantLocation).text;
                ibm_til.rightToDataPortability.url = FindClosestMentionOfType(result, "URL", rightRelevantLocation).text;
            }

            #endregion

            #region rightToWithdrawConsent 
            closestDistance = int.MaxValue;
            closestRightLocation = int.MaxValue;
            rightRelevantLocation = int.MaxValue;

            rightRelevantEN = "withdraw";
            rightRelevantDE = "iderruf";

            rightRelevantENControl = "delet";
            rightRelevantDEControl = "ösch";

            int[] rightToWithdrawConsentResults = CheckForRights(rightLocations, rightRelevantEN, rightRelevantDE, rightRelevantENControl, rightRelevantDEControl);
            closestDistance = rightToWithdrawConsentResults[0];
            closestRightLocation = rightToWithdrawConsentResults[1];
            rightRelevantLocation = rightToWithdrawConsentResults[2];

            if (closestDistance != int.MaxValue)
            {
                ibm_til.rightToWithdrawConsent.available = true;
                ibm_til.rightToWithdrawConsent.description = GetTextAroundHere(rightRelevantLocation, 50, 80);
                ibm_til.rightToWithdrawConsent.email = FindClosestMentionOfType(result, "EmailAddress", rightRelevantLocation).text;
                ibm_til.rightToWithdrawConsent.url = FindClosestMentionOfType(result, "URL", rightRelevantLocation).text;
            }

            #endregion

            #region rightToComplain todo
            closestDistance = int.MaxValue;
            closestRightLocation = int.MaxValue;
            rightRelevantLocation = int.MaxValue;

            rightRelevantEN = "complain";
            rightRelevantDE = "atenschutzaufsichtsbehörde";

            rightRelevantENControl = "delet";
            rightRelevantDEControl = "ösch";

            int[] rightToComplainResults = CheckForRights(rightLocations, rightRelevantEN, rightRelevantDE, rightRelevantENControl, rightRelevantDEControl);
            closestDistance = rightToComplainResults[0];
            closestRightLocation = rightToComplainResults[1];
            rightRelevantLocation = rightToComplainResults[2];

            if (closestDistance != int.MaxValue)
            {
                //if (IsThisCloseTo("nicht ", closestRightLocation, 20, "before") || IsThisCloseTo("not ", closestRightLocation, 20, "before"))
                //{
                //    ibm_til.rightToComplain.available = false;
                //}
                //else
                //{
                    ibm_til.rightToComplain.available = true;
                    ibm_til.rightToComplain.description = GetTextAroundHere(rightRelevantLocation, 50, 80);
                ibm_til.rightToComplain.email = FindClosestMentionOfType(result, "EmailAddress", rightRelevantLocation).text;
                ibm_til.rightToComplain.url = FindClosestMentionOfType(result, "URL", rightRelevantLocation).text;
                string[] supervisoryAuthTypes = { "Location", "Facility" };
                ibm_til.rightToComplain.supervisoryAuthority.address = GetTextAroundHere(FindClosestMentionOfType(result, "", rightRelevantLocation, supervisoryAuthTypes).location[0], 45);
                ibm_til.rightToComplain.supervisoryAuthority.country = GetTextAroundHere(FindClosestMentionOfType(result, "", rightRelevantLocation, supervisoryAuthTypes).location[0], 45);
                ibm_til.rightToComplain.supervisoryAuthority.name = FindClosestMentionOfType(result, "Organization", rightRelevantLocation).text;
                ibm_til.rightToComplain.supervisoryAuthority.email = FindClosestMentionOfType(result, "EmailAddress", rightRelevantLocation).text;
                ibm_til.rightToComplain.supervisoryAuthority.phone = FindClosestMentionOfType(result, "PhoneNumber", rightRelevantLocation).text;
                //foreach( var evidence in collection)
                //{
                //    ibm_til.rightToComplain.identificationEvidences.Add();
                //}
                //TODO AT COMPLAIN
                //}
            }
            #endregion
          #endregion

          #region automatedDecisionMaking todo

            foreach ( var location in AllOccurrancesOfText("automat"))
            {
                if (!IsThisCloseTo("nicht ", location, 30) 
                    && !IsThisCloseTo("keine ", location, 30)
                    && !IsThisCloseTo(" no ", location, 30)
                    && !IsThisCloseTo(" not ", location, 30))
                {
                    //if (IsThisCloseTo("erarbeit", location, 30) || IsThisCloseTo("process", location, 30))
                    //{

                    //}
                    ibm_til.automatedDecisionMaking.inUse = true;
                    ibm_til.automatedDecisionMaking.logicInvolved = GetTextAroundHere(location, 50); //TODO
                    ibm_til.automatedDecisionMaking.scopeAndIntendedEffects = GetTextAroundHere(location, 50);
                }
            }

          #endregion

          #region changesOfPurpose todo
            List<int> allChangeLocations = AllOccurrancesOfText("changes");
            //foreach (var item in AllOccurrancesOfText("änderung"))
            //{
            //    allChangeLocations.Add(item);
            //}
            foreach (var item in AllOccurrancesOfText("Änderung"))
            {
                allChangeLocations.Add(item);
            }

            foreach (var changeLocation in allChangeLocations)
            {
                //string empty = "N/A";
                TIL.ChangesOfPurpose changeOfPurpose = new TIL.ChangesOfPurpose();

                changeOfPurpose.description = GetTextAroundHere(changeLocation, 40);
                //foreach (var affecteddataCategory in affectedDataCategories)
                //{
                //    changeOfPurpose.affectedDataCategories.Add(affecteddataCategory.text); //TODO
                //}

                changeOfPurpose.plannedDateOfChange = FindClosestMentionOfType(result, "Date", changeLocation).text;
                changeOfPurpose.urlOfNewVersion = FindClosestMentionOfType(result, "URL", changeLocation).text;

                ibm_til.changesOfPurpose.Add(changeOfPurpose);
            }
          #endregion



            ibm_til.meta.language = result.language;
            ibm_til.controller.name = mostMentionedOrga;
            ibm_til.controller.address = GetTextAroundHere(closestLocationMention.location[0], 40, 30);
            if (!firstClosestMention.text.Contains("[No"))
            {
                UniEntity.mention help = FindClosestMentionOfType(result, "Facility", firstClosestMention.location[0]);//TODO Watch out, IBM only..
                if (!help.text.Contains("[No entity"))
                {
                    ibm_til.controller.division = GetTextAroundHere(help.location[0], 40, 30);
                }
            }
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
        private static TIL AnalyseAWSEntityResponse(UniEntity response)
        {
            return AnalyseIBMEntityResponse(response);
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rightLocations"></param>
        /// <param name="rightRelevantEN"></param>
        /// <param name="rightRelevantDE"></param>
        /// <returns>Array with the values closestDistance, closestRightLocation, rightRelevantLocation</returns>
        private static int[] CheckForRights(List<int> rightLocations, string rightRelevantEN, string rightRelevantDE, string rightRelevantENControl = "", string rightRelevantDEControl = "")
        {
            int distanceRight = int.MaxValue;
            int closestDistance = int.MaxValue;
            int closestRightLocation = int.MaxValue;
            int rightRelevantLocation = int.MaxValue;

            int distanceRightControl = int.MaxValue;
            rightRelevantENControl = "";
            rightRelevantDEControl = ""; //TODO REMOVE

            foreach (var location in rightLocations) //Determine correct location
            {
                if (IsThisCloseTo(rightRelevantEN, location, 300)
                    || IsThisCloseTo(rightRelevantDE, location, 300))
                {
                    distanceRight = HowCloseTo(location, rightRelevantEN);
                    distanceRightControl = HowCloseTo(location, rightRelevantENControl, 800);
                    if (distanceRight < closestDistance)
                    {
                        if (distanceRightControl < 800)
                        {
                            closestDistance = distanceRight;
                            closestRightLocation = location;

                            if (HowCloseTo(location, rightRelevantEN, 500, "after") < HowCloseTo(location, rightRelevantEN, 500, "before"))
                            {
                                rightRelevantLocation = location + closestDistance;
                            }
                            else
                            {
                                rightRelevantLocation = location - closestDistance;
                            }
                        }
                    }
                    distanceRight = HowCloseTo(location, rightRelevantDE);
                    distanceRightControl = HowCloseTo(location, rightRelevantDEControl, 800);

                    if (distanceRight < closestDistance)
                    {
                        if (distanceRightControl < 800)
                        {
                            closestDistance = distanceRight;
                            closestRightLocation = location;
                            if (HowCloseTo(location, rightRelevantDE, 500, "after") < HowCloseTo(location, rightRelevantDE, 500, "before"))
                            {
                                rightRelevantLocation = location + closestDistance;
                            }
                            else
                            {
                                rightRelevantLocation = location - closestDistance;
                            }
                        }
                    }
                }
            }
            int[] result = new int[] { closestDistance, closestRightLocation, rightRelevantLocation };
            return result;
        }
        private static Boolean IsThisCloseTo(string text, int start, int range = 100, string mode = "normal")
        {
            int datLen = datenschutzerkl.Length;
            if (String.Compare(mode, "normal")==0)
            {
                if (datLen - start <= range && start <= range)
                {
                    return datenschutzerkl.Substring(0, datLen).Contains(text);
                }
                else if (start <= range)
                {
                    return datenschutzerkl.Substring(0, range + start).Contains(text);
                }
                else if (datLen - start <= range)
                {
                    return datenschutzerkl.Substring(start - range, datLen - start + range).Contains(text);
                }
                else
                {
                    return datenschutzerkl.Substring(start - range, 2 * range).Contains(text);
                }
            }
            else if (String.Compare(mode, "after") == 0)
            {
                if (datLen - start <= range && start <= range)
                {
                    return datenschutzerkl.Substring(start, datLen - start).Contains(text);
                }
                else if (start <= range)
                {
                    return datenschutzerkl.Substring(start, range + start).Contains(text);
                }
                else if (datLen - start <= range)
                {
                    return datenschutzerkl.Substring(start, datLen - start).Contains(text);
                }
                else
                {
                    return datenschutzerkl.Substring(start, range).Contains(text);
                }
            }

            else if (String.Compare(mode, "before") == 0)
            {
                if (datLen - start <= range && start <= range)
                {
                    return datenschutzerkl.Substring(0, datLen).Contains(text);
                }
                else if (start <= range)
                {
                    return datenschutzerkl.Substring(0, range).Contains(text);
                }
                else
                {
                    return datenschutzerkl.Substring(start-range, range).Contains(text);
                }
            }
            return false;
        }
        private static string GetTextAroundHere(int start, int leftrange, int rightrange = -1) //TODO make more readable results and such
        {
            int datLen = datenschutzerkl.Length;
            if (rightrange == -1)
            { rightrange = leftrange; }

            if (datLen - 1 < start + rightrange && start < leftrange)
            {
                return Readable(datenschutzerkl.Substring(0, datLen));
            }
            else if (datLen - 1 <= start + rightrange)
            {
                return Readable(datenschutzerkl.Substring(start - leftrange, leftrange + datLen - start));
            }
            else if (start <= leftrange)
            {
                return Readable(datenschutzerkl.Substring(0, FindReadableTextRange(0, start + (rightrange / 2), start + rightrange)));
            }
            else return Readable(datenschutzerkl.Substring(start - leftrange, FindReadableTextRange(start-leftrange, leftrange + (rightrange / 2), leftrange + rightrange)));
        }
        private static int FindReadableTextRange(int start, int minrange, int maxrange)
        {
            for (int i = start + maxrange; i > start + minrange + 2; i--)
            {
                if (String.Compare(datenschutzerkl.Substring(i - 2, 2), "\r\n") == 0)
                {
                    return i - start;
                }
            }
            for (int i = start + maxrange; i > start + minrange + 1; i--)
            {
                if (String.Compare(datenschutzerkl.Substring(i - 1, 1), ".") == 0 
                    || String.Compare(datenschutzerkl.Substring(i - 1, 1), ";") == 0)
                {
                    return i - start;
                }
            }
            for (int i = start + maxrange; i > start + minrange + 1; i--)
            {
                if (Char.IsWhiteSpace(datenschutzerkl[i]))
                {
                    return i - start;
                }
            }
            return maxrange;
        }
        /// <summary>
        /// Should remove newlines and such from the input string. WIP since it causes bugged outputs.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>string</returns>
        private static string Readable(string input)
        {
            return input.Replace("\r", "").Replace("\n", "\n\t");
            //return input;
        }
        /// <summary>
        /// Finds the Entity Mention of the given type closest to the start location.
        /// mode = after -> search only after the start location.
        /// mode = before -> search only before the start location.
        /// else, search both ways.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="type"></param>
        /// <param name="start"></param>
        /// <param name="types"></param>
        /// <param name="mode"></param>
        /// <param name="ignoreList"></param>
        /// <returns>Entity Mention of the given type closest to the start location</returns>
        private static UniEntity.mention FindClosestMentionOfType(UniEntity root, string type, int start, string[] types = null, string mode = "normal", string[] ignoreList = null)
        {
            UniEntity.entity closestEntity = new UniEntity.entity();
            UniEntity.mention closestMention = new UniEntity.mention();
            closestEntity.text = "[No entity of type \"" + type + "\" found.]";
            closestMention.text = "[No entity of type \"" + type + "\" found.]";
            int closestDistance = int.MaxValue;
            bool isOfCorrectType = false;
            bool foundInIgnore = false;

            foreach (UniEntity.entity entity in root.entities)
            {
                isOfCorrectType = false;
                if (types != null)
                {
                    for (int i = 0; i < types.Length; i++)
                    {
                        if (String.Compare(entity.type, types[i]) == 0)
                        {
                            isOfCorrectType = true;
                        }
                    }
                }
                else
                {
                    if (String.Compare(entity.type, type) == 0)
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
                    if (String.Compare(mode, "before") == 0)
                    {
                        if (mention.location[0] > start)
                        {   continue;   }
                    }
                    else if (String.Compare(mode, "after") == 0)
                    {
                        if (mention.location[0] < start)
                        {   continue;   }
                    }
                    else
                    {
                        if (Math.Abs(mention.location[0] - start) < closestDistance)
                        {
                            closestDistance = Math.Abs(mention.location[0] - start);
                            closestEntity = entity;
                            closestMention = mention;
                        }
                    }
                }
            }
            return closestMention;
        }
        private static UniEntity.mention GetEarliestMention(UniEntity.entity entity)
        {
            int earliest = int.MaxValue;
            UniEntity.mention earliestMention = new UniEntity.mention();
            if (entity.mentions != null)
            {
                foreach (UniEntity.mention mention in entity.mentions)
                {
                    if (mention.location[0] < earliest)
                    {
                        earliest = mention.location[0];
                        earliestMention = mention;
                    }
                }
            }
            return earliestMention;
        }
        private static UniEntity.mention GetOverallClosestMentionOfType(UniEntity root, string type, UniEntity.entity entity, string mode = "normal", string[] types = null, string[] ignoreList = null)
        {
            //IBMEntity.entity closestEntity = new IBMEntity.entity();
            UniEntity.mention closestMention = new UniEntity.mention();
            //closestEntity.text = "[No entity of type \"" + types + "\" found.]";
            closestMention.location = new List<int>(); //TODO watch out for empty locations
            closestMention.location.Add(0);
            closestMention.location.Add(0);
            closestMention.text = "No fitting Entity found";


            int closestDistance = int.MaxValue;
            bool isOfCorrectType = false;
            foreach (UniEntity.entity item in root.entities)
            {
                isOfCorrectType = false;
                if (types != null)
                {
                    for (int i = 0; i < types.Length; i++)
                    {
                        if (String.Compare(item.type, types[i]) == 0)
                        {
                            isOfCorrectType = true;
                        }
                    }
                }
                else if (String.Compare(item.type, type) == 0)
                {
                    isOfCorrectType = true;
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
        private static UniEntity.mention GetOverallClosestMentionOfTypeToString(UniEntity root, string type, string text, string mode = "normal", string[] types = null)
        {
            int ClosestDistance = int.MaxValue;
            int distance;
            UniEntity.mention closestMention = new UniEntity.mention();
            closestMention.location = new List<int>();
            closestMention.location.Add(0);
            closestMention.location.Add(0);
            closestMention.text = "No entity found within range of the text " + text;

            bool isOfCorrectType;
            foreach (var entity in root.entities)
            {
                isOfCorrectType = false;
                if (types != null)
                {
                    for (int i = 0; i < types.Length; i++)
                    {
                        if (String.Compare(entity.type, types[i]) == 0)
                        {
                            isOfCorrectType = true;
                        }
                    }
                }
                else if (String.Compare(entity.type, type) == 0)
                {   isOfCorrectType = true; }
                if (!isOfCorrectType)
                {   continue;   }
                foreach (var mention in entity.mentions)
                {
                    distance = HowCloseTo(mention.location[0], text, 200, mode);
                    if (distance < ClosestDistance)
                    {
                        ClosestDistance = distance;
                        closestMention = mention;
                    }
                }
            }
            return closestMention;
        }
        private static int HowCloseTo(int location, string text, int maxRange = 500, string mode = "normal")
        { 
            for (int i = 0; i < maxRange; i++)
            {
                if (IsThisCloseTo(text, location, i, mode))
                {
                    return i;
                }
            }
            return maxRange;
        }
        private static List<int> AllOccurrancesOfText(string text)
        {
            List<int> result = new List<int>();
            int textLen = text.Length;
            for (int i = 0; i < datenschutzerkl.Length-textLen; i++)
            {
                if (datenschutzerkl.Substring(i, textLen).Contains(text))
                {
                    result.Add(i);
                }
            }
            //TODO optimize

            return result;
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
                            //printTILResult(AnalyseIBMEntityResponse(responseEntity));
                            printTILResultReadable(AnalyseIBMEntityResponse(responseEntity));
                            Console.Write("IBM");
                            break;
                        case "A":
                            Console.WriteLine("AWS - Beginning processing...");
                            printTILResult(AnalyseAWSEntityResponse(responseEntity));
                            Console.Write("AWS");
                            break;
                        case "M":
                            Console.WriteLine("Microsoft - Beginning processing...");
                            printTILResult(AnalyseMicrosoftEntityResponse(responseEntity));
                            Console.Write("Microsoft");
                            break;
                        case "G":
                            Console.WriteLine("Google - Beginning processing...");
                            printTILResult(AnalyseGoogleEntityResponse(responseEntity));
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

        private static void printTILResultReadable(TIL resultTIL)
        {
            Console.WriteLine("\n##########################################################################################\n");
            //Meta
            //Console.WriteLine("meta: ");
            //Console.WriteLine("\tmeta._id: " + resultTIL.meta._id);
            //Console.WriteLine("\tmeta.name: " + resultTIL.meta.name);
            //Console.WriteLine("\tmeta.created: " + resultTIL.meta.created);
            //Console.WriteLine("\tmeta.modified: " + resultTIL.meta.modified);
            //Console.WriteLine("\tmeta.version: " + resultTIL.meta.version);
            //Console.WriteLine("\tmete.language: " + resultTIL.meta.language);
            //Console.WriteLine("\tmeta.status: " + resultTIL.meta.status);
            //Console.WriteLine("\tmeta.url: " + resultTIL.meta.url);
            //Console.WriteLine("\tmeta.hash: " + resultTIL.meta._hash);
            //Console.WriteLine("");

            #region Controller
            //Controller
            Console.WriteLine("controller: ");
            Console.WriteLine("\tcontroller.name: " + resultTIL.controller.name);
            Console.WriteLine("\tcontroller.division: \n" + resultTIL.controller.division);
            Console.WriteLine("\tcontroller.address: \n" + resultTIL.controller.address);
            Console.WriteLine("\tcontroller.country: " + resultTIL.controller.country);
            Console.WriteLine("\tcontroller.representative: ");
            Console.WriteLine("\t\tcontroller.representative.name: " + resultTIL.controller.representative.name);
            Console.WriteLine("\t\tcontroller.representative.email: " + resultTIL.controller.representative.email);
            Console.WriteLine("\t\tcontroller.representative.phone: " + resultTIL.controller.representative.phone);
            Console.WriteLine("");
            #endregion

            #region DataProtectionOfficer
            //DataProtectionOfficer
            Console.WriteLine("dataProtectionOfficer: ");
            Console.WriteLine("\tdataProtectionOfficer.name: "+ resultTIL.dataProtectionOfficer.name);
            Console.WriteLine("\tdataProtectionOfficer.address: "+ resultTIL.dataProtectionOfficer.address);
            Console.WriteLine("\tdataProtectionOfficer.country: "+ resultTIL.dataProtectionOfficer.country);
            Console.WriteLine("\tdataProtectionOfficer.email: "+resultTIL.dataProtectionOfficer.email);
            Console.WriteLine("\tdataProtectionOfficer.phone: "+resultTIL.dataProtectionOfficer.phone);
            Console.WriteLine("");
            #endregion

            //DataDisclosed
            Console.WriteLine("dataDisclosed: ");
            foreach (var item in resultTIL.dataDisclosed)
            {
                Console.WriteLine("\tdataDisclosed._id: " + item._id);
                Console.WriteLine("\tdataDisclosed.category: " + item.category);
                Console.WriteLine("\tdataDisclosed.purposes: ");
                //Purposes
                foreach (var purpose in item.purposes)
                {
                    Console.WriteLine("\t\tdataDisclosed.purpose.purpose: " + purpose.purpose);
                    Console.WriteLine("\t\tdataDisclosed.purpose.description: " + purpose.description);
                }

                //legalBases
                Console.WriteLine("\tdataDisclosed.legalBases ");
                foreach (var legalBase in item.legalBases)
                {
                    Console.WriteLine("\t\tdataDisclosed.legalBase.reference: " + legalBase.reference);
                    Console.WriteLine("\t\tdataDisclosed.legalBase.description: " + legalBase.description);
                }

                //legitimateInterests
                Console.WriteLine("\tdataDisclosed.legitimateInterests ");
                foreach (var legitimateInterest in item.legitimateInterests)
                {
                    Console.WriteLine("\t\tdataDisclosed.legitimateInterest.exists: " + legitimateInterest.exists);
                    Console.WriteLine("\t\tdataDisclosed.legitimateInterest.reasoning: " + legitimateInterest.reasoning);
                }
                
                //recipients
                Console.WriteLine("\tdataDisclosed.recipients: ");
                foreach (var recipient in item.recipients)
                {

                    Console.WriteLine("\t\tdataDisclosed.recipient.name" + recipient.name);
                    Console.WriteLine("\t\tdataDisclosed.recipient.division" + recipient.division);
                    Console.WriteLine("\t\tdataDisclosed.recipient.address" + recipient.address);
                    Console.WriteLine("\t\tdataDisclosed.recipient.country" + recipient.country);
                    Console.WriteLine("\t\tdataDisclosed.recipient.address" + recipient.address);

                    //recipient.representative
                    Console.WriteLine("\t\tdataDisclosed.recipient.representative: ");
                    Console.WriteLine("\t\t\tdataDisclosed.recipient.representative.name: " + recipient.representative.name);
                    Console.WriteLine("\t\t\tdataDisclosed.recipient.representative.email: " + recipient.representative.email);
                    Console.WriteLine("\t\t\tdataDisclosed.recipient.representative.phone: " + recipient.representative.phone);

                    Console.WriteLine("\t\tdataDisclosed.recipient.category" + recipient.category);
                }

                Console.WriteLine("\tdataDisclosed.storage: ");
                //storage
                foreach (var storageItem in item.storage)
                {
                    //temporal
                    Console.WriteLine("\t\tdataDisclosed.storage.temporal: ");
                    foreach (var tempo in storageItem.temporal)
                    {
                        Console.WriteLine("\t\t\tdataDisclosed.storage.temporal.description: " + tempo.description);
                        Console.WriteLine("\t\t\tdataDisclosed.storage.temporal.ttl: " + tempo.ttl);
                    }
                    //purposeConditional
                    Console.WriteLine("\t\tdataDisclosed.storage.purposeConditional: ");
                    foreach (var purposeCondition in storageItem.purposeConditional)
                    {
                        Console.WriteLine("\t\t\tdataDisclosed.storage.purposeConditional: "+ purposeCondition);
                    }
                    //legalBasisConditional
                    Console.WriteLine("\t\tdataDisclosed.storage.legalBasisConditional: ");
                    foreach (var legalBasisCondition in storageItem.legalBasisConditional)
                    {
                        Console.WriteLine("\t\t\tdataDisclosed.storage.legalBasisConditional: " + legalBasisCondition);
                    }
                    //aggregationFunction
                    Console.WriteLine("\t\tdataDisclosed.storage.aggregationFunction: " + storageItem.aggregationFunction);
                }
                //nonDisclosure
                Console.WriteLine("\tdataDisclosed.nonDisclosure: ");
                Console.WriteLine("\t\tdataDisclosed.nonDisclosure.legalRequirement: " + item.nonDisclosure.legalRequirement);
                Console.WriteLine("\t\tdataDisclosed.nonDisclosure.contractualRegulation: " + item.nonDisclosure.contractualRegulation);
                Console.WriteLine("\t\tdataDisclosed.nonDisclosure.obligationToProvide: " + item.nonDisclosure.obligationToProvide);
                Console.WriteLine("\t\tdataDisclosed.nonDisclosure.consequences: " + item.nonDisclosure.consequences);
            }
            //ThirdCountryTransfers
            Console.WriteLine("thirdCountryTransfers: ");
            foreach (var thirdCountryTransfer in resultTIL.thirdCountryTransfers)
            {
                Console.WriteLine("\tthirdCountryTransfer.country: " + thirdCountryTransfer.country);

                //adequacyDecision
                Console.WriteLine("\tthirdCountryTransfer.adequacyDecision: ");
                    Console.WriteLine("\t\tthirdCountryTransfer.adequacyDecision.available: " + thirdCountryTransfer.adequacyDecision.available);
                    Console.WriteLine("\t\tthirdCountryTransfer.adequacyDecision.description: " + thirdCountryTransfer.adequacyDecision.description);

                //appropriateGuarantees
                Console.WriteLine("\tthirdCountryTransfer.appropriateGuarantees: ");
                Console.WriteLine("\t\tthirdCountryTransfer.appropriateGuarantees.available: " + thirdCountryTransfer.appropriateGuarantees.available);
                Console.WriteLine("\t\tthirdCountryTransfer.appropriateGuarantees.description: " + thirdCountryTransfer.appropriateGuarantees.description);

                //presenceOfEnforcableRightsAndEffectiveRemedies
                Console.WriteLine("\tthirdCountryTransfer.presenceOfEnforcableRightsAndEffectiveRemedies");
                    Console.WriteLine("\t\tthirdCountryTransfer.presenceOfEnforcableRightsAndEffectiveRemedies.available" + thirdCountryTransfer.presenceOfEnforcableRightsAndEffectiveRemedies.available);
                    Console.WriteLine("\t\tthirdCountryTransfer.presenceOfEnforcableRightsAndEffectiveRemedies.description" + thirdCountryTransfer.presenceOfEnforcableRightsAndEffectiveRemedies.description);

                //standardDataProtectionClause
                Console.WriteLine("\tthirdCountryTransfer.standardDataProtectionClause: ");
                    Console.WriteLine("\t\tthirdCountryTransfer.standardDataProtectionClause.available: " + thirdCountryTransfer.standardDataProtectionClause.available);
                    Console.WriteLine("\t\tthirdCountryTransfer.standardDataProtectionClause.description: " + thirdCountryTransfer.standardDataProtectionClause.description);
            }

            //accessAndDataPortability
            Console.WriteLine("accessAndDataPortability: ");
            Console.WriteLine("\taccessAndDataPortability.available: " + resultTIL.accessAndDataPortability.available);
            Console.WriteLine("\taccessAndDataPortability.description: " + resultTIL.accessAndDataPortability.description);
            Console.WriteLine("\taccessAndDataPortability.url: " + resultTIL.accessAndDataPortability.url);
            Console.WriteLine("\taccessAndDataPortability.email: " + resultTIL.accessAndDataPortability.email);
                //identificationEvidences
            Console.WriteLine("\taccessAndDataPortability.identificationEvidences: ");
            foreach (var identificationEvidence in resultTIL.accessAndDataPortability.identificationEvidences)
            {
                Console.WriteLine("\t\taccessAndDataPortability.identificationEvidence: " + identificationEvidence);
            }
            //administrativeFee
            Console.WriteLine("\taccessAndDataPortability.administrativeFee: ");
                Console.WriteLine("\t\taccessAndDataPortability.administrativeFee.amount: " + resultTIL.accessAndDataPortability.administrativeFee.amount);
                Console.WriteLine("\t\taccessAndDataPortability.administrativeFee.currency: " + resultTIL.accessAndDataPortability.administrativeFee.currency);
            //dataFormats
            Console.WriteLine("\taccessAndDataPortability.dataFormats: ");
            foreach (var format in resultTIL.accessAndDataPortability.dataFormats)
            {

                Console.WriteLine("\t\taccessAndDataPortability.dataFormat: " + format);
            }
            //Sources2
            Console.WriteLine("sources: ");

            foreach (var source in resultTIL.sources)
            {
                Console.WriteLine("\tsource._id: " + source._id);
                Console.WriteLine("\tsource.dataCategory: " + source.dataCategory);
                //sources1
                Console.WriteLine("\tsource.sources: ");
                foreach (var source1 in source.sources)
                {
                    Console.WriteLine("\t\tsource.sources.description: " + source1.description);
                    Console.WriteLine("\t\tsource.sources.url: " + source1.url);
                    Console.WriteLine("\t\tsource.sources.publiclyAvailable: " + source1.publiclyAvailable);
                }
            }

            //rightToInformation
            Console.WriteLine("rightToInformation: ");
            Console.WriteLine("\trightToInformation.available: " + resultTIL.rightToInformation.available);
            Console.WriteLine("\trightToInformation.description: " + resultTIL.rightToInformation.description);
            Console.WriteLine("\trightToInformation.url: " + resultTIL.rightToInformation.url);
            Console.WriteLine("\trightToInformation.email: " + resultTIL.rightToInformation.email);
                //identificationEvidences
            Console.WriteLine("\trightToInformation.identificationEvidences: ");
            foreach (var evidence in resultTIL.rightToInformation.identificationEvidences)
            {
                Console.WriteLine("\t\trightToInformation.identificationEvidence: " + evidence);
            }

            //rightToRectificationOrDeletion
            Console.WriteLine("rightToRectificationOrDeletion: ");
            Console.WriteLine("\trightToRectificationOrDeletion.available: " + resultTIL.rightToRectificationOrDeletion.available);
            Console.WriteLine("\trightToRectificationOrDeletion.description: " + resultTIL.rightToRectificationOrDeletion.description);
            Console.WriteLine("\trightToRectificationOrDeletion.url: " + resultTIL.rightToRectificationOrDeletion.url);
            Console.WriteLine("\trightToRectificationOrDeletion.email: " + resultTIL.rightToRectificationOrDeletion.email);
                //identificationEvidences
            Console.WriteLine("\trightToRectificationOrDeletion.identificationEvidences: ");
            foreach (var evidence in resultTIL.rightToRectificationOrDeletion.identificationEvidences)
            {
                Console.WriteLine("\t\trightToRectificationOrDeletion.identificationEvidence: " + evidence);
            }
            //rightToDataPortability

            Console.WriteLine("rightToDataPortability: ");
            Console.WriteLine("\trightToDataPortability.available: " + resultTIL.rightToDataPortability.available);
            Console.WriteLine("\trightToDataPortability.description: " + resultTIL.rightToDataPortability.description);
            Console.WriteLine("\trightToDataPortability.url: " + resultTIL.rightToDataPortability.url);
            Console.WriteLine("\trightToDataPortability.email: " + resultTIL.rightToDataPortability.email);
            //identificationEvidences
            Console.WriteLine("\trightToDataPortability.identificationEvidences: ");
            foreach (var evidence in resultTIL.rightToDataPortability.identificationEvidences)
            {
                Console.WriteLine("\t\trightToDataPortability.identificationEvidence: " + evidence);
            }
            //rightToWithdrawConsent

            Console.WriteLine("rightToWithdrawConsent: ");
            Console.WriteLine("\trightToWithdrawConsent.available: " + resultTIL.rightToWithdrawConsent.available);
            Console.WriteLine("\trightToWithdrawConsent.description: " + resultTIL.rightToWithdrawConsent.description);
            Console.WriteLine("\trightToWithdrawConsent.url: " + resultTIL.rightToWithdrawConsent.url);
            Console.WriteLine("\trightToWithdrawConsent.email: " + resultTIL.rightToWithdrawConsent.email);
            //identificationEvidences
            Console.WriteLine("\trightToWithdrawConsent.identificationEvidences: ");
            foreach (var evidence in resultTIL.rightToWithdrawConsent.identificationEvidences)
            {
                Console.WriteLine("\t\trightToWithdrawConsent.identificationEvidence: " + evidence);
            }
            //rightToComplain

            Console.WriteLine("rightToComplain: ");
            Console.WriteLine("\trightToComplain.available: " + resultTIL.rightToComplain.available);
            Console.WriteLine("\trightToComplain.description: " + resultTIL.rightToComplain.description);
            Console.WriteLine("\trightToComplain.url: " + resultTIL.rightToComplain.url);
            Console.WriteLine("\trightToComplain.email: " + resultTIL.rightToComplain.email);
            //identificationEvidences
            Console.WriteLine("\trightToComplain.identificationEvidences: ");
            foreach (var evidence in resultTIL.rightToComplain.identificationEvidences)
            {
                Console.WriteLine("\t\trightToComplain.identificationEvidence: " + evidence);
            }
            //supervisoryAuthority
            Console.WriteLine("\trightToComplain.supervisoryAuthority: ");
            Console.WriteLine("\t\trightToComplain.supervisoryAuthority.name: " + resultTIL.rightToComplain.supervisoryAuthority.name);
            Console.WriteLine("\t\trightToComplain.supervisoryAuthority.address: " + resultTIL.rightToComplain.supervisoryAuthority.address);
            Console.WriteLine("\t\trightToComplain.supervisoryAuthority.country: " + resultTIL.rightToComplain.supervisoryAuthority.country);
            Console.WriteLine("\t\trightToComplain.supervisoryAuthority.email: " + resultTIL.rightToComplain.supervisoryAuthority.email);
            Console.WriteLine("\t\trightToComplain.supervisoryAuthority.phone: " + resultTIL.rightToComplain.supervisoryAuthority.phone);

            //automatedDecisionMaking
            Console.WriteLine("automatedDecisionMaking: ");
            Console.WriteLine("\tautomatedDecisionMaking.inUse: " + resultTIL.automatedDecisionMaking.inUse);
            Console.WriteLine("\tautomatedDecisionMaking.logicInvolved: " + resultTIL.automatedDecisionMaking.logicInvolved);
            Console.WriteLine("\tautomatedDecisionMaking.scopeAndIntendedEffects: " + resultTIL.automatedDecisionMaking.scopeAndIntendedEffects);

            //changesOfPurpose
                Console.WriteLine("changesOfPurpose: ");
            foreach (var changeOfPurpose in resultTIL.changesOfPurpose)
            {
                Console.WriteLine("\tchangesOfPurpose.description: " + changeOfPurpose.description);
                foreach (var dataCateg in changeOfPurpose.affectedDataCategories)
                {
                    Console.WriteLine("\tchangesOfPurpose.affectedDataCategories: " + dataCateg);
                }
                Console.WriteLine("\tchangesOfPurpose.plannedDateOfChange: " + changeOfPurpose.plannedDateOfChange);
                Console.WriteLine("\tchangesOfPurpose.urlOfNewVersion: " + changeOfPurpose.urlOfNewVersion);
            }
            Console.WriteLine("");
            //Console.WriteLine(datenschutzerkl.Replace("\r\n", "WHAT"));
            return;
        }
    }
}

//NOTIZEN

    //TODO:
    //  Evtl Bei Google Entities mit den gleichen Metadaten (zB Wikieinträge) zu einem kombinieren.
    //maybe die Datenschutzrichtlinie einteilen / trennen nach Absätzen zB 3.3.3.3
    //make all categories capitalized
    //make it so if a txt file has been processed before, draw the results from a file if found. (toJSON, save in file -> extract from file, fromJson)