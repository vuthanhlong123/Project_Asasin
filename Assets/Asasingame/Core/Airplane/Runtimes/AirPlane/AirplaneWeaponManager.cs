using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Asasingame.Core.Airplane.Runtimes
{
    public class AirplaneWeaponManager : MonoBehaviour
    {
        [Serializable] 
        public class WeaponInputBinding
        {
            public string inputName;
            public string weaponUnitID;
        }

        [Header("References")]
        [SerializeField] private AirplaneController airplaneController;

        [Header("Input")]
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private WeaponInputBinding[] weaponInputBindings;

        [Header("Members")]
        [SerializeField] private AirplaneWeaponUnit[] weaponUnits;

        private AirplaneWeaponUnit currentReadyWeaponUnit;

        public PlayerInput PlayerInput => playerInput;
        public AirplaneController AirplaneController => airplaneController;

        private void Start()
        {
            RegisterInput();
            DeactiveAllWeaponUnit();
        }

        private void RegisterInput()
        {
            foreach (var inputBinding in weaponInputBindings)
            {
                playerInput.actions[inputBinding.inputName].performed += AirplaneWeaponManager_performed;
            }
        }

        private void DeactiveAllWeaponUnit()
        {
            foreach (var unit in weaponUnits)
            {
               unit.DeActive();
            }
        }

        private void AirplaneWeaponManager_performed(InputAction.CallbackContext obj)
        {
            AirplaneWeaponUnit weaponUnit = GetReadyWeaponUnit(GetWeaponUnitIDByInput(obj.action.name));

            if (weaponUnit == null) return;

            if(currentReadyWeaponUnit != null)
            {
                currentReadyWeaponUnit.DeActive();
            }

            currentReadyWeaponUnit = weaponUnit;
            weaponUnit.Active();
        }

        public string GetWeaponUnitIDByInput(string inputName)
        {
            foreach(var inputBinding in weaponInputBindings)
            {
                if (inputBinding.inputName == inputName)
                {
                    return inputBinding.weaponUnitID;
                }
            }

            return string.Empty;
        }

        public AirplaneWeaponUnit GetReadyWeaponUnit(string weaponUnitID)
        {
            foreach (var unit in weaponUnits)
            {
                if(unit.ID == weaponUnitID && unit.IsReady)
                {
                    return unit;
                }
            }

            return null;
        }
    }
}


