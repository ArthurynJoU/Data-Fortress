using System.Collections.Generic;
using UnityEngine;

public class GameBoard: MonoBehaviour
{
    public static GameBoard Instance { get; private set; }
    
    [SerializeField]
    private float _tileSize = 1f;

    [SerializeField]
    private LayerMask _wallMask;
    [SerializeField] 
    private LayerMask _enemySpawnerMask;
    [SerializeField]
    private LayerMask _serverMask;
    [SerializeField]
    private LayerMask _towerPlaceMask;

    private Tile[,] _grid;
    private int _width;
    private int _height;

    public Tile StartPoint { get; private set; } 
    public Tile EndPoint { get; private set; } 

    private void Awake()
    {
        if ( Instance == null )
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Initialize( int width, int height )
    {
        _width = width;
        _height = height;
        _grid = new Tile[width, height];

        Vector3 startPosition = transform.position;

        for ( int x = 0; x < _width; x++ )
        {
            for ( int y = 0; y < _height; y++ )
            {
                /* * Crucial Coordinate Mapping Note:
                 * We are iterating through a 2D logical array where 'x' is width and 'y' is height.
                 * However, in Unity's 3D space, the horizontal plane is defined by the X and Z axes.
                 * Therefore, our logical 'y' is mapped to the physical 'Z' axis, 
                 * whilst the physical 'Y' axis (vertical elevation) is kept at 0.
                 */
                Vector3 worldPosition = startPosition + new Vector3(x * _tileSize, 0, y * _tileSize);
                _grid[x, y] = new Tile(new Vector2Int(x, y), worldPosition);

                // Analyse the physical world at these coordinates to determine the tile's purpose
                // so that there are no problems later on with hosting the game entities
                bool isWall = Physics.CheckSphere(worldPosition, 0.4f, _wallMask);
                bool isEnemySpawner = Physics.CheckSphere(worldPosition, 0.4f, _enemySpawnerMask);
                bool isServer = Physics.CheckSphere(worldPosition, 0.4f, _serverMask);
                bool isTowerPlace = Physics.CheckSphere(worldPosition, 0.4f, _towerPlaceMask);

                if ( isWall )
                {
                    _grid[x, y].Type = Tile.TileType.Wall;
                }
                else if ( isEnemySpawner )
                {
                    _grid[x, y].Type = Tile.TileType.EnemySpawner;
                    StartPoint = new Tile(new Vector2Int(x, y), worldPosition);
                }
                else if ( isServer )
                {
                    _grid[x, y].Type = Tile.TileType.Server;
                    EndPoint = new Tile(new Vector2Int(x, y), worldPosition);
                }
                else if ( isTowerPlace )
                {
                    _grid[x, y].Type = Tile.TileType.Tower;
                    _grid[x, y].isTowerFree = true;
                }
                else
                {
                    _grid[x, y].Type = Tile.TileType.Nothing;
                }
            }
        }

        EnemyCollection.Instance.UpdateAllRoutes();
    }

    /// <summary>
    /// BFS algorithm.
    /// Searches for the shortest path around the towers. 
    /// </summary>
    public List<Vector3> FindPaths(Vector2Int startCoordinations, Vector2Int targetCoordinations)
    {
        Tile startTile = GetTile(startCoordinations);
        Tile targetTile = GetTile(targetCoordinations);

        if ( startTile == null || targetTile == null )
        {
            return null;
        }

        // queue for tile inspection
        Queue<Tile> _searchQueue = new Queue<Tile>();
        _searchQueue.Enqueue(startTile);

        // a dictionary that remembers how we got to this point 
        Dictionary<Tile, Tile> _navigationMap = new Dictionary<Tile, Tile>();
        _navigationMap[startTile] = startTile; 

        while ( _searchQueue.Count > 0 )
        {
            Tile current = _searchQueue.Dequeue();
            
            if ( current == targetTile )
            {
                break;
            }

            foreach ( Tile next in GetNeighbors(current) )
            {
                if ( (next.isWalkable() || next == targetTile) && !_navigationMap.ContainsKey(next) )
                {
                    _searchQueue.Enqueue(next);
                    _navigationMap[next] = current;
                }
            }
        }

        if ( !_navigationMap.ContainsKey(targetTile) )
        {
            return null;
        }

        List<Vector3> path = new List<Vector3>();
        Tile currentPathTile = targetTile;

        while ( currentPathTile != startTile )
        {
            path.Add(currentPathTile.WorldPosition);
            currentPathTile = _navigationMap[currentPathTile];
        }

        path.Add(startTile.WorldPosition);
        path.Reverse(); // flip it over so that the path goes from start to finish

        return path;
    }

    private List<Tile> GetNeighbors(Tile tile)
    {
        List<Tile> neighbors = new List<Tile>();
        Vector2Int[] directions = 
        {
            new Vector2Int(0, 1),   
            new Vector2Int(1, 0),   
            new Vector2Int(0, -1),  
            new Vector2Int(-1, 0),  
            new Vector2Int(1, 1),   
            new Vector2Int(1, -1),  
            new Vector2Int(-1, -1), 
            new Vector2Int(-1, 1)   
        };

        foreach ( var dir in directions )
        {
            Tile neighbor = GetTile(tile.Coordinates + dir);

            if ( neighbor != null && neighbor.isWalkable() )
            {
                if ( dir.x != 0 && dir.y != 0 )
                {
                    Tile side1 = GetTile(new Vector2Int(tile.Coordinates.x + dir.x, tile.Coordinates.y));
                    Tile side2 = GetTile(new Vector2Int(tile.Coordinates.x, tile.Coordinates.y + dir.y));

                    if ( side1 == null || !side1.isWalkable() || side2 == null || !side2.isWalkable() )
                    {
                        continue;
                    }
                }
                neighbors.Add(neighbor);
            }
        }
        
        return neighbors;
    }

    // works with integers
    public Tile GetTile(Vector2Int coordinates)
    {
        if ( coordinates.x >= 0 && coordinates.x < _width && coordinates.y >= 0 && coordinates.y < _height ) 
        {
            return _grid[coordinates.x, coordinates.y];
        }

        return null;
    }

    // works with Unity coordinates in metres (decimal numbers)
    public Tile GetTileFromPosition(Vector3 worldPosition)
    {
        Vector3 localPosition = worldPosition;
        int x = Mathf.RoundToInt(localPosition.x / _tileSize);
        int y = Mathf.RoundToInt(localPosition.z / _tileSize);
        
        return GetTile(new Vector2Int(x, y));
    }

    public bool IsTileValidForPlacement(Tile tile)
    {
        if ( tile == null || !tile.isWalkable() )
        {
            return false;
        }
        
        Tile.TileType firstTileType = tile.Type;
        tile.Type = Tile.TileType.Wall;
        
        var path = FindPaths(StartPoint.Coordinates, EndPoint.Coordinates);
        bool isPathValid = false;
        
        if ( path != null && path.Count > 0 )
        {
            isPathValid = true;
        }

        tile.Type = firstTileType;
        return isPathValid;
    }
}