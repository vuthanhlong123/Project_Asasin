using UnityEngine;

namespace Game.Runtimes.Sound
{
    public class SpeakerPoint : MonoBehaviour
    {
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }
        
        public void PlayAudio(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (!_audioSource) return;

            _audioSource.volume = volume;
            _audioSource.pitch = pitch;
            _audioSource.PlayOneShot(clip);
        }
    }
}


