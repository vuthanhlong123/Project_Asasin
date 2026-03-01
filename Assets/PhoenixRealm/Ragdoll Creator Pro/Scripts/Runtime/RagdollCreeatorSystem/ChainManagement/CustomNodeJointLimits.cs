using UnityEngine;

namespace PhoenixRealm.RagdollCreatorPro
{
    [System.Serializable]
    public class CustomSoftJointLimit
    {
        public float Limit;
        public float Bounciness;
        public float ContactDistance;

        public SoftJointLimit ToUnityLimit()
        {
            return new SoftJointLimit()
            {
                limit = Limit,
                bounciness = Bounciness,
                contactDistance = ContactDistance
            };
        }

        public static CustomSoftJointLimit FromUnityLimit(SoftJointLimit unityLimit)
        {
            return new CustomSoftJointLimit()
            {
                Limit = unityLimit.limit,
                Bounciness = unityLimit.bounciness,
                ContactDistance = unityLimit.contactDistance
            };
        }
    }

    [System.Serializable]
    public class CustomNodeJointLimits
    {
        [SerializeField] public CustomSoftJointLimit lowTwistLimit;
        [SerializeField] public CustomSoftJointLimit highTwistLimit;
        [SerializeField] public CustomSoftJointLimit swing1Limit;
        [SerializeField] public CustomSoftJointLimit swing2Limit;

        public CustomNodeJointLimits()
        {
            lowTwistLimit = CreateSoftJointLimit(-30f, 0f, 0f);
            highTwistLimit = CreateSoftJointLimit(30f, 0f, 0f);
            swing1Limit = CreateSoftJointLimit(30f, 0f, 0f);
            swing2Limit = CreateSoftJointLimit(30f, 0f, 0f);
        }

        public CustomNodeJointLimits(float lowTwist, float highTwist, float swing1, float swing2)
        {
            lowTwistLimit = CreateSoftJointLimit(lowTwist, 0f, 0f);
            highTwistLimit = CreateSoftJointLimit(highTwist, 0f, 0f);
            swing1Limit = CreateSoftJointLimit(swing1, 0f, 0f);
            swing2Limit = CreateSoftJointLimit(swing2, 0f, 0f);
        }

        public CustomNodeJointLimits(CustomNodeJointLimits other)
        {
            if (other == null)
            {
                lowTwistLimit = CreateSoftJointLimit(-30f, 0f, 0f);
                highTwistLimit = CreateSoftJointLimit(30f, 0f, 0f);
                swing1Limit = CreateSoftJointLimit(30f, 0f, 0f);
                swing2Limit = CreateSoftJointLimit(30f, 0f, 0f);
            }
            else
            {
                lowTwistLimit = CreateSoftJointLimit(other.lowTwistLimit.Limit, other.lowTwistLimit.Bounciness, other.lowTwistLimit.ContactDistance);
                highTwistLimit = CreateSoftJointLimit(other.highTwistLimit.Limit, other.highTwistLimit.Bounciness, other.highTwistLimit.ContactDistance);
                swing1Limit = CreateSoftJointLimit(other.swing1Limit.Limit, other.swing1Limit.Bounciness, other.swing1Limit.ContactDistance);
                swing2Limit = CreateSoftJointLimit(other.swing2Limit.Limit, other.swing2Limit.Bounciness, other.swing2Limit.ContactDistance);
            }
        }

        public static CustomNodeJointLimits Default()
        {
            return new CustomNodeJointLimits();
        }

        private static CustomSoftJointLimit CreateSoftJointLimit(float limit, float bounciness, float contactDistance)
        {
            return new CustomSoftJointLimit
            {
                Limit = limit,
                Bounciness = bounciness,
                ContactDistance = contactDistance
            };
        }

        public CustomNodeJointLimits Clone()
        {
            return new CustomNodeJointLimits(this);
        }

        public bool IsValid()
        {
            return lowTwistLimit != null && highTwistLimit != null &&
                   swing1Limit != null && swing2Limit != null;
        }

        public bool IsUninitialized()
        {
            return lowTwistLimit == null || highTwistLimit == null ||
                   swing1Limit == null || swing2Limit == null;
        }
    }
}
