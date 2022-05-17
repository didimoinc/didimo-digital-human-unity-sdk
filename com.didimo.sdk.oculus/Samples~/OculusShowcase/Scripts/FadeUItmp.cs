using System.Collections;
using UnityEngine;
using TMPro;
/// <summary>
/// Class to fade text.
/// </summary>
public class FadeUItmp : MonoBehaviour
{
    [SerializeField]
    private float fadeTime;

    public void Start()
    {
        StartCoroutine(FadeOuttmp());
    }

    private IEnumerator FadeOuttmp()
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


