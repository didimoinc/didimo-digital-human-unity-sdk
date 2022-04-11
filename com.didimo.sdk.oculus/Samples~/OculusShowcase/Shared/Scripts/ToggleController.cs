using UnityEngine;
using UnityEngine.UI;


namespace Didimo.Core.Examples.DidimoInspector
{
    public class ToggleController : MonoBehaviour
    {
        public bool isOn;

        public Color onColorBg;
        public Color offColorBg;

        public Image toggleBgImage;
        public RectTransform toggle;

        public GameObject handle;
        private RectTransform handleTransform;

        private float handleSize;
        private float onPosX;
        private float offPosX;

        public float handleOffset;

        public GameObject onIcon;
        public GameObject offIcon;


        public float speed;
        private float t = 0.0f;

        private bool switching = false;


        private void Awake()
        {
            handleTransform = handle.GetComponent<RectTransform>();
            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleSize = handleRect.sizeDelta.x;
            float toggleSizeX = toggle.sizeDelta.x;
            onPosX = (toggleSizeX / 2) - (handleSize/2) - handleOffset;
            offPosX = onPosX * -1;
        }


        private void Start()
        {
            if (isOn)
            {
                toggleBgImage.color = onColorBg;
                handleTransform.localPosition = new Vector3(onPosX, 0f, 0f);
                onIcon.SetActive(true);
                offIcon.SetActive(false);
            }
            else
            {
                toggleBgImage.color = offColorBg;
                handleTransform.localPosition = new Vector3(offPosX, 0f, 0f);
                onIcon.SetActive(false);
                offIcon.SetActive(true);
            }
        }
            
        private void Update()
        {
            if (switching) Toggle(isOn);
        }

        public void Switching()
        {
            switching = true;
        }
            
        
        public void Toggle(bool toggleStatus)
        {
            if (!onIcon.activeSelf || !offIcon.activeSelf)
            {
                onIcon.SetActive(true);
                offIcon.SetActive(true);
            }
            
            if (toggleStatus)
            {
                toggleBgImage.color = SmoothColor(onColorBg, offColorBg);
                Transparency (onIcon, 1f, 0f);
                Transparency (offIcon, 0f, 1f);
                handleTransform.localPosition = SmoothMove(handle, onPosX, offPosX);
            }
            else 
            {
                toggleBgImage.color = SmoothColor(offColorBg, onColorBg);
                Transparency (onIcon, 0f, 1f);
                Transparency (offIcon, 1f, 0f);
                handleTransform.localPosition = SmoothMove(handle, offPosX, onPosX);
            }
        }


        private Vector3 SmoothMove(GameObject toggleHandle, float startPosX, float endPosX)
        {
            Vector3 position = new Vector3 (Mathf.Lerp(startPosX, endPosX, t += speed * Time.deltaTime), 0f, 0f);
            StopSwitching();
            return position;
        }

        private Color SmoothColor(Color startCol, Color endCol)
        {
            return Color.Lerp(startCol, endCol, t += speed * Time.deltaTime);
        }

        private CanvasGroup Transparency(GameObject alphaObj, float startAlpha, float endAlpha)
        {
            CanvasGroup alphaVal = alphaObj.GetComponent<CanvasGroup>();
            alphaVal.alpha = Mathf.Lerp(startAlpha, endAlpha, t += speed * Time.deltaTime);
            return alphaVal;
        }

        private void StopSwitching()
        {
            if (t > 1.0f)
            {
                switching = false;
                t = 0.0f;
                isOn = !isOn;
            }
        }
    }
}
