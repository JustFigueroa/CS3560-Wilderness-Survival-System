using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public int width;
    public int height;
    public Difficulty difficulty;
    // Preserve the diagram type; jagged arrays are not Inspector-serializable.
    [System.NonSerialized] public Square[][] map;

    public void generateMap(Difficulty difficulty) => throw new System.NotImplementedException();
    public Square getSquare(int x, int y) => throw new System.NotImplementedException();
    public bool isInside(int x, int y) => throw new System.NotImplementedException();
}
