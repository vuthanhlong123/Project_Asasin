using System;
using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

namespace GameAsset.WarpSpeedFX
{
    public class WarpSpeedFXControl : MonoBehaviour
    {
        [SerializeField] private VisualEffect _vfx;
        [SerializeField] private MeshRenderer _cylinder;
        [SerializeField] private Light _light;

        [SerializeField] private float vfxSpeed;
        [SerializeField] private float cylinderSpeed;
        [SerializeField] private float cylinderDelay;
        [SerializeField] private float lightStrength;
        [SerializeField] private float lightSpeed;

        private bool warpActive;

        private void Start()
        {
            //_vfx.Stop();
            _vfx.SetFloat("Strength", 0);
            _cylinder.material.SetFloat("_Active", 0);
        }

        public void Active(Action completed = null)
        {
            gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(ActiveCoroutine(() =>
            {
                completed?.Invoke();
            }));
        }

        private IEnumerator ActiveCoroutine(Action completed = null)
        {
            warpActive = true;
            bool effectCompleted = false;
            bool cylinderCompleted = false;
            bool lightCompleted = false;

            StartCoroutine(ActivateEffect(() => { effectCompleted = true; }));
            StartCoroutine(ActivateCylinder(() => { cylinderCompleted = true; }));
            StartCoroutine(ActivateLight(() => { lightCompleted = true; }));

            while (!effectCompleted || !cylinderCompleted || !lightCompleted)
            {
                yield return null;
            }

            completed?.Invoke();
        }

        public void DeActive(Action completed = null)
        {
            StopAllCoroutines();
            StartCoroutine(DeActiveCoroutine(() =>
            {
                gameObject.SetActive(false);
                completed?.Invoke();
            }));
        }

        private IEnumerator DeActiveCoroutine(Action completed = null)
        {
            warpActive = false;
            bool effectCompleted = false;
            bool cylinderCompleted = false;
            bool lightCompleted = false;

            StartCoroutine(ActivateEffect(() => { effectCompleted = true; }));
            StartCoroutine(ActivateCylinder(() => { cylinderCompleted = true; }));
            StartCoroutine(ActivateLight(() => { lightCompleted = true; }));

            while (!effectCompleted || !cylinderCompleted || !lightCompleted)
            {
                yield return null;
            }

            completed?.Invoke();
        }

        private IEnumerator ActivateEffect(Action completed = null)
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

            completed?.Invoke();
        }

        private IEnumerator ActivateCylinder(Action completed = null)
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

            completed?.Invoke();
        }

        private IEnumerator ActivateLight(Action completed = null)
        {
            if (warpActive)
            {
                float strength = _light.intensity;
                while (strength < lightStrength)
                {
                    strength += lightSpeed;
                    if (strength > lightStrength)
                    {
                        strength = lightStrength;
                    }

                    _light.intensity = strength;
                    yield return new WaitForSeconds(0.1f);
                }
            }
            else
            {
                float strength = _light.intensity;
                while (strength > 0)
                {
                    strength -= lightSpeed;
                    if (strength < 0)
                    {
                        strength = 0;
                    }

                    _light.intensity = strength;
                    yield return new WaitForSeconds(0.1f);
                }
            }

            completed?.Invoke();
        }
    }
}

