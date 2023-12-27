using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IPrototype
{
    IPrototype Clone();
}
public class ClickableTile : IPrototype
{
    public int X { get; set; }
    public int Y { get; set; }
    public string TileType { get; set; }


    public IPrototype Clone()
    {
        return new ClickableTile
        {
            X = this.X,
            Y = this.Y,
            TileType = this.TileType
        };
    }
}

public class ClickableTileScript : MonoBehaviour    
{
    public int tileX;
    public int tileY;
    public GameObject unitOnTile;
    public tileMapScript map;
    public ClickableTile CloneTile(ClickableTile originalTile)
    {
        return originalTile.Clone() as ClickableTile;
    }
}
