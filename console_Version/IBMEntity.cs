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
            public int text_units { get; set; }
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
        public static IBMEntity operator +(IBMEntity ent, IBMEntity ity)
        {
            ent.usage.text_units += ity.usage.text_units;
            ent.usage.text_characters += ity.usage.text_characters;
            ent.usage.features = Math.Max(ent.usage.features, ity.usage.features);
            foreach (entity entEntity in ent.entities)
            {
                foreach (entity ityEntity in ity.entities)
                {
                    if (String.Compare(entEntity.text, ityEntity.text) == 0 && String.Compare(entEntity.type, ityEntity.type) == 0)
                    {
                        foreach (mention ityMention in ityEntity.mentions)
                        {
                            entEntity.mentions.Add(ityMention);
                        }
                        entEntity.count += ityEntity.count;
                    }
                }
            }
            foreach (entity ityEntity in ity.entities)
            {
                bool ityEntityFound = false;
                foreach (entity entEntity in ent.entities)
                {
                    if (String.Compare(entEntity.text, ityEntity.text) == 0 && String.Compare(entEntity.type, ityEntity.type) == 0)
                    {
                        ityEntityFound = true;
                    }
                }
                if (!ityEntityFound)
                {
                    ent.entities.Add(ityEntity);
                }
            }

            return ent;
        }
    }
}
