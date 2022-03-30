using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FadeUItmp : MonoBehaviour
{
    public float fadeTime;

    public void Start()
    {
        // Temporarily disabling FadeOuts as this seems to really impact performance
        StartCoroutine(FadeOuttmp());
    }

    public IEnumerator FadeOuttmp()
    {
        TextMeshPro text = GetComponent<TextMeshPro>();
        Color originalColor = new Color(0.1647059f, 0.02352941f, 0.3176471f, 1.0f);
        Color endColor = new Color(0.1647059f, 0.02352941f, 0.3176471f, 0.0f);

        for (float t = 0.01f; t < fadeTime; t += Time.deltaTime)
        {
            text.color = Color.Lerp(originalColor, endColor, Mathf.Min(1, t / fadeTime));
            yield return null;
        }
    }
}


