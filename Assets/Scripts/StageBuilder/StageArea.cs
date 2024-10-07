using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StageBuilder
{
    public class StageArea : MonoBehaviour
    {
        [ShowInInspector, ReadOnly]
        public int width;
        [ShowInInspector, ReadOnly]
        public int height;
    }
}