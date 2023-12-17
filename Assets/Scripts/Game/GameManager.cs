using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public bool LetsGo = false;
    public bool IsWaitingReponse = false;

    [SerializeField] private GameObject _spotRapper1;
    [SerializeField] private GameObject _spotRapper2;
    [SerializeField] private PredictionCaller _predictionCaller;
    [SerializeField] private SoundManager _soundManager;
    [SerializeField] private TextMeshProUGUI _textUI;
    private bool _hasBegun = false;

    void Update()
    {
        if(LetsGo && !_hasBegun)
        {
            _soundManager.WithoutTrumpet.loop = false;
            StartCoroutine(_soundManager.WaitForAudioEnd());
            _hasBegun = true;
        }

        if(_hasBegun && !IsWaitingReponse)
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                IsWaitingReponse = true;
                _predictionCaller.Predict();

                StartCoroutine(UpdateSpot());
            }
        }
    }

    IEnumerator UpdateSpot()
    {
        yield return new WaitForSeconds(2f);

        _textUI.text = "";

        if (_spotRapper1.activeSelf)
        {
            _spotRapper1.SetActive(false);
            _spotRapper2.SetActive(true);
        }
        else
        {
            _spotRapper2.SetActive(false);
            _spotRapper1.SetActive(true);
        }
    }
}
