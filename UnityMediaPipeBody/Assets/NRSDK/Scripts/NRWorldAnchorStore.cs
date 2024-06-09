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
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using UnityEngine;

    /// <summary> NR world anchor store. </summary>
    public class NRWorldAnchorStore : IDisposable
    {
        /// <summary> The native mapping. </summary>
        private NativeMapping m_NativeMapping;
        /// <summary> Dictionary of anchors. </summary>
        private Dictionary<UInt64, NRWorldAnchor> m_AnchorDict = new Dictionary<UInt64, NRWorldAnchor>();
        /// <summary> Dictionary of anchor uuid and UserDefinedKey. </summary>
        private Dictionary<string, string> m_Anchor2ObjectDict = new Dictionary<string, string>();
        /// <summary> The NRWorldAnchorStore instance </summary>
        public static NRWorldAnchorStore Instance;

        /// <summary> Filename of the map folder. </summary>
        public const string MapFolder = "XrealMaps";
        /// <summary> Path of the map folder. </summary>
        public readonly string MapPath;
        /// <summary> The anchor to object file. </summary>
        public const string Anchor2ObjectFile = "anchor2object.json";

        /// <summary> Default constructor. </summary>
        internal NRWorldAnchorStore()
        {
#if !UNITY_EDITOR
            m_NativeMapping = new NativeMapping(NRSessionManager.Instance.NativeAPI);
#endif
            Instance = this;
            MapPath =
#if UNITY_EDITOR
                Path.Combine(Directory.GetCurrentDirectory(), MapFolder);
#else
                Path.Combine(Application.persistentDataPath, MapFolder);
#endif
            if (!Directory.Exists(MapPath))
                Directory.CreateDirectory(MapPath);
            string path = Path.Combine(MapPath, Anchor2ObjectFile);
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                NRDebugger.Info("[NRWorldAnchorStore] Anchor2Object json: {0}", json);
                m_Anchor2ObjectDict = LitJson.JsonMapper.ToObject<Dictionary<string, string>>(json);
                for (int i = 0; i < m_Anchor2ObjectDict.Count;)
                {
                    var item = m_Anchor2ObjectDict.ElementAt(i).Key;
                    if (File.Exists(Path.Combine(MapPath, item)))
                        i++;
                    else
                        m_Anchor2ObjectDict.Remove(item);
                }
            }

            NRKernalUpdater.OnUpdate += OnUpdate;
        }

        /// <summary> Cleans up the WorldAnchorStore and releases memory. </summary>
        public void Dispose()
        {
            m_NativeMapping = null;
            NRKernalUpdater.OnUpdate -= OnUpdate;
        }

        /// <summary> Executes the 'update' action. </summary>
        private void OnUpdate()
        {
            if (m_AnchorDict.Count == 0)
                return;
#if !UNITY_EDITOR
            var listhandle = m_NativeMapping.CreateAnchorList();
            m_NativeMapping.UpdateAnchor(listhandle);
            var size = m_NativeMapping.GetAnchorListSize(listhandle);
            for (int i = 0; i < size; i++)
            {
                var anchorhandle = m_NativeMapping.AcquireItem(listhandle, i);
                if (m_AnchorDict.ContainsKey(anchorhandle))
                {
                    m_AnchorDict[anchorhandle].CurrentTrackingState = m_NativeMapping.GetTrackingState(anchorhandle);
                    if (m_AnchorDict[anchorhandle].CurrentTrackingState == TrackingState.Tracking)
                    {
                        Pose pose = ConversionUtility.ApiWorldToUnityWorld(m_NativeMapping.GetAnchorPose(anchorhandle));
                        m_AnchorDict[anchorhandle].UpdatePose(pose);
                    }
                }
            }
            m_NativeMapping.DestroyAnchorList(listhandle);
#endif
        }

        /// <summary> Creates an NRWorldAnchor. </summary>
        /// <param name="anchor"> The NRWorldAnchor handler.</param>
        /// <returns> The new anchor. </returns>
        public bool CreateAnchor(NRWorldAnchor anchor)
        {
            NRDebugger.Info("[NRWorldAnchorStore] Create a new NRWorldAnchor handle");
            Pose pose = new Pose(anchor.transform.position, anchor.transform.rotation);
            UInt64 handle = 0;
#if UNITY_EDITOR
            handle = (ulong)UnityEngine.Random.Range(1, int.MaxValue);
#else
            handle = m_NativeMapping.AddAnchor(pose);
#endif
            if (handle == 0)
                return false;
#if UNITY_EDITOR
            anchor.UUID = Guid.NewGuid().ToString();
#else
            anchor.UUID = m_NativeMapping.GetAnchorUUID(handle);
#endif
            anchor.AnchorHandle = handle;
            m_AnchorDict[handle] = anchor;

            return true;
        }

        /// <summary>
        /// Bind an anchor to an existing handle.
        /// </summary>
        /// <param name="anchor"> The NRWorldAnchor to be associated with. </param>
        /// <param name="handle"> The handle to be associated with. </param>
        public void BindAnchor(NRWorldAnchor anchor, UInt64 handle)
        {
            anchor.AnchorHandle = handle;
            m_AnchorDict[handle] = anchor;
        }

        /// <summary>
        /// Saves an NRWorldAnchor to the disk
        /// </summary>
        /// <param name="anchor"> The NRWorldAnchor to be saved. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool SaveAnchor(NRWorldAnchor anchor)
        {
            NRDebugger.Info("[NRWorldAnchorStore] Save Anchor: {0}", anchor.UserDefinedKey);
            if (m_Anchor2ObjectDict.ContainsKey(anchor.UUID))
            {
                NRDebugger.Warning("[NRWorldAnchorStore] Save a new anchor that has already been saved.");
                return false;
            }

            try
            {
                m_Anchor2ObjectDict.Add(anchor.UUID, anchor.UserDefinedKey);
                string json = LitJson.JsonMapper.ToJson(m_Anchor2ObjectDict);
                string path = Path.Combine(MapPath, Anchor2ObjectFile);
                NRDebugger.Info("[NRWorldAnchorStore] Save to the path:" + path + " json:" + json);
                File.WriteAllText(path, json);
                AsyncTaskExecuter.Instance.RunAction(() =>
                {
                    bool success = true;
#if UNITY_EDITOR
                    Thread.Sleep(1000);
                    File.Create(Path.Combine(MapPath, anchor.UUID)).Dispose();
#else
                    success = m_NativeMapping.SaveAnchor(anchor.AnchorHandle, Path.Combine(MapPath, anchor.UUID));
#endif
                    if (!success)
                    {
                        MainThreadDispather.QueueOnMainThread(() =>
                        {
                            NRDebugger.Info("[NRWorldAnchorStore] Save Anchor failed.");
                            m_Anchor2ObjectDict.Remove(anchor.UUID);
                        });
                    }
                });

                return true;
            }
            catch (Exception e)
            {
                NRDebugger.Warning("[NRWorldAnchorStore] Write anchor to object dict exception:" + e.ToString());
                return false;
            }
        }

        /// <summary> Saves all NRWorldAnchor. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool SaveAllAnchors()
        {
            NRDebugger.Info("[NRWorldAnchorStore] Save all worldanchors: {0}.", m_AnchorDict.Count);
            foreach (var item in m_AnchorDict.Values)
            {
                if (!m_Anchor2ObjectDict.ContainsKey(item.UUID))
                {
                    SaveAnchor(item);
                }
            }
            return true;
        }

        /// <summary> Destroy a NRWorldAnchor from the memory. </summary>
        /// <param name="anchor"> The NRWorldAnchor to be destroyed. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool DestroyAnchor(NRWorldAnchor anchor)
        {
            NRDebugger.Info("[NRWorldAnchorStore] Destroy Anchor {0}.", anchor.UUID);

            if (m_AnchorDict.ContainsKey(anchor.AnchorHandle))
            {
                m_AnchorDict.Remove(anchor.AnchorHandle);
            }
#if !UNITY_EDITOR
            AsyncTaskExecuter.Instance.RunAction(() =>
                {
                    m_NativeMapping.DestroyAnchor(anchor.AnchorHandle);
                }
            );
#endif
            GameObject.Destroy(anchor.gameObject);
            return true;
        }

        /// <summary> Destroy all NRWorldAnchors. </summary>
        public void Destroy()
        {
            NRDebugger.Info("[NRWorldAnchorStore] Destroy all worldanchors: {0}.", m_AnchorDict.Count);
            foreach (var item in m_AnchorDict)
            {
#if !UNITY_EDITOR
                var key = item.Key;
                AsyncTaskExecuter.Instance.RunAction(() =>
                    {
                        m_NativeMapping.DestroyAnchor(key);
                    }
                );
#endif
                GameObject.Destroy(item.Value.gameObject);
            }
            m_AnchorDict.Clear();
        }

        /// <summary> Erase a NRWorldAnchor from disk </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool EraseAnchor(NRWorldAnchor anchor)
        {
            NRDebugger.Info("[NRWorldAnchorStore] Erase Anchor: {0}.", anchor.UUID);
            if (m_Anchor2ObjectDict.ContainsKey(anchor.UUID))
                m_Anchor2ObjectDict.Remove(anchor.UUID);

            string path = Path.Combine(MapPath, anchor.UUID);
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }
            return false;
        }

        /// <summary> Loads a NRWorldAnchor from disk for given identifier.</summary>
        /// <param name="uuid"> anchor uuid .</param>
        /// <param name="action"> Execute in main thread after success load.</param>
        public void LoadwithUUID(string uuid, Action<UInt64> action)
        {
            if (m_Anchor2ObjectDict.ContainsKey(uuid))
            {
                string path = Path.Combine(MapPath, uuid);
                if (File.Exists(path))
                {
                    AsyncTaskExecuter.Instance.RunAction(() =>
                    {
                        UInt64 handle = 0;
#if UNITY_EDITOR
                        handle = (ulong)UnityEngine.Random.Range(1, int.MaxValue);
#else
                        handle = m_NativeMapping.LoadAnchor(path);
#endif
                        MainThreadDispather.QueueOnMainThread(() =>
                        {
                            if (handle == 0)
                            {
                                NRDebugger.Info("[NRWorldAnchorStore] Load Anchor failed: {0}.", uuid);
                                m_Anchor2ObjectDict.Remove(uuid);
                            }
                            else
                                action?.Invoke(handle);
                        });
                    });
                }
            }
        }

        /// <summary>
        /// Retrieves a dictionary of loadable anchor UUIDs that are not currently loaded in the session.
        /// </summary>
        /// <returns> A dictionary of UUIDs and user-defined keys.</returns>
        public Dictionary<string, string> GetLoadableAnchorUUID()
        {
            var existingUUID = m_AnchorDict.Select(x => x.Value.UUID).ToList();
            return m_Anchor2ObjectDict.Where(x => !existingUUID.Contains(x.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
