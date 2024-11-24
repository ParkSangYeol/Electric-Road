using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameStage
{
    public class StarController : MonoBehaviour
    {
        [SerializeField] 
        private GameObject starFilled;

        public void SetStar(bool filled)
        {
            gameObject.SetActive(true);
            if (filled)
            {
                starFilled.SetActive(true);
            }
        }
    }
}
