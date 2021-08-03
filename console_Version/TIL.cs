using System;
using System.Collections.Generic;
using System.Text;

namespace NLPServiceEndpoint_Console_Ver
{
    class TIL
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
        //public Root root { get; set; }
        public TIL()
        {
            meta = new Meta();
            controller = new Controller();
            controller.representative = new Representative();
            dataProtectionOfficer = new DataProtectionOfficer();
            dataDisclosed = new List<DataDisclosed>();
            thirdCountryTransfers = new List<ThirdCountryTransfer>();
            accessAndDataPortability = new AccessAndDataPortability();
            sources = new List<String>();
            rightToInformation = new RightToInformation();
            rightToRectificationOrDeletion = new RightToRectificationOrDeletion();
            rightToDataPortability = new RightToDataPortability();
            rightToWithdrawConsent = new RightToWithdrawConsent();
            rightToComplain = new RightToComplain();
            automatedDecisionMaking = new AutomatedDecisionMaking();
            changesOfPurpose = new List<ChangesOfPurpose>();
        }

        public Meta meta { get; set; }
        public Controller controller { get; set; }
        public DataProtectionOfficer dataProtectionOfficer { get; set; }
        public List<DataDisclosed> dataDisclosed { get; set; }
        public List<ThirdCountryTransfer> thirdCountryTransfers { get; set; }
        public AccessAndDataPortability accessAndDataPortability { get; set; }
        public List<String> sources { get; set; }
        public RightToInformation rightToInformation { get; set; }
        public RightToRectificationOrDeletion rightToRectificationOrDeletion { get; set; }
        public RightToDataPortability rightToDataPortability { get; set; }
        public RightToWithdrawConsent rightToWithdrawConsent { get; set; }
        public RightToComplain rightToComplain { get; set; }
        public AutomatedDecisionMaking automatedDecisionMaking { get; set; }
        public List<ChangesOfPurpose> changesOfPurpose { get; set; }

        public class Meta
        {
            public string _id { get; set; }
            public string name { get; set; }
            public string created { get; set; }
            public string modified { get; set; }
            public string version { get; set; }
            public string language { get; set; }
            public string status { get; set; }
            public string url { get; set; }
            public string _hash { get; set; }
        }

        public class Representative
        {
            public string name { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
        }

        public class Controller
        {
            public string name { get; set; }
            public string division { get; set; }
            public string address { get; set; }
            public string country { get; set; }
            public Representative representative { get; set; }
        }

        public class DataProtectionOfficer
        {
            public string name { get; set; }
            public string address { get; set; }
            public string country { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
        }

        public class Purpos
        {
            public string purpose { get; set; }
            public string description { get; set; }
        }

        public class LegalBas
        {
            public string reference { get; set; }
            public string description { get; set; }
        }

        public class LegitimateInterest
        {
            public bool exists { get; set; }
            public string reasoning { get; set; }
        }

        public class Recipient
        {
            public string name { get; set; }
            public string division { get; set; }
            public string address { get; set; }
            public string country { get; set; }
            public Representative representative { get; set; }
            public string category { get; set; }
        }

        public class Temporal
        {
            public string description { get; set; }
            public string ttl { get; set; }
        }

        public class Storage
        {
            public List<Temporal> temporal { get; set; }
            public List<string> purposeConditional { get; set; }
            public List<string> legalBasisConditional { get; set; }
            public string aggregationFunction { get; set; }
        }

        public class NonDisclosure
        {
            public bool legalRequirement { get; set; }
            public bool contractualRegulation { get; set; }
            public bool obligationToProvide { get; set; }
            public string consequences { get; set; }
        }

        public class DataDisclosed
        {
            public string _id { get; set; }
            public string category { get; set; }
            public List<Purpos> purposes { get; set; }
            public List<LegalBas> legalBases { get; set; }
            public List<LegitimateInterest> legitimateInterests { get; set; }
            public List<Recipient> recipients { get; set; }
            public List<Storage> storage { get; set; }
            public NonDisclosure nonDisclosure { get; set; }
        }

        public class AdequacyDecision
        {
            public bool available { get; set; }
            public string description { get; set; }
        }

