using System.Security.Cryptography.X509Certificates;
using UnityEngine;

[System.Serializable]
public struct CoordSize
{
    public int xSize;
    public int ySize;

    public int size
    {
        get
        {
            return xSize * ySize;
        }
    }
}
[System.Serializable]
public class Obstacle
{
    public GameObject prefab;
    public Coord pos;
    public CoordSize size;
    public bool isInEdge;


    public Obstacle(GameObject newObject, Coord position)
    {
        prefab = newObject;
        pos = position;
    }
    public Obstacle(GameObject newObject, Coord position,CoordSize size)
    {
        prefab = newObject;
        pos = position;
        this.size = size;
    }
    public int GetSize()
    {
        return size.size;
    }
}
