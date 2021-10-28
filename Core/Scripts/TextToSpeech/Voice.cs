using System.Collections.Generic;
using UnityEngine;

namespace Didimo
{
    public class Voice
    {
        public string  name;
        public string  language;
        public string  lang_tag;
        public string  gender;
        public Vector2 child_preset;
        public Vector2 elder_preset;

        private static readonly Voice[] voiceMap =
        {
            new Voice("Nicole", "English (Australian)", "en_AU", "F", new Vector2(69, -14), new Vector2(-8, 14)),
            new Voice("Russell", "English (Australian)", "en_AU", "M", new Vector2(69, -12), new Vector2(-8, 12)),
            new Voice("Amy", "English (British)", "en_GB", "F", new Vector2(51, -9), new Vector2(-14, 15)),
            new Voice("Brian", "English (British)", "en_GB", "M", new Vector2(51, -9), new Vector2(-28, 10)),
            new Voice("Emma", "English (British)", "en_GB", "F"),

            //new Voice( "Ivy", "English (US)", "en_US", "F", new Vector2(73,-13), new Vector2(-22,10)),
            //new Voice( "Joanna", "English (US)", "en_US", "F" , new Vector2(45,-14), new Vector2(-14,15)),
            new Voice("Kimberly", "English (US)", "en_US", "F", new Vector2(32, -14), new Vector2(-14, 15)),
            new Voice("Kendra", "English (US)", "en_US", "F", new Vector2(49, -16), new Vector2(-14, 15)),
            new Voice("Joey", "English (US)", "en_US", "M", new Vector2(38, -4), new Vector2(-28, 10)),
            new Voice("Justin", "English (US)", "en_US", "M"),
            new Voice("Matthew", "English (US)", "en_US", "M"),
            new Voice("Salli", "English (US)", "en_US", "F"),

            //new Voice( "Aditi", "English (Indian)", "en_IN", "F", new Vector2(76,-9), new Vector2(-5,15)),
            //new Voice( "Raveena", "English (Indian)", "en_IN", "F" ),

            //new Voice( "Geraint", "English (Welsh)", "en_GB_WLS", "M" , new Vector2(68,-21), new Vector2(-5,7)),

            new Voice("Mads", "Danish", "da_DK", "M", new Vector2(49, -8), new Vector2(-6, 7)),
            new Voice("Naja", "Danish", "da_DK", "F", new Vector2(49, -8), new Vector2(-9, 4)),
            new Voice("Lotte", "Dutch", "nl_NL", "F", new Vector2(42, -6), new Vector2(-14, 5)),
            new Voice("Ruben", "Dutch", "nl_NL", "M", new Vector2(42, -6), new Vector2(-9, 8)),
            new Voice("Celine", "French", "fr_FR", "F", new Vector2(33, -10), new Vector2(-10, 1)),
            new Voice("Mathieu", "French", "fr_FR", "M", new Vector2(33, -10), new Vector2(-7, 6)),

            //new Voice( "Chantal", "French (Canadian)", "fr_CA", "F", new Vector2(45,-6), new Vector2(-6,6)),

            new Voice("Hans", "German", "de_DE", "M", new Vector2(49, -10), new Vector2(-5, 8)),
            new Voice("Marlene", "German", "de_DE", "F", new Vector2(49, -10), new Vector2(-15, 5)),
            new Voice("Vicki", "German", "de_DE", "F"),
            new Voice("Dora", "Icelandic", "is_IS", "F", new Vector2(46, -10), new Vector2(-9, 5)),
            new Voice("Karl", "Icelandic", "is_IS", "M", new Vector2(46, -10), new Vector2(-7, 7)),
            new Voice("Carla", "Italian", "it_IT", "F", new Vector2(54, -10), new Vector2(-8, 4)),
            new Voice("Giorgio", "Italian", "it_IT", "M", new Vector2(52, -8), new Vector2(-5, 7)),
            new Voice("Mizuki", "Japanese", "ja_JP", "F", new Vector2(31, -10), new Vector2(-8, 2)),
            new Voice("Takumi", "Japanese", "ja_JP", "M", new Vector2(21, -6), new Vector2(-5, 7)),

            //new Voice( "Seoyeon", "Korean", "ko_KR", "F", new Vector2(28,-6), new Vector2(-10,3)),

            //new Voice( "Liv", "Norwegian", "nb_NO", "F" , new Vector2(84,-8), new Vector2(-10,4)),

            new Voice("Jacek", "Polish", "pl_PL", "M", new Vector2(81, -8), new Vector2(-5, 7)),
            new Voice("Jan", "Polish", "pl_PL", "M"),
            new Voice("Ewa", "Polish", "pl_PL", "F", new Vector2(91, -8), new Vector2(-10, 4)),
            new Voice("Maja", "Polish", "pl_PL", "F"),
            new Voice("Ricardo", "Portuguese (Brazilian)", "pt_BR", "M", new Vector2(51, -12), new Vector2(-5, 8)),
            new Voice("Vitoria", "Portuguese (Brazilian)", "pt_BR", "F", new Vector2(57, -12), new Vector2(-9, 6)),
            new Voice("Cristiano", "Portuguese (European)", "pt_PT", "M", new Vector2(53, -13), new Vector2(-6, 7)),
            new Voice("Ines", "Portuguese (European)", "pt_PT", "F", new Vector2(49, -17), new Vector2(-9, 6)),

            //new Voice( "Carmen", "Romanian", "ro_RO", "F" , new Vector2(44,-13), new Vector2(-9,6)),

            new Voice("Maxim", "Russian", "ru_RU", "M", new Vector2(54, -8), new Vector2(-5, 8)),
            new Voice("Tatyana", "Russian", "ru_RU", "F", new Vector2(60, -8), new Vector2(-7, 4)),
            new Voice("Conchita", "Spanish (Castilian)", "es_ES", "F", new Vector2(84, -10), new Vector2(-8, 4)),
            new Voice("Enrique", "Spanish (Castilian)", "es_ES", "M", new Vector2(52, -6), new Vector2(-5, 11)),
            new Voice("Miguel", "Spanish (Latin American)", "es_US", "M", new Vector2(57, -4), new Vector2(-5, 9)),
            new Voice("Penelope", "Spanish (Latin American)", "es_US", "F", new Vector2(99, -8), new Vector2(-5, 4))

            //new Voice( "Astrid", "Swedish", "sv_SE", "F" , new Vector2(68,-10), new Vector2(-10,6)),
            //new Voice( "Filiz", "Turkish", "tr_TR", "F" , new Vector2(49,-8), new Vector2(-20,8)),
            //new Voice( "Gwyneth", "Welsh", "cy_GB", "F" , new Vector2(48,-10), new Vector2(-8,6))
        };

