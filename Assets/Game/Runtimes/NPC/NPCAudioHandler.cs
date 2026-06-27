using UnityEngine;

namespace Game.Runtimes.NPC
{
    public class NPCAudioHandler : MonoBehaviour
    {
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        public void PlayAudio(AudioClip clip)
        {
            _audioSource?.PlayOneShot(clip);
        }
    }
}


