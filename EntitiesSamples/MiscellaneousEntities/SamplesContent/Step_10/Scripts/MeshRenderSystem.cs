using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Advanced.BlobAssets
{
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    [BurstCompile]
    public partial struct MeshRenderSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MeshBBComponent>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new DrawBBJob().ScheduleParallel();
        }

        [BurstCompile]
        public partial struct DrawBBJob : IJobEntity
        {
            public void Execute(in MeshBBComponent meshBB, in WorldTransform t)
            {
                if (!meshBB.BlobData.IsCreated)
                {
                    return;
                }

                var min = meshBB.BlobData.Value.MinBoundingBox;
                var max = meshBB.BlobData.Value.MaxBoundingBox;

                var pos = t.Position;

                // Draw a bounding box
                Debug.DrawLine(pos + new float3(min.x, min.y, min.z), pos + new float3(max.x, min.y, min.z));
                Debug.DrawLine(pos + new float3(min.x, max.y, min.z), pos + new float3(max.x, max.y, min.z));
                Debug.DrawLine(pos + new float3(min.x, min.y, min.z), pos + new float3(min.x, max.y, min.z));
                Debug.DrawLine(pos + new float3(max.x, min.y, min.z), pos + new float3(max.x, max.y, min.z));

                Debug.DrawLine(pos + new float3(min.x, min.y, max.z), pos + new float3(max.x, min.y, max.z));
                Debug.DrawLine(pos + new float3(min.x, max.y, max.z), pos + new float3(max.x, max.y, max.z));
                Debug.DrawLine(pos + new float3(min.x, min.y, max.z), pos + new float3(min.x, max.y, max.z));
                Debug.DrawLine(pos + new float3(max.x, min.y, max.z), pos + new float3(max.x, max.y, max.z));

                Debug.DrawLine(pos + new float3(min.x, min.y, min.z), pos + new float3(min.x, min.y, max.z));
                Debug.DrawLine(pos + new float3(max.x, min.y, min.z), pos + new float3(max.x, min.y, max.z));
                Debug.DrawLine(pos + new float3(min.x, max.y, min.z), pos + new float3(min.x, max.y, max.z));
                Debug.DrawLine(pos + new float3(max.x, max.y, min.z), pos + new float3(max.x, max.y, max.z));
            }
        }
    }
}
