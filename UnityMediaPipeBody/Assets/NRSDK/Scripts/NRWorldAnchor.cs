/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/

namespace NRKernal.Persistence
{
    using UnityEngine;
    using System;

    /// <summary>
    /// The WorldAnchor component allows a GameObject's position to be locked in physical space. For
    /// example, a cube arranged on top of a physical desk with a WorldAnchor applied will remain
    /// fixed even as an observer walks around the room. This overrides all manipulation of the
    /// GameObject's position and orientation. To move the GameObject, first remove the WorldAnchor
    /// and manipulate the Transform normally. While it is generally recommended to use Destroy
    /// instead of DestroyImmediate, it's best to call Destroy on WorldAnchor objects. Doing
    /// so will let you manipulate the Transform of the GameObject including adding a new WorldAnchor.
    /// </summary>
    public class NRWorldAnchor : MonoBehaviour
    {
        /// <summary> Event that is fired when this object's tracking state changes. </summary>
        public delegate void OnTrackingChangedDelegate(NRWorldAnchor worldAnchor, TrackingState state);

        /// <summary> Event queue for all listeners interested in OnTrackingChanged events. </summary>
        public event OnTrackingChangedDelegate OnTrackingChanged;

        /// <summary> State of the anchor tracking. </summary>
        private TrackingState m_TrackingState = TrackingState.Stopped;
        public TrackingState CurrentTrackingState
        {
            get => m_TrackingState;
            set
            {
                if (m_TrackingState != value)
                {
                    m_TrackingState = value;
                    OnTrackingChanged?.Invoke(this, m_TrackingState);
                }
            }
        }

        /// <summary> Unique identifier persist across sessions. </summary>
        public string UUID;
        /// <summary> The handle of the anchor object. </summary>
        public UInt64 AnchorHandle;
        /// <summary> A user-defined key associated with the anchor. </summary>
        public string UserDefinedKey;

        /// <summary>
        /// Bind this anchor to an existing handle.
        /// </summary>
        /// <param name="handle"> The handle of the anchor </param>
        public void BindAnchor(UInt64 handle)
        {
            NRWorldAnchorStore.Instance.BindAnchor(this, handle);
        }

        /// <summary>
        /// Create a new anchor at current pose
        /// </summary>
        /// <returns> return true if successful </returns>
        public bool CreateAnchor()
        {
            return NRWorldAnchorStore.Instance.CreateAnchor(this);
        }

        /// <summary>
        /// Save the anchor to the disk
        /// </summary>
        /// <returns> return true if successful </returns>
        public bool SaveAnchor()
        {
            return NRWorldAnchorStore.Instance.SaveAnchor(this);
        }

        /// <summary>
        /// Destroy the anchor from memory
        /// </summary>
        /// <returns> return true if successful </returns>
        public bool DestroyAnchor()
        {
            return NRWorldAnchorStore.Instance.DestroyAnchor(this);
        }

        /// <summary>
        /// Erase the anchor file from disk
        /// </summary>
        /// <returns> return true if successful </returns>
        public bool EraseAnchor()
        {
            return NRWorldAnchorStore.Instance.EraseAnchor(this);
        }

        /// <summary>
        /// Update anchor's position while the state is Tracking.
        /// </summary>
        public void UpdatePose(Pose pose)
        {
            transform.SetPositionAndRotation(pose.position, pose.rotation);
        }
    }
}
