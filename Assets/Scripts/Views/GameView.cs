using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;


namespace Stan.Osmos
{
    public class GameView : MonoBehaviour
    {
        public Button StartButton;
        public GameObject EntityPrefab;
        public GameObject MenuCanvas;
        public GameObject GameSpace;
        public GameObject GameCursor;
        public Text ScoreText;

        private List<GameObject> m_GameObjects;
        private Camera m_Camera;

        void Start()
        {
            m_Camera = Camera.main;
            m_GameObjects = new List<GameObject>();
        }

        void Update()
        {
            if (m_GameObjects.Count > 0)
            {
                Vector3 dir = (GameCursor.transform.position - m_GameObjects[0].transform.position).normalized;
                var angle = math.atan2(dir.y, dir.x);
                GameCursor.transform.rotation = quaternion.AxisAngle(new float3(0, 0, 1), angle);

                Vector3 position = m_Camera.ScreenToWorldPoint(Input.mousePosition);
                position.z = 0;

                GameCursor.transform.position = position;
            }
        }


        public void Instantiate(List<EntityModel> models)
        {
            for (int i = 0; i < models.Count; ++i)
            {
                Instantiate(models[i]);
            }
        }

        public void Instantiate(EntityModel model)
        {

            GameObject viewModel = Instantiate(EntityPrefab, GameSpace.transform);
            m_GameObjects.Add(viewModel);

            RedrawAt(m_GameObjects.Count - 1, model, Color.white);
        }

        public void RedrawAt(int index, EntityModel model, Color color)
        {
            GameObject viewModel = m_GameObjects[index];
            float radius = model.Radius * 0.4166666666666667f;
            viewModel.transform.localPosition = new Vector3(model.Position.x, model.Position.y, 0);
            viewModel.transform.localScale = new Vector3(radius, radius, radius);
            viewModel.transform.Rotate(0, 0, Time.deltaTime * 2f);
            var render = viewModel.GetComponent<SpriteRenderer>();
            render.color = color;
        }

        public void Remove(List<int> removed)
        {
            var objects = new List<GameObject>();
            for (int i = 0; i < m_GameObjects.Count; ++i)
            {
                if (!removed.Contains(i))
                {
                    objects.Add(m_GameObjects[i]);
                }
                else
                {
                    DestroyImmediate(m_GameObjects[i]);
                }
            }
            m_GameObjects = objects;
        }

        public void Dispose()
        {
            for (int i = 0; i < m_GameObjects.Count; ++i)
            {
                DestroyImmediate(m_GameObjects[i]);
            }
            m_GameObjects.Clear();
        }

        public void ScoreCaption(string value)
        {
            ScoreText.text = value;
        }
    }
}
