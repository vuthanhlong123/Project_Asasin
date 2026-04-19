using UnityEngine;
using UnityEngine.Events;

namespace Game.Runtimes.LunchSpaceShip
{
    public class LunchShipAnimatorController : MonoBehaviour
    {
        public event UnityAction AnimationFinishedEvent;

        //Call in animation 
        public void OnAnimationFinished()
        {
            AnimationFinishedEvent?.Invoke();
        }
    }
}


