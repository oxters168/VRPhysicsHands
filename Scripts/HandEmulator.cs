using UnityEngine;
using UnityHelpers;
using System.Linq;
using System;
using System.Collections.Generic;

namespace VRPhysicsHands
{
    public class HandEmulator : MonoBehaviour
    {
        public Transform handAnchor;

        public Transform trackedRoot;
        public Transform palm;

        [Space(10), Tooltip("If set to true, will snap to tracked position and rotation when max positional offset reached")]
        public bool resetOnPositionOffset;
        [Tooltip("How far the wrist can positionally in meters be before snapping the whole hand back in place")]
        public float maxPosOffsetValue = 2;
        [Tooltip("If set to true, will snap to tracked position and rotation when max rotational offset reached")]
        public bool resetOnRotationOffset;

        [Tooltip("How far the wrist can be rotationally in degrees before snapping the whole hand back in place")]
        public float maxRotOffsetValue = 45;
        private float wristPosOffset, wristRotOffset;

        [Space(10)]
        [RequireInterface(typeof(IHandBoneManipulator))]
        public GameObject handInterfaceObject;
        internal IHandBoneManipulator handInterface;
        [Tooltip("Flips the direction of the rays on the tips used for collision detection")]
        /// <summary>
        /// Flips the the direction of the rays on the tips used for collision detection
        /// </summary>
        public bool flipTipRays;
        [Tooltip("The distance from the finger tips to test for collision before stopping finger rotation")]
        /// <summary>
        /// The distance from the finger tips to test for collision before stopping finger rotation
        /// </summary>
        public float tipCollisionDistance = 0.016f;
        [Tooltip("Specifies how fast and with how much force fingers snap back into place")]
        /// <summary>
        /// Changes drive's position spring property in configurable joints
        /// </summary>
        public float fingerForce;
        [Tooltip("Lets you manually change finger rotation values in editor if an interface is getting in the way")]
        public bool testFingers;
        public HandBoneValues boneRotationValues;

        [Space(10)]
        public BonePart[] bones;

        void Awake()
        {
            if (handInterfaceObject != null)
                handInterface = handInterfaceObject.GetComponent<IHandBoneManipulator>();

            SaveStartRotations();
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
            if (handInterface != null && !testFingers)
                boneRotationValues = handInterface.GetValues();

            SetTracked();
            SetPhysics();

            if ((resetOnPositionOffset && wristPosOffset > maxPosOffsetValue) || (resetOnRotationOffset && wristRotOffset > maxRotOffsetValue))
                SnapToTracked();
        }

