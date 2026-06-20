using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Runtimes.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager instance;

        [SerializeField] private UIFrame[] frames;

        private List<UIFrame> createdUIFrameList = new List<UIFrame>();

        private void Awake()
        {
            instance = this;
        }

        public T PushFrame<T>(bool forceToBottom = false, Action<T> onBeforeShow = null) where T : class
        {
            var frame = GetFrameShowed<T>() as UIFrame;
            if (frame != null)
            {
                if(forceToBottom)
                {
                    createdUIFrameList.Remove(frame);
                    createdUIFrameList.Insert(0, frame);
                }
                else
                {
                    createdUIFrameList.Remove(frame);
                    createdUIFrameList.Add(frame);
                }
            }
            else
            {
                frame = InstantiateFrame<T>();
                if (frame != null)
                {
                    if(forceToBottom)
                    {
                        createdUIFrameList.Insert(0, frame);
                    }
                    else
                    {
                        createdUIFrameList.Add(frame);
                    }
                }
            }

            SortOrderLayerUI();
            onBeforeShow?.Invoke(frame as T);
            frame.Show();
            return frame as T;
        }

        private void SortOrderLayerUI()
        {
            for (int i = 0; i < createdUIFrameList.Count; i++) {
                createdUIFrameList[i].Canvas.sortingOrder = i;
            }
        }

        public T GetFrameShowed<T>() where T : class
        {
            foreach (var frame in createdUIFrameList)
            {
                if (frame is T typedFrame)
                {
                    return typedFrame;
                }
            }

            return null;
        }

        public bool IsFrameShowed<T>() where T : class
        {
            return GetFrameShowed<T>() != null;
        }

        public UIFrame InstantiateFrame<T>()
        {
            UIFrame createdFrame = null;
            foreach (var frame in frames)
            {
                if (frame is T typedFrame)
                {
                    createdFrame = Instantiate(frame, transform);
                    break;
                }
            }

            if(createdFrame == null)
            {
                Debug.LogError("Frame need to show has not been assign");
            }

            return createdFrame;
        }

        public void HideFrame<T>() where T : class
        {
            var frame = GetFrameShowed<T>();
            if (frame != null)
            {
                var uiFrame = frame as UIFrame;
                uiFrame.Hide();
            }
        }
    }
}


