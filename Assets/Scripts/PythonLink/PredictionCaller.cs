using UnityEngine;
using TMPro;

public class PredictionCaller : MonoBehaviour
{
    public GameManager Manager;

    [SerializeField] private TextMeshProUGUI _textUI;
    [SerializeField] private PredictionClient _client;
    private string _subject = "i am the best man in the world";
    private string _source = "i am the best man in the world";
    private bool _textHasChanged = false;

    private void Start()
    {
        _textUI.text = _source;
    }

    void Update()
    {
        if (_textHasChanged)
        {
            _textHasChanged = false;
            _textUI.text = _source;
            Manager.IsWaitingReponse = false;
        }
    }

    public void Predict()
    {
        string input = _subject;
        _client.Predict(input, output =>
        {
            _source = output;
            _textHasChanged = true;
        }, error =>
        {
            Debug.LogError(error);
        });
    }
}
