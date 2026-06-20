using Game.Runtimes.Input;
using Game.Runtimes.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game.Runtimes.Interaction
{
    public enum InteractionKeyType
    {
        Press,
        Hold
    }

    public class UIInteractionPoint : UIFrame
    {
        [Space(10)]
        [SerializeField] private InteractionKeyType type;
        [SerializeField] private TextMeshProUGUI text_Key;
        [SerializeField] private TextMeshProUGUI text_Content;
        [SerializeField] private Image image_Fill;
        [SerializeField] private RectTransform containerRect;

        [Space(5)]
        public UnityEvent OnSubmited;

        private GameInputManager inputManager;
        private float holdTime;
        private float currentHoldedTime;
        private Vector3 interacPosition;
        private Camera mainCamera;

        public void SetValue(InteractionKeyType type, string key, Vector3 interactPosition, string content = "", float holdTime = 1)
        {
            this.type = type;
            text_Key.text = key;
            text_Content.text = content != "" ? content : "Interact";
            this.holdTime = holdTime;
            this.interacPosition = interactPosition;
        }

        protected override void OnShow()
        {
            Init();
            base.OnShow();
        }

        private void Init()
        {
            mainCamera = Camera.main;
            UpdatePosition();

            OnSubmited.RemoveAllListeners();
            inputManager = GameInputManager.instance;
            currentHoldedTime = 0;
            image_Fill.fillAmount = currentHoldedTime;
        }

        private void Update()
        {
            UpdatePosition();

            switch (type)
            {
                case InteractionKeyType.Press:
                    HandlePress(); break;
                case InteractionKeyType.Hold:
                    HandleHold(); break;
            }
        }

        private void UpdatePosition()
        {
            //containerRect.position = mainCamera.WorldToScreenPoint(interacPosition);
        }

        private void HandlePress()
        {
            if(inputManager.GetInputAxis("Interaction")>0)
            {
                OnSubmited?.Invoke();
                Hide();
            }
        }

        private void HandleHold()
        {
            if (inputManager.GetInputAxis("Interaction") > 0)
            {
                currentHoldedTime += Time.deltaTime;

                if(currentHoldedTime >= holdTime)
                {
                    OnSubmited?.Invoke();
                    Hide();
                }
            }
            else
            {
                currentHoldedTime = 0;
            }

            image_Fill.fillAmount = currentHoldedTime / holdTime;
        }
    }
}


