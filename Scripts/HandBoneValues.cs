using UnityEngine;

namespace VRPhysicsHands
{
    [System.Serializable]
    public class HandBoneValues
    {
        public BoneRotValue[] bones;

        [System.Serializable]
        public class BoneRotValue
        {
            public BoneId id;
            [Range(0, 1)]
            public float value;
        }
    }
}