using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

namespace GameAsset.WarpSpeedFX
{
    public class WarpSpeedFXControl : MonoBehaviour
    {
        [SerializeField] private VisualEffect _vfx;
        [SerializeField] private MeshRenderer _cylinder;

        [SerializeField] private float vfxSpeed;
        [SerializeField] private float cylinderSpeed;
        [SerializeField] private float cylinderDelay;

        private bool warpActive;

        private void Start()
        {
            _vfx.Stop();
            _vfx.SetFloat("Strength", 0);

            _cylinder.material.SetFloat("_Active", 0);

            Active();

            Invoke(nameof(DeActive), 10);
        }

        public void Active()
        {
            warpActive = true;
            StopAllCoroutines();
            StartCoroutine(ActivateEffect());
            StartCoroutine(ActivateCylinder());
        }

        public void DeActive()
        {
            warpActive = false;
            StopAllCoroutines();
            StartCoroutine(ActivateEffect());
            StartCoroutine(ActivateCylinder());
        }

        private IEnumerator ActivateEffect()
        {
            if (warpActive)
            {
                _vfx.Play();

                float strength = _vfx.GetFloat("Strength");
                while(strength < 1)
                {
                    strength += vfxSpeed;
                    if (strength > 1)
                    {
                        strength = 1;
                    }

                    _vfx.SetFloat("Strength", strength);
                    yield return new WaitForSeconds(0.1f);
                }
            }
            else
            {
                float strength = _vfx.GetFloat("Strength");
                while (strength > 0)
                {
                    strength -= vfxSpeed;
                    if (strength<0)
                    {
                        strength = 0;
                    }

                    _vfx.SetFloat("Strength", strength);
                    yield return new WaitForSeconds(0.1f);
                }

                _vfx.Stop();
            }
        }

        private IEnumerator ActivateCylinder()
        {
            if (warpActive)
            {
                yield return new WaitForSeconds(cylinderDelay);

                float strength = _cylinder.material.GetFloat("_Active");
                while (strength < 1)
                {
                    strength += cylinderSpeed;
                    if (strength > 1)
                    {
                        strength = 1;
                    }

                    _cylinder.material.SetFloat("_Active", strength);
                    yield return new WaitForSeconds(0.1f);
                }
            }
            else
            {
                float strength = _cylinder.material.GetFloat("_Active");
                while (strength > 0)
                {
                    strength -= cylinderSpeed;
                    if (strength < 0)
                    {
                        strength = 0;
                    }

                    _cylinder.material.SetFloat("_Active", strength);
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }
    }
}

