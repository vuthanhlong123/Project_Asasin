using Game.Runtimes.Input;
using Game.Runtimes.UI;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Runtimes.Interaction
{
    public class InteractionPoint : MonoBehaviour
    {
        public enum TriggerInteractType
        {
            None =0,
            Auto,
            Press,
            Hold
        }

        [SerializeField] private TriggerInteractType type;
        [SerializeField] private float holdTime = 1;

        [SerializeField]
        private UnityEvent<GameObject> onInteract;

        private void OnTriggerEnter(Collider other)
        {
            Interactor interactor = other.GetComponent<Interactor>();
            if (interactor == null)
            {
                interactor = other.GetComponentInParent<Interactor>();
            }

            if (interactor == null) return;

            switch (type)
            {
                case TriggerInteractType.Auto:
                    HandleAutoInteract(interactor); break;
                case TriggerInteractType.Press:
                    HandlePressToInteract(interactor); break;
                case TriggerInteractType.Hold:
                    HandleHoldToInteract(interactor); break;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            UIManager.instance.HideFrame<UIInteractionPoint>();
        }

        private void HandleAutoInteract(Interactor interactor)
        {
            onInteract?.Invoke(interactor.gameObject);
        }

        private void HandlePressToInteract(Interactor interactor)
        {
            var ui = UIManager.instance.PushFrame<UIInteractionPoint>();
            ui.SetValue(InteractionKeyType.Press, GameInputManager.instance.GetInputKey("Interaction"), Vector3.zero, "", 1);
            ui.OnSubmited.AddListener(() =>
            {
                onInteract?.Invoke(interactor.gameObject);
            });
        }

        private void HandleHoldToInteract(Interactor interactor)
        {
            var ui = UIManager.instance.PushFrame<UIInteractionPoint>();
            ui.SetValue(InteractionKeyType.Hold, GameInputManager.instance.GetInputKey("Interaction"), Vector3.zero, "", 1);
            ui.OnSubmited.AddListener(() =>
            {
                onInteract?.Invoke(interactor.gameObject);
            });
        }
    }
}

