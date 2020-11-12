using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


namespace Stan.Osmos
{
    public class GameController : MonoBehaviour
    {
        private List<EntityModel> m_Entities;

        private GameView m_View;
        private InputController m_Input;
        private Rect m_Rect;

        private StateMachine m_StateMachine;
        public GameState State { get { return m_StateMachine.State; } }

        public GameSettings Settings;

        void Start()
        {
            m_StateMachine = new StateMachine();
            m_StateMachine.Add(GameState.Init, null, OnInitUpdate, null);
            m_StateMachine.Add(GameState.Menu, OnMenuStart, null, OnMenuExit);
            m_StateMachine.Add(GameState.Game, OnGameStart, OnGameUpdate, OnGameExit);

            m_StateMachine.SwitchState(GameState.Init);

            m_Entities = new List<EntityModel>();
            m_View = GetComponent<GameView>();

            m_Input = GetComponent<InputController>();
        }

        void OnInitUpdate()
        {
            Camera mainCamera = Camera.main;
            Vector3 zero = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, 0));
            Vector3 corner = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

            m_Rect = new Rect(zero.x, zero.y, corner.x - zero.x, corner.y - zero.y);

            Settings.LoadFromFile();

            m_StateMachine.SwitchState(GameState.Menu);
        }

        void OnMenuStart()
        {
            m_View.MenuCanvas.SetActive(true);
            m_View.StartButton.onClick.AddListener(() => 
            {
                m_StateMachine.SwitchState(GameState.Game);
            });
        }

        void OnMenuExit()
        {
            m_View.StartButton.onClick.RemoveAllListeners();
            m_View.MenuCanvas.SetActive(false);
        }

        void OnGameStart()
        {

            Cursor.visible = false;

            // add self entity
            m_Entities.Add(new EntityModel
            {
                Capacity = Settings.User.Capacity,
                Position = new float2(),
                Velocity = new float2()
            });

            // add enemy eneitites
            var posRandom = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(0, int.MaxValue));
            var a = m_Rect.x + 0.2f;
            var b = m_Rect.x + m_Rect.width - 0.2f;

            for (int i = 0; i < Settings.Enemy.Count; ++i)
            {
                var random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(0, int.MaxValue));

                float vDirX = random.NextFloat(-10f, 10f) > 0 ? 1 : -1;
                float vDirY = random.NextFloat(-10f, 10f) > 0 ? 1 : -1;

                var model = new EntityModel
                {
                    Capacity = random.NextFloat(Settings.Enemy.Capacity.x, Settings.Enemy.Capacity.y),
                    Position = new float2(
                        (a + (b - a) * posRandom.NextFloat()),
                        (a + (b - a) * posRandom.NextFloat()) * (m_Rect.height * 0.4f / b)
                    ),
                    Velocity = new float2(
                        random.NextFloat(Settings.Enemy.Velocity.x, Settings.Enemy.Velocity.y) * vDirX,
                        random.NextFloat(Settings.Enemy.Velocity.x, Settings.Enemy.Velocity.y) * vDirY
                    )
                };
                m_Entities.Add(model);
            }

            // visual Instantiate
            m_View.Instantiate(m_Entities);
            m_Input.Force += OnForceApply;
        }

        void OnGameUpdate()
        {
            var removed = new List<int>();
            for (int i = 0; i < m_Entities.Count; ++i)
            {
                // logic
                EntityModel model = m_Entities[i];

                model.Absorbing(i, m_Entities);
                model.BounceWalls(m_Rect);
                model.MoveForward(Time.deltaTime);

                m_Entities[i] = model;

                // visual
                if (model.Capacity > 0.0002f)
                {
                    var deltaCapacity = (model.Capacity / m_Entities[0].Capacity) * 0.5f;
                    var clampDelta = math.clamp(deltaCapacity, 0, 1);
                    Color clampColor = i == 0 ? Settings.User.Color : Settings.Enemy.Lerp(clampDelta);
                    m_View.RedrawAt(i, model, clampColor);
                }
                else
                {
                    // capacity very small = remove entity
                    if (i == 0)
                    {
                        m_View.ScoreCaption("Fail");
                        m_StateMachine.SwitchState(GameState.Menu);
                        return;
                    }
                    removed.Add(i);
                }
            }

            float capacity = 0;
            var entities = new List<EntityModel>();
            for (int i = 0; i < m_Entities.Count; ++i)
            {
                EntityModel model = m_Entities[i];
                if (!removed.Contains(i))
                {
                    entities.Add(model);
                    capacity += model.Capacity;
                }
            }

            m_Entities = entities;
            m_View.Remove(removed);

            // win condition
            if (capacity * 0.5f < m_Entities[0].Capacity)
            {
                m_View.ScoreCaption("Win");
                m_StateMachine.SwitchState(GameState.Menu);
                return;
            }

            var caption = $"Score: {math.floor(m_Entities[0].Capacity * 100f)} / {math.floor(capacity * 100f)}";
            m_View.ScoreCaption(caption);
        }

        void OnGameExit()
        {
            m_Entities.Clear();
            m_View.Dispose();
            m_Input.Force -= OnForceApply;
            Cursor.visible = true;
        }

        private void OnForceApply(float force, float2 position)
        {
            if(m_StateMachine.State == GameState.Game)
            {
                var selfEntity = m_Entities[0];
                var removedCapacity = force * Settings.ForceCapacity;
                float2 direction = math.normalize(selfEntity.Position - position);
                var additionVelocity = new float2(direction.x * force, direction.y * force);

                if (selfEntity.Capacity > 0.05f)
                {
                    selfEntity.Capacity -= removedCapacity;
   
                    var model = new EntityModel
                    {
                        Capacity = removedCapacity,
                        Velocity = additionVelocity * -20f
                    };
                    model.Position = selfEntity.Position + math.normalize(model.Velocity) * (selfEntity.Radius * 0.5f + model.Radius * 0.5f);

                    m_Entities.Add(model);
                    m_View.Instantiate(model);
                }

                selfEntity.Velocity += additionVelocity;
                m_Entities[0] = selfEntity;
                m_View.RedrawAt(0, selfEntity, Settings.User.Color);
            }            
        }

        void Update()
        {
            m_StateMachine.Update();
        }
    }

}