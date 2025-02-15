using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore(typeof(CartesianGridMoveForwardSystem))]
public unsafe partial class CartesianGridOnCubeSnapToFaceSystem : SystemBase
{
    EntityQuery m_GridQuery;

    // No inf available in Unity.Mathematics
    const float m_INF = 1.0f / 0.0f;

    protected override void OnCreate()
    {
        m_GridQuery = GetEntityQuery(ComponentType.ReadOnly<CartesianGridOnCube>());
        RequireForUpdate(m_GridQuery);
    }

    protected override void OnUpdate()
    {
        // Get component data from the Grid (GridCube or GridCube)
        var cartesianGridCube = SystemAPI.GetSingleton<CartesianGridOnCube>();
        var faceLocalToWorld = (float4x4*)cartesianGridCube.Blob.Value.FaceLocalToWorld.GetUnsafePtr();
        var faceWorldToLocal = (float4x4*)cartesianGridCube.Blob.Value.FaceWorldToLocal.GetUnsafePtr();

        // Offset center to grid cell
        var inf = m_INF;

        // Anything that's placed with CartesianGridDirection but does not have CubeFace assigned, needs to
        // find appropriate CubeFace, add it and convert transform into local space of face.
        Entities
            .WithName("Find_CubeFace")
            .WithStructuralChanges()
            .WithNativeDisableUnsafePtrRestriction(faceLocalToWorld)
            .WithNativeDisableUnsafePtrRestriction(faceWorldToLocal)
            .WithNone<CartesianGridOnCubeFace>()
            .WithAll<CartesianGridDirection>()
            .ForEach((Entity entity,
#if !ENABLE_TRANSFORM_V1
                ref LocalTransform transform,
#else
                ref Translation translation,
#endif
                ref CartesianGridCoordinates gridCoordinates) =>
                {
                    // Find closest GridFace
                    var bestDist = inf;
                    var bestCubeFace = 0;
                    for (int i = 0; i < 6; i++)
                    {
                        var n = faceLocalToWorld[i].c1.xyz;
                        var p = faceLocalToWorld[i].c3.xyz;
                        var d = -math.dot(n, p);
#if !ENABLE_TRANSFORM_V1
                        var dist = math.dot(transform.Position.xyz, n) + d;
#else
                        var dist = math.dot(translation.Value.xyz, n) + d;
#endif
                        if (math.abs(dist) < bestDist)
                        {
                            bestDist = dist;
                            bestCubeFace = i;
                        }
                    }

                    // Put translation in GridFace space
#if !ENABLE_TRANSFORM_V1
                    transform.Position = math.mul(faceWorldToLocal[bestCubeFace], new float4(transform.Position.xyz, 1.0f)).xyz;
#else
                    translation.Value = math.mul(faceWorldToLocal[bestCubeFace], new float4(translation.Value.xyz, 1.0f)).xyz;
#endif
                    EntityManager.AddComponentData(entity, new CartesianGridOnCubeFace { Value = (byte)bestCubeFace });
                }).Run();
    }
}
