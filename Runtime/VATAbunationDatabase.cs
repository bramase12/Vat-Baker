using System.Collections.Generic;
using UnityEngine;

namespace VATSystem
{
    [CreateAssetMenu(fileName = "VATAnimationDatabase", menuName = "VAT/Animation Database")]
    public class VATAnimationDatabase : ScriptableObject
    {
        public List<VATAnimationReference> animations = new List<VATAnimationReference>();

        public GameObject GetAnimationPrefab(string name)
        {
            var reference = animations.Find(a => a.animationName == name);
            return reference?.animationPrefab;
        }

        public bool Contains(string name) => GetAnimationPrefab(name) != null;

        public void AddOrUpdateAnimation(string name, GameObject prefab)
        {
            var existing = animations.Find(a => a.animationName == name);
            if (existing != null)
                existing.animationPrefab = prefab;
            else
                animations.Add(new VATAnimationReference { animationName = name, animationPrefab = prefab });
        }
    }
}