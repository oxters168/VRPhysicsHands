using UnityEngine;

namespace VRPhysicsHands
{
    [System.Serializable]
    public struct HandBoneValues
    {
        public BoneRotValue[] bones;

        [System.Serializable]
        public struct BoneRotValue
        {
            public BoneId id;
            [Range(0, 1)]
            public float value;

            public bool IsOrientation { get { return _isOrientation; } }
            [SerializeField, HideInInspector]
            private bool _isOrientation;
            public Quaternion localRotation { get { return _localRotation; } set { _isOrientation = true; _localRotation = value; } }
            [SerializeField, HideInInspector]
            private Quaternion _localRotation;
        }
    }
}