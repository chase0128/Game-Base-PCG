using UnityEngine;

public class TeachUI : MonoBehaviour
{
    private void Update()
    {
        if (Input.anyKeyDown)
        {
            gameObject.SetActive(false);
        }
    }
}
