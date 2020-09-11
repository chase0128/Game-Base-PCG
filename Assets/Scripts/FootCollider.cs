using UnityEngine;

public class FootCollider : MonoBehaviour
{
    public MovementInput movementInput;


    private void OnTriggerEnter(Collider other)
    {
        if (movementInput.IsState(MovementInput.State.attack))
        {
            Debug.Log("chase");
            other.gameObject.GetComponent<PatrolEnemy>().BeAttacked(movementInput.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        
    }
}
