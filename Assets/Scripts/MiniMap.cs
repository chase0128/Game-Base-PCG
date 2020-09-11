
using UnityEngine;
[RequireComponent(typeof(Camera))]
public class MiniMap : MonoBehaviour
{
    public GameObject player;
    private Camera camera;
    public GameObject MiniIcon;
  
    private void Awake()
    {
        camera = GetComponent<Camera>();
    }

    private void Update()
    {
        Vector3 pos = player.transform.position;
        pos.y = MiniIcon.transform.position.y;
        MiniIcon.transform.position = pos;
        
        pos.y = gameObject.transform.position.y;
        //Debug.Log(camera.orthographicSize);
        if (pos.x > 0)
        {
            if (pos.x > MapGenerator.width * MapGenerator.squareSize / 2f - camera.orthographicSize)
            {
                pos.x = MapGenerator.width * MapGenerator.squareSize/ 2f - camera.orthographicSize;
            }
        }
        else
        {
            if(pos.x < -MapGenerator.width * MapGenerator.squareSize/ 2f+ camera.orthographicSize)
            {
                pos.x = -MapGenerator.width * MapGenerator.squareSize/ 2f + camera.orthographicSize;
            }
        }

        if (pos.z > 0)
        {
            
            if (pos.z > MapGenerator.height * MapGenerator.squareSize/ 2f - camera.orthographicSize)
            {
                pos.z = MapGenerator.height * MapGenerator.squareSize/ 2f - camera.orthographicSize;
            }
        }
        else
        {
            if(pos.z < -MapGenerator.height * MapGenerator.squareSize/ 2f+ camera.orthographicSize)
            {
                pos.z = -MapGenerator.height * MapGenerator.squareSize/ 2f + camera.orthographicSize;
            }
        }
        gameObject.transform.position = pos;
    }
}
