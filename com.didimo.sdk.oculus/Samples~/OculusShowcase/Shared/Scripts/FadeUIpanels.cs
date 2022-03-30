using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeUIpanels : MonoBehaviour
{
    public float fadeTime;

    public void Start()
    {
        // Temporarily disabling FadeOuts as this seems to really impact performance
        // StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        Image img = GetComponent<Image>();
        Color originalColor = img.color;

        for (float t = 0.01f; t < fadeTime; t += Time.deltaTime)
        {
            img.color = Color.Lerp(originalColor, Color.clear, Mathf.Min(1, t / fadeTime));
            yield return null;
        }
    }
}
