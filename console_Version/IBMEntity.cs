using System;
using System.Collections.Generic;
using System.Text;

namespace NLPServiceEndpoint_Console_Ver
{
    class IBMEntity
    {
        public usageInfo usage { get; set; }
        public string language { get; set; }
        public List<entity> entities { get; set; }

        public class usageInfo
        {
            public string text_units { get; set; }
            public int text_characters { get; set; }
            public int features { get; set; }
        }
        public class entity
        {
            public string type { get; set; }
            public string text { get; set; }
            public sentim sentiment { get; set; }
            public float relevance { get; set; }
            public List<mention> mentions { get; set; }
            public int count { get; set; }
            public float confidence { get; set; }
        }
        public class sentim
        {
            public float score { get; set; }
            public string label { get; set; }
        }
        public class mention
        {
            public string text { get; set; }
            public List<int> location { get; set; }
            public float confidence { get; set; }
        }
    }
}
