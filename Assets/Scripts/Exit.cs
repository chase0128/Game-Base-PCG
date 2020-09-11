using UnityEngine;

public class Exit : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (MovementInput.instance.IsFinishTask())
        {
            GameManager.instance.GameWin();
        }
    }
}
