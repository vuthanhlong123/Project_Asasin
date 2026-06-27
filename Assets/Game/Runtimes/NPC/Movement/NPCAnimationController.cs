using UnityEngine;

namespace Game.Runtimes.NPC.Movement
{
    public class NPCAnimationController : MonoBehaviour
    {
        int forward = Animator.StringToHash("forward");

        private INPCMovement mover;
        private Animator _animator;

        private AnimatorOverrideController overrideController;

        private void Awake()
        {
            mover = GetComponent<INPCMovement>();
            _animator = GetComponent<Animator>();

            overrideController = _animator.runtimeAnimatorController as AnimatorOverrideController;
        }

        private void Update()
        {
            _animator.SetFloat(forward, mover.Forward());
        }

        public void PlayMotion(AnimationClip clip, float transitionDurartion)
        {
            overrideController["Motion"] = clip;

            _animator.CrossFade("Motion", transitionDurartion, 0, 0);
        }
    }
}

