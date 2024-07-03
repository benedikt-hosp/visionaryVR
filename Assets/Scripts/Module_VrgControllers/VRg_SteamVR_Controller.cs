//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//************ MODIFIED VERSION WORKING WITH VRGINEERS HMD ********************
//
// Purpose: Wrapper for working with SteamVR controller input and VRgineers
//          HMD.
//
// Example usage:
//
// VRg_SteamVR_Controller.Device input = trackedGameObject.GetComponent<VRg_SteamVR_Controller>().Input();
// bool triggerPressed = input.GetHairTrigger();
//
//=============================================================================

using UnityEngine;
using vrg;

public partial class VRg_SteamVR_Controller : VrgController
{
	public class ButtonMask
	{
		public const ulong System			= (1ul << (int)Controllers.EVRButtonId.k_EButton_System); // reserved
		public const ulong ApplicationMenu	= (1ul << (int)Controllers.EVRButtonId.k_EButton_ApplicationMenu);
		public const ulong Grip				= (1ul << (int)Controllers.EVRButtonId.k_EButton_Grip);
		public const ulong Axis0			= (1ul << (int)Controllers.EVRButtonId.k_EButton_Axis0);
		public const ulong Axis1			= (1ul << (int)Controllers.EVRButtonId.k_EButton_Axis1);
		public const ulong Axis2			= (1ul << (int)Controllers.EVRButtonId.k_EButton_Axis2);
		public const ulong Axis3			= (1ul << (int)Controllers.EVRButtonId.k_EButton_Axis3);
		public const ulong Axis4			= (1ul << (int)Controllers.EVRButtonId.k_EButton_Axis4);
		public const ulong Touchpad			= (1ul << (int)Controllers.EVRButtonId.k_EButton_SteamVR_Touchpad);
		public const ulong Trigger			= (1ul << (int)Controllers.EVRButtonId.k_EButton_SteamVR_Trigger);
	}

	public class Device
	{
		public Device(uint i) { index = i; }
		public uint index { get; private set; }

		public bool valid { get; private set; }
		public bool connected { get { Update(); return pose.bDeviceIsConnected; } }
		public bool hasTracking { get { Update(); return pose.bPoseIsValid; } }

		public bool outOfRange { get { Update(); return pose.eTrackingResult == Controllers.ETrackingResult.Running_OutOfRange || pose.eTrackingResult == Controllers.ETrackingResult.Calibrating_OutOfRange; } }
		public bool calibrating { get { Update(); return pose.eTrackingResult == Controllers.ETrackingResult.Calibrating_InProgress || pose.eTrackingResult == Controllers.ETrackingResult.Calibrating_OutOfRange; } }
		public bool uninitialized { get { Update(); return pose.eTrackingResult == Controllers.ETrackingResult.Uninitialized; } }

		// These values are only accurate for the last controller state change (e.g. trigger release), and by definition, will always lag behind
		// the predicted visual poses that drive SteamVR_TrackedObjects since they are sync'd to the input timestamp that caused them to update.
		//public SteamVR_Utils.RigidTransform transform { get { Update(); return new SteamVR_Utils.RigidTransform(pose.mDeviceToAbsoluteTracking); } }

        public Vector3 velocity { get { Update(); return pose.Velocity; } }
		public Vector3 angularVelocity { get { Update(); return pose.AngularVelocity; } }

		public Controllers.VRControllerState_t GetState() { Update(); return state; }
		public Controllers.VRControllerState_t GetPrevState() { Update(); return prevState; }
		public Controllers.TrackedDevicePose2_t GetPose() { Update(); return pose; }

        Controllers.VRControllerState_t state, prevState;
        Controllers.TrackedDevicePose2_t pose;
		int prevFrameCount = -1;
		public void Update()
		{
			if (Time.frameCount != prevFrameCount)
			{
				prevFrameCount = Time.frameCount;
				prevState = state;

				//var system = OpenVR.System;
				//if (system != null)
				{
                    valid = Controllers.GetControllerState(index, ref pose, ref state);
                    
                    UpdateHairTrigger();
				}
			}
		}

