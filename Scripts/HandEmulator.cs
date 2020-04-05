using System;
using UnityEngine;
using UnityHelpers;

public class HandEmulator : MonoBehaviour
{
    private readonly Quaternion wristFixupRotation = new Quaternion(0.0f, 1.0f, 0.0f, 0.0f);

    public Transform handAnchor;

    public Transform trackedRoot;
    public BoneParts[] bones;

    [SerializeField]
    private OVRSkeleton.IOVRSkeletonDataProvider _dataProvider;

    void Awake()
    {
        _dataProvider = GetComponent<OVRSkeleton.IOVRSkeletonDataProvider>();
        InitPreviousPositions();
        CacheRotations();
    }
    void Update()
    {
        if (_dataProvider != null)
        {
            var data = _dataProvider.GetSkeletonPoseData();

            trackedRoot.position = handAnchor.position;
            trackedRoot.rotation = handAnchor.rotation;

            for (var i = 0; i < bones.Length; ++i)
            {
                var currentBone = bones[i];

                if (data.IsDataValid && data.IsDataHighConfidence)
                {
                    if (currentBone.tracked != null)
                    {
                        currentBone.tracked.localRotation = data.BoneRotations[i].FromFlippedXQuatf();
                        if (i == (int)OVRSkeleton.BoneId.Hand_WristRoot)
                            currentBone.tracked.localRotation *= wristFixupRotation;

                        if (currentBone.physics != null)
                        {
                            if (i != (int)OVRSkeleton.BoneId.Hand_WristRoot)
                            {
                                var joint = currentBone.joint;
                                joint.SetTargetRotation(currentBone.tracked.localRotation, currentBone.cachedRotation);
                            }

                            currentBone.physics.position = currentBone.tracked.position;
                            currentBone.physics.rotation = currentBone.tracked.rotation; //Need to keep this for the wrists

                            Vector3 boneVelocity = (currentBone.tracked.position - currentBone.previousPosition) / Time.deltaTime;
                            currentBone.physics.velocity = boneVelocity;
                        }

                        currentBone.previousPosition = currentBone.tracked.position;
                    }
                }

                if (currentBone.mesh != null && currentBone.physics != null)
                {
                    currentBone.mesh.position = currentBone.physics.transform.position;
                    currentBone.mesh.rotation = currentBone.physics.transform.rotation;
                }
            }
        }
    }

    private void InitPreviousPositions()
    {
        for (int i = 0; i < bones.Length; i++)
            if (bones[i].tracked != null)
                bones[i].previousPosition = bones[i].tracked.position;
    }
    private void CacheRotations()
    {
        for (int i = 0; i < bones.Length; i++)
            if (bones[i].tracked != null)
                bones[i].cachedRotation = bones[i].tracked.localRotation;
    }
}

[Serializable]
public class BoneParts
{
    public Transform mesh;
    public PhysicsTransform physics;
    public Transform tracked;
    public ConfigurableJoint joint;

    [HideInInspector]
    public Vector3 previousPosition;
    [HideInInspector]
    public Quaternion cachedRotation;
}