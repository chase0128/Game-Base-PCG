using UnityEngine;

public class Crystal : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {

        MovementInput.instance.AroundCrystal(transform.parent.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Exit");
        MovementInput.instance.AwayFromCrystal(transform.parent.gameObject);
    }
}
