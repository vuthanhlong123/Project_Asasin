using Game.Runtimes.NPC.Movement;
using Game.Runtimes.Sound;
using UnityEngine;

namespace Game.Runtimes.NPC
{
    public class NPCHandler : MonoBehaviour
    {
        private NPCAnimationController _animationController;
        private NPCSplineMovement _splineMovement;
        private SpeakerPoint _speaker;

        public NPCAnimationController AnimationController => _animationController;
        public NPCSplineMovement SplineMovement => _splineMovement;
        public SpeakerPoint Speaker => _speaker;

        private void Awake()
        {
            _animationController = GetComponent<NPCAnimationController>();
            _splineMovement = GetComponent<NPCSplineMovement>();
            _speaker = GetComponentInChildren<SpeakerPoint>();
        }
    }
}


