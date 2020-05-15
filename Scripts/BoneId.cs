namespace VRPhysicsHands
{
    /// <summary>
    /// This is the enum from Oculus' integration
    /// </summary>
	public enum BoneId
	{
		Invalid                 = -1,

		Hand_Start              = 0,
		Hand_WristRoot          = Hand_Start + 0, // root frame of the hand, where the wrist is located
		Hand_ForearmStub        = Hand_Start + 1, // frame for user's forearm
		Hand_Thumb0             = Hand_Start + 2, // thumb trapezium bone
		Hand_Thumb1             = Hand_Start + 3, // thumb metacarpal bone
		Hand_Thumb2             = Hand_Start + 4, // thumb proximal phalange bone
		Hand_Thumb3             = Hand_Start + 5, // thumb distal phalange bone
		Hand_Index1             = Hand_Start + 6, // index proximal phalange bone
		Hand_Index2             = Hand_Start + 7, // index intermediate phalange bone
		Hand_Index3             = Hand_Start + 8, // index distal phalange bone
		Hand_Middle1            = Hand_Start + 9, // middle proximal phalange bone
		Hand_Middle2            = Hand_Start + 10, // middle intermediate phalange bone
		Hand_Middle3            = Hand_Start + 11, // middle distal phalange bone
		Hand_Ring1              = Hand_Start + 12, // ring proximal phalange bone
		Hand_Ring2              = Hand_Start + 13, // ring intermediate phalange bone
		Hand_Ring3              = Hand_Start + 14, // ring distal phalange bone
		Hand_Pinky0             = Hand_Start + 15, // pinky metacarpal bone
		Hand_Pinky1             = Hand_Start + 16, // pinky proximal phalange bone
		Hand_Pinky2             = Hand_Start + 17, // pinky intermediate phalange bone
		Hand_Pinky3             = Hand_Start + 18, // pinky distal phalange bone
		Hand_MaxSkinnable       = Hand_Start + 19,
		// Bone tips are position only. They are not used for skinning but are useful for hit-testing.
		// NOTE: Hand_ThumbTip == Hand_MaxSkinnable since the extended tips need to be contiguous
		Hand_ThumbTip           = Hand_Start + Hand_MaxSkinnable + 0, // tip of the thumb
		Hand_IndexTip           = Hand_Start + Hand_MaxSkinnable + 1, // tip of the index finger
		Hand_MiddleTip          = Hand_Start + Hand_MaxSkinnable + 2, // tip of the middle finger
		Hand_RingTip            = Hand_Start + Hand_MaxSkinnable + 3, // tip of the ring finger
		Hand_PinkyTip           = Hand_Start + Hand_MaxSkinnable + 4, // tip of the pinky
		Hand_End                = Hand_Start + Hand_MaxSkinnable + 5,

		// add new bones here

		Max = Hand_End + 0,
	}
}