using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Stan.Osmos
{

    [Serializable]
    public class EntityModel
    {

        const double Delta13 = (double)1 / 3;
        const double Delta43 = (double)4 / 3;

        public float2 Position;
        public float2 Velocity;
        public float Capacity;

        public float Radius
        {
            get
            {
                return (float)math.pow(3 * Capacity / (4 * math.PI), Delta13);
            }
        }

        public void BounceWalls(Rect rect)
        {
            float radius = Radius * 0.5f;
            if (Position.x - radius < rect.x ||
                    Position.x + radius > rect.x + rect.width)
            {
                Velocity.x *= -1;
            }

            if (Position.y - radius < rect.y ||
                    Position.y + radius > rect.y + rect.height)
            {
                Velocity.y *= -1;
            }
        }

        internal void MoveForward(float delta)
        {
            // friction
            Position += Velocity * delta;
        }

        internal void Absorbing(int index, List<EntityModel> entities)
        {
            float selfRadius = Radius * 0.5f;
            for (int i = 0; i < entities.Count; ++i)
            {
                if (i == index) continue; // skip myself

                EntityModel entity = entities[i];

                if (Capacity > entity.Capacity)
                {
                    float entityRadius = entity.Radius * 0.5f;
                    var distance = math.distance(Position, entity.Position);
                    if (distance < entityRadius + selfRadius)
                    {
                        // calc capacity
                        var depthDistance = (entityRadius + selfRadius) - distance;
                        var smallCapacity = Delta43 * math.PI * math.pow(entityRadius * 2f - depthDistance, 3);

                        Capacity += entity.Capacity - (float)smallCapacity;
                        entity.Capacity = (float)smallCapacity;

                        entities[i] = entity; // struct(scalar)
                    }
                }
            }
        }
    }
}