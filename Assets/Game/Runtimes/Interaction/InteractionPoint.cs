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
            Click,
            Hold
        }

        [SerializeField] private TriggerInteractType type;

        [Serializable]
        public class OnInteractEvent : UnityEvent<Interactor> { }

        [SerializeField]
        private OnInteractEvent onInteract = new OnInteractEvent();

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
                case TriggerInteractType.Click:
                    HandleAutoInteract(interactor); break;
                case TriggerInteractType.Hold:
                    HandleAutoInteract(interactor); break;
            }
           
        }

        private void HandleAutoInteract(Interactor interactor)
        {
            Debug.Log("auto interact sended");
            onInteract?.Invoke(interactor);
        }
    }
}

