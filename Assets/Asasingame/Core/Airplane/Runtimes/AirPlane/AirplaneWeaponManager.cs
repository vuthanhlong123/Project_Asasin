using Asasingame.Core.Airplane.Runtimes.UIs;
using NUnit.Framework;
using System;
using System.Collections.Generic;
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
            public string inputSymbol;
            public string weaponUnitID;
        }

        [Header("References")]
        [SerializeField] private AirplaneController airplaneController;
        [SerializeField] private AirplaneCamera airplaneCamera;
        [SerializeField] private UIWeaponManager weaponManager;

        [Header("Input")]
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private WeaponInputBinding[] weaponInputBindings;

        [Header("Members")]
        [SerializeField] private AirplaneWeaponUnit[] weaponUnits;

        private AirplaneWeaponUnit currentReadyWeaponUnit;

        public PlayerInput PlayerInput => playerInput;
        public AirplaneController AirplaneController => airplaneController;
        public AirplaneCamera AirplaneCamera => airplaneCamera;

        private void Start()
        {
            RegisterInput();
            InitializeWeaponUnit();
            DrawUI();
        }

        private void RegisterInput()
        {
            foreach (var inputBinding in weaponInputBindings)
            {
                playerInput.actions[inputBinding.inputName].performed += AirplaneWeaponManager_performed;
            }
        }

        private void InitializeWeaponUnit()
        {
            foreach (var unit in weaponUnits)
            {
                unit.FireEvent += WeaponUnit_FireEvent;
                unit.ReloadedEvent += WeaponUnit_ReloadedEvent;

                unit.DeActive();
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
            DrawActiveWeaponUI();
        }

        private void WeaponUnit_ReloadedEvent()
        {
            UpdateActiveWeaponUI();
            ChangeToOtherReadyWeaponUnit(currentReadyWeaponUnit.ID);
        }

        private void WeaponUnit_FireEvent()
        {
            UpdateActiveWeaponUI();
        }

        public void ChangeToOtherReadyWeaponUnit(string id)
        {
            AirplaneWeaponUnit weaponUnit = GetReadyWeaponUnit(id);

            if (weaponUnit == null) return;

            if (currentReadyWeaponUnit != null)
            {
                currentReadyWeaponUnit.DeActive();
            }

            currentReadyWeaponUnit = weaponUnit;
            weaponUnit.Active();
            DrawActiveWeaponUI();
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

        public int GetTotalBulletOfCurrentWeapon()
        {
            int totaBlBullet = 0;
            foreach (var unit in weaponUnits)
            {
                if (unit.ID == currentReadyWeaponUnit.ID)
                {
                    totaBlBullet += unit.GetAvailableBullet();
                }
            }
            return totaBlBullet;
        }

        public void DrawUI()
        {
            List<UIWeaponDrawData> drawDatas = new List<UIWeaponDrawData>();
            foreach(WeaponInputBinding binding in weaponInputBindings)
            {
                AirplaneWeaponUnit weaponUnit = GetReadyWeaponUnit(binding.weaponUnitID);
                if(weaponUnit == null) continue;

                UIWeaponDrawData newData = new UIWeaponDrawData();
                newData.icon = weaponUnit.Data.Icon;
                newData.symbolKey = binding.inputSymbol;
                drawDatas.Add(newData);
            }

            weaponManager.Initialize(drawDatas);
        }

        public void DrawActiveWeaponUI()
        {
            weaponManager.ShowWeaponActivate(currentReadyWeaponUnit.Data.Icon, GetTotalBulletOfCurrentWeapon(), currentReadyWeaponUnit.Data.BulletMode);
        }

        public void UpdateActiveWeaponUI()
        {
            weaponManager.UpdateWeaponBullet(GetTotalBulletOfCurrentWeapon(), currentReadyWeaponUnit.Data.BulletMode);
        }
    }
}


