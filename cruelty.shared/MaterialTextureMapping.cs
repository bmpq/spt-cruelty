using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace tarkin.cruelty.world
{
    public class MaterialTextureMapping : SerializedScriptableObject
    {
        [SerializeField] Dictionary<Texture2D, string[]> mapping;

        [SerializeField] Texture2D defaultTexture;

        [SerializeField] List<Texture2D> allTextures;

        public Texture GetMainTexture(Material mat)
        {
            foreach (var kvp in mapping)
            {
                foreach (var matName in kvp.Value)
                {
                    if (mat.name.Contains(matName))
                        return kvp.Key;
                }
            }

            return Random(allTextures);
        }

        static T Random<T>(IEnumerable<T> source)
        {
            T[] array = (source as T[]) ?? source.ToArray();
            return array[UnityEngine.Random.Range(0, array.Length)];
        }
    }
}
