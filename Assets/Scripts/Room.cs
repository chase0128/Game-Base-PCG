using System.Collections.Generic;
using UnityEngine;
using System;
using Random = System.Random;

public struct Coord
{
    public int tileX;
    public int tileY;

    public Coord(int x, int y)
    {
        tileX = x;
        tileY = y;
    }
}

public class TilesCollection
{
    public List<Coord> tiles ;
    public List<Coord> edgeTiles ;

    public Coord? center;
    
    public Vector3 GetDirection()
    {
        if (!center.HasValue)
        {
            return Vector3.zero;
        }

        return MapGenerator.CoordToWorldPoint(center.Value);
    }
    
    public Vector3 GetRandomPos(System.Random random )
    {
        if (tiles.Count == 0)
        {
            return MapGenerator.CoordToWorldPoint(
                MapGenerator.instance.mapRoomTiles[random.Next(0, MapGenerator.instance.mapRoomTiles.Count)]);
        }
        Coord tile = tiles[random.Next(0, tiles.Count)];
        Vector3 pos = MapGenerator.CoordToWorldPoint(tile);
        pos.y = -MapGenerator.wallHeight;
        return pos;
    }

    public int GetSize()
    {
        return tiles.Count + edgeTiles.Count;
    }
    
    public Vector3 GetRandomEdgPos(Random random)
    {
        
        return MapGenerator.CoordToWorldPoint(edgeTiles[random.Next(0, edgeTiles.Count)]);
    }
    
    
    public int GetMaxX()
    {
        List<Coord> sortedTiles = CollectAllTils();
        edgeTiles.Sort(((coord, coord1) => -coord.tileX.CompareTo(coord1.tileX)));
        return edgeTiles[0].tileX;
    }
    public int GetMinX()
    {
        List<Coord> sortedTiles = CollectAllTils();
        edgeTiles.Sort(((coord, coord1) => coord.tileX.CompareTo(coord1.tileX)));
        return edgeTiles[0].tileX;
    }
    public int GetMaxY()
    {
        List<Coord> sortedTiles = CollectAllTils();
        edgeTiles.Sort(((coord, coord1) => -coord.tileY.CompareTo(coord1.tileY)));
        return edgeTiles[0].tileY;
    }
    
    public int GetMinY()
    {
        List<Coord> sortedTiles = CollectAllTils();
        edgeTiles.Sort((coord, coord1) => coord.tileY.CompareTo(coord1.tileY));
        return edgeTiles[0].tileY;
    }

    public List<Coord> SortedByX()
    {
        List<Coord> sortedList = CollectAllTils();
        sortedList.Sort((coord, coord1) => coord.tileX.CompareTo(coord1.tileX));
        return sortedList;
    }
    
    public List<Coord> CollectAllTils()
    {
        List<Coord> allTiles = new List<Coord>();
        allTiles.AddRange(tiles);
        allTiles.AddRange(edgeTiles);
        return allTiles;
    }
    
    
}


public class  Edge:TilesCollection,IComparable<Edge>
{
    public Room startRoom;
    public Room endRoom;
    public int distance;
    public Coord startTile;
    public Coord endTile;

    public Edge(Room roomA, Room roomB,Coord tileA,Coord tileB, int distance, int[,] map)
    {
        startRoom = roomA;
        endRoom = roomB;
        this.distance = distance;
        startTile = tileA;
        endTile = tileB;

        center = null;
    }
    public void SetTiles(List<Coord> tiles, int[,] map)
    {
        this.tiles = tiles;
        edgeTiles = new List<Coord>();
        foreach (Coord tile in tiles)
        {
            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (x == tile.tileX || y == tile.tileY)
                    {
                        if (x>0 && x < map.GetLength(0) &&  y>0 && y < map.GetLength(1) && map[x, y] == 1) 
                        {
                            edgeTiles.Add(tile);
                        }
                    }
                }
            }
        }
    }
    public int CompareTo(Edge otherEdge)
    {
        return otherEdge.distance.CompareTo(distance);
    }


}

public class Room:TilesCollection,IComparable<Room>
    {
        
        public List<Room> connectedRooms;
        public List<Coord> centerTiles;
        public int roomSize;

        public Room()
        {
            
        }

        public Room(List<Coord> roomTiles)
        {
            tiles = roomTiles;
        }
        public Room(List<Coord> roomTiles, int[,] map)
        {
            tiles = roomTiles;
            roomSize = roomTiles.Count;
            connectedRooms = new List<Room>();
            center = null;
            edgeTiles = new List<Coord>();
            
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
                                edgeTiles.Add(tile);
                            }
                        }
                    }
                }
            }

            centerTiles = new List<Coord>(tiles);
            foreach (Coord tile in edgeTiles)
            {
                centerTiles.Remove(tile);
            }
            center = centerTiles[GameManager.random.Next(0, centerTiles.Count)];
        }
        
        public Coord GetRandomRoomEdge(Random random)
        {
            return edgeTiles[random.Next(0, edgeTiles.Count)];
        }
        
        public static void ConnectRoom(Room roomA, Room roomB)
        {
            roomA.connectedRooms.Add(roomB);
            roomB.connectedRooms.Add(roomA);
        }

        public  bool IsConnected(Room otherRoom)
        {
            return connectedRooms.Contains(otherRoom);
        }
        
        public int CompareTo(Room otherRoom)
        {
            return  tiles.Count.CompareTo(otherRoom.tiles.Count);
        }
    }

