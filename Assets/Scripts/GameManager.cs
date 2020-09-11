
using System;
using Cinemachine;
using UnityEngine;
using Random = System.Random;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance
    {
        get;
        private set;
    }

    public GameObject gameWinUI;
    public GameObject gameLoseUI;
    
    public GameObject player;
    public GameObject map;
    public CinemachineFreeLook freeLookCamera;

    private MapGenerator mapGenerator;
    public MiniMap miniMap;

    public static Random random;
    
    //出口
    public GameObject exit;
    private void Awake()
    {
        random = new Random(DateTime.Now.ToString().GetHashCode());
        instance = this;
    }

    private void Start()
    {
        Init();
        InitPlayer();
        GameBegin();
    }

    private void InitPlayer()
    {
        Vector3 playerInitPos = mapGenerator.GetRandomPosInPath();
        playerInitPos.y = -MapGenerator.wallHeight;
        player = Instantiate(player, playerInitPos, player.transform.rotation);
        player.transform.LookAt(playerInitPos);

        miniMap.player = player;
        //设置摄像机
        freeLookCamera.m_Follow = player.transform;
        freeLookCamera.m_LookAt = player.transform;
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }


    private void Init()
    {
        mapGenerator = map.GetComponent<MapGenerator>();
        NavMeshSurface navMeshSurface = map.GetComponent<NavMeshSurface>();
        navMeshSurface.defaultArea = 0;
        navMeshSurface.collectObjects = CollectObjects.Children;
        navMeshSurface.layerMask = LayerMask.GetMask("Environment");
        //先Build的一次去掉上次残留下来的一些障碍
        navMeshSurface.BuildNavMesh();
        SpawnEnemy.instance.Init();
        
        mapGenerator.GenerateMap();
        
        navMeshSurface.layerMask = LayerMask.GetMask("Environment","Default");
        navMeshSurface.BuildNavMesh();
        GetComponent<SpawnEnemy>().Spawn(mapGenerator,random);
    }

    public void GameWin()
    {
        Time.timeScale = 0;
        gameWinUI.SetActive(true);
    }

    public void GameBegin()
    {
        ProgressBar.instance.HideProgressBar();
        Time.timeScale = 1;
        gameLoseUI.SetActive(false);
        gameWinUI.SetActive(false);
    }

    public void GameLose()
    {
        Time.timeScale = 0;
        gameLoseUI.SetActive(true);
    }

    public void ReBegin()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
