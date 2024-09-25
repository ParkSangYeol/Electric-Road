using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects.Stage
{
    public class PathStruct
    {
        public TileNode path;
        private TileNode last;
        public float cost = float.MaxValue;

        public TileNode GetLast()
        {
            if (last == null)
            {
                last = path;
            }

            while (last.nexts.Count > 0)
            {
                last = last.nexts[0];
            }

            return last;
        }

        public void AddFirst(PathStruct addingPath)
        {
            if (addingPath.path == null || path == null)
            {
                return;
            }
                    
            path.nexts.Add(addingPath.path.Copy());
        }
                
        public void AddLast(PathStruct addingPath)
        {
            if (addingPath.path == null || path == null)
            {
                return;
            }
                    
            GetLast().nexts.Add(addingPath.path.Copy());
        }
                
        public override string ToString()
        {
            string str = "" + cost + '\n';
            PrintNodeInfo(path, ref str);
            return str;
        }

        private void PrintNodeInfo(TileNode node, ref string str)
        {
            if (node == null)
            {
                return;
            }
            str = str + (node.x + ", " + node.y + " | " +node.dir + ", " + node.tile + "\n");
            foreach (var n in node.nexts)
            {
                PrintNodeInfo(n, ref str);
            }
        }
    }
}