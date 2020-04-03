using System;
using UnityEngine;
using UnityHelpers;

public class HandEmulator : MonoBehaviour
{
    private readonly Quaternion wristFixupRotation = new Quaternion(0.0f, 1.0f, 0.0f, 0.0f);

    public Transform handAnchor;

    public Transform trackedRoot;
    public Transform[] trackedBones;
    public PhysicsTransform[] physicsBones;
    public Transform[] meshBones;

    private Vector3[] previousPositions;
    private Quaternion[] cachedRotations;

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

            if (data.IsDataValid && data.IsDataHighConfidence)
            {
                for (var i = 0; i < trackedBones.Length; ++i)
                {
                    var trackedBone = trackedBones[i];
                    var physicsBone = physicsBones[i];
                    var meshBone = meshBones[i];

                    if (trackedBone != null)
                    {
                        trackedBone.localRotation = data.BoneRotations[i].FromFlippedXQuatf();
                        if (i == (int)OVRSkeleton.BoneId.Hand_WristRoot)
                            trackedBone.localRotation *= wristFixupRotation;

                        if (physicsBone != null)
                        {
                            if (i != (int)OVRSkeleton.BoneId.Hand_WristRoot)
                            {
                                var joint = physicsBone.joint;
                                joint.SetTargetRotation(trackedBone.localRotation, cachedRotations[i]);
                            }

                            physicsBone.position = trackedBone.position;
                            physicsBone.rotation = trackedBone.rotation; //Need to keep this for the wrists

                            Vector3 boneVelocity = (trackedBone.position - previousPositions[i]) / Time.deltaTime;
                            physicsBone.velocity = boneVelocity;

                            if (meshBone != null)
                            {
                                meshBone.position = physicsBone.transform.position;
                                meshBone.rotation = physicsBone.transform.rotation;
                            }
                        }
                    }

                    previousPositions[i] = trackedBone.position;
                }
            }
        }
    }

    private void InitPreviousPositions()
    {
        previousPositions = new Vector3[trackedBones.Length];
        for (int i = 0; i < trackedBones.Length; i++)
            if (trackedBones[i] != null)
                previousPositions[i] = trackedBones[i].position;
    }
    private void CacheRotations()
    {
        cachedRotations = new Quaternion[trackedBones.Length];
        for (int i = 0; i < trackedBones.Length; i++)
            if (trackedBones[i] != null)
                cachedRotations[i] = trackedBones[i].localRotation;
    }
}

[Serializable]
public class PhysicsBone
{
    [SerializeField]
    public Transform anchor;
    [SerializeField]
    public PhysicsTransform bone;
}