using System;
using System.Collections.Generic;
using System.Text;

namespace NLPServiceEndpoint_Console_Ver
{
    class UniEntity
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
            public string subcategory { get; set; } //Microsoft only
            public string text { get; set; }
            public sentim sentiment { get; set; } // useless
            public float relevance { get; set; }
            public List<mention> mentions { get; set; }
            public int count { get; set; }
            public float confidence { get; set; }
            public Dictionary<string, string> Metadata { get; set; } //Google only?
            ///// <summary>
            ///// Ideally only use if type and text are the same
            ///// </summary>
            ///// <param name="ent"></param>
            ///// <param name="ity"></param>
            ///// <returns></returns>
            //public static UniEntity.entity operator +(UniEntity.entity ent, UniEntity.entity ity)
            //{
            //    if (ent.subcategory == null && ity.subcategory != null)
            //    {   ent.subcategory = ity.subcategory;  }
            //    if (ent.relevance != 0 && ity.relevance != 0)
            //    {   ent.relevance = ((ent.relevance + ity.relevance) / 2);  }
            //    foreach (mention ityMention in ity.mentions)
            //    {
            //        bool mentionAlreadyExists = false;
            //        foreach (var item in ent.mentions)
            //        {
            //            if (item.location[0] == ityMention.location[0])
            //            {
            //                item.confidence = ((item.confidence + ityMention.confidence) / 2);
            //                mentionAlreadyExists = true;
            //            }
            //        }
            //        if (!mentionAlreadyExists)
            //        { 
            //            ent.mentions.Add(ityMention);
            //            ent.count += 1;
            //        }
            //    }
            //    if (ent.Metadata == null && ity.Metadata != null)
            //    {
            //        ent.Metadata = new Dictionary<string, string>();
            //        foreach (var metDat in ity.Metadata)
            //        {    ent.Metadata.Add(metDat.Key, metDat.Value);   }
            //    }
            //    return ent;
            //}
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
        public static UniEntity operator +(UniEntity combineEnt, UniEntity combineIty)
        {
            //usage

            if (combineEnt.usage != null && combineIty.usage != null)
            {
                combineEnt.usage.text_units += combineIty.usage.text_units;
                combineEnt.usage.text_characters += combineIty.usage.text_characters;
                combineEnt.usage.features = Math.Max(combineEnt.usage.features, combineIty.usage.features);
            }

            //language

            if (combineIty.language != null && combineEnt.language == null)
            {
                combineEnt.language = combineIty.language;
            }

            //entities


            foreach (entity ity in combineIty.entities)
            {
                bool ityEntityFound = false;
                foreach (entity ent in combineEnt.entities)
                {
                    //"same" entity found
                    if (String.Compare(ent.text, ity.text) == 0 && String.Compare(ent.type, ity.type) == 0)
                    {
                        ityEntityFound = true;
                        if (ent.subcategory == null && ity.subcategory != null)
                        { ent.subcategory = ity.subcategory; }
                        if (ent.relevance != 0 && ity.relevance != 0)
                        { ent.relevance = ((ent.relevance + ity.relevance) / 2); }
                        if (ent.confidence != 0 && ity.confidence != 0)
                        { ent.confidence = ((ent.confidence + ity.confidence) / 2); }
                        //mentions
                        foreach (mention ityMention in ity.mentions)
                        {
                            bool mentionAlreadyExists = false;
                            foreach (var item in ent.mentions)
                            {
                                if (item.location[0] == ityMention.location[0])
                                {
                                    item.confidence = ((item.confidence + ityMention.confidence) / 2);
                                    mentionAlreadyExists = true;
                                }
                            }
                            if (!mentionAlreadyExists)
                            {
                                ent.mentions.Add(ityMention);
                                ent.count += 1;
                            }
                        }
                        if (ent.Metadata == null && ity.Metadata != null)
                        {
                            ent.Metadata = new Dictionary<string, string>();
                            foreach (var metDat in ity.Metadata)
                            { ent.Metadata.Add(metDat.Key, metDat.Value); }
                        }

                    }
                }
                //"same" entity not found
                if (!ityEntityFound)
                {   combineEnt.entities.Add(ity);    }
            }

            return combineEnt;
        }
    }
}
