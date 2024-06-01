using System.Collections;
using System.Collections.Generic;
using Tobii.G2OM;
using Tobii.XR.GazeModifier;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using ViveSR.anipal.Eye;

namespace Tobii.XR
{
    /// <summary>
    /// Utility class for handling gaze tracking functionality.
    /// </summary>
    public class GazeUtility
    {
        public bool calibNeeded = true;
        public bool saveTracking = true;

        public string playerName;
        public string experimentTag;

        public Vector3 rayOrigin { get; private set; }
        public Vector3 rayDirection { get; private set; }
        // public Vector3 rotatedGazeRay { get; private set; }

        public GazeUtility()
        {
            playerName = PlayerInfo.Instance.PlayerName;
            experimentTag = PlayerInfo.Instance.ExperimentTag;
        }

        /// <summary>
        /// Retrieves the gaze ray direction.
        /// </summary>
        /// <returns>The direction of the gaze ray.</returns>
        public Vector3 GetGazeRay()
        {
            var eyeTrackingData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World);
            if (eyeTrackingData.GazeRay.IsValid)
            {
                rayOrigin = eyeTrackingData.GazeRay.Origin;
                rayDirection = eyeTrackingData.GazeRay.Direction;

                return rayDirection;
            }
            else
            {
                return Vector3.zero;
            }
        }

        // Utility Methods

        /// <summary>
        /// Initiates the calibration process if needed.
        /// </summary>
        public void Calibrate()
        {
            if (calibNeeded)
            {
                SRanipal_Eye_v2.LaunchEyeCalibration();
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(rayOrigin, rayDirection * 3.0f);
            Gizmos.DrawWireSphere(rayDirection * 3.0f, 5.0f);
        }

        /// <summary>
        /// Calculates the rotation based on the heading direction.
        /// </summary>
        /// <param name="forward">The forward direction.</param>
        /// <param name="up">The up direction.</param>
        /// <returns>The calculated rotation (for heading rotation, not eye tracking) </returns>
        public Quaternion HeadingRotation(Vector3 forward, Vector3 up)
        {
            var provider = TobiiXR.Internal.Provider;
            var localToWorldMatrix = provider.LocalToWorldMatrix;
            var worldAxis = localToWorldMatrix.MultiplyVector(forward);
            var worldUp = localToWorldMatrix.MultiplyVector(up);

            return Quaternion.LookRotation(worldAxis, worldUp);
        }

        /// <summary>
        /// Calculates the rotation based on the gaze tracking direction.
        /// </summary>
        /// <param name="forward">The forward direction.</param>
        /// <param name="up">The up direction.</param>
        /// <returns>The calculated rotation (for eye tracking)</returns>
        public Quaternion GazeTrackingRotation(float speed, Vector3 forward, Quaternion? givenOffsetRotation = null)
        {
            Quaternion offsetRotation = Quaternion.identity;
            if (givenOffsetRotation != null)
            {
                offsetRotation = (Quaternion)givenOffsetRotation;
            }
            var provider = TobiiXR.Internal.Provider;
            var eyeTrackingData = new TobiiXR_EyeTrackingData();
            provider.GetEyeTrackingDataLocal(eyeTrackingData);
            var localToWorldMatrix = provider.LocalToWorldMatrix;
            var worldForward = localToWorldMatrix.MultiplyVector(forward);
            // var worldUp = localToWorldMatrix.MultiplyVector(up); // Not necessary with LookRotation
            EyeTrackingDataHelper.TransformGazeData(eyeTrackingData, localToWorldMatrix);
            var gazeModifierFilter = TobiiXR.Internal.Filter as GazeModifierFilter;

            if (gazeModifierFilter != null) gazeModifierFilter.FilterAccuracyOnly(eyeTrackingData, worldForward);

            var gazeRay = eyeTrackingData.GazeRay.Direction.normalized;

            float step = speed * Time.deltaTime;
            Vector3 shiftDirection = Vector3.RotateTowards(forward, gazeRay, step, 0f);
            return Quaternion.LookRotation(shiftDirection);
            // return Quaternion.LookRotation(gazeRay, worldUp); // Might have to correct and use Vector3.RotateTowards
        }
    }

}