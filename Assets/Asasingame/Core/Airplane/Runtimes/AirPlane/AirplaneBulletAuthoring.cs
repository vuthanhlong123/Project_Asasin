using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class AirplaneBulletAuthoring : MonoBehaviour
{
    public Vector3 velocity;
    public float radius = 0.5f;
    public float maxDistance = 50f;
}

public class AirplaneBullet : Baker<AirplaneBulletAuthoring>
{
    public override void Bake(AirplaneBulletAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent(entity, new Rocket
        {
            Velocity = authoring.velocity,
            Radius = authoring.radius,
            MaxDistance = authoring.maxDistance
        });

        AddComponent(entity, LocalTransform.Identity);
    }
}
