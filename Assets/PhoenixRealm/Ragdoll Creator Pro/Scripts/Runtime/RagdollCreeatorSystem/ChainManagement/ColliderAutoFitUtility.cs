using System.Collections.Generic;
using UnityEngine;

namespace PhoenixRealm.RagdollCreatorPro
{
    public enum FitAnchorMode
    {
        ForLimbs,
        ForNonLimbs
    }

    public enum FitSearchScope
    {
        BoneAndChildren = 1,
        BoneChildrenAndGrandchildren = 2,
        DeepHierarchy = 3
    }

    public static class ColliderAutoFitUtility
    {
        #region Public API

        public static bool AutoFitNodeCollider(
            CustomNode node,
            FitSearchScope scope = FitSearchScope.BoneChildrenAndGrandchildren,
            FitAnchorMode anchor = FitAnchorMode.ForNonLimbs,
            bool fitPosition = true,
            bool fitSize = true)
        {
            if (node == null || !node.Transform) return false;

            int searchDepth = ScopeToDepth(scope);
            var renderers = CollectRenderers(node.Transform, searchDepth);

            if (renderers.Count == 0)
                return FallbackFromChild(node, anchor, fitPosition, fitSize);

            // World AABB
            Bounds worldBounds = default;
            bool hasAny = false;
            foreach (var r in renderers)
            {
                if (!r) continue;
                if (!hasAny) { worldBounds = r.bounds; hasAny = true; }
                else worldBounds.Encapsulate(r.bounds);
            }
            if (!hasAny)
                return FallbackFromChild(node, anchor, fitPosition, fitSize);

            var t = node.Transform;
            var corners = GetBoundsCorners(worldBounds);
            var localMin = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            var localMax = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
            for (int i = 0; i < corners.Length; i++)
            {
                var p = t.InverseTransformPoint(corners[i]);
                localMin = Vector3.Min(localMin, p);
                localMax = Vector3.Max(localMax, p);
            }

            var localCenterBounds = 0.5f * (localMin + localMax);
            var localSize = Vector3.Max(Vector3.zero, localMax - localMin);
            localSize.x = Mathf.Max(localSize.x, 1e-5f);
            localSize.y = Mathf.Max(localSize.y, 1e-5f);
            localSize.z = Mathf.Max(localSize.z, 1e-5f);

            Vector3 desiredCenter = (anchor == FitAnchorMode.ForNonLimbs)
                ? localCenterBounds
                : ComputeLimbCenter(node, localCenterBounds);

            switch (node.ColliderType)
            {
                case ColliderType.Sphere:
                    ApplySphereFit(node, localSize, desiredCenter, fitPosition, fitSize);
                    break;

                case ColliderType.Capsule:
                    ApplyCapsuleFit(node, localMin, localMax, localSize, desiredCenter, fitPosition, fitSize);
                    break;

                case ColliderType.Box:
                    ApplyBoxFit(node, localSize, desiredCenter, fitPosition, fitSize);
                    break;
            }

            return true;
        }
        #endregion

        #region Fit Impl
        private static void ApplySphereFit(CustomNode node, Vector3 localSize, Vector3 desiredCenter, bool fitPosition, bool fitSize)
        {
            if (fitPosition) node.LocalOffset = desiredCenter;
            if (fitSize)
                node.ColliderRadius = 0.5f * Mathf.Max(localSize.x, Mathf.Max(localSize.y, localSize.z));
        }

        private static void ApplyCapsuleFit(
            CustomNode node,
            Vector3 localMin, Vector3 localMax,
            Vector3 localSize, Vector3 desiredCenter,
            bool fitPosition, bool fitSize)
        {
            int axis = (int)node.CapsuleDirection;
            float major = GetAxis(localSize, axis);
            float minorA = GetAxis(localSize, (axis + 1) % 3);
            float minorB = GetAxis(localSize, (axis + 2) % 3);

            float radius = 0.5f * Mathf.Max(minorA, minorB);
            float straight = Mathf.Max(0f, major - 2f * radius);

            float axisMin = GetAxis(localMin, axis);
            float axisMax = GetAxis(localMax, axis);
            float midVal = 0.5f * (axisMin + axisMax);

            Vector3 axisMid = new Vector3(
                axis == 0 ? midVal : desiredCenter.x,
                axis == 1 ? midVal : desiredCenter.y,
                axis == 2 ? midVal : desiredCenter.z
            );

            if (fitPosition) node.LocalOffset = axisMid;
            if (fitSize)
            {
                node.ColliderRadius = radius;
                node.ColliderHeight = straight;
            }
        }

        private static void ApplyBoxFit(CustomNode node, Vector3 localSize, Vector3 desiredCenter, bool fitPosition, bool fitSize)
        {
            if (fitPosition) node.LocalOffset = desiredCenter;
            if (fitSize) node.ColliderSize = localSize;
        }
        #endregion

        #region Fallback + Helpers
        private static Vector3 ComputeLimbCenter(CustomNode node, Vector3 boundsCenter)
        {
            var t = node.Transform;
            if (t.childCount == 0) return boundsCenter;

            var child = t.GetChild(0);
            var localChild = t.InverseTransformPoint(child.position);
            var mid = 0.5f * localChild;

            if (node.ColliderType == ColliderType.Capsule)
            {
                int a = (int)node.CapsuleDirection;
                return new Vector3(
                    a == 0 ? mid.x : boundsCenter.x,
                    a == 1 ? mid.y : boundsCenter.y,
                    a == 2 ? mid.z : boundsCenter.z
                );
            }
            return mid;
        }

