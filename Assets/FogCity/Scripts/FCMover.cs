using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILranch
{
    public class FCMover : MonoBehaviour
    {
        public float Speed = 0;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            transform.Translate(Speed * Time.deltaTime, 0, 0);
        }
    }
}
