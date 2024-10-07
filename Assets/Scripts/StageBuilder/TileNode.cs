using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects.Stage
{
    public class TileNode
    {
        public Direction dir;
        public Tile tile;
        public int x, y;

        public List<TileNode> nexts;

        public TileNode(TileNode node)
        {
            dir = node.dir;
            tile = node.tile;
            x = node.x;
            y = node.y;
                    
            nexts = new List<TileNode>();
        }

        public TileNode(int x, int y, Direction dir, Tile tile)
        {
            this.x = x;
            this.y = y;
            this.dir = dir;
            this.tile = tile;
                    
            nexts = new List<TileNode>();
        }

        public TileNode Copy()
        {
            TileNode copyNode = new TileNode(this);
            foreach (var next in this.nexts)
            {
                if (next != null)
                {
                    copyNode.nexts.Add(next.Copy());
                }
            }

            return copyNode;
        }

        public TileNode GetLast()
        {
            if (nexts.Count == 0)
            {
                return this;
            }

            return nexts[0].GetLast();
        }
    }
}