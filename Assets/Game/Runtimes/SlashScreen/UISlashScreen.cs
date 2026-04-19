using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game.Runtimes.SlashScreen
{
    public class UISlashScreen : MonoBehaviour
    {
        [SerializeField] private float fadeDuration;
        [SerializeField] private float midDuration;
        [SerializeField] private Image image_Graphic;

        [Serializable]
        public class SlashScreenEvent : UnityEvent { }

        [SerializeField] private SlashScreenEvent OnFadedIn = new SlashScreenEvent();
        [SerializeField] private SlashScreenEvent OnMidTimeEnded = new SlashScreenEvent();
        [SerializeField] private SlashScreenEvent OnFadedOut = new SlashScreenEvent();

        public event UnityAction FadedInEvent;
        public event UnityAction MidTimeEndedEvent;
        public event UnityAction FadedOutEvent;

        private void Start()
        {
            image_Graphic.enabled = false;
        }

        public void Run()
        {
            if (image_Graphic == null) return;
            image_Graphic.enabled = true;

            FadeIn(() =>
            {
                OnFadedIn?.Invoke();
                FadedInEvent?.Invoke();
            StartCoroutine(MidTime(() =>
                {
                    OnMidTimeEnded?.Invoke();
                    MidTimeEndedEvent?.Invoke();
                FadeOut(() => { 
                        image_Graphic.enabled = false;
                        OnFadedOut?.Invoke();
                        FadedOutEvent?.Invoke();
                    });
                }));
            });
        }

        private IEnumerator MidTime(Action onComplete = null)
        {
            yield return new WaitForSeconds(midDuration);

            onComplete?.Invoke();
        }    

        private void FadeIn(Action onComplete = null)
        {
            StartCoroutine(Fade(0,1, onComplete));
        }

        private void FadeOut(Action onComplete = null)
        {
            StartCoroutine(Fade(1, 0, onComplete));
        }

        private IEnumerator Fade(float start, float end, Action onComplete = null)
        {
            float startTime = Time.time;
            Color color = Color.white;
            while (Time.time - startTime <= fadeDuration)
            {
                color = image_Graphic.color;
                color.a = Mathf.Lerp(start, end, (Time.time - startTime) /fadeDuration);
                image_Graphic.color = color;

                yield return null;
            }

            color = image_Graphic.color;
            color.a = end;
            image_Graphic.color = color;
            onComplete?.Invoke();
        }
    }
}


