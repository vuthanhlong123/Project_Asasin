using System;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Runtimes.Events
{
    public class GameObjectActiveEvent : MonoBehaviour
    {
        [SerializeField] private float eventDelayTime;

        [Serializable]
        public class GameObjectActivateEvent : UnityEvent { }

        [SerializeField] private GameObjectActivateEvent OnActivate = new GameObjectActivateEvent();

        private void OnEnable()
        {
            Invoke(nameof(SendEvent), eventDelayTime);
        }

        private void SendEvent()
        {
            OnActivate?.Invoke();
        }
    }
}


