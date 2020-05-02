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

    [Space(10), Tooltip("If set to true, will snap to tracked position and rotation when max positional offset reached")]
    public bool resetOnPositionOffset;
    [Tooltip("How far the wrist can positionally in meters be before snapping the whole hand back in place")]
    public float maxPosOffsetValue = 2;
    [Tooltip("If set to true, will snap to tracked position and rotation when max rotational offset reached")]
    public bool resetOnRotationOffset;

    [Tooltip("How far the wrist can be rotationally in degrees before snapping the whole hand back in place")]
    public float maxRotOffsetValue = 45;
    private float wristPosOffset, wristRotOffset;

    void Awake()
    {
        _dataProvider = GetComponent<OVRSkeleton.IOVRSkeletonDataProvider>();
        InitPreviousPositions();
        CacheRotations();
    }
    private void OnDisable()
    {
        //This needs to be done so that any dislocations occuring on the hands caused by outside forces don't persist when re-enabled
        //Doing this causes the hand to sometimes be partly inside an object when enabled, need to find a fix
        SnapToTracked();
    }
    private void OnEnable()
    {
        //We need to do this since configurable joints reset their target rotation's 'default orientation' when deactivated then reactivated apparently
        CacheRotations();
    }
    void Update()
    {
        Track();
        if ((resetOnPositionOffset && wristPosOffset > maxPosOffsetValue) || (resetOnRotationOffset && wristRotOffset > maxRotOffsetValue))
            SnapToTracked();
    }

    private void Track()
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
                                wristPosOffset = Vector3.Distance(currentBone.physics.AffectedBody.position, currentBone.tracked.position);
                                wristRotOffset = Quaternion.Angle(currentBone.physics.AffectedBody.rotation, currentBone.tracked.rotation);
                            }

                            currentBone.physics.position = currentBone.tracked.position;
                            currentBone.physics.rotation = currentBone.tracked.rotation; //Need to keep this for the wrists

                            Vector3 boneVelocity = (currentBone.tracked.position - currentBone.previousPosition) / Time.deltaTime;
                            currentBone.physics.velocity = boneVelocity;
                        }

                        currentBone.previousPosition = currentBone.tracked.position;

                        currentBone.previousTrackedRotation = currentBone.tracked.localRotation;
                        currentBone.hasBeenTracked = true;
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
    private void SnapToTracked()
    {
        if (trackedRoot != null && handAnchor != null)
        {
            trackedRoot.position = handAnchor.position;
            trackedRoot.rotation = handAnchor.rotation;
        }
        for (var i = 0; i < bones.Length; ++i)
        {
            var currentBone = bones[i];

            if (currentBone.hasBeenTracked)
            {
                if (currentBone.tracked != null)
                    currentBone.tracked.localRotation = currentBone.previousTrackedRotation;
                if (currentBone.mesh != null)
                {
                    currentBone.mesh.position = currentBone.tracked.position;
                    currentBone.mesh.rotation = currentBone.tracked.rotation;
                }
                if (currentBone.physics != null)
                {
                    currentBone.physics.transform.position = currentBone.tracked.position;
                    currentBone.physics.transform.rotation = currentBone.tracked.rotation;
                }
            }
        }
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
    [HideInInspector]
    public Quaternion previousTrackedRotation;
    [HideInInspector]
    public bool hasBeenTracked;
}