
using UnityEngine;


 public class Observer : MonoBehaviour
{
    public Transform player;
    public GameEnding gameEnding;
    public Transform viewPoint;
    bool m_IsPlayerInRange;

    private float distance = 20;


    private void OnTriggerEnter(Collider other)
    {
        /*if(other.transform  ==player)
        {
            m_IsPlayerInRange = true;
        }*/
        other.gameObject.GetComponent<MovementInput>().Scan();
        /*Vector3 direction = other.gameObject.transform.position - viewPoint.position ;
        Ray ray = new Ray(viewPoint.position, direction);
        Debug.DrawRay(ray.origin,ray.direction,Color.red);
        RaycastHit hit;
        if (Physics.Raycast(ray,out hit,distance,LayerMask.GetMask("Environment", "Player")))
        {
            if(hit.collider.gameObject == other.gameObject)
            {
                Debug.Log(other.gameObject);
               

            }
        }*/
    }

}
