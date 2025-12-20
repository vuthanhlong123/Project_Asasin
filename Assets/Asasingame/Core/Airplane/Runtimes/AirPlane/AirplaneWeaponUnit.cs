using Asasingame.Core.Airplane.Runtimes.Data;
using UnityEngine;
using UnityEngine.Events;

namespace Asasingame.Core.Airplane.Runtimes
{
    public class AirplaneWeaponUnit : MonoBehaviour
    {
        [SerializeField] protected WeaponUnitData data;

        #region References
        protected AirplaneWeaponManager manager;
        #endregion

        protected bool isReady;

        public string ID => data.ID;
        public bool IsReady => isReady;
        public WeaponUnitData Data => data;

        protected int availableBullet;

        #region EVENTs
        public event UnityAction FireEvent;
        public void OnFireEvent() => FireEvent?.Invoke();

        public event UnityAction ReloadedEvent;
        public void OnReloadedEvent() => ReloadedEvent?.Invoke();
        #endregion

        protected virtual void Awake()
        {
            availableBullet = data.BulletAmount;
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

        public int GetAvailableBullet()
        {
            return availableBullet;
        }

        public void ClearListener()
        {
            FireEvent = null;
            ReloadedEvent = null;
        }
    }
}

