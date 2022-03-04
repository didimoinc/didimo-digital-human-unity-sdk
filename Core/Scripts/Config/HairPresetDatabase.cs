using System.Collections.Generic;
using UnityEngine;
using Didimo.Core.Deformables;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Didimo.Core.Config
{
    [System.Serializable]
    public class HairLayerSettings
    {
        [SerializeField]
        public Color color;

        [SerializeField]
        [Range(0.0f, 1000.0f)]
        public float glossiness1 = 10.5f;

        [SerializeField]
        [Range(0.0f, 1000.0f)]
        public float glossiness2 = 20.5f;

        //[Range(0f, 5f)]
        [SerializeField]
        [Range(0.0f, 100.0f)]
        public float shineMultiplier = 1.0f;

        [SerializeField]
        [Range(-1.0f, 1.0f)]
        public float specShift1 = 0.01f;

        [SerializeField]
        [Range(-1.0f, 1.0f)]
        public float specShift2 = 0.02f;

        [SerializeField]
        [Range(-2.0f, 2.0f)]
        public float flowMultiply = 1.0f;

        [SerializeField]
        [Range(-0.1f, 0.1f)]
        public float hairScaleNudge = 0.0f;


        public HairLayerSettings(HairLayerSettings other)
        {
            color = new Color(other.color.r, other.color.g, other.color.b);
            glossiness1 = other.glossiness1;
            glossiness2 = other.glossiness2;
            shineMultiplier = other.shineMultiplier;
            specShift1 = other.specShift1;
            specShift2 = other.specShift2;
            flowMultiply = other.flowMultiply;
            hairScaleNudge = other.hairScaleNudge;
        }

        public HairLayerSettings Clone()
        {
            return new HairLayerSettings(this);
        }

        public HairLayerSettings Clone(Color color)
        {
            var retval = new HairLayerSettings(this);
            retval.color = color;
            return retval;
        }

        public void SetValues(HairLayerSettings other)
        {
            color = new Color(other.color.r, other.color.g, other.color.b);
            glossiness1 = other.glossiness1;
            glossiness2 = other.glossiness2;
            shineMultiplier = other.shineMultiplier;
            specShift1 = other.specShift1;
            specShift2 = other.specShift2;
            flowMultiply = other.flowMultiply;
            hairScaleNudge = other.hairScaleNudge;
        }

        public void SetNonColourValues(HairLayerSettings other)
        {           
            glossiness1 = other.glossiness1;
            glossiness2 = other.glossiness2;
            shineMultiplier = other.shineMultiplier;
            specShift1 = other.specShift1;
            specShift2 = other.specShift2;
            flowMultiply = other.flowMultiply;
            hairScaleNudge = other.hairScaleNudge;
        }
        

        private static readonly char[] HairNameSplits = { '_', ' ' };
        public static string ExtractValidHairName(string testName)
        {
            testName = testName.ToLower();
            if (testName.StartsWith("hair"))
            {
                var subnames = testName.Split(HairNameSplits);
                if (subnames.Length >= 2)
                {
                    return subnames[0] + "_" + subnames[1];
                }
            }
            return null;
        }

        public static string GetHairIDFromObject(Hair hair)
        {
            var meshes = hair.GetComponentsInChildren<MeshFilter>();
            var hairpiece_name = ExtractValidHairName(hair.gameObject.name);

            foreach (var m in meshes)
            {

                var name = ExtractValidHairName(m.sharedMesh.name.ToLower());
                if (name != null)
                    return name;

            }
            var meshRenderers = hair.GetComponentsInChildren<MeshRenderer>();
            foreach (var m in meshRenderers)
            {
                foreach (var mat in m.sharedMaterials)
                {
                    var name = ExtractValidHairName(mat.name.ToLower());
                    if (name != null)
                        return name;
                }
            }
            return hairpiece_name;
        }
    }

    [System.Serializable]
    public enum HairLayer
    {
        Inner,
        Outer,
        Fringe,
        Strands,
        PonyTail
    }

    [System.Serializable]
    public class HairLayerDatabaseEntry :  HairLayerSettings
    {
        [SerializeField]
        public string key;
        [SerializeField]
        public HairLayer hairLayer;

        public HairLayerDatabaseEntry(HairLayerSettings value, string _key, HairLayer _hairLayer) : base(value)
        {
            key = _key;
            hairLayer = _hairLayer;
        }
    }


    [System.Serializable]
    public class HairLayerDatabaseGroupEntry
    {
        [SerializeField]
        public string name;
        [SerializeField]
        public List<HairLayerDatabaseEntry> list;

        public bool HasMeshSpecificEntry(string key)
        {
            for (var i = 0; i < list.Count; ++i)
            {
                if (list[i].key == key)
                    return true;
            }
            return false;
            
        }

        public void RemoveEntry(string key, HairLayer layer)
        {
            var entry = FindEntry(key, layer);
            if (entry != null)
            {
                list.Remove(entry);
            }
        }

        public HairLayerDatabaseEntry FindOrAddentry(string key, HairLayer layer, HairLayerSettings template)
        {
            var hldbe = FindEntry(key, layer);
            if (hldbe != null)
            {
                hldbe.SetNonColourValues(template);
                return hldbe;
            }
            else
            {
                hldbe = new HairLayerDatabaseEntry(template, key, layer);
                list.Add(hldbe);
            }
            return hldbe;
        }


        public HairLayerDatabaseEntry FindOrAddentryUseColourDefault(string key, HairLayer layer, HairLayerSettings template)
        {
            var hldbe = FindEntry(key, layer);
            var defaultEntry = FindEntry("", layer);
            if (hldbe != null)
            {
                hldbe.SetNonColourValues(template);
                return hldbe;
            }
            else
            {
                hldbe = new HairLayerDatabaseEntry(template, key, layer);
                hldbe.color = defaultEntry.color;
                list.Add(hldbe);
            }
            return hldbe;
        }

        public HairLayerDatabaseEntry FindEntry(string key, HairLayer layer)
        {
            foreach (HairLayerDatabaseEntry hldbe in list)
            {
                if ((hldbe.key == key) && (hldbe.hairLayer == layer))
                {
                    return hldbe;
                }
            }
            return null;
        }

        public void SetEntry(HairLayerSettings settings, string key, HairLayer layer)
        {
            HairLayerDatabaseEntry hlde = FindEntry(key, layer);
            if (hlde != null)
            {
                hlde.SetValues(settings);
            }
            else
            {
                HairLayerDatabaseEntry dbi = new HairLayerDatabaseEntry(settings, key, layer);
                list.Add(dbi);
            }
        }

        public void Apply(Hair hairObject)
        {
            if (list.Count > 0)
            {
                var hairpieceName = HairLayerSettings.GetHairIDFromObject(hairObject);
                HairLayerDatabaseEntry bestOuterMatch = null;
                HairLayerDatabaseEntry bestInnerMatch = null;
                foreach (HairLayerDatabaseEntry hldbe in list)
                {
                    if (hldbe.key == "")
                    {
                        if ((bestOuterMatch == null) && (hldbe.hairLayer == HairLayer.Outer))
                            bestOuterMatch = hldbe;
                        if ((bestInnerMatch == null) && (hldbe.hairLayer == HairLayer.Inner))
                            bestInnerMatch = hldbe;
                    }
                    else if (hldbe.key.StartsWith(hairpieceName))
                    {
                        if (hldbe.hairLayer == HairLayer.Outer)
                            bestOuterMatch = hldbe;
                        else if (hldbe.hairLayer == HairLayer.Inner)
                            bestInnerMatch = hldbe;
                    }
                }
                if (bestInnerMatch != null)
                    hairObject.innerHairLayer.SetValues(bestInnerMatch);
                if (bestOuterMatch != null)
                    hairObject.outerHairLayer.SetValues(bestOuterMatch);
            }
        }
    }

    [CreateAssetMenu(fileName = "HairPresetDatabase", menuName = "Didimo/Hair Preset Database")]
    public class HairPresetDatabase : ScriptableObject
    {
        public HairLayerDatabaseGroupEntry[] Hairs;
        public void RemoveEntriesReferringTo(string Key)
        {

            for (var i = 0; i < Hairs.Length; ++i)
            {
                var hdb = Hairs[i];

                hdb.RemoveEntry(Key, HairLayer.Outer);
                hdb.RemoveEntry(Key, HairLayer.Inner);

            }
            UpdateDatabase();
        }

        public void UpdateDatabase()
        {
#if UNITY_EDITOR
            var so = new SerializedObject(this);
            so.Update();
            so.ApplyModifiedProperties();
            so.UpdateIfRequiredOrScript();
#endif

        }

    }
}