        public class AppropriateGuarantees
        {
            public bool available { get; set; }
            public string description { get; set; }
        }

        public class PresenceOfEnforcableRightsAndEffectiveRemedies
        {
            public bool available { get; set; }
            public string description { get; set; }
        }

        public class StandardDataProtectionClause
        {
            public bool available { get; set; }
            public string description { get; set; }
        }

        public class ThirdCountryTransfer
        {
            public string country { get; set; }
            public AdequacyDecision adequacyDecision { get; set; }
            public AppropriateGuarantees appropriateGuarantees { get; set; }
            public PresenceOfEnforcableRightsAndEffectiveRemedies presenceOfEnforcableRightsAndEffectiveRemedies { get; set; }
            public StandardDataProtectionClause standardDataProtectionClause { get; set; }
        }

        public class AdministrativeFee
        {
            public double amount { get; set; }
            public string currency { get; set; }
        }

        public class AccessAndDataPortability
        {
            public bool available { get; set; }
            public string description { get; set; }
            public string url { get; set; }
            public string email { get; set; }
            public List<string> identificationEvidences { get; set; }
            public AdministrativeFee administrativeFee { get; set; }
            public List<string> dataFormats { get; set; }
        }

        public class Source2
        {
            public string description { get; set; }
            public string url { get; set; }
            public bool publiclyAvailable { get; set; }
            public string _id { get; set; }
            public string dataCategory { get; set; }
            public List<String> sources { get; set; }
        }

        public class RightToInformation
        {
            public bool available { get; set; }
            public string description { get; set; }
            public string url { get; set; }
            public string email { get; set; }
            public List<string> identificationEvidences { get; set; }
        }

        public class RightToRectificationOrDeletion
        {
            public bool available { get; set; }
            public string description { get; set; }
            public string url { get; set; }
            public string email { get; set; }
            public List<string> identificationEvidences { get; set; }
        }

        public class RightToDataPortability
        {
            public bool available { get; set; }
            public string description { get; set; }
            public string url { get; set; }
            public string email { get; set; }
            public List<string> identificationEvidences { get; set; }
        }

        public class RightToWithdrawConsent
        {
            public bool available { get; set; }
            public string description { get; set; }
            public string url { get; set; }
            public string email { get; set; }
            public List<string> identificationEvidences { get; set; }
        }

        public class SupervisoryAuthority
        {
            public string name { get; set; }
            public string address { get; set; }
            public string country { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
        }

        public class RightToComplain
        {
            public bool available { get; set; }
            public string description { get; set; }
            public string url { get; set; }
            public string email { get; set; }
            public List<string> identificationEvidences { get; set; }
            public SupervisoryAuthority supervisoryAuthority { get; set; }
        }

        public class AutomatedDecisionMaking
        {
            public bool inUse { get; set; }
            public string logicInvolved { get; set; }
            public string scopeAndIntendedEffects { get; set; }
        }

        public class ChangesOfPurpose
        {
            public string description { get; set; }
            public List<string> affectedDataCategories { get; set; }
            public string plannedDateOfChange { get; set; }
            public string urlOfNewVersion { get; set; }
        }

        //public class Root
        //{
        //    public Meta meta { get; set; }
        //    public Controller controller { get; set; }
        //    public DataProtectionOfficer dataProtectionOfficer { get; set; }
        //    public List<DataDisclosed> dataDisclosed { get; set; }
        //    public List<ThirdCountryTransfer> thirdCountryTransfers { get; set; }
        //    public AccessAndDataPortability accessAndDataPortability { get; set; }
        //    public List<String> sources { get; set; }
        //    public RightToInformation rightToInformation { get; set; }
        //    public RightToRectificationOrDeletion rightToRectificationOrDeletion { get; set; }
        //    public RightToDataPortability rightToDataPortability { get; set; }
        //    public RightToWithdrawConsent rightToWithdrawConsent { get; set; }
        //    public RightToComplain rightToComplain { get; set; }
        //    public AutomatedDecisionMaking automatedDecisionMaking { get; set; }
        //    public List<ChangesOfPurpose> changesOfPurpose { get; set; }
        //}

    }
}
