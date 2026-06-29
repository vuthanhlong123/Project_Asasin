using Game.Runtimes.Interaction;
using UnityEngine;

namespace Game.Runtimes.BehaviorScripts.HeavyStation
{
    public class SuitEquipPoint : MonoBehaviour
    {
        [SerializeField] private GameObject suitModel;

        private InteractionPoint _interactionPoint;

        private void Start()
        {
            _interactionPoint = GetComponent<InteractionPoint>();
        }

        public void Execute()
        {
            suitModel.SetActive(false);
            _interactionPoint.enabled = false;
        }
    }
}


