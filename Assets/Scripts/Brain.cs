using System.Collections.Generic;
using UnityEngine;

public abstract class Brain : MonoBehaviour
{
    public abstract Direction makeMove(Player player, Map map);
}
