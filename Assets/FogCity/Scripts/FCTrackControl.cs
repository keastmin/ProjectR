using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ILranch
{
    public class FCTrackControl : MonoBehaviour
    {
        public enum State
        {
            Stopped = 0,
            Forward = 1,
            Backward = 2,
        }
        public State _State = State.Stopped;
        public float Speed = 1f;
        public Animator _Animator;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnValidate()
        {
            if (_State == State.Stopped)
            {
                //_Animator.speed = 0;
            }
            else if (_State == State.Forward)
            {
                //_Animator.CrossFade("Base Layer.Forward", 1.1f);
                _Animator.Play("Base Layer.Forward");
                _Animator.speed = Speed;
            }
            else
            {
                //_Animator.CrossFade("Base Layer.Backward", 1.1f);
                _Animator.Play("Base Layer.Backward");
                _Animator.speed = Speed;
            }
        }
    }
}
