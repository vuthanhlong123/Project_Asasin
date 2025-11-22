using UnityEngine;

namespace Asasingame.Core.Airplane.Runtimes
{
    public class AirplaneCollider : MonoBehaviour
    {
        public bool collideSometing;

        [HideInInspector]
        public AirplaneController controller;

        private void OnTriggerEnter(Collider other)
        {
            /*//Collide someting bad
            if(other.gameObject.GetComponent<SimpleAirPlaneCollider>() == null && other.gameObject.GetComponent<LandingArea>() == null)
            {
                collideSometing = true;
            }*/
        }
    }
}