		public bool GetPress(ulong buttonMask) { Update(); return (state.ulButtonPressed & buttonMask) != 0; }
		public bool GetPressDown(ulong buttonMask) { Update(); return (state.ulButtonPressed & buttonMask) != 0 && (prevState.ulButtonPressed & buttonMask) == 0; }
		public bool GetPressUp(ulong buttonMask) { Update(); return (state.ulButtonPressed & buttonMask) == 0 && (prevState.ulButtonPressed & buttonMask) != 0; }

		public bool GetPress(Controllers.EVRButtonId buttonId) { return GetPress(1ul << (int)buttonId); }
		public bool GetPressDown(Controllers.EVRButtonId buttonId) { return GetPressDown(1ul << (int)buttonId); }
		public bool GetPressUp(Controllers.EVRButtonId buttonId) { return GetPressUp(1ul << (int)buttonId); }

		public bool GetTouch(ulong buttonMask) { Update(); return (state.ulButtonTouched & buttonMask) != 0; }
		public bool GetTouchDown(ulong buttonMask) { Update(); return (state.ulButtonTouched & buttonMask) != 0 && (prevState.ulButtonTouched & buttonMask) == 0; }
		public bool GetTouchUp(ulong buttonMask) { Update(); return (state.ulButtonTouched & buttonMask) == 0 && (prevState.ulButtonTouched & buttonMask) != 0; }

		public bool GetTouch(Controllers.EVRButtonId buttonId) { return GetTouch(1ul << (int)buttonId); }
		public bool GetTouchDown(Controllers.EVRButtonId buttonId) { return GetTouchDown(1ul << (int)buttonId); }
		public bool GetTouchUp(Controllers.EVRButtonId buttonId) { return GetTouchUp(1ul << (int)buttonId); }

		public Vector2 GetAxis(Controllers.EVRButtonId buttonId = Controllers.EVRButtonId.k_EButton_SteamVR_Touchpad)
		{
			Update();
			var axisId = (uint)buttonId - (uint)Controllers.EVRButtonId.k_EButton_Axis0;
			switch (axisId)
			{
				case 0: return new Vector2(state.rAxis0.x, state.rAxis0.y);
				case 1: return new Vector2(state.rAxis1.x, state.rAxis1.y);
				case 2: return new Vector2(state.rAxis2.x, state.rAxis2.y);
				case 3: return new Vector2(state.rAxis3.x, state.rAxis3.y);
				case 4: return new Vector2(state.rAxis4.x, state.rAxis4.y);
			}
			return Vector2.zero;
		}

		public void TriggerHapticPulse(ushort durationMicroSec = 500, Controllers.EVRButtonId buttonId = Controllers.EVRButtonId.k_EButton_SteamVR_Touchpad)
		{
			//var system = OpenVR.System;
			//if (system != null)
			//{
				var axisId = (uint)buttonId - (uint)Controllers.EVRButtonId.k_EButton_Axis0;
				Controllers.TriggerHapticPulse(index, axisId, (char)durationMicroSec);
			//}
		}

		public float hairTriggerDelta = 0.1f; // amount trigger must be pulled or released to change state
		float hairTriggerLimit;
		bool hairTriggerState, hairTriggerPrevState;
		void UpdateHairTrigger()
		{
			hairTriggerPrevState = hairTriggerState;
			var value = state.rAxis1.x; // trigger
			if (hairTriggerState)
			{
				if (value < hairTriggerLimit - hairTriggerDelta || value <= 0.0f)
					hairTriggerState = false;
			}
			else
			{
				if (value > hairTriggerLimit + hairTriggerDelta || value >= 1.0f)
					hairTriggerState = true;
			}
			hairTriggerLimit = hairTriggerState ? Mathf.Max(hairTriggerLimit, value) : Mathf.Min(hairTriggerLimit, value);
		}

