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

        public HandBoneValues GetValues()
        {
            HandBoneValues providedData = default;

            var data = DataProvider.GetSkeletonPoseData();
            if (data.IsDataValid && data.IsDataHighConfidence)
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
                            localRotation = data.BoneRotations[(int)currentBoneId].FromFlippedXQuatf()
                                * ((currentBoneId == BoneId.Hand_WristRoot) ? wristFixupRotation : Quaternion.identity)
                        });
                    }
                }
                providedData.bones = retrievedData.ToArray();
            }
            
            return providedData;
        }
    }
}