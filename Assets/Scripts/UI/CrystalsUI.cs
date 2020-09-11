using UnityEngine.UI;
using UnityEngine;

public class CrystalsUI : MonoBehaviour
{
    public static CrystalsUI instance
    {
        get;
        private set;
    }
    
    public Text crystalsCount;
    private int maxCount;
    private int ownCount;

    private void Start()
    {
        instance = this;
        maxCount = 5;
        ownCount = 0;
    }

    public void GatherCrystal(int count)
    {
        ownCount = Mathf.Clamp(ownCount + count, 0, maxCount);
        crystalsCount.text = ownCount + " / " + maxCount;
    }
}
