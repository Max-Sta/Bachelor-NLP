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
            //string empty = "N/A";
            meta = new Meta();
            controller = new Controller();
            dataProtectionOfficer = new DataProtectionOfficer();
            dataDisclosed = new List<DataDisclosed>();
            thirdCountryTransfers = new List<ThirdCountryTransfer>();
            accessAndDataPortability = new AccessAndDataPortability();
            sources = new List<Source2>();
            rightToInformation = new RightToInformation();
            rightToRectificationOrDeletion = new RightToRectificationOrDeletion();
            rightToDataPortability = new RightToDataPortability();
            rightToWithdrawConsent = new RightToWithdrawConsent();
            rightToComplain = new RightToComplain();
            automatedDecisionMaking = new AutomatedDecisionMaking();
            changesOfPurpose = new List<ChangesOfPurpose>();
        }
        #region 
        public Meta meta { get; set; }
        public Controller controller { get; set; }
        public DataProtectionOfficer dataProtectionOfficer { get; set; }
        public List<DataDisclosed> dataDisclosed { get; set; }
        public List<ThirdCountryTransfer> thirdCountryTransfers { get; set; }
        public AccessAndDataPortability accessAndDataPortability { get; set; }
        public List<Source2> sources { get; set; }
        public RightToInformation rightToInformation { get; set; }
        public RightToRectificationOrDeletion rightToRectificationOrDeletion { get; set; }
        public RightToDataPortability rightToDataPortability { get; set; }
        public RightToWithdrawConsent rightToWithdrawConsent { get; set; }
        public RightToComplain rightToComplain { get; set; }
        public AutomatedDecisionMaking automatedDecisionMaking { get; set; }
        public List<ChangesOfPurpose> changesOfPurpose { get; set; }
        #endregion
        public class Meta
        {
            public Meta()
            {
                string empty = "N/A";
                _id = empty;
                name = empty;
                created = empty;
                modified = empty;
                version = empty;
                language = empty;
                status = empty;
                url = empty;
                _hash = empty;
            }
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
            public Representative()
            {
                string empty = "N/A";
                name = empty;
                email = empty;
                phone = empty;
            }
            public string name { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
        }

        public class Controller
        {
            public Controller()
            {
                string empty = "N/A";
                name = empty;
                division = empty;
                address = empty;
                country = empty;
                representative = new Representative();
            }
            public string name { get; set; }
            public string division { get; set; }
            public string address { get; set; }
            public string country { get; set; }
            public Representative representative { get; set; }
        }

        public class DataProtectionOfficer
        {
            public DataProtectionOfficer()
            {
                string empty = "N/A";
                name = empty;
                address = empty;
                country = empty;
                email = empty;
                phone = empty;
            }
            public string name { get; set; }
            public string address { get; set; }
            public string country { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
        }

        public class Purpos
        {
            public Purpos()
            {
                string empty = "N/A";
                purpose = empty;
                description = empty;
            }
            public string purpose { get; set; }
            public string description { get; set; }
        }

        public class LegalBas
        {
            public LegalBas()
            {
                string empty = "N/A";
                reference = empty;
                description = empty;
            }
            public string reference { get; set; }
            public string description { get; set; }
        }

        public class LegitimateInterest
        {
            public LegitimateInterest()
            {
                string empty = "N/A";
                exists = false;
                reasoning = empty;
            }
            public bool exists { get; set; }
            public string reasoning { get; set; }
        }

        public class Recipient
        {
            public Recipient()
            {
                string empty = "N/A";
                name = empty;
                division = empty;
                address = empty;
                country = empty;
                representative = new Representative();
                category = empty;
            }
            public string name { get; set; }
            public string division { get; set; }
            public string address { get; set; }
            public string country { get; set; }
            public Representative representative { get; set; }
            public string category { get; set; }
        }

        public class Temporal
        {
            public Temporal()
            {
                string empty = "N/A";
                description = empty;
                ttl = empty;
            }
            public string description { get; set; }
            public string ttl { get; set; }
        }

        public class Storage
        {
            public Storage()
            {
                string empty = "N/A";
                temporal = new List<Temporal>();
                purposeConditional = new List<string>();
                legalBasisConditional = new List<string>();
                aggregationFunction = empty;
            }
            public List<Temporal> temporal { get; set; }
            public List<string> purposeConditional { get; set; }
            public List<string> legalBasisConditional { get; set; }
            public string aggregationFunction { get; set; }
        }

        public class NonDisclosure
        {
            public NonDisclosure()
            {
                string empty = "N/A";
                legalRequirement = false;
                contractualRegulation = false;
                obligationToProvide = false;
                consequences = empty;
            }
            public bool legalRequirement { get; set; }
            public bool contractualRegulation { get; set; }
            public bool obligationToProvide { get; set; }
            public string consequences { get; set; }
        }

        public class DataDisclosed
        {
            public DataDisclosed()
            {
                string empty = "N/A";
                _id = empty;
                category = empty;
                purposes = new List<Purpos>();
                legalBases = new List<LegalBas>();
                legitimateInterests = new List<LegitimateInterest>();
                recipients = new List<Recipient>();
                storage = new List<Storage>();
                nonDisclosure = new NonDisclosure();
            }
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
            public AdequacyDecision()
            {
                string empty = "N/A";
                available = false;
                description = empty;
            }
            public bool available { get; set; }
            public string description { get; set; }
        }

        public class AppropriateGuarantees
        {
            public AppropriateGuarantees()
            {
                string empty = "N/A";
                available = false;
                description = empty;
            }
            public bool available { get; set; }
            public string description { get; set; }
        }

        public class PresenceOfEnforcableRightsAndEffectiveRemedies
        {
            public PresenceOfEnforcableRightsAndEffectiveRemedies()
            {
                string empty = "N/A";
                available = false;
                description = empty;
            }
            public bool available { get; set; }
            public string description { get; set; }
        }

        public class StandardDataProtectionClause
        {
            public StandardDataProtectionClause()
            {
                string empty = "N/A";
                available = false;
                description = empty;
            }
            public bool available { get; set; }
            public string description { get; set; }
        }

        public class ThirdCountryTransfer
        {
            public ThirdCountryTransfer()
            {
                string empty = "N/A";
                country = empty;
                adequacyDecision = new AdequacyDecision();
                appropriateGuarantees = new AppropriateGuarantees();
                presenceOfEnforcableRightsAndEffectiveRemedies = new PresenceOfEnforcableRightsAndEffectiveRemedies();
                standardDataProtectionClause = new StandardDataProtectionClause();
            }
            public string country { get; set; }
            public AdequacyDecision adequacyDecision { get; set; }
            public AppropriateGuarantees appropriateGuarantees { get; set; }
            public PresenceOfEnforcableRightsAndEffectiveRemedies presenceOfEnforcableRightsAndEffectiveRemedies { get; set; }
            public StandardDataProtectionClause standardDataProtectionClause { get; set; }
        }

        public class AdministrativeFee
        {
            public AdministrativeFee()
            {
                string empty = "N/A";
                amount = 0;
                currency = empty;
            }
            public double amount { get; set; }
            public string currency { get; set; }
        }

        public class AccessAndDataPortability
        {
            public AccessAndDataPortability()
            {
                string empty = "N/A";
                available = false;
                description = empty;
                url = empty;
                email = empty;
                identificationEvidences = new List<string>();
                administrativeFee = new AdministrativeFee();
                dataFormats = new List<string>();
            }
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
            public Source2()
            {
                string empty = "N/A";
                _id = empty;
                dataCategory = empty;
                sources = new List<Source1>();
            }
            public string _id { get; set; }
            public string dataCategory { get; set; }
            public List<Source1> sources { get; set; }
        }
        public class Source1{
            public Source1()
            {
                string empty = "N/A";
                description = empty;
                url = empty;
                publiclyAvailable = false;
            }
            public string description { get; set; }
            public string url { get; set; }
            public bool publiclyAvailable { get; set; }
            }

        public class RightToInformation
        {
            public RightToInformation()
            {
                string empty = "N/A";
                available = false;
                description = empty;
                url = empty;
                email = empty;
                identificationEvidences = new List<string>();
            }
            public bool available { get; set; }
            public string description { get; set; }
            public string url { get; set; }
            public string email { get; set; }
            public List<string> identificationEvidences { get; set; }
        }

        public class RightToRectificationOrDeletion
        {
            public RightToRectificationOrDeletion()
            {
                string empty = "N/A";
                available = false;
                description = empty;
                url = empty;
                email = empty;
                identificationEvidences = new List<string>();
            }
            public bool available { get; set; }
            public string description { get; set; }
            public string url { get; set; }
            public string email { get; set; }
            public List<string> identificationEvidences { get; set; }
        }

        public class RightToDataPortability
        {
            public RightToDataPortability()
            {
                string empty = "N/A";
                available = false;
                description = empty;
                url = empty;
                email = empty;
                identificationEvidences = new List<string>();
            }
            public bool available { get; set; }
            public string description { get; set; }
            public string url { get; set; }
            public string email { get; set; }
            public List<string> identificationEvidences { get; set; }
        }

        public class RightToWithdrawConsent
        {
            public RightToWithdrawConsent()
            {
                string empty = "N/A";
                available = false;
                description = empty;
                url = empty;
                email = empty;
                identificationEvidences = new List<string>();
            }
            public bool available { get; set; }
            public string description { get; set; }
            public string url { get; set; }
            public string email { get; set; }
            public List<string> identificationEvidences { get; set; }
        }

        public class SupervisoryAuthority
        {
            public SupervisoryAuthority()
            {
                string empty = "N/A";
                name = empty;
                address = empty;
                country = empty;
                email = empty;
                phone = empty;
            }
            public string name { get; set; }
            public string address { get; set; }
            public string country { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
        }

        public class RightToComplain
        {
            public RightToComplain()
            {
                string empty = "N/A";
                available = false;
                description = empty;
                url = empty;
                email = empty;
                identificationEvidences = new List<string>();
                supervisoryAuthority = new SupervisoryAuthority();
            }
            public bool available { get; set; }
            public string description { get; set; }
            public string url { get; set; }
            public string email { get; set; }
            public List<string> identificationEvidences { get; set; }
            public SupervisoryAuthority supervisoryAuthority { get; set; }
        }

        public class AutomatedDecisionMaking
        {
            public AutomatedDecisionMaking()
            {
                string empty = "N/A";
                inUse = false;
                logicInvolved = empty;
                scopeAndIntendedEffects = empty;
            }
            public bool inUse { get; set; }
            public string logicInvolved { get; set; }
            public string scopeAndIntendedEffects { get; set; }
        }

        public class ChangesOfPurpose
        {
            public ChangesOfPurpose()
            {
                string empty = "N/A";
                description = empty;
                affectedDataCategories = new List<string>();
                plannedDateOfChange = empty;
                urlOfNewVersion = empty;
            }
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
//done microsoft und aws und google und ibm
//fehlen nix