using System.Collections.Generic;
using UnityEngine;

public class Square : MonoBehaviour
{
    public int x;
    public int y;
    public Terrain terrainType;
    // Interface lists require custom Inspector support for component references.
    [System.NonSerialized] public List<Item> items = new List<Item>();

    public void addItem(Item item) => throw new System.NotImplementedException();
    public List<Item> getItems() => throw new System.NotImplementedException();
}