        public Voice(string name, string language, string lang_tag, string gender)
        {
            this.name = name;
            this.language = language;
            this.lang_tag = lang_tag;
            this.gender = gender;
            child_preset = new Vector2(35, -6);
            elder_preset = new Vector2(-5, 2);
        }

        public Voice(string name, string language, string lang_tag, string gender, Vector2 child_preset, Vector2 elder_preset)
        {
            this.name = name;
            this.language = language;
            this.lang_tag = lang_tag;
            this.gender = gender;
            this.child_preset = child_preset;
            this.elder_preset = elder_preset;
        }

        public static List<string> GetLanguageList(string gender)
        {
            List<string> m_DropOptions = new List<string>();
            foreach (Voice v in voiceMap)
            {
                if (string.Compare(gender, v.gender) == 0 && !m_DropOptions.Contains(v.language))
                    m_DropOptions.Add(v.language);
            }

            return m_DropOptions;
        }

        public static List<string> GetVoiceList(string gender, string lang)
        {
            List<string> m_DropOptions = new List<string>();
            foreach (Voice v in voiceMap)
            {
                if (string.Compare(lang, v.language) == 0 && string.Compare(gender, v.gender) == 0)
                {
                    m_DropOptions.Add(v.name);
                }
            }

            return m_DropOptions;
        }

        public static string GetLanguageLabelFromLocale(string locale)
        {
            foreach (Voice v in voiceMap)
            {
                if (string.Compare(locale, v.lang_tag) == 0)
                    return v.language;
            }

            return null;
        }

        public static string GetLocaleFromLanguageLabel(string langLabel)
        {
            foreach (Voice v in voiceMap)
            {
                if (string.Compare(langLabel, v.language) == 0)
                    return v.lang_tag;
            }

            return null;
        }

        public static Vector2 GetVoiceChildPreset(string gender, string lang)
        {
            foreach (Voice v in voiceMap)
            {
                if (string.Compare(lang, v.language) == 0 && string.Compare(gender, v.gender) == 0)
                    return v.child_preset;
            }

            return new Vector2(35, -6); //default
        }

        public static Vector2 GetVoiceElderPreset(string gender, string lang)
        {
            foreach (Voice v in voiceMap)
            {
                if (string.Compare(lang, v.language) == 0 && string.Compare(gender, v.gender) == 0)
                    return v.elder_preset;
            }

            return new Vector2(-5, 2); //default
        }
    }
}