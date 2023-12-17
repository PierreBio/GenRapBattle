using System.Collections;
using UnityEngine;
using TMPro;

public class BlinkText : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(Blink());
    }

    public IEnumerator Blink()
    {
        while (true)
        {
            GetComponent<TextMeshProUGUI>().enabled = false;
            yield return new WaitForSeconds(0.5f);
            GetComponent<TextMeshProUGUI>().enabled = true;
            yield return new WaitForSeconds(0.5f);
        }
    }
}