        private static bool FallbackFromChild(CustomNode node, FitAnchorMode anchor, bool fitPosition, bool fitSize)
        {
            var t = node.Transform;
            if (!t || t.childCount == 0) return false;

            var child = t.GetChild(0);
            var delta = child.position - t.position;
            float length = Mathf.Max(delta.magnitude, 1e-3f);

            // Deterministic factors (tweak if you like, but they’re not using current collider values)
            const float kSphereRadiusFromSeg = 0.30f; // sphere radius = 30% of bone→child distance
            const float kCapsuleRadiusFromSeg = 0.15f; // capsule radius = 15% of distance
            const float kBoxMinorFromSeg = 1f; // box minor thickness = 25% of distance

            // Guess limb major axis (only for capsules/boxes that need direction)
            float px = Mathf.Abs(Vector3.Dot(delta.normalized, t.right));
            float py = Mathf.Abs(Vector3.Dot(delta.normalized, t.up));
            float pz = Mathf.Abs(Vector3.Dot(delta.normalized, t.forward));
            int guessedAxis = (px >= py && px >= pz) ? 0 : (py >= px && py >= pz ? 1 : 2);

            // If the current collider is a capsule, respect user’s chosen axis
            if (node.ColliderType == ColliderType.Capsule)
                guessedAxis = (int)node.CapsuleDirection;

            // Local midpoint between bone and child
            var localChild = t.InverseTransformPoint(child.position);
            var mid = 0.5f * localChild;

            Vector3 centerForLimbsProjected = new Vector3(
                guessedAxis == 0 ? mid.x : 0f,
                guessedAxis == 1 ? mid.y : 0f,
                guessedAxis == 2 ? mid.z : 0f
            );
            Vector3 chosenCenter = (anchor == FitAnchorMode.ForLimbs) ? centerForLimbsProjected : mid;

            switch (node.ColliderType)
            {
                case ColliderType.Sphere:
                    {
                        if (fitPosition) node.LocalOffset = chosenCenter;
                        if (fitSize)
                            node.ColliderRadius = length * kSphereRadiusFromSeg;
                        break;
                    }

                case ColliderType.Capsule:
                    {
                        if (fitPosition) node.LocalOffset = new Vector3(
                            guessedAxis == 0 ? mid.x : 0f,
                            guessedAxis == 1 ? mid.y : 0f,
                            guessedAxis == 2 ? mid.z : 0f
                        );

                        if (fitSize)
                        {
                            float r = length * kCapsuleRadiusFromSeg;
                            float straight = Mathf.Max(0f, length - 2f * r);
                            node.ColliderRadius = r;
                            node.ColliderHeight = straight;
                        }
                        break;
                    }

                case ColliderType.Box:
                    {
                        if (fitPosition) node.LocalOffset = chosenCenter;

                        if (fitSize)
                        {
                            float thickness = Mathf.Max(1e-5f, length * kBoxMinorFromSeg);
                            Vector3 size;
                            if (guessedAxis == 0) size = new Vector3(length, thickness, thickness);
                            else if (guessedAxis == 1) size = new Vector3(thickness, length, thickness);
                            else size = new Vector3(thickness, thickness, length);
                            node.ColliderSize = size;
                        }
                        break;
                    }
            }

            return true;
        }

        private static int ScopeToDepth(FitSearchScope scope)
        {
            switch (scope)
            {
                case FitSearchScope.BoneAndChildren: return 1;
                case FitSearchScope.DeepHierarchy: return 3;
                default: return 2;
            }
        }

        private static float GetAxis(Vector3 v, int axisIndex)
        {
            switch (axisIndex % 3)
            {
                case 0: return v.x;
                case 1: return v.y;
                default: return v.z;
            }
        }

        private static List<Renderer> CollectRenderers(Transform root, int maxDepth)
        {
            var list = new List<Renderer>(32);
            CollectRenderersRecursive(root, 0, maxDepth, list);
            return list;
        }

        private static void CollectRenderersRecursive(Transform t, int depth, int maxDepth, List<Renderer> list)
        {
            if (!t) return;

            var mr = t.GetComponent<MeshRenderer>();
            if (mr) list.Add(mr);

            var smr = t.GetComponent<SkinnedMeshRenderer>();
            if (smr) list.Add(smr);

            if (depth >= maxDepth) return;

            for (int i = 0; i < t.childCount; i++)
                CollectRenderersRecursive(t.GetChild(i), depth + 1, maxDepth, list);
        }

        private static Vector3[] GetBoundsCorners(Bounds b)
        {
            Vector3 c = b.center, e = b.extents;
            return new[]
            {
                new Vector3(c.x - e.x, c.y - e.y, c.z - e.z),
                new Vector3(c.x + e.x, c.y - e.y, c.z - e.z),
                new Vector3(c.x - e.x, c.y + e.y, c.z - e.z),
                new Vector3(c.x + e.x, c.y + e.y, c.z - e.z),
                new Vector3(c.x - e.x, c.y - e.y, c.z + e.z),
                new Vector3(c.x + e.x, c.y - e.y, c.z + e.z),
                new Vector3(c.x - e.x, c.y + e.y, c.z + e.z),
                new Vector3(c.x + e.x, c.y + e.y, c.z + e.z),
            };
        }
        #endregion
    }
}
