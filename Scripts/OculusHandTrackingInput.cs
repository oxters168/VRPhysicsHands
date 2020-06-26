using System.Collections.Generic;
using UnityEngine;

namespace VRPhysicsHands
{
    public class OculusHandTrackingInput : MonoBehaviour, IHandBoneManipulator
    {
        private readonly Quaternion wristFixupRotation = new Quaternion(0.0f, 1.0f, 0.0f, 0.0f);

        private OVRSkeleton.IOVRSkeletonDataProvider DataProvider { get { if (_dataProvider == null) _dataProvider = GetComponent<OVRSkeleton.IOVRSkeletonDataProvider>(); return _dataProvider; } }
        [SerializeField]
        private OVRSkeleton.IOVRSkeletonDataProvider _dataProvider;

        /// <summary>
        /// Will request hand to be hidden if provided data is not valid
        /// </summary>
        [Tooltip("Will request hand to be hidden if provided data is not valid")]
        public bool hideWhenDataIsNotValid = true;
        /// <summary>
        /// Will request hand to be hidden if provided data is not high confidence
        /// </summary>
        [Tooltip("Will request hand to be hidden if provided data is not high confidence")]
        public bool hideWhenDataIsNotHighConfidence = true;
        private bool goodData;

        public HandBoneValues GetValues()
        {
            HandBoneValues providedData = default;

            var data = DataProvider.GetSkeletonPoseData();
            goodData = (!hideWhenDataIsNotValid || data.IsDataValid) && (!hideWhenDataIsNotHighConfidence || data.IsDataHighConfidence);
            if (goodData)
            {
                List<HandBoneValues.BoneRotValue> retrievedData = new List<HandBoneValues.BoneRotValue>();
                BoneId[] allBoneIds = (BoneId[])System.Enum.GetValues(typeof(BoneId));
                for (int i = 0; i < allBoneIds.Length; i++)
                {
                    if (i < (int)BoneId.Hand_MaxSkinnable)
                    {
                        var currentBoneId = allBoneIds[i];
                        retrievedData.Add(new HandBoneValues.BoneRotValue()
                        {
                            id = currentBoneId,
                            //localRotation = data.BoneRotations[(int)currentBoneId].FromFlippedZQuatf() //Maybe this or the FromQuatf function can be used for blender models?
                            localRotation = data.BoneRotations[(int)currentBoneId].FromFlippedXQuatf()
                                * ((currentBoneId == BoneId.Hand_WristRoot) ? wristFixupRotation : Quaternion.identity)
                        });
                    }
                }
                providedData.bones = retrievedData.ToArray();
            }
            
            return providedData;
        }

        public bool ShowHand()
        {
            return goodData;
        }
    }
}