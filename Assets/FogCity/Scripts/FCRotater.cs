using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILranch
{
    public class FCRotater : MonoBehaviour
    {
        public float SpeedRotate;
        bool Mode;

        // Start is called before the first frame update
        void Start()
        {
            if (UnityEngine.Random.Range(0, 100) < 50)
            {
                Mode = true;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Mode)
            {
                transform.Rotate(0, 0, SpeedRotate * Time.deltaTime);
            }
            else
            {
                transform.Rotate(0, 0, -SpeedRotate * Time.deltaTime);
            }
        }
    }
}
