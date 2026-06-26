using UnityEngine;

namespace Game.Runtimes.NPC.Movement
{
    public class NPCAnimationController : MonoBehaviour
    {
        int forward = Animator.StringToHash("forward");

        private INPCMovement mover;
        private Animator _animator;

        private void Awake()
        {
            mover = GetComponent<INPCMovement>();
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            _animator.SetFloat(forward, mover.Forward());
        }
    }

}

