using UnityEngine;

namespace PhoenixRealm.RagdollCreatorPro
{
    public enum ColliderType
    {
        Sphere,
        Capsule,
        Box
    }

    public enum CapsuleDirection
    {
        X = 0,
        Y = 1,
        Z = 2
    }

    [System.Serializable]
    public class CustomNode
    {
        #region Vars + Properties

        [SerializeField] private Transform m_transform;
        [SerializeField] private string m_nodeName;
        [SerializeField] private bool m_isSelected;
        [SerializeField] private ColliderType m_colliderType = ColliderType.Capsule;
        [SerializeField] private Vector3 m_colliderSize = Vector3.one * .8f;
        [SerializeField] private float m_colliderRadius = 0.1f;
        [SerializeField] private float m_colliderHeight = 0.3f;
        [SerializeField] private CapsuleDirection m_capsuleDirection = CapsuleDirection.X;
        [SerializeField] private float m_massOverride = 1f;
        [SerializeField] private bool m_jointStability = true;
        [SerializeField] private Vector3 m_localOffset = Vector3.zero;
        [SerializeField] private Vector3 m_jointAnchorLocal = Vector3.zero;
        [SerializeField] private Vector3 m_jointConnectedAnchorLocal = Vector3.zero;
        [SerializeField] private Vector3 m_jointAxisLocal = Vector3.forward;
        [SerializeField] private CustomNodeJointLimits m_jointLimits = CustomNodeJointLimits.Default();
        [SerializeField] private bool m_jointEnableProjection = false;
        [SerializeField] private bool m_jointEnablePreprocessing = false;
        public Transform Transform
        {
            get => m_transform;
            set => m_transform = value;
        }

        public string NodeName
        {
            get => m_nodeName;
            set => m_nodeName = value;
        }

        public bool IsSelected
        {
            get => m_isSelected;
            set => m_isSelected = value;
        }

        public ColliderType ColliderType
        {
            get => m_colliderType;
            set => m_colliderType = value;
        }

        public Vector3 ColliderSize
        {
            get => m_colliderSize;
            set => m_colliderSize = value;
        }

        public float ColliderRadius
        {
            get => m_colliderRadius;
            set => m_colliderRadius = Mathf.Max(0f, value);
        }

        public float ColliderHeight
        {
            get => m_colliderHeight;
            set => m_colliderHeight = Mathf.Max(0f, value);
        }

        public CapsuleDirection CapsuleDirection
        {
            get => m_capsuleDirection;
            set => m_capsuleDirection = value;
        }

        public float MassOverride
        {
            get => m_massOverride;
            set => m_massOverride = Mathf.Max(0.0001f, value);
        }

        public bool JointStability
        {
            get => m_jointStability;
            set => m_jointStability = value;
        }

        public Vector3 LocalOffset
        {
            get => m_localOffset;
            set => m_localOffset = value;
        }

        public Vector3 JointAnchorLocal
        {
            get => m_jointAnchorLocal;
            set => m_jointAnchorLocal = value;
        }

        public Vector3 JointConnectedAnchorLocal
        {
            get => m_jointConnectedAnchorLocal;
            set => m_jointConnectedAnchorLocal = value;
        }

        public Vector3 JointAxisLocal
        {
            get => m_jointAxisLocal.sqrMagnitude > 1e-6f ? m_jointAxisLocal : Vector3.right;
            set => m_jointAxisLocal = value;
        }

        public CustomNodeJointLimits JointLimits
        {
            get => m_jointLimits;
            set => m_jointLimits = value;
        }

        public bool JointEnableProjection
        {
            get => m_jointEnableProjection;
            set => m_jointEnableProjection = value;
        }

        public bool JointEnablePreprocessing
        {
            get => m_jointEnablePreprocessing;
            set => m_jointEnablePreprocessing = value;
        }

        #endregion

        #region Custom Functions

        public CustomNode()
        {
            m_jointLimits = new CustomNodeJointLimits();
        }

        public CustomNode(Transform t) : this()
        {
            m_transform = t;
            m_nodeName = t != null ? t.name : "Node";
        }

        public bool IsValid(out string error)
        {
            if (m_transform == null)
            {
                error = $"Node '{m_nodeName}' has no transform assigned.";
                return false;
            }

            if (m_massOverride <= 0f)
            {
                error = $"Node '{m_nodeName}' has invalid mass ({m_massOverride}).";
                return false;
            }

            error = string.Empty;
            return true;
        }

        public CustomNode Clone()
        {
            var clone = new CustomNode
            {
                m_transform = null,
                m_nodeName = m_nodeName,
                m_isSelected = false,
                m_colliderType = m_colliderType,
                m_colliderSize = m_colliderSize,
                m_colliderRadius = m_colliderRadius,
                m_colliderHeight = m_colliderHeight,
                m_capsuleDirection = m_capsuleDirection,
                m_massOverride = m_massOverride,
                m_jointStability = m_jointStability,
                m_localOffset = m_localOffset,
                m_jointAnchorLocal = m_jointAnchorLocal,
                m_jointConnectedAnchorLocal = m_jointConnectedAnchorLocal,
                m_jointAxisLocal = m_jointAxisLocal,
                m_jointLimits = m_jointLimits != null ? m_jointLimits.Clone() : CustomNodeJointLimits.Default(),
                m_jointEnableProjection = m_jointEnableProjection,
                m_jointEnablePreprocessing = m_jointEnablePreprocessing
            };

            return clone;
        }

        #endregion

        #region Getters

        public Quaternion GetWorldRotation()
        {
            if (m_transform == null) return Quaternion.identity;
            return m_transform.rotation;
        }

        public Vector3 GetWorldAxis()
        {
            var worldRotation = GetWorldRotation();

            switch (m_colliderType)
            {
                case ColliderType.Capsule:
                    return GetCapsuleWorldAxis();
                case ColliderType.Box:
                    return worldRotation * Vector3.forward;
                case ColliderType.Sphere:
                    return worldRotation * Vector3.forward;
                default:
                    return worldRotation * Vector3.forward;
            }
        }

        public Vector3 GetCapsuleWorldAxis()
        {
            if (m_colliderType != ColliderType.Capsule || m_transform == null)
                return Vector3.up;

            var worldRotation = GetWorldRotation();

            switch (m_capsuleDirection)
            {
                case CapsuleDirection.X:
                    return worldRotation * Vector3.right;
                case CapsuleDirection.Y:
                    return worldRotation * Vector3.up;
                case CapsuleDirection.Z:
                    return worldRotation * Vector3.forward;
                default:
                    return worldRotation * Vector3.up;
            }
        }

        public Vector3 GetWorldRightAxis()
        {
            return GetWorldRotation() * Vector3.right;
        }

        public Vector3 GetWorldUpAxis()
        {
            return GetWorldRotation() * Vector3.up;
        }

        public Vector3 GetWorldForwardAxis()
        {
            return GetWorldRotation() * Vector3.forward;
        }

        #endregion
    }
}
