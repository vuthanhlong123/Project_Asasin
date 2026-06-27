using Game.Runtimes.NPC.Movement;
using UnityEngine;

namespace Game.Runtimes.NPC
{
    public class NPCHandler : MonoBehaviour
    {
        private NPCAnimationController _animationController;
        private NPCSplineMovement _splineMovement;

        public NPCAnimationController AnimationController => _animationController;
        public NPCSplineMovement SplineMovement => _splineMovement;

        private void Awake()
        {
            _animationController = GetComponent<NPCAnimationController>();
            _splineMovement = GetComponent<NPCSplineMovement>();
        }
    }
}


