using Game.Runtimes.NPC;
using Game.Runtimes.Sound;
using System;
using System.Collections;
using UnityEngine;
using static Game.Runtimes.BehaviorScripts.HeavyStation.HeavyStationScriptsManager;

namespace Game.Runtimes.BehaviorScripts.HeavyStation
{
    public class HeavyStationScriptsManager : MonoBehaviour
    {
        [Header("NPC")]
        [SerializeField] private NPCHandler capton;
        [SerializeField] private NPCHandler female;
        [SerializeField] private NPCHandler male;

        [SerializeField] private WakeUpScriptProperty _wakeUpScriptProperty;
        [SerializeField] private WelComeScriptProperty _welComeScriptProperty;
        [SerializeField] private ListenMessageScriptProperty _listenMessageScriptProperty;
        [SerializeField] private GoToSpaceShipScriptProperty _gotoSpaceshipScriptProperty;

        private bool isWakeUpScriptRuned = false;
        private bool isWelComeScriptRuned = false;
        private bool isListenMessageScriptRuned = false;
        private bool isGoToSpaceshipScriptRuned = false;

        private void Start()
        {
            RunWakeUpScript();
            _listenMessageScriptProperty.Init();
        }

        #region WakeUp
        public void RunWakeUpScript()
        {
            if(isWakeUpScriptRuned) return;

            isWakeUpScriptRuned = true;

            _wakeUpScriptProperty.SpeakerPoint.PlayAudio(_wakeUpScriptProperty.captonSound);
        }
        #endregion

        #region Welcome
        public void RunWelComeScript()
        {
            if (!isWelComeScriptRuned)
            {
                isWelComeScriptRuned = true;
                StartCoroutine(HandleWelComeScript(onComplete: () =>
                {
                    RunListenMessageScript();
                }));
            }
        }

        private IEnumerator HandleWelComeScript(Action onComplete)
        {
            Vector3 captonStartEuler = capton.transform.eulerAngles;

            capton.SplineMovement.Rotate(_welComeScriptProperty.captonEuler, 2f);
            yield return new WaitForSeconds(1.5f);

            capton.AnimationController.PlayMotion(_welComeScriptProperty.captonWelcomeClip, 0.1f);
            capton.Speaker.PlayAudio(_welComeScriptProperty.captonSound);

            yield return new WaitForSeconds(5f);
            capton.SplineMovement.Rotate(captonStartEuler, 2);

            yield return new WaitForSeconds(1.5f);

            onComplete?.Invoke();
        }
        #endregion

        #region ListenMessage
        public void RunListenMessageScript()
        {
            if (isListenMessageScriptRuned) return;
            isListenMessageScriptRuned = true;

            StartCoroutine(ListenMessageScriptHandle(onComplete: () =>
            {
                RunGoToSpaceshipScript();
            }));
        }

        private IEnumerator ListenMessageScriptHandle(Action onComplete)
        {
            yield return new WaitForSeconds(1);

            capton.AnimationController.PlayMotion(_listenMessageScriptProperty.clickButtonAnim, 0.1f);

            yield return new WaitForSeconds(2f);

            _listenMessageScriptProperty.planetGO.SetActive(false);
            yield return new WaitForSeconds(0.5f);

            _listenMessageScriptProperty.hologramGO.SetActive(true);

            float hologramFadeInDuration = 1;
            Color mainColor = _listenMessageScriptProperty.hologramMaterial.GetColor("_Hologram_Color");
            Color texColor = _listenMessageScriptProperty.hologramMaterial.GetColor("_Texture_Tint_Color");
            
            while (hologramFadeInDuration > 0)
            {
                yield return null;
                hologramFadeInDuration -= Time.deltaTime;
                mainColor.a = texColor.a = Mathf.Lerp(1, 0, hologramFadeInDuration);
                _listenMessageScriptProperty.hologramMaterial.SetColor("_Hologram_Color", mainColor);
                _listenMessageScriptProperty.hologramMaterial.SetColor("_Texture_Tint_Color", texColor);
            }

            yield return new WaitForSeconds(1);

            _listenMessageScriptProperty.hologramSpeaker.PlayAudio(_listenMessageScriptProperty.messageAudioClip);

            yield return new WaitForSeconds(25);

            hologramFadeInDuration = 1;
            mainColor = _listenMessageScriptProperty.hologramMaterial.GetColor("_Hologram_Color");
            texColor = _listenMessageScriptProperty.hologramMaterial.GetColor("_Texture_Tint_Color");

            while (hologramFadeInDuration > 0)
            {
                yield return null;
                hologramFadeInDuration -= Time.deltaTime;
                mainColor.a = texColor.a = Mathf.Lerp(0, 1, hologramFadeInDuration);
                _listenMessageScriptProperty.hologramMaterial.SetColor("_Hologram_Color", mainColor);
                _listenMessageScriptProperty.hologramMaterial.SetColor("_Texture_Tint_Color", texColor);
            }

            _listenMessageScriptProperty.hologramGO.SetActive(false);

            onComplete?.Invoke();
        }
        #endregion

        #region GoToSpaceship
        public void RunGoToSpaceshipScript()
        {
            if (isGoToSpaceshipScriptRuned) return;
            isGoToSpaceshipScriptRuned = true;

            StartCoroutine(HandleGoToSpaceshipScript());
        }

        private IEnumerator HandleGoToSpaceshipScript(Action onComplete = null)
        {
            yield return new WaitForSeconds(1f);
            capton.SplineMovement.EnableMovement();
            capton.Speaker.PlayAudio(_gotoSpaceshipScriptProperty.captonTalkSound);

            yield return new WaitForSeconds(3f);

            female.SplineMovement.EnableMovement();
            male.SplineMovement.EnableMovement();

        }
        #endregion

        public void HideNPC()
        {
            capton.gameObject.SetActive(false);
            female.gameObject.SetActive(false);
            male.gameObject.SetActive(false);
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

        [Serializable]
        public class ListenMessageScriptProperty
        {
            public Material hologramMaterial;
            public GameObject hologramGO;
            public GameObject planetGO;
            public AnimationClip clickButtonAnim;
            public SpeakerPoint hologramSpeaker;
            public AudioClip messageAudioClip;

            public void Init()
            {
                Color mainColor = hologramMaterial.GetColor("_Hologram_Color");
                Color texColor = hologramMaterial.GetColor("_Texture_Tint_Color");

                mainColor.a = 0;
                texColor.a = 0;

                hologramMaterial.SetColor("_Hologram_Color", mainColor);
                hologramMaterial.SetColor("_Texture_Tint_Color", texColor);
            }
        }

        [Serializable]
        public class GoToSpaceShipScriptProperty
        {
            public AudioClip captonTalkSound;
        }
    }
}


