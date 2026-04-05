using UnityEngine;

namespace Game.Runtimes.SpaceJump
{
    public class SpaceJumpManager : MonoBehaviour
    {
        public static SpaceJumpManager instance;

        private void Awake()
        {
            instance = this;
        }

        public void RunSpaceJump(string targetScene)
        {

        }
    }
}


