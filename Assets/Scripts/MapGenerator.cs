 using UnityEngine;
using System;
using System.Collections.Generic;
 using System.Linq;
 using Random = System.Random;

public class MapGenerator : MonoBehaviour
{
    static public MapGenerator instance
    {
        get;
        private set;
    }
    // Start is called before the first frame update
    public static int  width = 100;
    public static int height = 100;
    public static int squareSize = 1;
    public static int wallHeight = 5;
    [Range(0,100)]
    public int randomFillPercent;

    public int mapWidth;
    public int mapHeight;
    
    public int smoothTime;
    public string seed;
    public bool useRandomSeed;
    public int[,] map;
    
    public List<Room> survivingRooms;
    public  List<Edge> roomPaths;

    //地图中可行走的边缘区域和非边缘区域
    public List<Coord> mapEdgeTiles;
    public List<Coord> mapRoomTiles;
    public List<Coord> mapTileInUsed;
    
    private List<Coord> activeTile;
    private List<Coord> edgeTile;
    private List<Coord> pathTile;
    private List<Coord> inactiveTile;
    private List<Coord> inactiveEdgeTile;
    private List<Coord> inactivePathTile;
    public List<Prefab> prefabs = new List<Prefab>();
    public List<GameObject> crystals = new List<GameObject>();
    public int crystalsCount;
    private static  Random random;
        
    public GameObject treasure;
    private List<GameObject> decorations = new List<GameObject>();
    private List<Obstacle> obstacles = new List<Obstacle>();
    
