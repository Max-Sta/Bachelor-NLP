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
        private static string inputPath;
        private static UniEntityResponse calcEntity;
        private static string calcSaveFilePath = "";
        private static string calcNewAllFilesString = "";
        private static string calcNewAlleAbdeckung = "";

        static void Main(string[] args)
        {
            if (LoadCredentials() == 0)
            {
                for (int i = 0; i < 100000; i++)
                {
                    Console.WriteLine("Please choose (d)emo, (a)nalyseDemo, (f)ullRun, (c)alculateAccuracy or (q)uit\n");
                    string read = Console.ReadLine();
                    try
                    {
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
                            case "c":
                                CalculateAccuracy();
                                break;
                            default:
                                Console.WriteLine("Command not recognized");
                                break;
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("An error has arisen during execution: " + ex);
                    }
                }
            }
        }

        private static void CalculateAccuracy()
        {
            calcNewAllFilesString = "FileName\tUsefulness\tORGANIZATION\tCOMMERCIALITEM\tDATE\tEVENT\tLOCATION\tOTHER\tPERSON\t" +
                    "QUANTITY\tTITLE\tPERSONTYPE\tPRODUCT\tSKILL\tADDRESS\tPHONENUMBER\tEMAILADDRESS\tURL\tIPADDRESS\t" +
                    "UNKNOWN\tWORKOFART\tCONSUMERGOOD\tNUMBER\tPRICE\tCOMPANY\tDURATION\tFACILITY\tGEOGRAPHICFEATURE\tHASHTAG\tJOBTITLE\t" +
                    "MEASURE\tMONEY\tORDINAL\tPERCENT\tTIME\tTWITTERHANDLE\r\n";
            calcNewAlleAbdeckung = "";

            Console.WriteLine("Please enter the base privacy policy directory path, or (q)uit.");
            string baseDirectory = Console.ReadLine();
            if (String.Compare(baseDirectory, "q")==0)
            {   return; }
            if (!Directory.Exists(baseDirectory))
            {
                Console.WriteLine("The specified path could not be found");
                CalculateAccuracy();
                return;
            }
            else
            {
                string tilDirectory = baseDirectory + "\\TIL";
                string resDirectory = baseDirectory + "\\Results";
                string entityResponseDirectory = baseDirectory + "\\Responses";
                string allF1ScoresString = "F1-Scores\r\nFileName#ServiceCode\tController\tDataProtectionOfficer\tRightToInformation\tRightToRectificationOrDeletion\t" +  //AccessAndDataPortability\t
                    "RightToDataPortability\tRightToWithdrawConsent\tRightToComplain\tAutomatedDecisionMaking\tChangesOfPurpose\tInsgesamt" +"\r\n";
                string allAccuraciesString = "Accuracies (TP + TN / tn+tp+fp+fn)\r\n";

                foreach (var resultFile in Directory.GetFiles(resDirectory))    //Check result file completion compared to TIL file
                {
                    
                    string fileWithoutExt = Path.GetFileNameWithoutExtension(resultFile);
                    string[] fileParts = fileWithoutExt.Split("#");
                    string rawTextFilePath = baseDirectory + "\\" + fileParts[0] + ".txt";
                    string tilFilePath = tilDirectory + "\\" + fileParts[0] + ".json";

                    if (Directory.GetFiles(baseDirectory).Contains(rawTextFilePath)
                        && Directory.GetFiles(tilDirectory).Contains(tilFilePath))
                    {
                        Console.WriteLine("\nMatching files found for " + fileWithoutExt + ". Beginning analysis.");

                        List<float> contrAccNew = new List<float>();
                        List<float> reprAccNew = new List<float>();
                        List<float> dataProtAccNew = new List<float>();
                        List<float> accessAndAccNew = new List<float>();
                        List<float> rightToInfoAccNew = new List<float>();
                        List<float> rightToRectiAccNew = new List<float>();
                        List<float> rightToDataPortAccNew = new List<float>();
                        List<float> rightToWithdrawAccNew = new List<float>();
                        List<float> rightToComplainAccNew = new List<float>();
                        List<float> supervisoryAccNew = new List<float>();
                        List<float> automatedDecAccNew = new List<float>();
                        List<float> changeOfAccTILRecall = new List<float>();
                        List<float> changeOfAccRESPrecision = new List<float>();
                        List<float> changeOfAccNewTotal = new List<float>();
                        List<float> overallAccNew = new List<float>();


                        //float[] contrAcc = new float[5];
                        //float[] reprAcc = new float[3];
                        //float[] dataProtAcc = new float[5];
                        //float[] accessAndAcc = new float[4];    //no identEvid., administrativeFee or dataFormat
                        //float[] rightToInfoAcc = new float[4]; //no identificationEvidences
                        //float[] rightToRectiAcc = new float[4]; //no identificationEvidences
                        //float[] rightToDataPortAcc = new float[4]; //no identificationEvidences
                        //float[] rightToWithdrawAcc = new float[4]; //no identificationEvidences
                        //float[] rightToComplainAcc = new float[5]; //no identificationEvidences
                        //float[] supervisoryAcc = new float[5];
                        //float[] automatedDecAcc = new float[3];
                        //float changesOfAcc = 0;
                        //float overallAcc = 0;


                        try
                        {
                            TIL tilFile = JsonConvert.DeserializeObject<TIL>(File.ReadAllText(tilFilePath).Replace("null", "false"));
                            TIL resFile = JsonConvert.DeserializeObject<TIL>(File.ReadAllText(resultFile).Replace("N/A", ""));
                            #region old accuracy
                            ////Controller
                            //    contrAcc[0] = CompareResToTIL(resFile.controller.name, tilFile.controller.name);
                            //    contrAcc[1] = CompareResToTIL(resFile.controller.division, tilFile.controller.division);
                            //    contrAcc[2] = CompareResToTIL(resFile.controller.address, tilFile.controller.address);
                            //    contrAcc[3] = CompareResToTIL(resFile.controller.country, tilFile.controller.country);

                            ////Controller.representative
                            //    reprAcc[0] = CompareResToTIL(resFile.controller.representative.name, tilFile.controller.representative.name);
                            //    reprAcc[1] = CompareResToTIL(resFile.controller.representative.email, tilFile.controller.representative.email);
                            //    reprAcc[2] = CompareResToTIL(resFile.controller.representative.phone, tilFile.controller.representative.phone);
                            //    contrAcc[4] = reprAcc.Sum() / 3;
                            //    Console.WriteLine("ControllerAccuracy: " + contrAcc.Sum() / 5);

                            ////DataProtectionOfficer
                            //    dataProtAcc[0] = CompareResToTIL(resFile.dataProtectionOfficer.name, tilFile.dataProtectionOfficer.name);
                            //    dataProtAcc[1] = CompareResToTIL(resFile.dataProtectionOfficer.address, tilFile.dataProtectionOfficer.address);
                            //    dataProtAcc[2] = CompareResToTIL(resFile.dataProtectionOfficer.country, tilFile.dataProtectionOfficer.country);
                            //    dataProtAcc[3] = CompareResToTIL(resFile.dataProtectionOfficer.email, tilFile.dataProtectionOfficer.email);
                            //    dataProtAcc[4] = CompareResToTIL(resFile.dataProtectionOfficer.phone, tilFile.dataProtectionOfficer.phone);
                            //    Console.WriteLine("dataProtectionOfficerAccuracy: " + dataProtAcc.Sum() / 5);

                            ////AccessAndDataPortabilityforeach (var item in accessAndAcc)

                            //    accessAndAcc[0] = CompareResToTIL(resFile.accessAndDataPortability.available.ToString(), tilFile.accessAndDataPortability.available.ToString());
                            //    accessAndAcc[1] = CompareResToTIL(resFile.accessAndDataPortability.description, tilFile.accessAndDataPortability.description);
                            //    accessAndAcc[2] = CompareResToTIL(resFile.accessAndDataPortability.url, tilFile.accessAndDataPortability.url);
                            //    accessAndAcc[3] = CompareResToTIL(resFile.accessAndDataPortability.email, tilFile.accessAndDataPortability.email);
                            //    Console.WriteLine("AccessAndDataPortabilityAccuracy: " + accessAndAcc.Sum() / 4);


                            ////rightToInformation
                            //    rightToInfoAcc[0] = CompareResToTIL(resFile.rightToInformation.available.ToString(), tilFile.rightToInformation.available.ToString());
                            //    rightToInfoAcc[1] = CompareResToTIL(resFile.rightToInformation.description, tilFile.rightToInformation.description);
                            //    rightToInfoAcc[2] = CompareResToTIL(resFile.rightToInformation.url, tilFile.rightToInformation.url);
                            //    rightToInfoAcc[3] = CompareResToTIL(resFile.rightToInformation.email, tilFile.rightToInformation.email);
                            //    Console.WriteLine("RightToInformationAccuracy: " + rightToInfoAcc.Sum() / 4);

                            ////rightToRectificationOrDeletion
                            //    rightToRectiAcc[0] = CompareResToTIL(resFile.rightToRectificationOrDeletion.available.ToString(), tilFile.rightToRectificationOrDeletion.available.ToString());
                            //    rightToRectiAcc[1] = CompareResToTIL(resFile.rightToRectificationOrDeletion.description, tilFile.rightToRectificationOrDeletion.description);
                            //    rightToRectiAcc[2] = CompareResToTIL(resFile.rightToRectificationOrDeletion.url, tilFile.rightToRectificationOrDeletion.url);
                            //    rightToRectiAcc[3] = CompareResToTIL(resFile.rightToRectificationOrDeletion.email, tilFile.rightToRectificationOrDeletion.email);
                            //    Console.WriteLine("RightToRectificationOrDeletionAccuracy: " + rightToRectiAcc.Sum() / 4);

                            ////rightToDataPortability
                            //    rightToDataPortAcc[0] = CompareResToTIL(resFile.rightToDataPortability.available.ToString(), tilFile.rightToDataPortability.available.ToString());
                            //    rightToDataPortAcc[1] = CompareResToTIL(resFile.rightToDataPortability.description, tilFile.rightToDataPortability.description);
                            //    rightToDataPortAcc[2] = CompareResToTIL(resFile.rightToDataPortability.url, tilFile.rightToDataPortability.url);
                            //    rightToDataPortAcc[3] = CompareResToTIL(resFile.rightToDataPortability.email, tilFile.rightToDataPortability.email);
                            //    Console.WriteLine("rightToDataPortabilityAccuracy: " + rightToDataPortAcc.Sum() / 4);

                            ////rightToWithdrawConsent
                            //    rightToWithdrawAcc[0] = CompareResToTIL(resFile.rightToWithdrawConsent.available.ToString(), tilFile.rightToWithdrawConsent.available.ToString());
                            //    rightToWithdrawAcc[1] = CompareResToTIL(resFile.rightToWithdrawConsent.description, tilFile.rightToWithdrawConsent.description);
                            //    rightToWithdrawAcc[2] = CompareResToTIL(resFile.rightToWithdrawConsent.url, tilFile.rightToWithdrawConsent.url);
                            //    rightToWithdrawAcc[3] = CompareResToTIL(resFile.rightToWithdrawConsent.email, tilFile.rightToWithdrawConsent.email);
                            //    Console.WriteLine("rightToWithdrawConsentAccuracy: " + rightToWithdrawAcc.Sum() / 4);

                            ////rightToComplain
                            //    rightToComplainAcc[0] = CompareResToTIL(resFile.rightToComplain.available.ToString(), tilFile.rightToComplain.available.ToString());
                            //    rightToComplainAcc[1] = CompareResToTIL(resFile.rightToComplain.description, tilFile.rightToComplain.description);
                            //    rightToComplainAcc[2] = CompareResToTIL(resFile.rightToComplain.url, tilFile.rightToComplain.url);
                            //    rightToComplainAcc[3] = CompareResToTIL(resFile.rightToComplain.email, tilFile.rightToComplain.email);
                            ////supervisoryAuthority
                            //    supervisoryAcc[0] = CompareResToTIL(resFile.rightToComplain.supervisoryAuthority.name, tilFile.rightToComplain.supervisoryAuthority.name);
                            //    supervisoryAcc[1] = CompareResToTIL(resFile.rightToComplain.supervisoryAuthority.address, tilFile.rightToComplain.supervisoryAuthority.address);
                            //    supervisoryAcc[2] = CompareResToTIL(resFile.rightToComplain.supervisoryAuthority.country, tilFile.rightToComplain.supervisoryAuthority.country);
                            //    supervisoryAcc[3] = CompareResToTIL(resFile.rightToComplain.supervisoryAuthority.email, tilFile.rightToComplain.supervisoryAuthority.email);
                            //    supervisoryAcc[4] = CompareResToTIL(resFile.rightToComplain.supervisoryAuthority.phone, tilFile.rightToComplain.supervisoryAuthority.phone);

                            //    rightToComplainAcc[4] = supervisoryAcc.Sum() / 5;
                            //    Console.WriteLine("rightToComplainAccuracy: " + rightToComplainAcc.Sum() / 5);

                            ////AutomatedDecisionMaking
                            //    automatedDecAcc[0] = CompareResToTIL(resFile.automatedDecisionMaking.inUse.ToString(), tilFile.automatedDecisionMaking.inUse.ToString());
                            //    automatedDecAcc[1] = CompareResToTIL(resFile.automatedDecisionMaking.logicInvolved, tilFile.automatedDecisionMaking.logicInvolved);
                            //    automatedDecAcc[2] = CompareResToTIL(resFile.automatedDecisionMaking.scopeAndIntendedEffects, tilFile.automatedDecisionMaking.scopeAndIntendedEffects);
                            //    Console.WriteLine("automatedDecisionMakingAccuracy: " + automatedDecAcc.Sum() / 3);

                            ////ChangesOfPurpose
                            //    int changeCount = 0;
                            //    foreach (var tilFileChange in tilFile.changesOfPurpose)
                            //    {
                            //        float[] singularchangeOfAcc = new float[4];
                            //        foreach (var resFileChange in resFile.changesOfPurpose)
                            //        {
                            //            singularchangeOfAcc[0] = CompareResToTIL(resFileChange.description, tilFileChange.description);

                            //            foreach (var tilFileaffectedDataCat in tilFileChange.affectedDataCategories)
                            //            {
                            //                foreach (var resFileaffectedDataCat in resFileChange.affectedDataCategories)
                            //                {
                            //                    if (CompareResToTIL(resFileaffectedDataCat, tilFileaffectedDataCat) == 1) 
                            //                    {
                            //                        singularchangeOfAcc[1]++;
                            //                        break;
                            //                    }
                            //                }
                            //            }
                            //            singularchangeOfAcc[1] = (singularchangeOfAcc[1] / tilFileChange.affectedDataCategories.Count());

                            //            singularchangeOfAcc[2] = CompareResToTIL(resFileChange.plannedDateOfChange, tilFileChange.plannedDateOfChange);
                            //            singularchangeOfAcc[3] = CompareResToTIL(resFileChange.urlOfNewVersion, tilFileChange.urlOfNewVersion);

                            //            if (singularchangeOfAcc.Sum() >= 1)
                            //            {
                            //                changeCount++;
                            //                changesOfAcc += (singularchangeOfAcc.Sum() / 4);
                            //            }
                            //        }
                            //    }
                            //    changesOfAcc = changesOfAcc / changeCount;

                            //    Console.WriteLine("changesOfPurposeAccuracy: " + changesOfAcc);
                            //    if (float.IsNaN(changesOfAcc))
                            //    {
                            //        changesOfAcc = 0;
                            //    }

                            #endregion



                            //Controller
                            contrAccNew.Add(CompareResToTIL(resFile.controller.name, tilFile.controller.name));
                            contrAccNew.Add(CompareResToTIL(resFile.controller.division, tilFile.controller.division));
                            contrAccNew.Add(CompareResToTIL(resFile.controller.address, tilFile.controller.address));
                            contrAccNew.Add(CompareResToTIL(resFile.controller.country, tilFile.controller.country));

                            //Controller.representative
                            reprAccNew.Add(CompareResToTIL(resFile.controller.representative.name, tilFile.controller.representative.name));
                            reprAccNew.Add(CompareResToTIL(resFile.controller.representative.email, tilFile.controller.representative.email));
                            reprAccNew.Add(CompareResToTIL(resFile.controller.representative.phone, tilFile.controller.representative.phone));

                            contrAccNew.AddRange(reprAccNew);

                            //DataProtectionOfficer
                            dataProtAccNew.Add(CompareResToTIL(resFile.dataProtectionOfficer.name, tilFile.dataProtectionOfficer.name));
                            dataProtAccNew.Add(CompareResToTIL(resFile.dataProtectionOfficer.address, tilFile.dataProtectionOfficer.address));
                            dataProtAccNew.Add(CompareResToTIL(resFile.dataProtectionOfficer.country, tilFile.dataProtectionOfficer.country));
                            dataProtAccNew.Add(CompareResToTIL(resFile.dataProtectionOfficer.email, tilFile.dataProtectionOfficer.email));
                            dataProtAccNew.Add(CompareResToTIL(resFile.dataProtectionOfficer.phone, tilFile.dataProtectionOfficer.phone));

                            //AccessAndDataPortability

                            accessAndAccNew.Add(CompareResToTIL(resFile.accessAndDataPortability.available.ToString(), tilFile.accessAndDataPortability.available.ToString()));
                            accessAndAccNew.Add(CompareResToTIL(resFile.accessAndDataPortability.description, tilFile.accessAndDataPortability.description));
                            accessAndAccNew.Add(CompareResToTIL(resFile.accessAndDataPortability.url, tilFile.accessAndDataPortability.url));
                            accessAndAccNew.Add(CompareResToTIL(resFile.accessAndDataPortability.email, tilFile.accessAndDataPortability.email));


                            //rightToInformation
                            rightToInfoAccNew.Add(CompareResToTIL(resFile.rightToInformation.available.ToString(), tilFile.rightToInformation.available.ToString()));
                            rightToInfoAccNew.Add(CompareResToTIL(resFile.rightToInformation.description, tilFile.rightToInformation.description));
                            rightToInfoAccNew.Add(CompareResToTIL(resFile.rightToInformation.url, tilFile.rightToInformation.url));
                            rightToInfoAccNew.Add(CompareResToTIL(resFile.rightToInformation.email, tilFile.rightToInformation.email));

                            //rightToRectificationOrDeletion
                            rightToRectiAccNew.Add(CompareResToTIL(resFile.rightToRectificationOrDeletion.available.ToString(), tilFile.rightToRectificationOrDeletion.available.ToString()));
                            rightToRectiAccNew.Add(CompareResToTIL(resFile.rightToRectificationOrDeletion.description, tilFile.rightToRectificationOrDeletion.description));
                            rightToRectiAccNew.Add(CompareResToTIL(resFile.rightToRectificationOrDeletion.url, tilFile.rightToRectificationOrDeletion.url));
                            rightToRectiAccNew.Add(CompareResToTIL(resFile.rightToRectificationOrDeletion.email, tilFile.rightToRectificationOrDeletion.email));

                            //rightToDataPortability
                            rightToDataPortAccNew.Add(CompareResToTIL(resFile.rightToDataPortability.available.ToString(), tilFile.rightToDataPortability.available.ToString()));
                            rightToDataPortAccNew.Add(CompareResToTIL(resFile.rightToDataPortability.description, tilFile.rightToDataPortability.description));
                            rightToDataPortAccNew.Add(CompareResToTIL(resFile.rightToDataPortability.url, tilFile.rightToDataPortability.url));
                            rightToDataPortAccNew.Add(CompareResToTIL(resFile.rightToDataPortability.email, tilFile.rightToDataPortability.email));

                            //rightToWithdrawConsent
                            rightToWithdrawAccNew.Add(CompareResToTIL(resFile.rightToWithdrawConsent.available.ToString(), tilFile.rightToWithdrawConsent.available.ToString()));
                            rightToWithdrawAccNew.Add(CompareResToTIL(resFile.rightToWithdrawConsent.description, tilFile.rightToWithdrawConsent.description));
                            rightToWithdrawAccNew.Add(CompareResToTIL(resFile.rightToWithdrawConsent.url, tilFile.rightToWithdrawConsent.url));
                            rightToWithdrawAccNew.Add(CompareResToTIL(resFile.rightToWithdrawConsent.email, tilFile.rightToWithdrawConsent.email));

                            //rightToComplain
                            rightToComplainAccNew.Add(CompareResToTIL(resFile.rightToComplain.available.ToString(), tilFile.rightToComplain.available.ToString()));
                            rightToComplainAccNew.Add(CompareResToTIL(resFile.rightToComplain.description, tilFile.rightToComplain.description));
                            rightToComplainAccNew.Add(CompareResToTIL(resFile.rightToComplain.url, tilFile.rightToComplain.url));
                            rightToComplainAccNew.Add(CompareResToTIL(resFile.rightToComplain.email, tilFile.rightToComplain.email));
                            //rightToComplain.supervisoryAuthority
                            supervisoryAccNew.Add(CompareResToTIL(resFile.rightToComplain.supervisoryAuthority.name, tilFile.rightToComplain.supervisoryAuthority.name));
                            supervisoryAccNew.Add(CompareResToTIL(resFile.rightToComplain.supervisoryAuthority.address, tilFile.rightToComplain.supervisoryAuthority.address));
                            supervisoryAccNew.Add(CompareResToTIL(resFile.rightToComplain.supervisoryAuthority.country, tilFile.rightToComplain.supervisoryAuthority.country));
                            supervisoryAccNew.Add(CompareResToTIL(resFile.rightToComplain.supervisoryAuthority.email, tilFile.rightToComplain.supervisoryAuthority.email));
                            supervisoryAccNew.Add(CompareResToTIL(resFile.rightToComplain.supervisoryAuthority.phone, tilFile.rightToComplain.supervisoryAuthority.phone));

                            rightToComplainAccNew.AddRange(supervisoryAccNew);

                            //AutomatedDecisionMaking
                            automatedDecAccNew.Add(CompareResToTIL(resFile.automatedDecisionMaking.inUse.ToString(), tilFile.automatedDecisionMaking.inUse.ToString()));
                            automatedDecAccNew.Add(CompareResToTIL(resFile.automatedDecisionMaking.logicInvolved, tilFile.automatedDecisionMaking.logicInvolved));
                            automatedDecAccNew.Add(CompareResToTIL(resFile.automatedDecisionMaking.scopeAndIntendedEffects, tilFile.automatedDecisionMaking.scopeAndIntendedEffects));

                            //ChangesOfPurpose
                            foreach (var tilFileChange in tilFile.changesOfPurpose)
                            {
                                int maxAccuracy = 0;
                                List<float> singularChangeNew = new List<float>();
                                foreach (var resFileChange in resFile.changesOfPurpose)
                                {
                                    List<float> resFileResults = new List<float>();
                                    int currAccuracy = 0;
                                    resFileResults.Add(CompareResToTIL(resFileChange.description, tilFileChange.description));
                                    resFileResults.Add(CompareResToTIL(resFileChange.plannedDateOfChange, tilFileChange.plannedDateOfChange));
                                    resFileResults.Add(CompareResToTIL(resFileChange.urlOfNewVersion, tilFileChange.urlOfNewVersion));
                                    foreach (var gg in resFileResults)
                                    {
                                        if (gg == 1)
                                        {
                                            currAccuracy++;
                                        }
                                    }
                                    if (currAccuracy > maxAccuracy)
                                    {
                                        singularChangeNew.Clear();
                                        foreach (var zi in resFileResults)
                                        {
                                            singularChangeNew.Add(zi);
                                        }
                                    }
                                    else if (maxAccuracy == 0)
                                    {
                                        singularChangeNew.Clear();
                                        foreach (var zi in resFileResults)
                                        {
                                            singularChangeNew.Add(zi);
                                        }
                                    }
                                }
                                foreach (var acc in singularChangeNew)
                                {
                                    changeOfAccTILRecall.Add(acc);
                                }
                            }


                            foreach (var resFileChange in resFile.changesOfPurpose)
                            {
                                List<float> singularChangeNew = new List<float>();
                                int maxAccuracy = 0;
                                foreach (var tilFileChange in resFile.changesOfPurpose)
                                {
                                    List<float> resFileResults = new List<float>();
                                    int currAccuracy = 0;

                                    resFileResults.Add(CompareResToTIL(resFileChange.description, tilFileChange.description));
                                    resFileResults.Add(CompareResToTIL(resFileChange.plannedDateOfChange, tilFileChange.plannedDateOfChange));
                                    resFileResults.Add(CompareResToTIL(resFileChange.urlOfNewVersion, tilFileChange.urlOfNewVersion));

                                    foreach (var gg in resFileResults)
                                    {
                                        if (gg == 1)
                                        {
                                            currAccuracy++;
                                        }
                                    }
                                    if (currAccuracy > maxAccuracy)
                                    {
                                        singularChangeNew.Clear();
                                        foreach (var zi in resFileResults)
                                        {
                                            singularChangeNew.Add(zi);
                                        }
                                    }
                                    else if (maxAccuracy == 0)
                                    {
                                        singularChangeNew.Clear();
                                        foreach (var zi in resFileResults)
                                        {
                                            singularChangeNew.Add(zi);
                                        }
                                    }
                                }
                                foreach (var acc in singularChangeNew)
                                {
                                    changeOfAccRESPrecision.Add(acc);
                                }
                            }


                            foreach (var num in changeOfAccTILRecall)
                            {
                                if (num == 1 || num == -2)
                                {
                                    changeOfAccNewTotal.Add(num);
                                }
                            }
                            foreach (var num in changeOfAccRESPrecision)
                            {
                                if (num == -1)
                                {
                                    changeOfAccNewTotal.Add(num);
                                }
                            }





                            //overall
                            overallAccNew.AddRange(contrAccNew);
                            overallAccNew.AddRange(reprAccNew);
                            overallAccNew.AddRange(dataProtAccNew);
                            //overallAccNew.AddRange(accessAndAccNew);
                            overallAccNew.AddRange(rightToInfoAccNew);
                            overallAccNew.AddRange(rightToRectiAccNew);
                            overallAccNew.AddRange(rightToDataPortAccNew);
                            overallAccNew.AddRange(rightToWithdrawAccNew);
                            overallAccNew.AddRange(rightToComplainAccNew);
                            overallAccNew.AddRange(supervisoryAccNew);
                            overallAccNew.AddRange(automatedDecAccNew);
                            overallAccNew.AddRange(changeOfAccNewTotal);




                            //overallAcc = ((contrAccNew.Sum() / 5) + (dataProtAcc.Sum() / 5)
                            //    + (accessAndAcc.Sum() / 4) + (rightToInfoAcc.Sum() / 4) + (rightToRectiAcc.Sum() / 4)
                            //    + (rightToDataPortAcc.Sum() / 4) + (rightToWithdrawAcc.Sum() / 4) + (rightToComplainAcc.Sum() / 5)
                            //    + (automatedDecAcc.Sum() / 3) + changesOfAcc) / 10;

                            //Console.WriteLine("+++ overallAccuracy: " + overallAcc + " +++");





                        }
                        catch (Exception ex)
                        {   
                            Console.WriteLine("Error while loading or comparing TIL Jsons: "+ex);
                            continue;
                        }
                        if (!Directory.Exists(baseDirectory+"\\Calculations_Accuracy"))
                        {
                            Directory.CreateDirectory(baseDirectory + "\\Calculations_Accuracy");
                        }
                        string calcFileName = baseDirectory + "\\Calculations_Accuracy\\" + Path.GetFileNameWithoutExtension(resultFile) + "#Calc.txt";
                        if (File.Exists(calcFileName))
                        {
                            File.Delete(calcFileName);
                        }
                        //string finalString = contrAcc.Sum() / 5 + "\t" + dataProtAcc.Sum() / 5 + "\t" + accessAndAcc.Sum() / 4
                        //     + "\t" + rightToInfoAcc.Sum() / 4 + "\t" + rightToRectiAcc.Sum() / 4 + "\t" + rightToDataPortAcc.Sum() / 4
                        //     + "\t" + rightToWithdrawAcc.Sum() / 4 + "\t" + rightToComplainAcc.Sum() / 5 + "\t" + automatedDecAcc.Sum() / 3
                        //     + "\t" + changesOfAcc + "\t" + overallAcc;
                        string finalString = "" + CalculateF1Score(contrAccNew) + "\t" + CalculateF1Score(dataProtAccNew) + "\t" + /*CalculateF1Score(accessAndAccNew) + "\t" +*/
                            CalculateF1Score(rightToInfoAccNew) + "\t" + CalculateF1Score(rightToRectiAccNew) + "\t" + CalculateF1Score(rightToDataPortAccNew) + "\t" +
                            CalculateF1Score(rightToWithdrawAccNew) + "\t" + CalculateF1Score(rightToComplainAccNew) + "\t" + CalculateF1Score(automatedDecAccNew) + "\t" +
                            CalculateF1Score(changeOfAccNewTotal) + "\t" + CalculateF1Score(overallAccNew);
                        File.WriteAllText(calcFileName, finalString);
                        allF1ScoresString += (Path.GetFileNameWithoutExtension(resultFile)) + "\t" + finalString + "\r\n";
                        allAccuraciesString += (Path.GetFileNameWithoutExtension(resultFile)) + "\t" + CalculateAccuracyScore(contrAccNew) + "\t" + CalculateAccuracyScore(dataProtAccNew) + "\t" + /*CalculateF1Score(accessAndAccNew) + "\t" +*/
                            CalculateAccuracyScore(rightToInfoAccNew) + "\t" + CalculateAccuracyScore(rightToRectiAccNew) + "\t" + CalculateAccuracyScore(rightToDataPortAccNew) + "\t" +
                            CalculateAccuracyScore(rightToWithdrawAccNew) + "\t" + CalculateAccuracyScore(rightToComplainAccNew) + "\t" + CalculateAccuracyScore(automatedDecAccNew) + "\t" +
                            CalculateAccuracyScore(changeOfAccNewTotal) + "\t" + CalculateAccuracyScore(overallAccNew) + "\r\n";

                    } //if
                } //foreach
                if (!Directory.Exists(baseDirectory + "\\Calculations_Accuracy"))
                {   Directory.CreateDirectory(baseDirectory + "\\Calculations_Accuracy");   }

                string allAccFileName = baseDirectory + "\\Calculations_Accuracy\\" + "allAccuracies#Calc.txt";
                if (File.Exists(allAccFileName))
                {   File.Delete(allAccFileName);    }
                allF1ScoresString += "\r\n" +  allAccuraciesString;
                File.WriteAllText(allAccFileName, allF1ScoresString);

                foreach (var responseFile in Directory.GetFiles(entityResponseDirectory))   //Check recognized entities compared to TIL file
                {
                    string fileWithoutExt = Path.GetFileNameWithoutExtension(responseFile);
                    string[] fileParts = fileWithoutExt.Split("#");
                    string tilFilePath = tilDirectory + "\\" + fileParts[0] + ".json";

                    if (Directory.GetFiles(tilDirectory).Contains(tilFilePath)) //Vergleiche Entitäten mit manuell erstellten TILs
                    {
                        try
                        {

                            if (!Directory.Exists(baseDirectory + "\\Calculations_new"))
                            {
                                Directory.CreateDirectory(baseDirectory + "\\Calculations_new");
                            }

                            calcSaveFilePath = baseDirectory + "\\Calculations_new\\" + Path.GetFileNameWithoutExtension(responseFile) + "#Calc.txt";
                            

                            Console.WriteLine("\nMatching files found for " + fileWithoutExt + ". Beginning entity analysis.");
                            UniEntityResponse resUniEntity = JsonConvert.DeserializeObject<UniEntityResponse>(File.ReadAllText(responseFile));
                            calcEntity = null;
                            calcEntity = JsonConvert.DeserializeObject<UniEntityResponse>(File.ReadAllText(responseFile));
                            TIL manTIL = JsonConvert.DeserializeObject<TIL>(File.ReadAllText(tilFilePath).Replace("null", "false"));

                            Dictionary<string, float> usefulEntities = new Dictionary<string, float>();

                            Dictionary<string, float> ulE = new Dictionary<string, float>(); //uselessEntities

                            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(tilFilePath).Replace("null", "false"));
                            calcNewAllFilesString += Path.GetFileNameWithoutExtension(responseFile) + "\tUseful\t";
                            calcNewAlleAbdeckung += Path.GetFileNameWithoutExtension(responseFile) + "\t" + Path.GetFileNameWithoutExtension(responseFile) + "\r\n";
                            List<float> iterateResults = IterateCalc(jsonObj, resUniEntity, 0);

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            continue;
                        }
                    }
                }
                if (!Directory.Exists(baseDirectory + "\\Calculations_new"))
                { Directory.CreateDirectory(baseDirectory + "\\Calculations_new"); }

                string allUsabilitiesFileName = baseDirectory + "\\Calculations_new\\" + "allUsabilities#Calc.txt";
                if (File.Exists(allUsabilitiesFileName))
                { File.Delete(allUsabilitiesFileName); }

                string allAbdeckungFileName = baseDirectory + "\\Calculations_new\\" + "alleAbdeckungen#Calc.txt";
                if (File.Exists(allAbdeckungFileName))
                { File.Delete(allAbdeckungFileName); }

                File.WriteAllText(allUsabilitiesFileName, calcNewAllFilesString);
                File.WriteAllText(allAbdeckungFileName, calcNewAlleAbdeckung);

            } //else
            Console.WriteLine("Calculation terminated.");
            return;
        } //CalculateAccuracy
        public static float CalculateAccuracyScore(List<float> floatList)
        {
            int countTP = 0;    //true positive, 1
            int countFP = 0;    //false positive, -1
            int countTN = 0;    //true negative, 0
            int countFN = 0;    //false negative, -2

            foreach (var item in floatList)
            {
                if (item == 0)
                { countTN++; }
                else if (item == 1)
                { countTP++; }
                else if (item == -1)
                { countFP++; }
                else if (item == -2)
                { countFN++; }
            }
            float insgCount = floatList.Count();
            if (insgCount == 0)
            {
                insgCount = 1;
            }
            return (countTP + countTN) / (insgCount);
        }
        public static float CalculateF1Score(List<float> floatList)
        {
            int countTP = 0;    //true positive, 1
            int countFP = 0;    //false positive, -1
            int countTN = 0;    //true negative, 0
            int countFN = 0;    //false negative, -2

            foreach (var item in floatList)
            {
                if (item == 0)
                {   countTN++;  }
                else if (item == 1)
                {   countTP++;  }
                else if (item == -1)
                {   countFP++;  }
                else if (item == -2)
                {   countFN++;  }
            }

            return (float)(countTP / (countTP + 0.5 * (countFP + countFN)));
        }
        public static float CompareResToTIL(string res, string til)
        {
            if ((String.IsNullOrEmpty(RWS(res)) && String.IsNullOrEmpty(RWS(til))) || (String.Compare(til, "false") == 0 && String.Compare(res, "false") == 0))
            {
                return 0;  //true negative
            }
            else if ((!String.IsNullOrEmpty(RWS(til)) && RWS(res).Contains(RWS(til)))
                || (!String.IsNullOrEmpty(RWS(res)) && (RWS(til).Contains(RWS(res)))))
            {
                return 1;   //true positive
            }
            else if (String.IsNullOrEmpty(til) || String.Compare(til, "false") == 0 || (!String.IsNullOrEmpty(til) && !String.IsNullOrEmpty(res)))
            {
                return -1; //false positive
            }
            else if (!String.IsNullOrEmpty(til) && String.IsNullOrEmpty(res))
            {
                return -2;  //false negative
            }
            else
            {
                return -1;
            }
        }
        public static string RWS(string input)
        {
            return input.Replace("\n", "").Replace("\r", "").Replace("\t", "").Trim();
        }
        public static List<float> IterateCalc(dynamic variable, UniEntityResponse entObj, int depth = 0)
        {
            string AbdeckungString = "";
            List<float> returnVal = new List<float>();  //attention: returnVal[0] is usefulness where 0 ^= useful. //old but ingrained into code
                                                        //rest is "accuracies / Abdeckung" of objects in variable.  //aktuell
            //returnVal.Add(1);

            if (variable.GetType() == typeof(Newtonsoft.Json.Linq.JObject))
            {
                foreach (var property in variable)
                {
                    //Console.WriteLine("property name: " + property.Name.ToString());
                    //Console.WriteLine("property type: " + property.GetType().ToString());
                    List<float> tempErg = IterateCalc(property.Value, entObj, depth+1);
                    if (tempErg.Count > 0)
                    {
                        float abd = tempErg.Sum() / (tempErg.Count);
                        foreach (var erg in tempErg)
                        {
                            returnVal.Add(erg); //new
                        }
                        //returnVal.Add(abd); //old
                        AbdeckungString += (property.Name.ToString() + "\t" + abd + "\n");
                    }
                    else
                    {
                        AbdeckungString += (property.Name.ToString() + "\t\n");
                    }
                }
            }

            else if (variable.GetType() == typeof(Newtonsoft.Json.Linq.JArray))
            {
                //Console.WriteLine("type is Array");
                foreach (var item in variable)
                {
                    List<float> tempErg = IterateCalc(item, entObj, depth+1);
                    if (tempErg.Count > 0)
                    {
                        //returnVal.Add(tempErg.Sum() / (tempErg.Count));  //old
                        foreach (var erg in tempErg)
                        {
                            returnVal.Add(erg);
                        }
                    }
                }
            }

            else if (variable.GetType() == typeof(Newtonsoft.Json.Linq.JValue))
            {
                string varSmall = variable.ToString().ToLower();
                //Console.WriteLine("type is Variable, value: " + variable.ToString());
                if (String.Compare(varSmall, "false") == 0 || String.Compare(varSmall, "true")==0 || String.Compare(varSmall, "") == 0 || String.Compare(varSmall, "N/A") == 0)
                {
                    return returnVal;
                }
                bool abgedeckt = false;
                foreach (var ent in calcEntity.entities)
                {
                    string entityText = ent.text;
                    if (entityText.Length < 3)
                    { continue; }

                    if (variable.ToString().Contains(entityText))
                    {
                        ent.usefulness = true;    //means useful
                        returnVal.Add(1);
                        abgedeckt = true;
                        //Console.WriteLine("Found match");
                    }
                }
                if (!abgedeckt)
                {
                    returnVal.Add(0);   
                }
            }

            if (depth == 0)
            {
                //Console.WriteLine(AbdeckungString);
                AbdeckungString += "Overall:\t" + (returnVal.Sum() / returnVal.Count) + "\n";

                Dictionary<string, float> usefulEntities = new Dictionary<string, float>();
                usefulEntities.Add("ORGANIZATION", 0);
                usefulEntities.Add("COMMERCIALITEM", 0);
                usefulEntities.Add("DATE", 0);
                usefulEntities.Add("EVENT", 0);
                usefulEntities.Add("LOCATION", 0);
                usefulEntities.Add("OTHER", 0);
                usefulEntities.Add("PERSON", 0);
                usefulEntities.Add("QUANTITY", 0);
                usefulEntities.Add("TITLE", 0);
                usefulEntities.Add("PERSONTYPE", 0);
                usefulEntities.Add("PRODUCT", 0);
                usefulEntities.Add("SKILL", 0);
                usefulEntities.Add("ADDRESS", 0);
                usefulEntities.Add("PHONENUMBER", 0);
                usefulEntities.Add("EMAILADDRESS", 0);
                usefulEntities.Add("URL", 0);
                usefulEntities.Add("IPADDRESS", 0);
                usefulEntities.Add("UNKNOWN", 0);
                usefulEntities.Add("WORKOFART", 0);
                usefulEntities.Add("CONSUMERGOOD", 0);
                usefulEntities.Add("NUMBER", 0);
                usefulEntities.Add("PRICE", 0);
                usefulEntities.Add("COMPANY", 0);
                usefulEntities.Add("DURATION", 0);
                usefulEntities.Add("FACILITY", 0);
                usefulEntities.Add("GEOGRAPHICFEATURE", 0);
                usefulEntities.Add("HASHTAG", 0);
                usefulEntities.Add("JOBTITLE", 0);
                usefulEntities.Add("MEASURE", 0);
                usefulEntities.Add("MONEY", 0);
                usefulEntities.Add("ORDINAL", 0);
                usefulEntities.Add("PERCENT", 0);
                usefulEntities.Add("TIME", 0);
                usefulEntities.Add("TWITTERHANDLE", 0);

                

                Dictionary<string, float> ulE = new Dictionary<string, float>(); //uselessEntities
                ulE.Add("ORGANIZATION", 0);
                ulE.Add("COMMERCIALITEM", 0);
                ulE.Add("DATE", 0);
                ulE.Add("EVENT", 0);
                ulE.Add("LOCATION", 0);
                ulE.Add("OTHER", 0);
                ulE.Add("PERSON", 0);
                ulE.Add("QUANTITY", 0);
                ulE.Add("TITLE", 0);
                ulE.Add("PERSONTYPE", 0);
                ulE.Add("PRODUCT", 0);
                ulE.Add("SKILL", 0);
                ulE.Add("ADDRESS", 0);
                ulE.Add("PHONENUMBER", 0);
                ulE.Add("EMAILADDRESS", 0);
                ulE.Add("URL", 0);
                ulE.Add("IPADDRESS", 0);
                ulE.Add("UNKNOWN", 0);
                ulE.Add("WORKOFART", 0);
                ulE.Add("CONSUMERGOOD", 0);
                ulE.Add("NUMBER", 0);
                ulE.Add("PRICE", 0);
                ulE.Add("COMPANY", 0);
                ulE.Add("DURATION", 0);
                ulE.Add("FACILITY", 0);
                ulE.Add("GEOGRAPHICFEATURE", 0);
                ulE.Add("HASHTAG", 0);
                ulE.Add("JOBTITLE", 0);
                ulE.Add("MEASURE", 0);
                ulE.Add("MONEY", 0);
                ulE.Add("ORDINAL", 0);
                ulE.Add("PERCENT", 0);
                ulE.Add("TIME", 0);
                ulE.Add("TWITTERHANDLE", 0);

                foreach (var ent in calcEntity.entities)
                {
                    if (String.Compare(ent.type, "IPADDRESSADDRESS")==0)
                    {
                        ent.type = "IPADDRESS";
                    }
                    if (ent.usefulness)
                    {
                        if (usefulEntities.ContainsKey(ent.type))
                        {
                            usefulEntities[ent.type]++;
                        }
                    }
                    else
                    {
                        if (usefulEntities.ContainsKey(ent.type))
                        {
                            ulE[ent.type]++;
                        }
                    }
                }
                string resultstring = "";
                foreach (var entType in usefulEntities.Keys)
                {
                    //Console.WriteLine("to:" + entType);
                    float usefulness = usefulEntities[entType];
                    float uselessness = ulE[entType];
                    float Quotient = usefulness / (usefulness + uselessness);
                    if (float.IsNaN(Quotient))
                    { Quotient = 0; }
                    //resultstring += "Type: \t" + entType + "\t useful: \t" + usefulness + " \ttimes. Useless: \t" + uselessness + "\t times. Quotient: \t" + Quotient + "\n";
                    resultstring +=  usefulness + "\t" + uselessness + "\n";
                }
                foreach (var usefulType in usefulEntities.Keys)
                {
                    calcNewAllFilesString += usefulEntities[usefulType] + "\t";
                }
                calcNewAllFilesString += "\r\n\"\"\tUseless\t";
                foreach (var usefulType in ulE.Keys)
                {
                    calcNewAllFilesString += ulE[usefulType] + "\t";
                }
                calcNewAllFilesString += "\r\n";

                //Console.WriteLine(resultstring);
                resultstring += "\n" + AbdeckungString;
                calcNewAlleAbdeckung += AbdeckungString + "\r\n";
                File.WriteAllText(calcSaveFilePath, resultstring);
            }
            return returnVal;
        }
        public static int LoadCredentials()
        {
            if (File.Exists("nlpConfig.config"))
            {
                try
                {
                    var fileName = "nlpConfig.config";
                    using var sr = new StreamReader(fileName);
                    string[] lines = File.ReadAllLines(fileName);
                    Console.WriteLine("Config File Found");
                    ibm_api_key = lines[0];
                    ibm_service_url = lines[1];
                    google_api_path = lines[2];
                    try
                    {
                        azureCredentials = new AzureKeyCredential(lines[3]);
                        azureEndpoint = new Uri(lines[4]);
                    }
                    catch (Exception exa)
                    {
                        Console.WriteLine("Error while loading Microsoft Azure credentials: " + exa);
                    }
                    microsoft_standort = lines[5];
                    return 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error while loading credentials: " + ex);
                    Console.WriteLine("Continue anyways? Y/N");
                    string akl = Console.ReadLine();
                    if (String.Compare(akl, "Y")==0)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }
            }
            Console.WriteLine("Config file not Found, please put a file by the name of nlpConfig.config in the same folder as the program.");
            Console.WriteLine("Continue anyways? Y/N");
            string ak = Console.ReadLine();
            if (String.Compare(ak, "Y") == 0)
            {
                return 0;
            }
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
        private static UniEntityResponse MicrosoftCompleteEntityRecognition(string input)
        {
            string[] datenSplit = SplitDatenschutz(input);

            UniEntityResponse[] entResultEntities = new UniEntityResponse[datenSplit.Length];
            for (int i = 0; i < datenSplit.Length; i++)
            {
                entResultEntities[i] = ConvertMicrosoftToUniEntity(MicrosoftEntityRecognize(datenSplit[i]));
                //Console.WriteLine(entResults[i]);
            }
            Console.WriteLine("Microsoft rec. done, combining results...");
            UniEntityResponse microsoftResEnt = CombineUniEntitySplitParts(entResultEntities);
            Console.WriteLine("Microsoft results successfully combined.");

            return microsoftResEnt;
        }
        private static UniEntityResponse ConvertMicrosoftToUniEntity(CategorizedEntityCollection microsoftResponse)
        {
            UniEntityResponse result = new UniEntityResponse();
            result.entities = new List<UniEntityResponse.entity>();
            result.language = "";
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
                            //Combining existing entities alias adding mentions.

                            UniEntityResponse.mention newMention = new UniEntityResponse.mention();
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
                    UniEntityResponse.entity resEnt = new UniEntityResponse.entity();
                    resEnt.type = microsoftEntity.Category.ToString().ToUpper().Replace(" ", "").Replace("EMAIL", "EMAILADDRESS").Replace("DATETIME", "DATE");
                    if (microsoftEntity.SubCategory != null && String.Compare(microsoftEntity.SubCategory, "") != 0)
                    {      resEnt.subcategory =microsoftEntity.SubCategory.ToUpper();     }
                    else { resEnt.subcategory = ""; }

                    resEnt.text = microsoftEntity.Text;
                    resEnt.sentiment = new UniEntityResponse.sentim();
                    resEnt.confidence = (float)microsoftEntity.ConfidenceScore;
                    //if (microsoftEntity. != null)
                    //{
                    //    resEnt.sentiment.score = microsoftEntity.Sentiment.Score;
                    //    resEnt.sentiment.label = microsoftEntity.Sentiment.Magnitude.ToString();    //ACHTUNG nongleich
                    //}
                    //resEnt.relevance = microsoftEntity.Salience;  //hat nicht viel ausgesagt und Probleme verursacht

                    resEnt.mentions = new List<UniEntityResponse.mention>();

                    UniEntityResponse.mention newMention = new UniEntityResponse.mention();

                    newMention.confidence = (float)microsoftEntity.ConfidenceScore;
                    newMention.text = microsoftEntity.Text;
                    newMention.location = new List<int>();
                    newMention.location.Add(microsoftEntity.Offset);
                    newMention.location.Add(microsoftEntity.Offset + microsoftEntity.Length);
                    resEnt.mentions.Add(newMention);

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
                Console.WriteLine($"\tType: {entity.Type.ToString().ToUpper()}");
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
        private static UniEntityResponse ConvertGoogleToUniEntity(AnalyzeEntitiesResponse googleResponse)
        {
            UniEntityResponse result = new UniEntityResponse();
            result.entities = new List<UniEntityResponse.entity>();
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
                                UniEntityResponse.mention newMention = new UniEntityResponse.mention();
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
                    UniEntityResponse.entity resEnt = new UniEntityResponse.entity();
                    resEnt.type = googleEntity.Type.ToString().ToUpper().Replace("_", "");
                    resEnt.text = googleEntity.Name;
                    resEnt.sentiment = new UniEntityResponse.sentim();
                    if (googleEntity.Sentiment != null)
                    {
                        resEnt.sentiment.score = googleEntity.Sentiment.Score;
                        resEnt.sentiment.label = googleEntity.Sentiment.Magnitude.ToString();    //ACHTUNG nongleich
                    }
                    resEnt.relevance = googleEntity.Salience;
                    resEnt.mentions = new List<UniEntityResponse.mention>();
                    foreach (var responseMention in googleEntity.Mentions)
                    {
                        UniEntityResponse.mention newMention = new UniEntityResponse.mention();
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
        private static UniEntityResponse GoogleCompleteEntityRecognition(string input)
        {
            string[] datenSplit = SplitDatenschutz(input);
            UniEntityResponse[] entResultEntities = new UniEntityResponse[datenSplit.Length];
            for (int i = 0; i < datenSplit.Length; i++)
            {
                entResultEntities[i] = ConvertGoogleToUniEntity(GoogleEntityRecognize(datenSplit[i]));
                //Console.WriteLine(entResults[i]);
            }
            Console.WriteLine("Google rec. done, combining results...");
            UniEntityResponse googleResEnt = CombineUniEntitySplitParts(entResultEntities);
            Console.WriteLine("Google results successfully combined.");

            return googleResEnt;
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
        private static UniEntityResponse IBMCompleteEntityRecognition(string input)
        {
            string[] datenSplit = SplitDatenschutz(input);
            UniEntityResponse[] entResultEntities = new UniEntityResponse[datenSplit.Length];
            for (int i = 0; i < datenSplit.Length; i++)
            {
                string response = IBMEntityRecognize(datenSplit[i]);
                //Console.WriteLine(response);
                entResultEntities[i] = ConvertIBMJson(response);
            }
            //TODO umformen in unientities
            Console.WriteLine("ibm rec. done, combining results...");
            UniEntityResponse ibmResEnt = CombineUniEntitySplitParts(entResultEntities);
            Console.WriteLine("ibm results successfully combined.");

            foreach (UniEntityResponse.entity entity in ibmResEnt.entities) //CAPITALIZE the entity types
            {   entity.type = entity.type.ToUpper().Replace("_", "").Replace(" ","");    }

            return ibmResEnt;
        }
        private static UniEntityResponse ConvertIBMJson(string jsonString)
        {
            try
            {
                UniEntityResponse result = JsonConvert.DeserializeObject<UniEntityResponse>(jsonString);
                return result;
            }
            catch (Exception)
            {
                Console.WriteLine("Error parsing response JSON");
            }
            return null;
        }
        private static TIL AnalyseResponseUniEntity(UniEntityResponse result)
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
            UniEntityResponse.entity mostweOrgaEntity = new UniEntityResponse.entity();
            UniEntityResponse.entity mostMentionedOrgaEntity = new UniEntityResponse.entity();
            result.entities = result.entities.OrderBy(ent => ent.type).ToList();
            foreach (var ent1 in result.entities)
            {
                ent1.type = ent1.type.Replace(" ", "").Replace("_", "");
                foreach (var ent2 in result.entities)
                {
                    ent2.type = ent2.type.Replace(" ", "").Replace("_", "");
                    if (String.Compare(ent1.type, ent2.type) == 0 && (ent1.text.Contains(" " + ent2.text) || ent1.text.Contains(ent2.text + " ")) && ent1 != ent2) //last one is new at 25/09/2021
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
                if (String.Compare(entity.type, "ORGANIZATION") == 0 || String.Compare(entity.type, "COMPANY") == 0)
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

            string[] typesToCheck = { "LOCATION", "FACILITY", "COMPANY" };
            UniEntityResponse.mention firstClosestMention = FindClosestMentionOfType(result, "", GetEarliestMention(mostMentionedOrgaEntity).location[0], typesToCheck);
            UniEntityResponse.mention closestLocationMention = GetOverallClosestMentionOfType(result, "", mostMentionedOrgaEntity, "normal", typesToCheck);
            UniEntityResponse.mention overallClosestFollowingMention = GetOverallClosestMentionOfType(result, "", mostMentionedOrgaEntity, "after", typesToCheck);

            //Console.WriteLine("The earliest organization is: " + earliestOrga);
            Console.WriteLine("The most mentioned organization is: " + mostMentionedOrga);
            //Console.WriteLine("Earliest mention of that found at: " + GetEarliestMention(mostMentionedOrgaEntity).location[0]);
            //Console.WriteLine("\nThe organization/company with the most WeUsWirUns context mentions is: " + mostWeOrga);
            //Console.WriteLine("\nThe first closest Entity of types including \"" + typesToCheck[0] + "\" to that is: " + firstClosestMention.text);
            //if (!firstClosestMention.text.Contains("[No")){
            //    Console.WriteLine("The text around this entity is as follows:\n" + GetTextAroundHere(firstClosestMention.location[0], 40, 30));
            //}
            //Console.WriteLine("\nThe overall closest Entity of types including  \"" + typesToCheck[0] + "\" to that is: " + closestLocationMention.text);
            //Console.WriteLine("The text around this entity is as follows:\n" + GetTextAroundHere(closestLocationMention.location[0], 40, 30));
            //Console.WriteLine("\nOverall closest following Mention of type Location/Facility: " + overallClosestFollowingMention.text);
            //Console.WriteLine("The text around that is:\n" + GetTextAroundHere(overallClosestFollowingMention.location[0], 40, 30) + "\n");

            if (IsThisCloseTo("erantwortlich", closestLocationMention.location[0], 50)
                || IsThisCloseTo("epresent", closestLocationMention.location[0], 50)
                || IsThisCloseTo("orsitzend", closestLocationMention.location[0], 50))
            {
                UniEntityResponse.mention reprMailMention = FindClosestMentionOfType(result, "EMAILADDRESS", closestLocationMention.location[0]);
                Console.WriteLine("reprMailMention: " + reprMailMention.text);
                UniEntityResponse.mention reprNameMention = FindClosestMentionOfType(result, "PERSON", closestLocationMention.location[0]);
                Console.WriteLine("reprNameMention: " + reprNameMention.text);
                UniEntityResponse.mention reprPhoneMention = FindClosestMentionOfType(result, "PHONENUMBER", closestLocationMention.location[0]);
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

            UniEntityResponse.mention closestDatenschutzbeaufMention = GetOverallClosestMentionOfTypeToString(result, "LOCATION", deutschDatenschutz, dataProtMode, typesToCheck);
            int closestDatenschutzbeaufDistanz = HowCloseTo(closestDatenschutzbeaufMention.location[0], deutschDatenschutz, 500);

            UniEntityResponse.mention closestOfficerMention = GetOverallClosestMentionOfTypeToString(result, "LOCATION", "fficer", dataProtMode, typesToCheck);
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

            UniEntityResponse.mention closestDatenschutzbeaufPersonMention = GetOverallClosestMentionOfTypeToString(result, "PERSON", deutschDatenschutz, dataProtMode);
            int closestDatenschutzbeaufPersonDistanz = HowCloseTo(closestDatenschutzbeaufPersonMention.location[0], deutschDatenschutz, 250);

            UniEntityResponse.mention closestOfficerPersonMention = GetOverallClosestMentionOfTypeToString(result, "PERSON", "fficer", dataProtMode);
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

            UniEntityResponse.mention datProtEmailMentionDE = GetOverallClosestMentionOfTypeToString(result, "EMAILADDRESS", deutschDatenschutz, dataProtMode);
            int datProtEmailDistanzDE = HowCloseTo(datProtEmailMentionDE.location[0], deutschDatenschutz, 250);

            UniEntityResponse.mention datProtEmailMentionEN = GetOverallClosestMentionOfTypeToString(result, "EMAILADDRESS", "fficer", dataProtMode);
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

            UniEntityResponse.mention datProtPhoneMentionDE = GetOverallClosestMentionOfTypeToString(result, "PHONENUMBER", deutschDatenschutz, dataProtMode);
            int datProtPhoneDistanzDE = HowCloseTo(datProtPhoneMentionDE.location[0], deutschDatenschutz, 250);

            UniEntityResponse.mention datProtPhoneMentionEN = GetOverallClosestMentionOfTypeToString(result, "PHONENUMBER", "fficer", dataProtMode);
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
                //Should be if it is a type of information that is collected. Barely represented in the Entities.
                if (String.Compare(resultEntity.type, "Ordinal") == 0 && (resultEntity.text.Length > 15))   //ordinal not really accurate
                {
                    TIL.DataDisclosed newDataDisclosed = new TIL.DataDisclosed();
                    newDataDisclosed.category = resultEntity.text;



                    foreach (var innerEntity in result.entities)
                    {
                        //fill datadisclosed elem - fruitless if ordinal is not correct.
                    }

                    ibm_til.dataDisclosed.Add(newDataDisclosed);

                }
            }

            #endregion

        #region thirdCountryTransfers 

            #endregion

        #region sources 

            #endregion

        #region rightsTo 
            //TODO all rights need identificationEvidences
            //TODO maybe combined location, one right location has to also be close to another right location (or all)
            int closestDistance = int.MaxValue;
            int closestRightLocation = int.MaxValue;
            int rightRelevantLocation = int.MaxValue;
            string rightRelevantEN = "";
            string rightRelevantDE = "";
            string rightRelevantENControl = "";
            string rightRelevantDEControl = "";
            List<int> rightLocations = AllOccurrancesOfText("right");

            foreach (var item in AllOccurrancesOfText("Recht "))
            { rightLocations.Add(item); }
            foreach (var item in AllOccurrancesOfText("Recht, "))
            { rightLocations.Add(item); }
            foreach (var item in AllOccurrancesOfText("Rechte"))
            { rightLocations.Add(item); }
            foreach (var item in AllOccurrancesOfText("recht "))
            {   rightLocations.Add(item);   }
            foreach (var item in AllOccurrancesOfText("recht,"))
            {   rightLocations.Add(item);   }
            foreach (var item in AllOccurrancesOfText("rechte"))
            {   rightLocations.Add(item); }

            #region accessAndDataPortability 
            closestDistance = int.MaxValue;
            closestRightLocation = int.MaxValue;
            rightRelevantLocation = int.MaxValue;

            rightRelevantEN = "informat";
            rightRelevantDE = "Kopie";
            //available
            //ibm_til.accessAndDataPortability.available = 
            //description
            //ibm_til.accessAndDataPortability.description = 
            //url
            //ibm_til.accessAndDataPortability.url = 
            //email
            //ibm_til.accessAndDataPortability.email = 
            //identificationEvidences
            //ibm_til.accessAndDataPortability.identificationEvidences.Add(
            //administrativeFee
            //ibm_til.accessAndDataPortability.administrativeFee.amount = 
            //ibm_til.accessAndDataPortability.administrativeFee.currency = 
            //dataFormats
            //ibm_til.accessAndDataPortability.dataFormats = 
            #endregion

            #region rightToInformation 
            closestDistance = int.MaxValue;
            closestRightLocation = int.MaxValue;
            rightRelevantLocation = int.MaxValue;

            rightRelevantEN = "informat";
            rightRelevantDE = "Auskunft";

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
                ibm_til.rightToInformation.email = FindClosestMentionOfType(result, "EMAILADDRESS", rightRelevantLocation).text;
                ibm_til.rightToInformation.url = FindClosestMentionOfType(result, "URL", rightRelevantLocation).text;
            }
            #endregion

            #region rightToRectificationOrDeletion 
            closestDistance = int.MaxValue;
            closestRightLocation = int.MaxValue;
            rightRelevantLocation = int.MaxValue;

            rightRelevantEN = "delet";
            rightRelevantDE = "lösch";

            rightRelevantENControl = "complain";
            rightRelevantDEControl = "ehörde";

            int[] rightToRectificationResults = CheckForRights(rightLocations, rightRelevantEN, rightRelevantDE, rightRelevantENControl, rightRelevantDEControl, true);
            closestDistance = rightToRectificationResults[0];
            closestRightLocation = rightToRectificationResults[1];
            rightRelevantLocation = rightToRectificationResults[2];

            if (closestDistance != int.MaxValue)
            {
                ibm_til.rightToRectificationOrDeletion.available = true;
                ibm_til.rightToRectificationOrDeletion.description = GetTextAroundHere(rightRelevantLocation, 50, 80);
                ibm_til.rightToRectificationOrDeletion.email = FindClosestMentionOfType(result, "EMAILADDRESS", rightRelevantLocation).text;
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

            int[] rightToDataPortabilityResults = CheckForRights(rightLocations, rightRelevantEN, rightRelevantDE, rightRelevantENControl, rightRelevantDEControl, true);
            closestDistance = rightToDataPortabilityResults[0];
            closestRightLocation = rightToDataPortabilityResults[1];
            rightRelevantLocation = rightToDataPortabilityResults[2];

            if (closestDistance != int.MaxValue)
            {
                ibm_til.rightToDataPortability.available = true;
                ibm_til.rightToDataPortability.description = GetTextAroundHere(rightRelevantLocation, 50, 80);
                ibm_til.rightToDataPortability.email = FindClosestMentionOfType(result, "EMAILADDRESS", rightRelevantLocation).text;
                ibm_til.rightToDataPortability.url = FindClosestMentionOfType(result, "URL", rightRelevantLocation).text;
            }

            #endregion

            #region rightToWithdrawConsent 
            closestDistance = int.MaxValue;
            closestRightLocation = int.MaxValue;
            rightRelevantLocation = int.MaxValue;

            rightRelevantEN = "withdraw";
            rightRelevantDE = " wider";

            rightRelevantENControl = "delet";
            rightRelevantDEControl = "ösch";

            int[] rightToWithdrawConsentResults = CheckForRights(rightLocations, rightRelevantEN, rightRelevantDE, rightRelevantENControl, rightRelevantDEControl, true);
            closestDistance = rightToWithdrawConsentResults[0];
            closestRightLocation = rightToWithdrawConsentResults[1];
            rightRelevantLocation = rightToWithdrawConsentResults[2];

            if (closestDistance != int.MaxValue)
            {
                ibm_til.rightToWithdrawConsent.available = true;
                ibm_til.rightToWithdrawConsent.description = GetTextAroundHere(rightRelevantLocation, 50, 80);
                ibm_til.rightToWithdrawConsent.email = FindClosestMentionOfType(result, "EMAILADDRESS", rightRelevantLocation).text;
                ibm_til.rightToWithdrawConsent.url = FindClosestMentionOfType(result, "URL", rightRelevantLocation).text;
            }

            #endregion

            #region rightToComplain todo
            closestDistance = int.MaxValue;
            closestRightLocation = int.MaxValue;
            rightRelevantLocation = int.MaxValue;

            rightRelevantEN = "complain";
            rightRelevantDE = "ehörde";

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
                ibm_til.rightToComplain.email = FindClosestMentionOfType(result, "EMAILADDRESS", rightRelevantLocation).text;
                ibm_til.rightToComplain.url = FindClosestMentionOfType(result, "URL", rightRelevantLocation).text;
                string[] supervisoryAuthTypes = { "LOCATION", "FACILITY" };
                string[] ignoreMostMentioned = { mostMentionedOrgaEntity.text };
                try
                {
                    ibm_til.rightToComplain.supervisoryAuthority.address = GetTextAroundHere(FindClosestMentionOfType(result, "", rightRelevantLocation, supervisoryAuthTypes, "normal", ignoreMostMentioned).location[0], 45);
                    ibm_til.rightToComplain.supervisoryAuthority.country = GetTextAroundHere(FindClosestMentionOfType(result, "", rightRelevantLocation, supervisoryAuthTypes, "normal", ignoreMostMentioned).location[0], 45);
                    ibm_til.rightToComplain.supervisoryAuthority.name = FindClosestMentionOfType(result, "ORGANIZATION", rightRelevantLocation, null, "normal", ignoreMostMentioned).text;
                    ibm_til.rightToComplain.supervisoryAuthority.email = FindClosestMentionOfType(result, "EMAILADDRESS", rightRelevantLocation, null, "normal", ignoreMostMentioned).text;
                    ibm_til.rightToComplain.supervisoryAuthority.phone = FindClosestMentionOfType(result, "PHONENUMBER", rightRelevantLocation, null, "normal", ignoreMostMentioned).text;

                }
                catch (Exception)
                {
                    ibm_til.rightToComplain.supervisoryAuthority.name = "None found.";
                }
                //foreach( var evidence in collection)
                //{
                //    ibm_til.rightToComplain.identificationEvidences.Add();
                //}
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

                changeOfPurpose.plannedDateOfChange = FindClosestMentionOfType(result, "DATE", changeLocation).text;
                changeOfPurpose.urlOfNewVersion = FindClosestMentionOfType(result, "URL", changeLocation).text;

                ibm_til.changesOfPurpose.Add(changeOfPurpose);
            }
            #endregion

            //Organizations and their affiliated data
            string orgasOutput = "";
            foreach (var entityA in result.entities)
            {
                if (String.Compare(entityA.type, "ORGANIZATION") != 0)
                {   continue; }
                orgasOutput += "Organization found: " + entityA.text + "\n";
                orgasOutput += "Closest Location: " + GetOverallClosestMentionOfType(result, "LOCATION", entityA).text + "\n";
                orgasOutput += "Closest Mail: " + GetOverallClosestMentionOfType(result, "EMAILADDRESS", entityA).text + "\n";
                orgasOutput += "Closest URL: " + GetOverallClosestMentionOfType(result, "URL", entityA).text + "\n";
                orgasOutput += "Closest Phone Number: " + GetOverallClosestMentionOfType(result, "PHONENUMBER", entityA).text + "\n";
                orgasOutput += "Closest Ordinance: " + GetOverallClosestMentionOfType(result, "ORDINANCE", entityA).text + "\n";
                orgasOutput += "Closest Measure: " + GetOverallClosestMentionOfType(result, "MEASURE", entityA).text + "\n\n";
            }

            Console.WriteLine("\n##########################################################################################\n");
            Console.WriteLine(orgasOutput);
            Console.WriteLine("\n##########################################################################################\n");

            ibm_til.meta.language = result.language;
            ibm_til.controller.name = mostMentionedOrga;
            ibm_til.controller.address = GetTextAroundHere(closestLocationMention.location[0], 40, 30);
            if (!firstClosestMention.text.Contains("[No"))
            {
                UniEntityResponse.mention help = FindClosestMentionOfType(result, "FACILITY", firstClosestMention.location[0], typesToCheck);//TODO Watch out, IBM only..
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
                    e.Text, e.Type.ToString().ToUpper(), e.Score, e.BeginOffset, e.EndOffset);
            }
            foreach (var item in response.ResponseMetadata.Metadata)
            {
                Console.WriteLine("key: "+item.Key+" value: "+item.Value+"");
            }

            Console.WriteLine("Done");
        }
        private static UniEntityResponse AWSCompleteEntityRecognition(string input)
        {
            string[] datenSplit = SplitDatenschutz(input);
            UniEntityResponse[] entResultEntities = new UniEntityResponse[datenSplit.Length];
            for (int i = 0; i < datenSplit.Length; i++)
            {
                entResultEntities[i] = ConvertAWSToUniEntity(AWSEntityRecognize(datenSplit[i]));
            }
            Console.WriteLine("AWS rec. done, combining results...");
            UniEntityResponse awsResEnt = CombineUniEntitySplitParts(entResultEntities);
            Console.WriteLine("AWS results successfully combined.");

            return awsResEnt;
        }
        private static UniEntityResponse ConvertAWSToUniEntity(DetectEntitiesResponse awsResponse)
        {

            UniEntityResponse result = new UniEntityResponse();
            result.entities = new List<UniEntityResponse.entity>();
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

                            UniEntityResponse.mention newMention = new UniEntityResponse.mention();
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
                    UniEntityResponse.entity resEnt = new UniEntityResponse.entity();
                    resEnt.type = awsEntity.Type.ToString().ToUpper().Replace("_", "");
                    //resEnt.type = resEnt.type[0] + resEnt.type[1..].ToLower();
                    resEnt.text = awsEntity.Text;
                    resEnt.confidence = awsEntity.Score;
                    resEnt.relevance = 0;
                    resEnt.mentions = new List<UniEntityResponse.mention>();

                    UniEntityResponse.mention newMention = new UniEntityResponse.mention();

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
        private static int[] CheckForRights(List<int> rightLocations, string rightRelevantEN, string rightRelevantDE, string rightRelevantENControl = "", string rightRelevantDEControl = "", bool caseInsensitive = false)
        {
            int distanceRight = int.MaxValue;
            int closestDistance = int.MaxValue;
            int closestRightLocation = int.MaxValue;
            int rightRelevantLocation = int.MaxValue;

            int distanceRightControl = int.MaxValue;
            rightRelevantENControl = ""; //TODO This Should be removed but these don't work well yet..
            rightRelevantDEControl = ""; //TODO This Should be removed but these don't work well yet..

            foreach (var location in rightLocations) //Determine correct location
            {
                if (IsThisCloseTo(rightRelevantEN, location, 300, "normal", caseInsensitive)
                    || IsThisCloseTo(rightRelevantDE, location, 300, "normal", caseInsensitive))
                {
                    distanceRight = HowCloseTo(location, rightRelevantEN, 500, "normal", caseInsensitive);
                    distanceRightControl = HowCloseTo(location, rightRelevantENControl, 800, "normal", caseInsensitive);
                    if (distanceRight < closestDistance)
                    {
                        if (distanceRightControl < 800)
                        {
                            closestDistance = distanceRight;
                            closestRightLocation = location;

                            if (HowCloseTo(location, rightRelevantEN, 500, "after", caseInsensitive) < HowCloseTo(location, rightRelevantEN, 500, "before", caseInsensitive))
                            {
                                rightRelevantLocation = location + closestDistance;
                            }
                            else
                            {
                                rightRelevantLocation = location - closestDistance;
                            }
                        }
                    }
                    distanceRight = HowCloseTo(location, rightRelevantDE, 500, "normal", caseInsensitive);
                    distanceRightControl = HowCloseTo(location, rightRelevantDEControl, 800, "normal", caseInsensitive);

                    if (distanceRight < closestDistance)
                    {
                        if (distanceRightControl < 800)
                        {
                            closestDistance = distanceRight;
                            closestRightLocation = location;
                            if (HowCloseTo(location, rightRelevantDE, 500, "after", caseInsensitive) < HowCloseTo(location, rightRelevantDE, 500, "before", caseInsensitive))
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
        /// <summary>
        /// Returns whether or not a specified string can be found
        /// in the given range around the start offset.
        /// Checks in the string "datenschutzerkl".
        /// </summary>
        /// <param name="text"></param>
        /// <param name="start"></param>
        /// <param name="range"></param>
        /// <returns>boolean result of Search within range</returns>
        private static Boolean IsThisCloseTo(string text, int start, int range = 100, string mode = "normal", bool caseInsensitive = false) //todo give List<String> to search for
        {
            int datLen = datenschutzerkl.Length;
            if (String.Compare(mode, "normal")==0)
            {
                if (datLen - start <= range && start <= range)
                {
                    if (caseInsensitive)
                    { return datenschutzerkl.Substring(0, datLen).ToUpper().Contains(text.ToUpper()); }
                    else { return datenschutzerkl.Substring(0, datLen).Contains(text); }
                }
                else if (start <= range)
                {
                    if (caseInsensitive){ return datenschutzerkl.Substring(0, range + start).ToUpper().Contains(text.ToUpper()); }
                    else { return datenschutzerkl.Substring(0, range + start).Contains(text); }
                }
                else if (datLen - start <= range)
                {
                    if (caseInsensitive) { return datenschutzerkl.Substring(start - range, datLen - start + range).ToUpper().Contains(text.ToUpper()); }
                    else { return datenschutzerkl.Substring(start - range, datLen - start + range).Contains(text); }
                }
                else
                {
                    if (caseInsensitive) { return datenschutzerkl.Substring(start - range, 2 * range).ToUpper().Contains(text.ToUpper()); }
                    else { return datenschutzerkl.Substring(start - range, 2 * range).Contains(text); }
                }
            }
            else if (String.Compare(mode, "after") == 0)
            {
                if (datLen - start <= range && start <= range)
                {
                    if (caseInsensitive) { return datenschutzerkl.Substring(start, datLen - start).ToUpper().Contains(text.ToUpper()); }
                    else { return datenschutzerkl.Substring(start, datLen - start).Contains(text); }
                }
                else if (start <= range)
                {
                    if (caseInsensitive) { return datenschutzerkl.Substring(start, range + start).ToUpper().Contains(text.ToUpper()); }
                    else { return datenschutzerkl.Substring(start, range + start).Contains(text); }
                }
                else if (datLen - start <= range)
                {
                    if (caseInsensitive) { return datenschutzerkl.Substring(start, datLen - start).ToUpper().Contains(text.ToUpper()); }
                    else { return datenschutzerkl.Substring(start, datLen - start).Contains(text); }
                }
                else
                {
                    if (caseInsensitive) { return datenschutzerkl.Substring(start, range).ToUpper().Contains(text.ToUpper()); }
                    else { return datenschutzerkl.Substring(start, range).Contains(text); }
                }
            }

            else if (String.Compare(mode, "before") == 0)
            {
                if (datLen - start <= range && start <= range)
                {
                    if (caseInsensitive) { return datenschutzerkl.Substring(0, datLen).ToUpper().Contains(text.ToUpper()); }
                    else { return datenschutzerkl.Substring(0, datLen).Contains(text); }
                }
                else if (start <= range)
                {
                    if (caseInsensitive) { return datenschutzerkl.Substring(0, range).ToUpper().Contains(text.ToUpper()); }
                    else { return datenschutzerkl.Substring(0, range).Contains(text); }
                }
                else
                {
                    if (caseInsensitive) { return datenschutzerkl.Substring(start - range, range).ToUpper().Contains(text.ToUpper()); }
                    else { return datenschutzerkl.Substring(start - range, range).Contains(text); }
                }
            }
            return false;
        }
        /// <summary>
        /// Liest aus "datenschutzerkl." in dem angegebenen Radius. 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="leftrange"></param>
        /// <param name="rightrange"></param>
        /// <returns></returns>
        private static string GetTextAroundHere(int start, int leftrange, int rightrange = -1)
        {
            int datLen = datenschutzerkl.Length;
            if (rightrange == -1)
            { rightrange = leftrange; }

            //if-Abfragen verhindern outOfBounds-exceptions
            if (datLen - 1 < start + rightrange && start < leftrange)
            {
                return Readable(datenschutzerkl.Substring(0, datLen));
            }
            else if (datLen - 1 <= start + rightrange)
            {
                int startoffset = start - leftrange - FindReadableTextStart(start - leftrange);
                return Readable(datenschutzerkl.Substring(FindReadableTextStart(start - leftrange), FindReadableTextRange(start-leftrange-startoffset, leftrange +((datLen-start)/2)+startoffset, leftrange + datLen - start+startoffset))); 
            }
            else if (start <= leftrange)
            {
                return Readable(datenschutzerkl.Substring(0, FindReadableTextRange(0, start + (rightrange / 2), start + rightrange)));
            }
            else
            {
                int startoffset = start - leftrange - FindReadableTextStart(start - leftrange);
                return Readable(datenschutzerkl.Substring(FindReadableTextStart(start - leftrange), FindReadableTextRange(start-leftrange-startoffset, leftrange + (rightrange / 2)+startoffset, leftrange + rightrange+startoffset)));
            }
        }
        private static int FindReadableTextStart(int start)
        {
            for (int i = start; i >= 0; i--)
            {
                if (datenschutzerkl[i] == '.')
                {
                    return i + 1;
                }
                if (datenschutzerkl[i] == '\n')
                {
                    return i + 1;
                }
            }
            return start;
        }
        private static int FindReadableTextRange(int start, int minrange, int maxrange)
        {
            try
            {
                for (int i = start + maxrange; i > start + minrange + 2; i--)
                {
                    if (datenschutzerkl[i] == '\n')
                    {
                        if (datenschutzerkl[i - 1] == '\r')
                        {
                            //Console.WriteLine("Debug Code 1");
                            return i - 2 - start;
                        }
                    }
                }
                for (int i = start + maxrange; i > start + minrange + 1; i--)
                {
                    if (datenschutzerkl[i].Equals('.') || datenschutzerkl[i].Equals(';'))
                    {
                        //Console.WriteLine("Debug Code 2");
                        return i + 1 - start;
                    }
                }
                for (int i = start + maxrange; i < datenschutzerkl.Length - 1; i++)
                {
                    if (datenschutzerkl[i].Equals('.'))
                    {
                        //Console.WriteLine("Debug Code 3");
                        return i + 1 - start;
                    }
                    if (datenschutzerkl[i].Equals('\n'))
                    {
                        if (datenschutzerkl[i - 1].Equals('\r'))
                        {
                            //Console.WriteLine("Debug Code 4");
                            return i - 2 - start;
                        }
                    }
                }
                for (int i = start + maxrange; i > start + minrange + 1; i--)
                {
                    if (Char.IsWhiteSpace(datenschutzerkl[i]))
                    {
                        //Console.WriteLine("Debug Code 5");
                        return i - start;
                    }
                }
            }
            catch (Exception)
            {
                return maxrange;
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
        private static UniEntityResponse.mention FindClosestMentionOfType(UniEntityResponse root, string type, int start, string[] types = null, string mode = "normal", string[] ignoreList = null)
        {
            UniEntityResponse.entity closestEntity = new UniEntityResponse.entity();
            UniEntityResponse.mention closestMention = new UniEntityResponse.mention();
            closestEntity.text = "[No entity of type \"" + type + "\" found.]";
            closestMention.text = "[No entity of type \"" + type + "\" found.]";
            int closestDistance = int.MaxValue;
            bool isOfCorrectType = false;
            bool foundInIgnore = false;

            foreach (UniEntityResponse.entity entity in root.entities)
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
                foreach (UniEntityResponse.mention mention in entity.mentions)
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
        private static UniEntityResponse.mention GetEarliestMention(UniEntityResponse.entity entity)
        {
            int earliest = int.MaxValue;
            UniEntityResponse.mention earliestMention = new UniEntityResponse.mention();
            if (entity.mentions != null)
            {
                foreach (UniEntityResponse.mention mention in entity.mentions)
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
        /// <summary>
        /// Finds the mention (of the correct type) that is closest to any of the mentions of the input entity.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="type"></param>
        /// <param name="entity"></param>
        /// <param name="mode"></param>
        /// <param name="types"></param>
        /// <param name="ignoreList"></param>
        /// <returns></returns>
        private static UniEntityResponse.mention GetOverallClosestMentionOfType(UniEntityResponse root, string type, UniEntityResponse.entity entity, string mode = "normal", string[] types = null, string[] ignoreList = null)
        {
            //IBMEntity.entity closestEntity = new IBMEntity.entity();
            UniEntityResponse.mention closestMention = new UniEntityResponse.mention();
            //closestEntity.text = "[No entity of type \"" + types + "\" found.]";
            closestMention.location = new List<int>(); //TODO watch out for empty locations
            closestMention.location.Add(0);
            closestMention.location.Add(0);
            closestMention.text = "No fitting Entity found";


            int closestDistance = int.MaxValue;
            bool isOfCorrectType = false;
            foreach (UniEntityResponse.entity item in root.entities)
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
                foreach (UniEntityResponse.mention entityMention in entity.mentions)
                {
                    foreach (UniEntityResponse.mention itemMention in item.mentions)
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
        /// <summary>
        /// Finds the closest mention (of the correct type) to all occurances of the input text in the string datenschutzerkl
        /// </summary>
        /// <param name="root"></param>
        /// <param name="type"></param>
        /// <param name="text"></param>
        /// <param name="mode"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        private static UniEntityResponse.mention GetOverallClosestMentionOfTypeToString(UniEntityResponse root, string type, string text, string mode = "normal", string[] types = null)
        {
            int ClosestDistance = int.MaxValue;
            int distance;
            UniEntityResponse.mention closestMention = new UniEntityResponse.mention();
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
        
        private static int HowCloseTo(int location, string text, int maxRange = 500, string mode = "normal", bool caseInsensitive = false)
        { 
            for (int i = 0; i < maxRange; i++)
            {
                if (IsThisCloseTo(text, location, i, mode, caseInsensitive))
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

      #region Allgemein
        public static void DemoRun(string deMode)
        {
            string read;

            for (int i = 0; i < 100000; i++)
            {
                Console.WriteLine("Please choose (A)WS, (M)icrosoft, (G)oogle, (I)BM, (C)ombined, (T)ype specific combined or (q)uit\n");
                read = Console.ReadLine();
                datenschutzerkl = "";
                string inputLine = "";
                switch (read)
                {
                    case "A":   //CASE AWS
                        Console.WriteLine("Please insert the filename of a text to be analysed\n");
                        inputLine = Console.ReadLine();
                        entityRecognitionExecute(deMode, "A", inputLine);
                        return;
                    case "M":   //CASE MICROSOFT AZURE
                        Console.WriteLine("Please insert the filename of a text to be analysed\n");
                        inputLine = Console.ReadLine();
                        entityRecognitionExecute(deMode, "M", inputLine);
                        return;
                    case "G":   //CASE GOOGLE
                        Console.WriteLine("Please insert the filename of a text to be analysed\n");
                        inputLine = Console.ReadLine();
                        entityRecognitionExecute(deMode, "G", inputLine);
                        return;
                    case "I":   //CASE IBM
                        Console.WriteLine("Please insert the filename of a text to be analysed\n");
                        inputLine = Console.ReadLine();
                        entityRecognitionExecute(deMode, "I", inputLine);
                        return;
                    case "C": //CASE COMBINED
                        Console.WriteLine("Please insert the filename of a text to be analysed\n");
                        inputLine = Console.ReadLine();
                        entityRecognitionExecute(deMode, "C", inputLine);
                        return;
                    case "T": //CASE COMBINED
                        Console.WriteLine("Please insert the filename of a text to be analysed\n");
                        inputLine = Console.ReadLine();
                        entityRecognitionExecute(deMode, "T", inputLine);
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
            Dictionary<string, int> serviceCodes = new Dictionary<string, int>();
            serviceCodes.Add("A", 1);
            serviceCodes.Add("M", 1);
            serviceCodes.Add("G", 1);
            serviceCodes.Add("I", 1);
            serviceCodes.Add("C", 1);
            serviceCodes.Add("T", 1);

            Console.WriteLine("Please insert input directory name");
            string read = Console.ReadLine();

            if (!Directory.Exists(read))
            {
                Console.WriteLine("Sorry, this directory could not be found.");
                return;
            }

            Console.WriteLine("If you do NOT want to use certain services, please enter the corresponding letters separated by a space. \nOtherwise, just press enter.");
            Console.WriteLine("The Service Codes are: (A)WS, (M)icrosoft, (G)oogle, (I)BM, (C)ombined and (T)ype specific combined");
            string read2 = Console.ReadLine();
            string[] read3 = read2.Split(" ");
            foreach (var item in read3)
            {
                if (serviceCodes.ContainsKey(item))
                {
                    serviceCodes[item] = 0;
                }
            }

            foreach (var fileName in Directory.GetFiles(read))
            {
                if(String.Compare(Path.GetExtension(fileName), ".txt") == 0)
                {
                    foreach (var key in serviceCodes.Keys)
                    {
                        if (serviceCodes[key] == 1)
                        {
                            entityRecognitionExecute("a", key, fileName);
                        }
                    }
                }
            }
            Console.WriteLine("Execution complete.");
            return;
        }
        private static void entityRecognitionExecute(string deMode, string serviceCode, string inputLine)
        {

            string outputPathWithoutFileExtension;

            if (String.IsNullOrEmpty(inputLine))
            {
                Console.WriteLine("Error - Input empty.");
                return;
            }
            if (File.Exists(inputLine))
            {
                inputPath = inputLine;
                outputPathWithoutFileExtension = Path.GetDirectoryName(inputPath)+ "\\" +Path.GetFileNameWithoutExtension(inputPath)+"#"+serviceCode;
                datenschutzerkl = File.ReadAllText(inputLine); //TODO eher jedes Mal wenn nötig einlesen.
                UniEntityResponse responseEntity = new UniEntityResponse();
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
                    case "C":
                        Console.WriteLine("Beginning Combined recognition...");
                        UniEntityResponse[] allServiceResults = new UniEntityResponse[4];

                        Console.WriteLine("Google - Beginning recognition...");
                        allServiceResults[0] = GoogleCompleteEntityRecognition(datenschutzerkl);
                        Console.WriteLine("Google - Recognition finished.");

                        Console.WriteLine("IBM - Beginning recognition...");
                        allServiceResults[1] = IBMCompleteEntityRecognition(datenschutzerkl);
                        Console.WriteLine("IBM - Recognition finished.");

                        Console.WriteLine("AWS - Beginning recognition...");
                        allServiceResults[2] = AWSCompleteEntityRecognition(datenschutzerkl);
                        Console.WriteLine("AWS - Recognition finished.");

                        Console.WriteLine("Microsoft - Beginning recognition...");
                        allServiceResults[3] = MicrosoftCompleteEntityRecognition(datenschutzerkl);
                        Console.WriteLine("Microsoft - Recognition finished.");

                        Console.WriteLine("Combining results...");

                        //responseEntity = CombineUniEntities(allServiceResults);
                        responseEntity = FuseUniEntities(allServiceResults, false);

                        Console.WriteLine("Results successfully combined.");

                        Console.Write("Combined recognition");
                        break;
                    case "T":
                        Console.WriteLine("Beginning type specific combined recognition...");
                        UniEntityResponse[] allServiceResults2 = new UniEntityResponse[4];

                        Console.WriteLine("Google - Beginning recognition...");
                        allServiceResults2[0] = GoogleCompleteEntityRecognition(datenschutzerkl);
                        Console.WriteLine("Google - Recognition finished.");

                        Console.WriteLine("IBM - Beginning recognition...");
                        allServiceResults2[1] = IBMCompleteEntityRecognition(datenschutzerkl);
                        Console.WriteLine("IBM - Recognition finished.");

                        Console.WriteLine("AWS - Beginning recognition...");
                        allServiceResults2[2] = AWSCompleteEntityRecognition(datenschutzerkl);
                        Console.WriteLine("AWS - Recognition finished.");

                        Console.WriteLine("Microsoft - Beginning recognition...");
                        allServiceResults2[3] = MicrosoftCompleteEntityRecognition(datenschutzerkl);
                        Console.WriteLine("Microsoft - Recognition finished.");

                        Console.WriteLine("Combining results...");

                        responseEntity = FuseUniEntities(allServiceResults2, true);

                        Console.WriteLine("Results successfully combined.");

                        Console.Write("Type specific combined recognition");
                        break;
                    default:
                        return;
                }
                Console.WriteLine(" - Recognition finished.");

                if (deMode == "a")
                {
                    TIL res;
                    switch (serviceCode)
                    {
                        case "I":
                            Console.WriteLine("IBM - Beginning processing...");
                            //printTILResult(AnalyseIBMEntityResponse(responseEntity));
                            res = AnalyseResponseUniEntity(responseEntity);
                            printTILResultReadable(AnalyseResponseUniEntity(responseEntity));
                            Console.Write("IBM");
                            break;
                        case "A":
                            Console.WriteLine("AWS - Beginning processing...");
                            res = AnalyseResponseUniEntity(responseEntity);
                            printTILResultReadable(res);
                            Console.Write("AWS");
                            break;
                        case "M":
                            Console.WriteLine("Microsoft - Beginning processing...");
                            res = AnalyseResponseUniEntity(responseEntity);
                            printTILResultReadable(res);
                            Console.Write("Microsoft");
                            break;
                        case "G":
                            Console.WriteLine("Google - Beginning processing...");
                            res = AnalyseResponseUniEntity(responseEntity);
                            printTILResultReadable(res);
                            Console.Write("Google");
                            break;
                        case "C":
                            Console.WriteLine("Beginning combined processing...");
                            //printTILResult(AnalyseCombinedEntityResponse(responseEntity));
                            res = AnalyseResponseUniEntity(responseEntity);
                            printTILResultReadable(res);
                            Console.Write("Combined processing");
                            break;
                        case "T":
                            Console.WriteLine("Beginning type specific combined processing...");
                            res = AnalyseResponseUniEntity(responseEntity);
                            printTILResultReadable(res);
                            Console.Write("Type specific combined processing");
                            break;
                        default:
                            return;
                    }
                    saveTILResult(res, outputPathWithoutFileExtension);
                    Console.WriteLine(" - Processing finished.");

                    responseEntity.entities = responseEntity.entities.OrderBy(ent => ent.type).ToList();    //order by type 
                    saveEntityResult(responseEntity, outputPathWithoutFileExtension);
                }
                else
                {   //print entities
                    responseEntity.entities = responseEntity.entities.OrderBy(ent => ent.type).ToList();    //order by type 
                    string output = "";
                    foreach (var entity in responseEntity.entities)
                    {
                        output += "Type: " + entity.type;
                        if (entity.subcategory != null && String.Compare(entity.subcategory, "") != 0)
                        {   output += ", Subcategory: " + entity.subcategory;   }

                        if (entity.relevance != 0)
                        {   output += ", Relevance: " + entity.relevance;   }

                        output += ", Text: \"" + entity.text + "\", mentions:\t";
                        foreach (var mention in entity.mentions)
                        {   output += "" + mention.location[0] + "-" + mention.location[1] + "; ";  }

                        output += "\n";
                        if (entity.Metadata != null)
                        {
                            if (entity.Metadata.Count > 0)
                            {
                                output += "\tMetaData: \n";
                        
                                foreach (var meta in entity.Metadata)
                                {
                                    output += "\tKey: " + meta.Key + "; Value: " + meta.Value + "\n";
                                }

                            }
                        }
                    }
                    Console.WriteLine(output);
                    saveEntityResult(responseEntity, outputPathWithoutFileExtension);
                }
            }
            else
            {   Console.WriteLine("Sorry, no file with that path/name could be found.");    }
            return;
        }
        private static void saveEntityResult(UniEntityResponse entityRes, string filePathWithoutExtension)
        {
            string resultDirectory = Path.GetDirectoryName(filePathWithoutExtension) + "\\Responses";
            if (!Directory.Exists(resultDirectory))
            {
                Directory.CreateDirectory(resultDirectory);
            }
            filePathWithoutExtension += ".json";
            filePathWithoutExtension = resultDirectory + "\\" + Path.GetFileName(filePathWithoutExtension);
            Console.WriteLine("Trying to save file " + filePathWithoutExtension);
            File.WriteAllText(filePathWithoutExtension, JsonConvert.SerializeObject(entityRes));//.Replace("\",", "\",\n\t").Replace(",\"", ",\n\t\"").Replace(",{", ",\n{\n"));    //für Lesbarkeit

            return;
        }
        private static void saveTILResult(TIL resultTIL, string filePathWithoutExtension)
        {
            string resultDirectory = Path.GetDirectoryName(filePathWithoutExtension)+"\\Results";
            if (!Directory.Exists(resultDirectory))
            {
                Directory.CreateDirectory(resultDirectory);
            }
            filePathWithoutExtension += ".json";
            filePathWithoutExtension = resultDirectory + "\\" + Path.GetFileName(filePathWithoutExtension);
            //Console.WriteLine("Trying to save file " + filePathWithoutExtension);
            File.WriteAllText(filePathWithoutExtension, JsonConvert.SerializeObject(resultTIL).Replace("\",", "\",\n").Replace(",\"", ",\n\"").Replace("{","{\n"));

            return;
        }

        /// <summary>
        /// Gibt den Inputstring in size (standardmäßig splitsize) großen Teilen zurück.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="size"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Combines UniEntities 
        /// Fixes locations in mentions
        /// ONLY USE ON PARTS OF A WHOLE, single Service only!
        /// </summary>
        /// <param name="input"></param>
        /// <returns>The combined entity as if the service had processed the entire file</returns>
        private static UniEntityResponse CombineUniEntitySplitParts(UniEntityResponse[] input)
        {
            if (input.Length == 0)
            {
                return null;
            }
            UniEntityResponse resEntity = input[0];
            for (int i = 1; i < input.Length; i++)
            {
                UniEntityResponse midEntity = input[i];
                foreach (UniEntityResponse.entity entity in midEntity.entities)
                {
                    foreach (UniEntityResponse.mention mention in entity.mentions)
                    {   //fixt die location-offsets da der Text in Segmenten verschickt wurde
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
        /// <summary>
        /// Combines multiple UniEntities without touching the data.
        /// Can be used to cómbine multiple Services' results
        /// </summary>
        /// <param name="input"></param>
        /// <returns>The combined UniEntityResponse</returns>
        private static UniEntityResponse CombineUniEntities(UniEntityResponse[] input)
        {
            if (input.Length == 0)
            {
                return null;
            }
            UniEntityResponse resEntity = input[0];
            for (int i = 1; i < input.Length; i++)
            {
                resEntity += input[i];
            }
            return resEntity;
        }
        /// <summary>
        /// Combines multiple UniEntities with certain restrictions.
        /// Can be used to cómbine multiple Services' results
        /// </summary>
        /// <param name="input"></param>
        /// <returns>The combined UniEntityResponse</returns>
        private static UniEntityResponse FuseUniEntities(UniEntityResponse[] input, bool sameTypesOnly)
        {
            if (input.Length == 0)
            {    return null;    }
            UniEntityResponse resEntity = new UniEntityResponse();
            resEntity.usage = input[0].usage;
            resEntity.language = input[0].language;
            resEntity.entities = new List<UniEntityResponse.entity>();


            foreach (UniEntityResponse iER1 in input)
            {
                if (String.Compare(iER1.language, "")!=0)
                {   resEntity.language = iER1.language;   }
                foreach (UniEntityResponse.entity ent1 in iER1.entities)
                {
                    List<UniEntityResponse.mention> entMentions = new List<UniEntityResponse.mention>();

                    foreach (UniEntityResponse iER2 in input)
                    {
                        bool found = false;
                        if (iER1 == iER2)
                        {   continue;   }
                        foreach (UniEntityResponse.entity ent0 in resEntity.entities)
                        {
                            if (String.Compare(ent1.text, ent0.text) == 0)
                            {
                                if (!sameTypesOnly)
                                {
                                    found = true;
                                    break;
                                }
                                else if(String.Compare(ent1.type, ent0.type) == 0)
                                {
                                    found = true;
                                    break;
                                }
                            }
                        }
                        if (found)
                        { break; }
                        foreach (UniEntityResponse.entity ent2 in iER2.entities)
                        {

                            if (String.Compare(ent1.text, ent2.text)==0)
                            {
                                if (sameTypesOnly && String.Compare(ent1.type, ent2.type) != 0)
                                { continue; }

                                foreach (UniEntityResponse.mention ment1 in ent1.mentions)
                                {
                                    foreach (UniEntityResponse.mention ment2 in ent2.mentions)
                                    {
                                        if (ment1.location[0] == ment2.location[0])
                                        {
                                            entMentions.Add(ment1);
                                            break;
                                        }
                                    }
                                }
                                if (entMentions.Count != 0)
                                {
                                    ent1.mentions = entMentions;
                                    resEntity.entities.Add(ent1);
                                    found = true;
                                    break;
                                }
                            }
                        }
                        if (found)
                        {   break;  }
                    }
                }
            }
            //todo
            return resEntity;
        }


        #endregion Allgemein

        #region printing
        private static void printTILResult(TIL resultTIL)
        {
            Console.WriteLine(JsonConvert.SerializeObject(resultTIL));
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
            return;
        }
      #endregion
    }
}

//NOTIZEN

    //TODO / Mögliche Verbesserungen:
    //  Evtl Bei Google Entities mit den gleichen Metadaten (zB Wikieinträge) zu einem kombinieren.
    // die Datenschutzrichtlinie einteilen / trennen nach Absätzen zB 3.3.3.3 ?
    //make it so if a txt file has been processed before, draw the results from a file if found. (toJSON, save in file -> extract from file, fromJson)
    //bei PERSON type die rausfiltern die "street" idN haben oder mit B. anfangen.
    //Clean up.