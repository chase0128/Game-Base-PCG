using UnityEngine;

public class PatrolEasyFound : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<MovementInput>().IsEasyFound() &&
            !other.gameObject.GetComponent<MovementInput>().IfTransparent() 
            )
        {
            transform.parent.GetComponent<PatrolEnemy>().FoundPlayer(other.gameObject);
        }
    }
}
