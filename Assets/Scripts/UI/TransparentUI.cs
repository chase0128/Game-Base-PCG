using UnityEngine.UI;
using UnityEngine;

public class TransparentUI : MonoBehaviour
{
    public Image transparentImg;
    public Text cdText;

    static public TransparentUI instance
    {
        get;
        private set;
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        OverCD();
    }

    public void InCD()
    {
        Color imgColor = transparentImg.color;
        imgColor.a = 0.5f;
        transparentImg.color = imgColor;
        cdText.gameObject.SetActive(true);
    }

    public void OverCD()
    {
        Color imgColor = transparentImg.color;
        imgColor.a = 1f;
        transparentImg.color = imgColor;
        cdText.gameObject.SetActive(false);
    }

    public void SetValue(int value)
    {
        cdText.text = value.ToString();
    }
}
