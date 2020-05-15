using UnityEngine;
using UnityHelpers;

namespace VRPhysicsHands
{
    [System.Serializable]
    public class BonePart
    {
        public Transform mesh;
        public PhysicsTransform physics;
        public Transform tracked;
        public ConfigurableJoint joint;

        [HideInInspector]
        public Quaternion startRotation;
        [HideInInspector]
        public Vector3 previousPosition;
        [HideInInspector]
        public Quaternion cachedRotation;
        [HideInInspector]
        public Quaternion previousTrackedRotation;
        [HideInInspector]
        public bool hasBeenTracked;
    }
}