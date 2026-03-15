using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Asasingame.Core.Runtimes.RenderFeature
{
    public class ChangeFullScreenRenderAtStart : MonoBehaviour
    {
        [SerializeField] private Material mat;
        [SerializeField] private FullScreenPassRendererFeature fsFeature;

        private void OnEnable()
        {
            if (fsFeature != null)
            {
                fsFeature.SetActive(true);
                fsFeature.passMaterial = mat;
            }
        }

        private void OnDisable()
        {
            if (fsFeature != null)
            {
                fsFeature.SetActive(false);
            }
        }
    }
}

