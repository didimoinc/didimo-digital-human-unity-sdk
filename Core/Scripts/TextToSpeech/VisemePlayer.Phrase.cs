using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Didimo.Speech
{
    public class Phrase
    {
        public List<TTSElement> Elements;
        public AudioClip        Audio;

        public IReadOnlyList<VisemeElement> Visemes { get; }

        public static Phrase Empty => new Phrase(null, null);
        public bool IsValid => Elements != null && Elements.Count > 0 && Audio != null;

        public Phrase(List<TTSElement> elements, AudioClip audio)
        {
            Elements = elements;
            Audio = audio;

            Visemes = Elements.Where(e => e is VisemeElement viseme).Cast<VisemeElement>().ToList();
        }
    }
}