        private void SetTracked()
        {
            if (trackedRoot != null && handAnchor != null)
            {
                trackedRoot.position = handAnchor.position;
                trackedRoot.rotation = handAnchor.rotation;
            }

            if (boneRotationValues.bones != null && boneRotationValues.bones.Length > 0)
            {
                //string debugTips = "";
                List<(BoneId, bool)> fingerTipTests = new List<(BoneId, bool)>(); //Adding this helps reduce the amount of raycasts done on each finger tip by around 3 times, since each finger has 3 or 4 bones
                var boneIds = (BoneId[])Enum.GetValues(typeof(BoneId));
                for (int i = 0; i < boneIds.Length; i++)
                {
                    var currentBoneId = boneIds[i];
                    var bonesWithId = boneRotationValues.bones.Where((bone) => bone.id == currentBoneId);
                    if (bonesWithId.Count() > 0)
                    {
                        var currentBone = bones[(int)currentBoneId];
                        var boneRotValue = bonesWithId.First();

                        #region Getting the rotation value from given hand bone values
                        Quaternion localRotation;
                        if (!boneRotValue.IsOrientation)
                        {
                            Vector3 rotationAxis = -Vector3.forward;
                            if (currentBoneId == BoneId.Hand_Thumb0)
                                rotationAxis = -Vector3.up;
                            float currentAngle = boneRotValue.value * 90;
                            localRotation = Quaternion.AngleAxis(currentAngle, rotationAxis) * currentBone.startRotation;
                        }
                        else
                            localRotation = boneRotValue.localRotation;
                        #endregion

                        #region Adjusting rotation based on collision (less rotation toward collider if tip colliding)
                        BoneId currentFingerTip = GetTip(currentBoneId);
                        if (currentFingerTip != BoneId.Invalid)
                        {
                            var fingerTipTransform = bones[(int)currentFingerTip].mesh;
                            var fingerTipRay = new Ray(fingerTipTransform.position, fingerTipTransform.up * (flipTipRays ? -1 : 1));

                            //debugTips += currentBoneId + " => " + currentFingerTip + "\n";
                            (BoneId, bool) fingerTipTest;
                            var matchingTests = fingerTipTests.Where(currentTest => { return currentTest.Item1 == currentFingerTip; });
                            if (matchingTests.Count() <= 0)
                            {
                                bool tipCollided = Physics.Raycast(fingerTipRay, tipCollisionDistance);
                                fingerTipTest = (currentFingerTip, tipCollided);
                                fingerTipTests.Add(fingerTipTest);
                                Debug.DrawRay(fingerTipRay.origin, fingerTipRay.direction * tipCollisionDistance, tipCollided ? Color.green : Color.red);
                            }
                            else
                                fingerTipTest = matchingTests.First();

                            //Calculate the difference in rotation between the next tracked local rotation and the previous tracked local rotation
                            Quaternion deltaRotation = currentBone.tracked.localRotation * Quaternion.Inverse(localRotation);
                            //Convert the difference to an angle on an axis
                            float angle;
                            Vector3 axis;
                            deltaRotation.ToAngleAxis(out angle, out axis);
                            //Local to world of axis direction
                            axis = currentBone.tracked.TransformDirection(axis);
                            //Comparing the axis of the rotation with the forward axis of the current bone (the forward axis
                            //of the bone is the one it uses to close and open)
                            bool isClosing = Vector3.Dot(currentBone.tracked.forward, axis) > 0;

                            //If finger tip is colliding with an object and the finger is closing then set the current joint's
                            //rotation to be the previous frame's
                            if (fingerTipTest.Item2 && isClosing)
                            {
                                //localRotation = currentBone.tracked.localRotation;
                                Quaternion meshDelta = currentBone.mesh.localRotation * Quaternion.Inverse(currentBone.startRotation);
                                float maxAngle = meshDelta.PollAxisAngle(currentBone.tracked.forward, currentBone.tracked.up);
                                Quaternion preferredAxisRotation = Quaternion.AngleAxis(maxAngle, currentBone.tracked.forward);

                                Quaternion trackedDelta = currentBone.tracked.localRotation * Quaternion.Inverse(currentBone.startRotation);
                                float currentAngle = meshDelta.PollAxisAngle(currentBone.tracked.forward, currentBone.tracked.up);
                                Quaternion currentAxisRotation = Quaternion.AngleAxis(currentAngle, currentBone.tracked.forward);

                                Quaternion shiftDelta = currentAxisRotation * Quaternion.Inverse(preferredAxisRotation);
                                localRotation = currentBone.tracked.localRotation * shiftDelta;
                            }

                            Debug.DrawRay(currentBone.mesh.position, currentBone.tracked.forward * 0.02f, fingerTipTest.Item2 && isClosing ? Color.red : Color.green);
                        }
                        #endregion
                        
                        currentBone.tracked.localRotation = localRotation;
                    }
                }
                //Debug.Log(debugTips);
            }
        }
        private void SetPhysics()
        {
            for (var i = 0; i < bones.Length; ++i)
            {
                var currentBone = bones[i];

                if (currentBone.tracked != null)
                {
                    if (currentBone.physics != null)
                    {
                        if (i != (int)BoneId.Hand_WristRoot)
                        {
                            var joint = currentBone.joint;
                            joint.SetTargetRotation(currentBone.tracked.localRotation, currentBone.cachedRotation);

                            JointDrive drive = new JointDrive
                            {
                                positionSpring = fingerForce,
                                maximumForce = Mathf.Infinity
                            };
                            joint.xDrive = drive;
                            joint.yDrive = drive;
                            joint.zDrive = drive;
                        }
                        else
                        {
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

                if (currentBone.mesh != null && currentBone.physics != null)
                {
                    currentBone.mesh.position = currentBone.physics.transform.position;
                    currentBone.mesh.rotation = currentBone.physics.transform.rotation;
                }
            }
        }

        /// <summary>
        /// Finds the bone id of the tip of the given bone id's finger
        /// </summary>
        /// <param name="currentBoneId">A bone id on a finger</param>
        /// <returns>The bone id of the tip of the given bone id (unless the given bone id is not from a finger, then returns invalid)</returns>
        private static BoneId GetTip(BoneId currentBoneId)
        {
            BoneId theTip = BoneId.Invalid;
            if (IsFingerBone(currentBoneId))
            {
                //var boneNames = Enum.GetNames(typeof(BoneId));
                string currentBoneName = currentBoneId.ToString();
                if (!currentBoneName.Contains("Tip"))
                {
                    string tipBoneName = currentBoneName.Substring(0, currentBoneName.Length - 1) + "Tip";
                    theTip = (BoneId)Enum.Parse(typeof(BoneId), tipBoneName);
                }
                else
                    theTip = currentBoneId;
            }
            return theTip;
        }
        /// <summary>
        /// Checks whether the given bone id is part of a finger
        /// </summary>
        /// <param name="currentBoneId">The bone id to be checked</param>
        /// <returns>True if the given id is part of a finger, false otherwise</returns>
        private static bool IsFingerBone(BoneId currentBoneId)
        {
            return currentBoneId != BoneId.Hand_End && currentBoneId != BoneId.Hand_ForearmStub && currentBoneId != BoneId.Hand_MaxSkinnable && currentBoneId != BoneId.Hand_Start && currentBoneId != BoneId.Hand_WristRoot && currentBoneId != BoneId.Invalid && currentBoneId != BoneId.Max;
        }

        private void SaveStartRotations()
        {
            for (int i = 0; i < bones.Length; i++)
                if (bones[i].tracked != null)
                    bones[i].startRotation = bones[i].tracked.localRotation;
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
}