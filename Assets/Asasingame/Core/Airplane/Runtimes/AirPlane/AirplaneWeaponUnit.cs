using UnityEngine;
using UnityEngine.InputSystem;

namespace Asasingame.Core.Airplane.Runtimes
{
    public class AirplaneWeaponUnit : MonoBehaviour
    {
        [SerializeField] protected string id;

        #region References
        protected AirplaneWeaponManager manager;
        #endregion

        protected bool isReady;

        public string ID => id;
        public bool IsReady => isReady;

        protected virtual void Awake()
        {
            
        }

        private void Start()
        {
            if(manager == null) 
                manager = GetComponentInParent<AirplaneWeaponManager>();
        }

        public virtual void Active()
        {
            if(manager == null)
            {
                manager = GetComponentInParent<AirplaneWeaponManager>();
            }

            this.enabled = true;
        }

        public virtual void DeActive()
        {
            this.enabled = false;
        }

        public void SetReady()
        {
            isReady = true;
        }
    }
}

