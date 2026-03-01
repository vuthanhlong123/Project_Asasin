using UnityEngine;
using System.Collections.Generic;

namespace PhoenixRealm.RagdollCreatorPro
{
    public class RagdollPoolData
    {
        public GameObject Prefab;
        public Queue<RagdollInstance> Pool = new Queue<RagdollInstance>();
        public int TotalCreated = 0;

        public RagdollPoolData(GameObject prefab)
        {
            Prefab = prefab;
        }

        public int TotalPooled => Pool.Count;
        public int TotalInUse => TotalCreated - TotalPooled;
    }
}
