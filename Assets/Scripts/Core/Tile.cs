using UnityEngine;

public class Tile
{
    public Vector2Int Coordinates;
    public Vector3 WorldPosition;
    public TileType Type = TileType.Nothing;
    public bool isTowerFree = false;

    public Tile(Vector2Int coordinates, Vector3 worldPosition)
    {
        Coordinates = coordinates;
        WorldPosition = worldPosition;
    }

    // the division into types is necessary to prevent bugs relating to the placement of towers and effects on the board
    public enum TileType
    {
        Nothing,        
        Tower,          
        EnemySpawner,
        Server,
        Wall            
    }

    public bool isWalkable()
    {
        if ( Type != TileType.Wall ) return true;
        return false;
    }
}