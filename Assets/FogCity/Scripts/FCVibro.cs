using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILranch
{
    public class FCVibro : MonoBehaviour
    {
        //public float offset;
        public float power = 0.7f;

        Quaternion rotat;

        void Awake()
        {
            UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks * 1000);
        }

        // Start is called before the first frame update
        void Start()
        {
            rotat = transform.rotation;
        }

        // Update is called once per frame
        void Update()
        {
            transform.rotation = rotat;
            transform.Rotate(Random.Range(0, power), Random.Range(0, power), Random.Range(0, power));
        }
    }
}
