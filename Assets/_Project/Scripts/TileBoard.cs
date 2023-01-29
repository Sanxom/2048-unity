using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBoard : MonoBehaviour
{
    [SerializeField] private TileState[] _tileStates;
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private GameManager gameManager;

    private TileGrid _grid;
    private List<Tile> _tiles;
    private bool _waiting;

    private void Awake()
    {
        _grid = GetComponentInChildren<TileGrid>();
        // Allocating 16 tiles worth of memory for small performance gains
        _tiles = new List<Tile>(16);
    }

    private void Update()
    {
        if (!_waiting)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                MoveTiles(Vector2Int.up, 0, 1, 1, 1);
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                MoveTiles(Vector2Int.down, 0, 1, _grid.Height - 2, -1);
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                MoveTiles(Vector2Int.left, 1, 1, 0, 1);
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                MoveTiles(Vector2Int.right, _grid.Width - 2, -1, 0, 1);
            }
        }
    }

    public void ClearBoard()
    {
        foreach (var cell in _grid.Cells)
        {
            cell.Tile = null;
        }

        foreach (var tile in _tiles)
        {
            Destroy(tile.gameObject);
        }

        _tiles.Clear();
    }

    public void CreateTile()
    {
        Tile tile = Instantiate(_tilePrefab, _grid.transform);
        tile.SetState(_tileStates[0], 2);
        tile.Spawn(_grid.GetRandomEmptyCell());
        _tiles.Add(tile);
    }

    private void MoveTiles(Vector2Int direction, int startX, int incrementX, int startY, int incrementY)
    {
        bool changed = false;
        for (int x = startX; x >= 0 && x < _grid.Width; x += incrementX)
        {
            for (int y = startY; y >= 0 && y < _grid.Height; y += incrementY)
            {
                TileCell cell = _grid.GetCell(x, y);

                if (cell.Occupied)
                {
                    changed |= MoveTile(cell.Tile, direction);
                }
            }
        }

        if (changed)
            StartCoroutine(WaitForChanges());
    }

    /// <summary>
    /// Moves a tile in the direction specified by the player until it either finds a tile to merge with or finds the edge of the grid.
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="direction"></param>
    private bool MoveTile(Tile tile, Vector2Int direction)
    {
        TileCell newCell = null;
        TileCell adjacent = _grid.GetAdjacentCell(tile.Cell, direction);

        while(adjacent != null)
        {
            if(adjacent.Occupied)
            {
                if (CanMerge(tile, adjacent.Tile))
                {
                    Merge(tile, adjacent.Tile);
                    return true;
                }
                break;
            }

            newCell = adjacent;
            adjacent = _grid.GetAdjacentCell(adjacent, direction);
        }

        if(newCell != null)
        {
            tile.MoveTo(newCell);
            return true;
        }

        return false;
    }

    private void Merge(Tile a, Tile b)
    {
        _tiles.Remove(a);
        a.Merge(b.Cell);

        int index = Mathf.Clamp(IndexOf(b.State) + 1, 0, _tileStates.Length - 1);
        int number = b.Number * 2;

        b.SetState(_tileStates[index], number);

        gameManager.IncreaseScore(number);
    }

    private bool CanMerge(Tile a, Tile b)
    {
        return a.Number == b.Number && !b.Locked;
    }

    private int IndexOf(TileState state)
    {
        for (int i = 0; i < _tileStates.Length; i++)
        {
            if (state == _tileStates[i])
                return i;
        }

        return -1;
    }

    private IEnumerator WaitForChanges()
    {
        _waiting = true;

        yield return new WaitForSeconds(0.1f);

        _waiting = false;

        foreach (var tile in _tiles)
            tile.Locked = false;

        if(_tiles.Count != _grid.Size)
            CreateTile();

        if (CheckForGameOver())
            gameManager.GameOver();
    }

    private bool CheckForGameOver()
    {
        if (_tiles.Count != _grid.Size)
            return false;

        foreach (var tile in _tiles)
        {
            TileCell up = _grid.GetAdjacentCell(tile.Cell, Vector2Int.up);
            TileCell down = _grid.GetAdjacentCell(tile.Cell, Vector2Int.down);
            TileCell left = _grid.GetAdjacentCell(tile.Cell, Vector2Int.left);
            TileCell right = _grid.GetAdjacentCell(tile.Cell, Vector2Int.right);

            if (up != null && CanMerge(tile, up.Tile))
                return false;
            if (down != null && CanMerge(tile, down.Tile))
                return false;
            if (left != null && CanMerge(tile, left.Tile))
                return false;
            if (right != null && CanMerge(tile, right.Tile))
                return false;
        }

        return true;
    }
}