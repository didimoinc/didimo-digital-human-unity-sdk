using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Didimo.Speech
{
    /// <summary>
    /// Class that holds the data for a generated TTS phrase.
    /// This contains both the animation data and the audio clip.
    /// </summary>
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