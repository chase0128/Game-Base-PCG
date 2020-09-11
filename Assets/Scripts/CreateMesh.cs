
using System.IO;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class CreateMesh : MonoBehaviour
{
    // Start is called before the first frame update
    public int xSize, zSize;
    private Vector3[] vertices;
    
    // Width and height of the texture in pixels.
    public int pixWidth;
    public int pixHeight;

    // The origin of the sampled area in the plane.
    public float xOrg;
    public float yOrg;

    public float maxTerrainHeight = 10.0f;
    // The number of cycles of the basic noise pattern that are repeated
    // over the width and height of the texture.
    public float scale = 1.0F;

    private Texture2D noiseTex;
    private Color[] pix;
    private Renderer rend;
    
    private void Awake()
    {
        GenerateNoise();
        Generate();
        //tartCoroutine();
        
    }

    private void GenerateNoise()
    {
        //rend = GetComponent<Renderer>();

        // Set up the texture and a Color array to hold pixels during processing.
        noiseTex = new Texture2D(pixWidth, pixHeight);
        pix = new Color[noiseTex.width * noiseTex.height];
        //rend.material.mainTexture = noiseTex;
        CalcNoise();
        //SaveRenderTextureToPNG(noiseTex, "./Assets", "noise");
    }

    private void CalcNoise()
    {
        // For each pixel in the texture...
        float z = 0.0F;

        while (z < noiseTex.height)
        {
            float x = 0.0F;
            while (x < noiseTex.width)
            {
                float xCoord = xOrg + x / noiseTex.width * scale;
                float yCoord = yOrg + z / noiseTex.height * scale;
   
                float sample = Mathf.PerlinNoise(xCoord, yCoord);             
                pix[(int)z * noiseTex.width + (int)x] = new Color(sample, sample, sample);
                x++;
            }
            z++;
        }
        // Copy the pixel data to the texture and load it into the GPU.
        noiseTex.SetPixels(pix);
        noiseTex.Apply();
    }
    
    private void Generate()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        Vector4[] tangents = new Vector4[vertices.Length];
        Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
        for (int i = 0, y = 0; y <= zSize; y++)
        {
            for (int x = 0; x <= xSize; x++, i++)
            {
                float _r = noiseTex.GetPixel ((int)((float)x/xSize * pixWidth), (int)((float)y/zSize * pixHeight)).r;
                vertices[i] = new Vector3(x * 1f, maxTerrainHeight * _r,y *1f);
                //yield return new WaitForSeconds(0.05f);
                uv[i] = new Vector2((float)x / xSize, (float)y / zSize);
                tangents[i] = tangent;
            }
        }
        Mesh mesh = new Mesh();
        Debug.Log(mesh.bounds);
        mesh.name = "Procedural Grid";
        mesh.vertices = vertices;
        mesh.uv = uv;
        //mesh.tangents = tangents;
        int[] triangles = new int[xSize *zSize* 6];
        for (int y = 0, ti = 0, i = 0; y < zSize; y++, i++)
        {
            for (int  x =0 ; x < xSize; x++,ti+=6, i++)
            {
                triangles[ti] = i;
                triangles[ti + 2] = triangles[ti + 3] = i + 1;
                triangles[ti + 1] = triangles[ti + 4] = i + xSize + 1;
                triangles[ti + 5] = i + xSize + 2;
                mesh.triangles = triangles;
                mesh.RecalculateNormals();
                GetComponent<MeshFilter>().mesh = mesh;
               // yield return new WaitForSeconds(0.5f);
            }

        }


        
    }
    /*private void OnDrawGizmos()
    {
        if (vertices == null)
        {
            return;
        }
        Gizmos.color = Color.yellow;
        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], 0.1f);
        }
    }*/
    
    
    public bool SaveRenderTextureToPNG(Texture2D rt,string contents, string pngName)
    {
        /*RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;*/
        Texture2D png = rt;
        //  png.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        byte[] bytes = png.EncodeToPNG();
        if (!Directory.Exists(contents))
            Directory.CreateDirectory(contents);
        FileStream file = File.Open(contents + "/" + pngName + ".png", FileMode.Create);
        BinaryWriter writer = new BinaryWriter(file);
        writer.Write(bytes);
        file.Close();
        //Texture2D.DestroyImmediate(png);
        //png = null;
        //RenderTexture.active = prev;
        return true;
 
    } 

}
