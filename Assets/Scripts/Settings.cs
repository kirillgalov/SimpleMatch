using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleMatch
{
    [CreateAssetMenu]
    public class Settings : ScriptableObject
    {
        public int Width;
        public int Hight;
        
        public List<TilePrefab> TilePrefabs;
        
        
        [Serializable]
        public class TilePrefab
        {
            public string Id;
            public GameObject Prefab;
        }
    }
}
