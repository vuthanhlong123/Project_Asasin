using UnityEngine;
using UnityEngine.Events;

namespace Game.Runtimes.UI
{
    public class UIFrame : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;

        public UnityEvent OnShowed = new UnityEvent();
        public UnityEvent OnHiden = new UnityEvent();

        public Canvas Canvas { get { 
            if(canvas == null)
            {
                canvas = GetComponent<Canvas>();
            }
            return canvas; }
        }

        private void Awake()
        {
            if (canvas == null)
            {
                canvas = GetComponent<Canvas>();
            }
        }

        public void Show()
        {
            OnShow();
        }

        protected virtual void OnShow()
        {
            gameObject.SetActive(true);
            OnShowed?.Invoke();
        }

        public void Hide()
        {
            OnHide();
        }

        protected void OnHide()
        {
            gameObject.SetActive(false);
            OnHiden?.Invoke();
        }

    }
}


