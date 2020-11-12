using System;
using Unity.Mathematics;
using UnityEngine;


namespace Stan.Osmos
{
    public class InputController : MonoBehaviour
    {
        private Camera m_Camera;
        private GameController m_Controller;
        private float m_Force;
        public float ForcePower = 1f;

        public Action<float, float2> Force;

        void Start()
        {
            m_Camera = Camera.main;
            m_Controller = GetComponent<GameController>();
        }

        void Update()
        {
            if(m_Controller.State == GameState.Game)
            {
                if (Input.GetMouseButton(0))
                {
                    m_Force += Time.deltaTime;
                }

                if (Input.GetMouseButtonUp(0))
                {
                    if(m_Force > 0)
                    {
                        Vector3 position = m_Camera.ScreenToWorldPoint(Input.mousePosition);
                        Force.Invoke(m_Force * ForcePower, new float2(position.x, position.y));
                        m_Force = 0;
                    }                    
                }
            }
            
        }
    }
}
