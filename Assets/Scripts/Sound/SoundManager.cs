using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource _scrash;
    [SerializeField] private AudioSource _trumpet;
    public AudioSource WithoutTrumpet;

    private bool _hasTransitioned;

    public IEnumerator WaitForAudioEnd()
    {
        if (!_hasTransitioned)
        {
            while (WithoutTrumpet.isPlaying)
            {
                yield return null;
            }
            _scrash.Play();
            while (_scrash.isPlaying)
            {
                yield return null;
            }
            _trumpet.Play();
            _trumpet.loop = true;
            _hasTransitioned = true;
        }
    }
}
