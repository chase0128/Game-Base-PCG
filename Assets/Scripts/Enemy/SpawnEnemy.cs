using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class SpawnEnemy : MonoBehaviour
{
    public static SpawnEnemy instance
    {
        get;
        private set;
    }
    
    private List<GameObject> generatedEnemies = new List<GameObject>();
    public List<GameObject> patrolEnemyPrefabs;
    public GameObject scanEnemyPrefab;
    public int patrolEnemyNum;

    //巨龙
    public GameObject dragon;

    private void Awake()
    {
        instance = this;
        Init();
        
    }

    public void Init()
    {
        if (generatedEnemies.Count != 0)
        {
            foreach (GameObject obj in generatedEnemies)
            {
                Destroy(obj);
            }
            generatedEnemies = new List<GameObject>();
        }
        
        
    }

    public void Spawn(MapGenerator mapGenerator,System.Random random)
    {
        SpawnPatrolEnemies(mapGenerator, random);
        SpawnScanEnemies(mapGenerator, random);
        SpawnDragon(mapGenerator,random);
    }

    private void SpawnPatrolEnemies(MapGenerator mapGenerator, System.Random random)
    {
        int threshold = 150;
        List<Room> walkableRooms = mapGenerator.survivingRooms;
        List<Edge> walkablePaths = mapGenerator.roomPaths;
        foreach (Room room in walkableRooms)
        {
            if (room.GetSize() >= 100 && room.GetSize() <= 150)
            {
                GameObject prefab = patrolEnemyPrefabs[random.Next(0, patrolEnemyPrefabs.Count)];
                prefab = Instantiate(prefab, room.GetRandomPos(random), prefab.transform.rotation);
                PatrolEnemy newEnemy = prefab.GetComponent<PatrolEnemy>();
                newEnemy.InitPatrol(room);
                generatedEnemies.Add(prefab);
                continue;
            }

            List<List<Coord>> splitSets = SplitRoom(room);
            foreach (List<Coord> set in splitSets)
            {
                if (set.Count > 120)
                {
                    GameObject prefab = patrolEnemyPrefabs[random.Next(0, patrolEnemyPrefabs.Count)];
                    prefab = Instantiate(prefab, room.GetRandomPos(random), prefab.transform.rotation);
                    PatrolEnemy newEnemy = prefab.GetComponent<PatrolEnemy>();
                    newEnemy.InitPatrol(new Room(set));
                    generatedEnemies.Add(prefab);
                }
            }
        }

        /*foreach (Room room in mapGenerator.survivingRooms)
        {    
            if (room.GetSize() >= threshold)
            {
                int spawnNum = room.GetSize() / (threshold * 4);
                for (int i = 0; i < spawnNum; i++)
                {
                    GameObject prefab = patrolEnemyPrefabs[random.Next(0, patrolEnemyPrefabs.Count)];
                    prefab = Instantiate(prefab, room.GetRandomPos(random), prefab.transform.rotation);
                    PatrolEnemy newEnemy = prefab.GetComponent<PatrolEnemy>();
                    newEnemy.InitPatrol(room);
                    generatedEnemies.Add(prefab);
                }
       
            }
            else
            {
                Edge patrolPath = mapGenerator.roomPaths.Find(path => path.endRoom == room || path.startRoom == room);
                GameObject prefab = patrolEnemyPrefabs[random.Next(0, patrolEnemyPrefabs.Count)];
                prefab = Instantiate(prefab, patrolPath.GetRandomPos(random), prefab.transform.rotation);
                
                PatrolEnemy newEnemy = prefab.GetComponent<PatrolEnemy>();
                newEnemy.InitPatrol(patrolPath);
                generatedEnemies.Add(prefab);
            }
        }*/

    }

    private void SpawnScanEnemies(MapGenerator mapGenerator, System.Random random)
    {
        int threshold = 100;
        int radius = 80;
        foreach (Edge path in mapGenerator.roomPaths)
        {
            Spawn(path,scanEnemyPrefab,true,random);

        }

        foreach (Room room in mapGenerator.survivingRooms)
        {
            if (room.GetSize() <= threshold)
            {
                Spawn(room, scanEnemyPrefab, true, random);
                continue;
            }
            /*int maxX = room.GetMaxX();
            int minX = room.GetMinX();
            int maxY = room.GetMaxY();
            int minY = room.GetMinY();

            List<Coord> Tiles = room.SortedByX();
            List<List<Coord>> tilesSet = new List<List<Coord>>();

            int cur = minX;
            List<Coord> set = new List<Coord>();
            foreach (Coord tile in Tiles)
            {
                if (tile.tileX <= cur + 1)
                {
                    set.Add(tile);
                    cur = tile.tileX;
                }
                else
                {
                    tilesSet.Add(set);
                    set = new List<Coord>();
                    set.Add(tile);
                    cur = tile.tileX;
                }
            }
            tilesSet.Add(set);

            List<List<Coord>> tempTilesSet = new List<List<Coord>>();

            foreach (List<Coord> xSet in tilesSet)
            {
                tempTilesSet.AddRange(SplitTilesSetsByX(xSet, 20));
            }

            tilesSet = new List<List<Coord>>();

            foreach (List<Coord> xSet in tempTilesSet)
            {
                xSet.Sort((coord, coord1) => coord.tileY.CompareTo(coord1.tileY));
                set = new List<Coord>();
                cur = xSet[0].tileY;
                foreach (Coord tile in xSet)
                {
                    if (tile.tileY <= cur + 1)
                    {
                        set.Add(tile);
                        cur = tile.tileY;
                    }
                    else
                    {
                        tilesSet.Add(set);
                        set = new List<Coord>();
                        set.Add(tile);
                        cur = tile.tileY;
                    }
                }
                tilesSet.Add(set);
            }

            tempTilesSet = new List<List<Coord>>();*/
            /*foreach (List<Coord> ySet in tilesSet)
            {
                tempTilesSet.AddRange(SplitTilesSetsByY(ySet,20));
            }

            tilesSet = tempTilesSet;*/
            List<List<Coord>> tilesSet = SplitRoom(room);
            foreach (List<Coord> tileSet in tilesSet)
            {
                if (tileSet.Count/20 > 0)
                {
                    int num = tileSet.Count / 15;
                    List<List<Coord>> tileSets = SplitTilesSetsByY(tileSet, tileSet.Count / num);

                    foreach (List<Coord> spawnSet in tileSets)
                    {

                        if (spawnSet.Count > 15)
                        {
                            List<Coord> edgeTiles = GetEdgeTiles(spawnSet,mapGenerator.map);
                            foreach (Coord tile in edgeTiles)
                            {
                                spawnSet.Remove(tile);
                            }

                            Coord center = spawnSet[random.Next(0, spawnSet.Count)];
                            if(edgeTiles.Count <=0 ) continue;
                            Coord pos = edgeTiles[random.Next(0, edgeTiles.Count)];
                            Vector3? forward = mapGenerator.GetForward(pos);
                            Spawn(scanEnemyPrefab, MapGenerator.CoordToWorldPoint(pos),
                                forward.HasValue ? forward.Value : MapGenerator.CoordToWorldPoint(pos));
                        }
                    }
                }
            }
        }



    }

    private List<List<Coord>> SplitRoom(Room room)
    {
            int maxX = room.GetMaxX();
            int minX = room.GetMinX();
            int maxY = room.GetMaxY();
            int minY = room.GetMinY();

            List<Coord> Tiles = room.SortedByX();
            List<List<Coord>> tilesSet = new List<List<Coord>>();

            int cur = minX;
            List<Coord> set = new List<Coord>();
            foreach (Coord tile in Tiles)
            {
                if (tile.tileX <= cur + 1)
                {
                    set.Add(tile);
                    cur = tile.tileX;
                }
                else
                {
                    tilesSet.Add(set);
                    set = new List<Coord>();
                    set.Add(tile);
                    cur = tile.tileX;
                }
            }
            tilesSet.Add(set);

            List<List<Coord>> tempTilesSet = new List<List<Coord>>();

            foreach (List<Coord> xSet in tilesSet)
            {
                tempTilesSet.AddRange(SplitTilesSetsByX(xSet, 20));
            }

            tilesSet = new List<List<Coord>>();

            foreach (List<Coord> xSet in tempTilesSet)
            {
                if(xSet.Count == 0 ) continue;
                xSet.Sort((coord, coord1) => coord.tileY.CompareTo(coord1.tileY));
                set = new List<Coord>();
                cur = xSet[0].tileY;
                foreach (Coord tile in xSet)
                {
                    if (tile.tileY <= cur + 1)
                    {
                        set.Add(tile);
                        cur = tile.tileY;
                    }
                    else
                    {
                        tilesSet.Add(set);
                        set = new List<Coord>();
                        set.Add(tile);
                        cur = tile.tileY;
                    }
                }
                tilesSet.Add(set);
            }

            tempTilesSet = new List<List<Coord>>();
            foreach (List<Coord> ySet in tilesSet)
            {
                tempTilesSet.AddRange(SplitTilesSetsByY(ySet,20));
            }

            tilesSet = tempTilesSet;
            return tilesSet;
    }
    
    private List<Coord> GetEdgeTiles(List<Coord> tiles, int[,] map)
    {
        HashSet<Coord> tempTilesEdge = new HashSet<Coord>();
        foreach (Coord tile in tiles)
        {
            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (x == tile.tileX || y == tile.tileY)
                    {
                        if (map[x, y] == 1)
                        {
                            tempTilesEdge.Add(tile);
                        }
                    }
                }
            }
        }

        List<Coord> edgeTiles = new List<Coord>(tempTilesEdge);
        return edgeTiles;
    }

  
    
    private List<List<Coord>> SplitTilesSetsByX(List<Coord> tilesSet, int threshold)
    {
        tilesSet.Sort((coord, coord1) => coord.tileX.CompareTo(coord1.tileX));
        List<List<Coord>> newTileSets =
            new List<List<Coord>>();
        for (int i = 0; i <= (tilesSet[tilesSet.Count - 1].tileX - tilesSet[0].tileX)/ threshold; i++)
        {
            List<Coord> temps = new List<Coord>();
            newTileSets.Add(temps);
        }


        for (int i = 1; i < tilesSet.Count; i++)
        {
            newTileSets[(tilesSet[i].tileX - tilesSet[0].tileX) / threshold].Add(tilesSet[i]);
            
        }
        return newTileSets;
        
    }
    private List<List<Coord>> SplitTilesSetsByY(List<Coord> tilesSet, int threshold)
    {
        tilesSet.Sort((coord, coord1) => coord.tileY.CompareTo(coord1.tileY));
        List<List<Coord>> newTileSets =
            new List<List<Coord>>();
        for (int i = 0; i <= (tilesSet[tilesSet.Count - 1].tileY - tilesSet[0].tileY)/ threshold; i++)
        {
            List<Coord> temps = new List<Coord>();
            newTileSets.Add(temps);
        }


        for (int i = 1; i < tilesSet.Count; i++)
        {
            newTileSets[(tilesSet[i].tileY - tilesSet[0].tileY) / threshold].Add(tilesSet[i]);
            
        }
        return newTileSets;
        
    }
    
    private GameObject Spawn(TilesCollection collection, GameObject prefab, bool isInEdge,Random random)
    {
        Vector3 position = isInEdge ? collection.GetRandomEdgPos(random) : collection.GetRandomPos(random);
        GameObject newObject = Instantiate(prefab, position,
            scanEnemyPrefab.transform.rotation);
        Vector3 lookDir = collection.GetDirection();
        newObject.transform.LookAt(lookDir);
        generatedEnemies.Add(newObject);
        return newObject;
    }

    private void Spawn(GameObject prefab, Vector3 pos, Vector3 center)
    {
        GameObject newObject = Instantiate(prefab, pos,
            scanEnemyPrefab.transform.rotation);
        newObject.transform.LookAt(center);
        generatedEnemies.Add(newObject);
    }
    private void SpawnDragon(MapGenerator mapGenerator, System.Random random)
    {
        for (int i = 0; i < 2; i++)
        {
            Vector3 pos = mapGenerator.SpawnPlaceInMapEdge(1, 2);
        
            if (pos == Vector3.one)
            {
                return;
            }
            GameObject prefab = Instantiate(dragon, pos, dragon.transform.rotation);
            generatedEnemies.Add(prefab);
        }

    }

    public void SpawnOnePatrol(Room room , Vector3 pos)
    {
        GameObject prefab = patrolEnemyPrefabs[0];
        prefab = Instantiate(prefab, pos, prefab.transform.rotation);
        PatrolEnemy newEnemy = prefab.GetComponent<PatrolEnemy>();
        newEnemy.InitPatrol(room);
        generatedEnemies.Add(prefab);
    }
    
}