		public bool GetHairTrigger() { Update(); return hairTriggerState; }
		public bool GetHairTriggerDown() { Update(); return hairTriggerState && !hairTriggerPrevState; }
		public bool GetHairTriggerUp() { Update(); return !hairTriggerState && hairTriggerPrevState; }
	}

	private static Device[] devices;

	public static Device Input(int deviceIndex)
	{
		if (devices == null)
		{
			devices = new Device[Controllers.k_unMaxTrackedDeviceCount];
			for (uint i = 0; i < devices.Length; i++)
				devices[i] = new Device(i);
		}

		return devices[deviceIndex];
	}

	public static void UpdateDev()
	{
		for (int i = 0; i < Controllers.k_unMaxTrackedDeviceCount; i++)
			Input(i).Update();
	}


    public Device Input()
    {
        if (myId == -1) return null;
        if (devices == null)
        {
            devices = new Device[Controllers.k_unMaxTrackedDeviceCount];
            for (uint i = 0; i < devices.Length; i++)
                devices[i] = new Device(i);
        }
        return devices[myId];
    }
    
    public void Update()
    {
        if (myId == -1) return;
        Controllers.TrackedDevicePose2_t pose = Input().GetPose();
        transform.localPosition = pose.DevicePosition;
        transform.localRotation = pose.DeviceRotation;
    }

	// This helper can be used in a variety of ways.  Beware that indices may change
	// as new devices are dynamically added or removed, controllers are physically
	// swapped between hands, arms crossed, etc.
	public enum DeviceRelation
	{
		First,
		// radially
		Leftmost,
		Rightmost,
		// distance - also see vr.hmd.GetSortedTrackedDeviceIndicesOfClass
		FarthestLeft,
		FarthestRight,
	}
	//public static int GetDeviceIndex(DeviceRelation relation,
 //       Controllers.ETrackedDeviceClass deviceClass = Controllers.ETrackedDeviceClass.Controller,
	//	int relativeTo = (int)Controllers.k_unTrackedDeviceIndex_Hmd) // use -1 for absolute tracking space
	//{
	//	var result = -1;

	//	var invXform = ((uint)relativeTo < Controllers.k_unMaxTrackedDeviceCount) ?
	//		Input(relativeTo).transform.GetInverse() : SteamVR_Utils.RigidTransform.identity;

	//	var system = OpenVR.System;
	//	if (system == null)
	//		return result;

	//	var best = -float.MaxValue;
	//	for (int i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
	//	{
	//		if (i == relativeTo || system.GetTrackedDeviceClass((uint)i) != deviceClass)
	//			continue;

	//		var device = Input(i);
	//		if (!device.connected)
	//			continue;

	//		if (relation == DeviceRelation.First)
	//			return i;

	//		float score;

	//		var pos = invXform * device.transform.pos;
	//		if (relation == DeviceRelation.FarthestRight)
	//		{
	//			score = pos.x;
	//		}
	//		else if (relation == DeviceRelation.FarthestLeft)
	//		{
	//			score = -pos.x;
	//		}
	//		else
	//		{
	//			var dir = new Vector3(pos.x, 0.0f, pos.z).normalized;
	//			var dot = Vector3.Dot(dir, Vector3.forward);
	//			var cross = Vector3.Cross(dir, Vector3.forward);
	//			if (relation == DeviceRelation.Leftmost)
	//			{
	//				score = (cross.y > 0.0f) ? 2.0f - dot : dot;
	//			}
	//			else
	//			{
	//				score = (cross.y < 0.0f) ? 2.0f - dot : dot;
	//			}
	//		}
			
	//		if (score > best)
	//		{
	//			result = i;
	//			best = score;
	//		}
	//	}

	//	return result;
	//}
}

