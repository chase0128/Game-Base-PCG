using UnityEngine;

public class Events : MonoBehaviour
{
    private float jumpHeight = 50f;
    private float gravityValue = -9.8f;

    private CharacterController controller;
    // Start is called before the first frame update
    public void Awake()
    {
       
    }

    public void ActionEvent()
    {
        Debug.Log("ACTION");
        Vector3 playerVelocity = Vector3.zero;
        playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue * 10f);
        controller = GetComponent<CharacterController>();
        controller.Move(playerVelocity * Time.deltaTime);
    }
}
