using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private AudioSource _announcement;
    [SerializeField] private float _durationSmoothMovement = 2.5f;
    [SerializeField] private GameObject _startText;
    [SerializeField] private GameObject _titleText;
    [SerializeField] private GameObject _generatedText;
    [SerializeField] private GameManager _gameManager;

    private bool _hasPlayed;
    private bool _letsMove;

    void Start()
    {
        _hasPlayed = false;
        _letsMove = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _startText.SetActive(false);
            _letsMove = true;

            if (!_hasPlayed)
            {
                _hasPlayed = true;
                _announcement.Play();
                StartCoroutine(WaitForAnnonceToEnd());
                StartCoroutine(HandleTextDisplay());
            }
        }

        if (_letsMove)
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(0f, -15f, -10f), Time.deltaTime / _durationSmoothMovement);
        }
    }

    private IEnumerator HandleTextDisplay()
    {
        yield return new WaitForSeconds(_durationSmoothMovement * 3);
        _titleText.SetActive(false);

        yield return new WaitForSeconds(_durationSmoothMovement);
        _generatedText.SetActive(true);
        _gameManager.LetsGo = true;
    }

    private IEnumerator WaitForAnnonceToEnd()
    {
        while (_announcement.isPlaying)
        {
            yield return null;
        }
    }
}
