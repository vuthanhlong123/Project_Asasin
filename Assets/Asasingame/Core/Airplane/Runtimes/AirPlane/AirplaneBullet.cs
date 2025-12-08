using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Collections;
using Unity.Burst;

// Component dữ liệu cho rocket
public struct Rocket : IComponentData
{
    public float3 Velocity;
    public float Radius;
    public float MaxDistance;
}

// Struct để lưu kết quả va chạm
public struct RocketHit
{
    public Entity RocketEntity;
    public Entity TargetEntity;
    public float3 HitPosition;
}

// Job xử lý rocket
[BurstCompile]
public partial struct RocketJob : IJobEntity
{
    [ReadOnly] public PhysicsWorld PhysicsWorld;
    public float DeltaTime;
    public NativeQueue<RocketHit>.ParallelWriter HitQueue;

    void Execute(Entity entity, ref LocalTransform transform, in Rocket rocket)
    {
        float3 start = transform.Position;
        float3 end = start + rocket.Velocity * DeltaTime;
        float3 direction = math.normalize(rocket.Velocity);

        if (PhysicsWorld.SphereCast(start, 1f, direction, 10, out ColliderCastHit hit, CollisionFilter.Default))
        {
            // Ghi kết quả vào queue
            HitQueue.Enqueue(new RocketHit
            {
                RocketEntity = entity,
                TargetEntity = hit.Entity,
                HitPosition = hit.Position
            });

            // Dừng rocket tại vị trí va chạm
            transform.Position = hit.Position;
        }
        else
        {
            transform.Position = end;
        }
    }
}

// System quản lý rocket
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct RocketSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Rocket>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;

        // Tạo NativeQueue để lưu kết quả va chạm
        var hitQueue = new NativeQueue<RocketHit>(Allocator.TempJob);

        // Chạy job song song
        new RocketJob
        {
            PhysicsWorld = physicsWorld,
            DeltaTime = SystemAPI.Time.DeltaTime,
            HitQueue = hitQueue.AsParallelWriter()
        }.ScheduleParallel();

        state.Dependency.Complete();

        // Xử lý kết quả va chạm sau khi job hoàn thành
        while (hitQueue.TryDequeue(out var hit))
        {
            UnityEngine.Debug.Log(
                $"Rocket {hit.RocketEntity} hit {hit.TargetEntity} at {hit.HitPosition}"
            );

            // Ví dụ: thêm component DamageEvent cho target
            state.EntityManager.AddComponentData(hit.TargetEntity, new DamageEvent { Value = 50 });
        }

        hitQueue.Dispose();
    }
}

// Component damage event (ví dụ)
public struct DamageEvent : IComponentData
{
    public int Value;
}