    //出口
    public GameObject exit;
    
    
    void Awake()
    {
        random = new Random(System.DateTime.Now.ToString().GetHashCode());
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //enerateMap();
        }
    }

    public void GenerateMap()
    {
        width = mapWidth;
        height = mapHeight;
        map = new int[width, height];
        survivingRooms = new List<Room>();
        activeTile = new List<Coord>();
        edgeTile = new List<Coord>();
        pathTile = new List<Coord>();
        roomPaths = new List<Edge>();
        inactiveTile = new List<Coord>();
        inactiveEdgeTile = new List<Coord>();
        inactivePathTile = new List<Coord>();
        if (decorations.Count != 0)
        {
            foreach (var decoration in decorations)
            {
                Destroy(decoration);
            }
            decorations = new List<GameObject>();
        }

        if (obstacles.Count != 0)
        {
            foreach (var obstacle in obstacles)
            {
                Destroy(obstacle.prefab);
            }
            obstacles = new List<Obstacle>();
        }
        
        
        
        RandomFillMap();
        for (int i = 0; i < smoothTime; i++)
            SmoothMap();
        ProcessMap();
        int borderSize = 5;
        int[,] borderedMap = new int[width + 2 * borderSize, height + 2 * borderSize];
        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize)
                {
                    borderedMap[x, y] = map[x-borderSize, y -borderSize];
                }
                else
                {
                    borderedMap[x, y] = 1;
                }
            }
        }

        foreach (Room room in survivingRooms)
        {
            activeTile.AddRange(room.tiles);
            edgeTile.AddRange(room.edgeTiles);
        }
        foreach (Coord tile in edgeTile)
        {
            activeTile.Remove(tile);
        }
        
        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen .GenerateMesh( borderedMap ,squareSize);



        PostProcessMap();
        InitExit();
        GenerateCrystals();
        DecorateEnvironment();

    }
    
    /// <summary>
    /// 初始化终点所在位置，因为终点只有一个且每个关卡都有，所以这里不对终点对象进行销毁，而是每次重置终点的位置
    /// </summary>
    void InitExit()
    {
        List<Coord> usefulCoords = GetUsefulCoords(2, 2);
        List<Coord> tilesInLargestRoom = new List<Coord>();
        foreach (Coord tile in usefulCoords)
        {
            if (survivingRooms[survivingRooms.Count - 1].tiles.Contains(tile))
            {
                tilesInLargestRoom.Add(tile);
            }
        }

        Coord tileInLargestRoom = tilesInLargestRoom[random.Next(0, tilesInLargestRoom.Count)];
        UseReigonCoords(tileInLargestRoom, 2, 2);
        exit.transform.position = CoordToWorldPoint(tileInLargestRoom);
    }
    /// <summary>
    /// 对地图进行处理，保证不同列表存储的Coord都是相同的引用且不出现重复，然后对survivingRooms 和 roomPaths 根据区域大小进行排序
    /// </summary>
    void PostProcessMap()
    {
        mapEdgeTiles = new List<Coord>();
        mapRoomTiles = new List<Coord>();
        mapTileInUsed = new List<Coord>();
        for (int i = 1; i < map.GetLength(0)-1; i++)
        {
            for (int j = 1; j < map.GetLength(1)-1; j++)
            {
                if (map[i, j] == 0)
                {
                    if (map[i, j + 1] == 1 || map[i, j - 1] == 1 || map[i - 1, j] == 1 || map[i + 1, j] == 1)
                    {
                        mapEdgeTiles.Add(new Coord(i, j));
                    }
                    else
                    {
                        mapRoomTiles.Add(new Coord(i,j));
                    }
                }
            }
        }

        foreach (Room room in survivingRooms)
        {
            HashSet<Coord> edges = new HashSet<Coord>();
            foreach (Coord tile in room.edgeTiles)
            {
                if (mapEdgeTiles.Exists(t => t.tileX == tile.tileX && t.tileY == tile.tileY)) ;
                {
                    Coord edgeTile = mapEdgeTiles.Find(t => t.tileX == tile.tileX && t.tileY == tile.tileY);
                    edges.Add(edgeTile);
                }
            }

            room.edgeTiles = new List<Coord>();
            foreach (Coord tile in edges)
                room.edgeTiles.Add(tile);
        }

        foreach (Room room in survivingRooms)
        {
            HashSet<Coord> tiles = new HashSet<Coord>();
            foreach (Coord tile in room.tiles)
            {
                if (mapRoomTiles.Exists(t => t.tileX == tile.tileX && t.tileY == tile.tileY)) ;
                {
                    Coord edgeTile = mapRoomTiles.Find(t => t.tileX == tile.tileX && t.tileY == tile.tileY);
                    tiles.Add(edgeTile);
                }
            }

            room.tiles = new List<Coord>();
            foreach (Coord tile in tiles)
                room.tiles.Add(tile);
        }
        
        foreach (Edge path in roomPaths)
        {
            HashSet<Coord> edges = new HashSet<Coord>();
            foreach (Coord tile in path.edgeTiles)
            {
                if (mapEdgeTiles.Exists(t => t.tileX == tile.tileX && t.tileY == tile.tileY)) ;
                {
                    Coord edgeTile = mapEdgeTiles.Find(t => t.tileX == tile.tileX && t.tileY == tile.tileY);
                    edges.Add(edgeTile);
                }
            }

            path.edgeTiles = new List<Coord>();
            foreach (Coord tile in edges)
                path.edgeTiles.Add(tile);
        }
        
        foreach (Edge path in roomPaths)
        {
            HashSet<Coord> tiles = new HashSet<Coord>();
            foreach (Coord tile in path.tiles)
            {
                if (mapRoomTiles.Exists(t => t.tileX == tile.tileX && t.tileY == tile.tileY)) ;
                {
                    Coord edgeTile = mapRoomTiles.Find(t => t.tileX == tile.tileX && t.tileY == tile.tileY);
                    tiles.Add(edgeTile);
                }
            }

            path.tiles = new List<Coord>();
            foreach (Coord tile in tiles)
                path.tiles.Add(tile);
        }
        
        HashSet<Coord> tilesSet = new HashSet<Coord>();
        foreach (Edge path in roomPaths)
        {
            foreach (Coord tile in path.tiles)
            {
                tilesSet.Add(tile);
            }

            foreach (Coord tile in path.edgeTiles)
            {
                tilesSet.Add(tile);
            }
        }

        foreach (Room room in survivingRooms)
        {
            HashSet<Coord> repeatTiles = new HashSet<Coord>();
            foreach (Coord tile in room.tiles)
            {
                if (tilesSet.Contains(tile))
                    repeatTiles.Add(tile);
            }

            foreach (Coord tile in repeatTiles)
            {
                room.tiles.Remove(tile);
            }

            repeatTiles.Clear();
            foreach (Coord tile in room.edgeTiles)
            {
                if (tilesSet.Contains(tile))
                {
                    repeatTiles.Add(tile);
                }
            }

            foreach (Coord tile in repeatTiles)
                room.edgeTiles.Remove(tile);
            
            repeatTiles.Clear();

        }
        
        survivingRooms.Sort(((room, room1) => room.CompareTo(room1) ));
    }

    public void UseEdgeCoord(Coord tile)
    {
        if (mapEdgeTiles.Contains(tile))
        {
            mapEdgeTiles.Remove(tile);
        }

        foreach (Room room in survivingRooms)
        {
            if (room.edgeTiles.Contains(tile))
            {
                room.edgeTiles.Remove(tile);
                break;
            }
        }
        foreach (Edge path in roomPaths)
        {
            if (path.edgeTiles.Contains(tile))
            {
                path.edgeTiles.Remove(tile);
                break;
            }
        }
        mapTileInUsed.Add(tile);
        
        map[tile.tileX, tile.tileY] = 2;
    }

    void UseRoomCoord(Coord tile)
    {
        if (mapRoomTiles.Contains(tile))
        {
            mapRoomTiles.Remove(tile);
        }

        foreach (Room room in survivingRooms)
        {
            if (room.tiles.Contains(tile))
            {
                room.tiles.Remove(tile);
                break;
            }
        }
        foreach (Edge path in roomPaths)
        {
            if (path.tiles.Contains(tile))
            {
                path.tiles.Remove(tile);
                break;
            }
        }
        mapTileInUsed.Add(tile);
        map[tile.tileX, tile.tileY] = 2;
    }
    
    /// <summary>
    /// 标记一个区域已经被占用.
    /// </summary>
    /// <param name="centerTile">区域中心</param>
    /// <param name="xSize">区域x占用的长度，会把x增加一倍以预留可行走区域</param>
    /// <param name="ySize">区域y占用的长度，会把y增加一倍以预留可行走区域</param>
    void UseReigonCoords(Coord centerTile, int xSize, int ySize)
    {
        UseRoomCoord(centerTile);
        for (int i = centerTile.tileX - xSize * 2; i <= centerTile.tileX + xSize * 2; i++)
        {
            for (int j = centerTile.tileY - ySize * 2; j <= centerTile.tileY + ySize * 2; j++)
            {
                if (mapRoomTiles.Exists(t => t.tileX == i && t.tileY == j))
                {
                    UseRoomCoord(mapRoomTiles.Find(t => t.tileX == i && t.tileY == j));
                }
                if (mapEdgeTiles.Exists(t => t.tileX == i && t.tileY == j))
                {
                    UseRoomCoord(mapEdgeTiles.Find(t => t.tileX == i && t.tileY == j));
                }
            }
        }
    }
    void RandomFillMap()
    {
        if (useRandomSeed)
        {
            seed = System.DateTime.Now.ToString();
        }

        Random pseudoRandom = new Random(seed.GetHashCode());

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                {
                    map[x, y] = 1;
                }
                else
                {
                    map[x, y] = pseudoRandom.Next(0, 100) < randomFillPercent ? 1 : 0;
                }
            }
        }
    }
    
    private void SmoothMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neightbourWallCount =  GetSurroundingWallCount(x, y);
                if (neightbourWallCount > 4)
                {
                    map[x, y] = 1;
                }
                else if (neightbourWallCount < 4)
                {
                    map[x, y] = 0;
                }
            }
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY) {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX ++) {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY ++) {
                if (isInMap(neighbourX,neighbourY)) {
                    if (neighbourX != gridX || neighbourY != gridY) {
                        wallCount += map[neighbourX,neighbourY];
                    }
                }
                else {
                    wallCount ++;
                }
            }
        }

        return wallCount;
    }

    void ProcessMap()
    {
        List<List<Coord>> wallRegions = GetRigions(1);
        int wallThreshold = 50;
        foreach (List<Coord> region in wallRegions)
        {
            if (region.Count < wallThreshold)
            {
                foreach (Coord tile in region)
                {
                    map[tile.tileX, tile.tileY] = 0;
                }
            }
        }
        
        List<List<Coord>> roomRegions = GetRigions(0);
        int roomThreshold = 50;
        foreach (List<Coord> region in roomRegions)
        {
            if (region.Count < roomThreshold)
            {
                foreach (Coord tile in region)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            }
            else
            {
                survivingRooms.Add(new Room(region, map));
            }
        }
        
        
        ConnectClosestRoom(survivingRooms);
    }

    Edge FindCloestEdge(Room roomA, Room roomB)
    {
        int bestDistance = -1;
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();
        for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
        {
            for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
            {
                Coord tileA = roomA.edgeTiles[tileIndexA];
                Coord tileB = roomB.edgeTiles[tileIndexB];
                int distance = (int) (Mathf.Pow(tileA.tileX - tileB.tileX, 2) +
                                      Mathf.Pow(tileA.tileY - tileB.tileY, 2));
                if (distance < bestDistance || bestDistance ==-1)
                {
                    bestDistance = distance;
                    bestTileA = tileA;
                    bestTileB = tileB;
                }
            }
        }

        return new Edge(roomA, roomB, bestTileA, bestTileB, bestDistance,map);
    }
    
    void ConnectClosestRoom(List<Room> allRooms)
    {
        List<Room> connected = new List<Room>();
        List<Room> inconnected = new List<Room>();
        inconnected.AddRange(allRooms);
        Room firstRoom = inconnected[0];
        connected.Add(firstRoom);
        inconnected.Remove(firstRoom);
        Dictionary<Room, Edge> cloestDistance = null; 


        while (inconnected.Count > 0)
        {
            if (cloestDistance == null)
            {
                cloestDistance =  new Dictionary<Room, Edge>();
                foreach (Room roomB in inconnected)
                {
                    cloestDistance.Add(roomB, FindCloestEdge(connected[0], roomB));
                }
            }
            else
            {
                foreach (Room roomB in inconnected)
                {
                    Edge edgeBetweenRoom= FindCloestEdge(connected[connected.Count - 1], roomB);
                    if (edgeBetweenRoom.distance < cloestDistance[roomB].distance)
                    {
                        cloestDistance[roomB] = edgeBetweenRoom;
                    }
                }
            }

            var distance = cloestDistance.ToList();
            distance.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
            
            Edge cloestEdge = distance[0].Value;
            cloestDistance.Remove(cloestEdge.endRoom);
            CreatePassage(cloestEdge);
            connected.Add(cloestEdge.endRoom);
            inconnected.Remove(cloestEdge.endRoom);
        }
    }

    void CreatePassage(Edge path)
    {
        
        Room roomA = path.startRoom;
        Room roomB = path.endRoom;
        Coord tileA = path.startTile;
        Coord tileB = path.endTile;
        
        Room.ConnectRoom(roomA, roomB);
        List<Coord> line = GetLine(tileA, tileB);
        List<Coord> pathTiles = new List<Coord>();
        foreach (Coord point in line)
        {
            DrawCircle(point, 5,ref pathTiles);
        }
        path.SetTiles(pathTiles,map);
        pathTiles.AddRange(pathTiles);
        Coord center = line[line.Count / 2];
        path.center = center;
        foreach (Coord tile in pathTiles)
        {
            if (activeTile.Contains(tile))
            {
                activeTile.Remove(tile);
            }

            if (edgeTile.Contains(tile))
            {
                edgeTile.Remove(tile);
            }
        }
        roomPaths.Add(path);
    }

    void DrawCircle(Coord tile, int radius,ref List<Coord> pathTiles)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    int drawX = tile.tileX + x;
                    int drawY = tile.tileY + y;
                    if (isInMap(drawX, drawY))
                    {
                        map[drawX, drawY] = 0;
                        if (!pathTiles.Exists((t) => t.tileX == drawX && t.tileY == drawY))
                        {
                            pathTiles.Add(new Coord(drawX, drawY));
                        }
                    }
                }
            }
        }
    }

    public Vector3 GetRandomPosInPath()
    {
        Edge initTiles = roomPaths[random.Next(0, roomPaths.Count)];
        Coord tile = mapRoomTiles[random.Next(0, mapRoomTiles.Count)];

        if (tile.tileX > 0 && tile.tileY > 0 && tile.tileX < mapWidth && tile.tileY < mapHeight)
        {
            return CoordToWorldPoint(tile);
        }

        return CoordToWorldPoint(initTiles.tiles[random.Next(0, initTiles.tiles.Count)]);
    }
    
    
    
    
    public Coord GetRandomPosInEdge()
    {
        Coord tile = edgeTile[random.Next(0, edgeTile.Count)];
        UseEdgeCoord(tile);
        return tile;
    }

    public Vector3 GetRandomPosInPath(bool unique = true)
    {
        Coord tile = pathTile[random.Next(0, pathTile.Count)];
        if (unique)
        {
            inactivePathTile.Add(tile);
            pathTile.Remove(tile);
        }
        return CoordToWorldPoint(tile);
    }

    public Room GetRandomSurvivingRoom()
    {
        return survivingRooms[random.Next(0, survivingRooms.Count)];
    }
    List<Coord> GetLine(Coord from, Coord to)
    {
        List<Coord> line = new List<Coord> ();

        int x = from.tileX;
        int y = from.tileY;

        int dx = to.tileX - from.tileX;
        int dy = to.tileY - from.tileY;

        bool inverted = false;
        int step = Math.Sign (dx);
        int gradientStep = Math.Sign (dy);

        int longest = Mathf.Abs (dx);
        int shortest = Mathf.Abs (dy);

        if (longest < shortest) {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign (dy);
            gradientStep = Math.Sign (dx);
        }

        int gradientAccumulation = longest / 2;
        for (int i =0; i < longest; i ++) {
            line.Add(new Coord(x,y));

            if (inverted) {
                y += step;
            }
            else {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest) {
                if (inverted) {
                    x += gradientStep;
                }
                else {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }

        return line;
    }
    public  static Vector3 CoordToWorldPoint(Coord tile)
    {
        return new Vector3((-width / 2f + .5f + tile.tileX) * squareSize, -wallHeight, (-height / 2f + .5f + tile.tileY) * squareSize);
    }
    List<List<Coord>> GetRigions(int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);
                    foreach (Coord tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }

    List<Coord> GetRegionTiles(int startX, int startY)
    {
        List<Coord> region = new List<Coord>();
        int[,] mapFlags = new int[width, height];
        int tileType = map[startX, startY];
        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1;
        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            region.Add(tile);
            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY+ 1; y++)
                {
                    if ((x == tile.tileX || y == tile.tileY)&&isInMap(x ,y) )
                    {
                        if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }

        return region;
    }

    bool isInMap(int x, int y)
    {
        return x >= 0 && y >= 0 && x < width && y < height;
    }

    void GenerateCrystals()
    {
        List<Coord> LeftBottom = GetLeftBottom(mapEdgeTiles);
        List<Coord> LeftTop = GetLeftTop(mapEdgeTiles);
        List<Coord> RightBottom = GetRightBottom(mapEdgeTiles);
        List<Coord> RightTop = GetRightTop(mapEdgeTiles);
        List<Coord> Center = GetCenter(mapEdgeTiles);
        
        Coord tile1 = LeftBottom.Count >0 ?LeftBottom[random.Next(0, LeftBottom.Count)]: GetRandomPosInEdge();
        UseEdgeCoord(tile1);
        SpawnCrystals(CoordToWorldPoint(tile1),0);
        SpawnEnemy.instance.SpawnOnePatrol(new Room(GetLeftBottom(mapRoomTiles)),CoordToWorldPoint(tile1) );

        Coord tile2 = LeftTop.Count>0?LeftTop[random.Next(0, LeftTop.Count)] : GetRandomPosInEdge();
        UseEdgeCoord(tile2);
        SpawnCrystals(CoordToWorldPoint(tile2),1);
        SpawnEnemy.instance.SpawnOnePatrol(new Room(GetLeftTop(mapRoomTiles)),CoordToWorldPoint(tile2) );

        Coord tile3 = RightBottom.Count >0?RightBottom[random.Next(0, RightBottom.Count)]: GetRandomPosInEdge();
        UseEdgeCoord(tile3);
        SpawnCrystals(CoordToWorldPoint(tile3),2);
        SpawnEnemy.instance.SpawnOnePatrol(new Room(GetRightBottom(mapRoomTiles)),CoordToWorldPoint(tile3) );

        Coord tile4 = RightTop.Count > 0 ?RightTop[random.Next(0, RightTop.Count)] : GetRandomPosInEdge();
        Debug.Log(string.Format("Debug: tile4.x:{0},tile4.y:{1}", tile4.tileX, tile4.tileY));
        Center.Sort((coord, coord1) => coord.tileX.CompareTo(coord1.tileX));
        Debug.Log(string.Format("Debug: center[0].x:{0},center[0].y:{1}", Center[0].tileX, Center[0].tileY));

        UseEdgeCoord(tile4);
        SpawnCrystals(CoordToWorldPoint(tile4),3);
        SpawnEnemy.instance.SpawnOnePatrol(new Room(GetRightTop(mapRoomTiles)),CoordToWorldPoint(tile4) );

        Coord tile5 = Center.Count>0?Center[random.Next(0, Center.Count)] : GetRandomPosInEdge();
        UseEdgeCoord(tile5);
        SpawnCrystals(CoordToWorldPoint(tile5),4);
        SpawnEnemy.instance.SpawnOnePatrol(new Room(GetCenter(mapRoomTiles)),CoordToWorldPoint(tile5) );
    }

    List<Coord> GetLeftBottom(List<Coord> tiles)
    {
        int threadsholdX = mapWidth * 2/5;
        int threadsholdY = mapHeight * 2/5;
        List<Coord> newTiles = new List<Coord>();
        foreach (Coord tile in tiles)
        {
            if (tile.tileX <= threadsholdX && tile.tileY <= threadsholdY)
            {
                newTiles.Add(tile);
            }
        }

        return newTiles;
    }
    List<Coord> GetLeftTop(List<Coord> tiles)
    {
        int threadsholdX = mapWidth * 2/5;
        int threadsholdY = mapHeight * 2/5;
        List<Coord> newTiles = new List<Coord>();
        foreach (Coord t in tiles)
        {
            if (t.tileX <= threadsholdX)
            {
                if (t.tileY >= mapHeight - threadsholdY)
                {
                    newTiles.Add(t);
                }
            }
        }

        return newTiles;
    }
    List<Coord> GetRightBottom(List<Coord> tiles)
    {
        int threadsholdX = mapWidth *2/5;
        int threadsholdY = mapHeight *2/5;
        List<Coord> newTiles = new List<Coord>();
        foreach (Coord tile in tiles)
        {
            if (tile.tileX >= mapWidth -threadsholdX  )
            {
                if (tile.tileY <= threadsholdY)
                {
                    newTiles.Add(tile);
                }
                
            }
        }

        return newTiles;
    }
    List<Coord> GetRightTop(List<Coord> tiles)
    {
        float threadsholdX = mapWidth*4/5f;
        float threadsholdY = mapHeight*4/5f;
        List<Coord> newTiles = new List<Coord>();
        foreach (Coord tile in tiles)
        {
            if ( threadsholdX < tile.tileX)
            {
                if ( threadsholdY < tile.tileY)
                {
                    newTiles.Add(tile);
                }
            }
        }

        return newTiles;
    }

    List<Coord> GetCenter(List<Coord> tiles)
    {
        float xMax= mapWidth * 3/5f;
        float xMin= mapWidth * 2/5f;
        
        float yMin= mapHeight * 2/5f;
        float yMax= mapHeight * 3/5f;

        List<Coord> newTiles = new List<Coord>();
        foreach (Coord tile in tiles)
        {
            if (tile.tileX > xMin && tile.tileX < xMax)
            {
                if ( yMin < tile.tileY && yMax > tile.tileY)
                {
                    newTiles.Add(tile);
                }
            }
        }

        return newTiles;
    }
    void SpawnCrystals(Vector3 init,int i  )
    {
        GameObject newObject = crystals[i];
        newObject = Instantiate(newObject, init,
            newObject.transform.rotation, gameObject.transform);
        decorations.Add(newObject);
    }
    
    /// <summary>
    ///根据占用的x,y格子来查找可用的Coord,为了预留一定的可行走区域，这里查找区域时候把x,y增加了一倍
    /// </summary>
    /// <param name="x">占用的x地图长度</param>
    /// <param name="y">占用的y地图长度</param>
    /// <returns></returns>
    List<Coord>  GetUsefulCoords(int x,int y)
    {
        List<Coord> usefulCoords = new List<Coord>();
        foreach (Coord tile in mapRoomTiles)
        {
            bool flag = true;
            for (int i = tile.tileX - x * 2; i <= tile.tileX + x * 2; i++)
            {
                for (int j = tile.tileY - y * 2; j <= tile.tileY + y * 2; j++)
                {
                    if (isInMap(i, j))
                    {
                        if (map[i, j] != 0)
                        {
                            flag = false;
                            break;
                        }
                    }
                    else
                    {
                        flag = false;
                        break;
                    }
                }

                if (!flag)
                    break;
            }

            if (flag)
            {
                usefulCoords.Add(tile);
            }
        }

        return usefulCoords;
    }
    
    
    /// <summary>
    /// 生成障碍物,当障碍物占用面积大于1时把x,y增加一倍以预留行走区域,避免障碍物太拥挤
    /// </summary>
    /// <param name="obs"></param>
    void GenerateObstacle(Obstacle obs)
    {
        if (obs.GetSize() <= 1)
        {
            Coord tile = mapRoomTiles[random.Next(0, mapRoomTiles.Count)];
            UseRoomCoord(tile);
            GameObject obj = Instantiate(obs.prefab, CoordToWorldPoint(tile), obs.prefab.transform.rotation,
                gameObject.transform);
            obj.transform.Rotate(Vector3.up, random.Next(0, 360));
            Obstacle obstacle = new Obstacle(obj, tile);
            obstacles.Add(obstacle);
        }
        else
        {
            int maxLength = obs.size.xSize > obs.size.ySize ? obs.size.xSize : obs.size.ySize;
            List<Coord> usefulCoords = GetUsefulCoords(obs.size.xSize, obs.size.ySize);
            if(usefulCoords.Count ==0) return;
            Coord genTile = usefulCoords[random.Next(0, usefulCoords.Count)];
            UseReigonCoords(genTile,obs.size.xSize,obs.size.ySize);
            GameObject obj = Instantiate(obs.prefab, CoordToWorldPoint(genTile), obs.prefab.transform.rotation,
                gameObject.transform);
            obj.transform.Rotate(Vector3.up, random.Next(0, 360));
            Obstacle obstacle = new Obstacle(obj, genTile,obs.size);
            obstacles.Add(obstacle);
        }

    }
     
    void DecorateEnvironment()
    {
        
        foreach (Prefab prefab in prefabs)
        {
            for (int i = 0; i < prefab.num; i++)
            {
                GenerateObstacle(prefab.obstacle);
            }
        }

        //decorate treasures
        foreach (Room room in survivingRooms)
        {
            if (room.tiles.Count < 100)
            {
                Coord tile =  room.GetRandomRoomEdge(random);
                UseEdgeCoord(tile);
                GameObject newObject = Instantiate(treasure, CoordToWorldPoint(tile),
                    treasure.transform.rotation);
                newObject.transform.LookAt(CoordToWorldPoint(room.center.Value));
                decorations.Add(newObject);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (map != null)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    if (map[x, y] == 0)
                    {
                        Gizmos.color = Color.white;
                    }
                    else
                    {
                        Gizmos.color = Color.black;
                    }
                    Gizmos.DrawCube(new Vector3((-map.GetLength(0) / 2f + .5f + x), 0, (-map.GetLength(1) / 2f + .5f + y)),
                        Vector3.one);
                }
            }
        }

    }

    /// <summary>
    /// 在地图边缘找一个区域来放置怪物。用于龙等大型生物
    /// </summary>
    /// <param name="xSize"></param>
    /// <param name="ySize"></param>
    /// <returns></returns>
    public Vector3 SpawnPlaceInMapEdge(int xSize, int ySize)
    {
        List<Coord> usefulTiles = GetUsefulCoords(xSize, ySize);
        int size = 20;
        bool isFind = false;
        Coord usefulTile = new Coord();
        int threadshold = 50;
        while (size <= threadshold)
        {
            foreach (Coord tile in usefulTiles)
            {
                if (tile.tileX <= size || tile.tileY <= size || tile.tileX >= map.GetLength(0) - size ||
                    tile.tileY >= map.GetLength(1))
                {
                    usefulTile = tile;
                    isFind = true;
                    break;
                }
            }

            if (isFind)
            {
                UseReigonCoords(usefulTile,xSize,ySize);
                return CoordToWorldPoint(usefulTile);
            }

            size += 10;
        }
        
        
        return Vector3.zero;
        
    }

    public Vector3? GetForward(Coord tile)
    {
        List<Coord> around = new List<Coord>();
        foreach (Coord t in mapRoomTiles)
        {
            if (Math.Abs(tile.tileX - t.tileX) <= 2 && Math.Abs(tile.tileY - t.tileY) <= 2)
            {
                around.Add(t);
            }
        }

        if (around.Count != 0) return CoordToWorldPoint(around[random.Next(0, around.Count)]);
        else return null;
    }
    
    
    [Serializable]
    public struct Prefab
    {
        public Obstacle obstacle;
        public int num;
        public bool isInEdge;
    }
    
}
