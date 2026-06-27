using Game.Runtimes.NPC;
using Game.Runtimes.Sound;
using System;
using System.Collections;
using UnityEngine;

namespace Game.Runtimes.BehaviorScripts.HeavyStation
{
    public class HeavyStationScriptsManager : MonoBehaviour
    {
        [Header("NPC")]
        [SerializeField] private NPCHandler capton;
        [SerializeField] private NPCHandler female;
        [SerializeField] private NPCHandler male;

        [Header("Hologram")]
        [SerializeField] private Material hologramMaterial;
        [SerializeField] private GameObject hologramGO;

        [SerializeField] private WakeUpScriptProperty _wakeUpScriptProperty;
        [SerializeField] private WelComeScriptProperty _welComeScriptProperty;

        private bool isWakeUpScriptRuned = false;
        private bool isWelComeScriptRuned = false;

        private void Start()
        {
            RunWakeUpScript();
        }

        public void RunWakeUpScript()
        {
            if(isWakeUpScriptRuned) return;

            isWakeUpScriptRuned = true;

            _wakeUpScriptProperty.SpeakerPoint.PlayAudio(_wakeUpScriptProperty.captonSound);
        }

        public void RunWelComeScript()
        {
            if (!isWelComeScriptRuned)
            {
                isWelComeScriptRuned = true;
                HandleWelComeScript();
            }
        }

        private IEnumerator HandleWelComeScript()
        {
            capton.SplineMovement.Rotate(_welComeScriptProperty.captonEuler, 1.5f);
            yield return new WaitForSeconds(1.5f);

            capton.AnimationController.PlayMotion(_welComeScriptProperty.captonWelcomeClip, 0.1f);
        }

        [Serializable]
        public class WakeUpScriptProperty
        {
            public AudioClip captonSound;
            public SpeakerPoint SpeakerPoint;
        }

        [Serializable]
        public class WelComeScriptProperty
        {
            public Vector3 captonEuler;
            public AudioClip captonSound;
            public AnimationClip captonWelcomeClip;
        }
    }
}


