using Game.Runtimes.Managers;
using System.Linq;
using UnityEngine;

namespace Game.Runtimes.SpaceJump
{
    public class SpaceJumpManager : MonoBehaviour
    {
        public static SpaceJumpManager instance;

        [SerializeField] private SpaceJumpGate[] gates;

        private void Awake()
        {
            instance = this;
        }

        public SpaceJumpGate FindConnectGate(string sceneTarget)
        {
            return gates.FirstOrDefault<SpaceJumpGate>(a => a.TargetScene == sceneTarget);
        }

        public SpaceJumpGate GetConnectGate()
        {
            string sceneTarget = GameSceneManager.instance.LastAdditiveSceneName;
            return gates.FirstOrDefault<SpaceJumpGate>(a => a.TargetScene == sceneTarget);
        }
    }
}


