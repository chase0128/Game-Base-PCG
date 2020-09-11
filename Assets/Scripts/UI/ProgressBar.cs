using UnityEngine.UI;
using UnityEngine;

public class ProgressBar : MonoBehaviour
{
    public static ProgressBar instance
    {
        get;
        private set;
    }

    public CanvasGroup canvasGroup;
    public Image mask;
    float originalSize;

    private void Awake()
    {
        instance = this;

    }
    private void Start()
    {
        originalSize = mask.rectTransform.rect.width;
    }

    public void HideProgressBar()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void ShowProgressBar()
    {
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }
    
    public void SetValue(float value)
    {				      
        mask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, originalSize * value);
    }
}
