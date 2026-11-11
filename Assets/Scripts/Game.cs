using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public Map map;
    public Player player;
    public Difficulty difficulty;
    public int turn;

    public void runGame() => throw new System.NotImplementedException();
    public void playTurn() => throw new System.NotImplementedException();
    public bool hasWon() => throw new System.NotImplementedException();
}
