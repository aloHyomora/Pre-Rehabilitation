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
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    /// <summary> An anchor item. </summary>
    public class AnchorItem : MonoBehaviour, IPointerClickHandler
    {
        /// <summary> The key. </summary>
        public string key;
        /// <summary> The on anchor item click. </summary>
        public Action<string, GameObject> OnAnchorItemClick;
        /// <summary> The anchor panel. </summary>
        [SerializeField]
        private GameObject canvas;
        [SerializeField]
        private Text anchorUUID;

        private NRWorldAnchor m_NRWorldAnchor;
        private Material m_Material;

        void Start()
        {
            if (TryGetComponent(out m_NRWorldAnchor))
            {
                if (canvas != null)
                    canvas.SetActive(true);
                if (anchorUUID != null)
                    anchorUUID.text = m_NRWorldAnchor.UUID;
                m_Material = GetComponentInChildren<Renderer>()?.material;
                if (m_Material != null)
                {
                    m_NRWorldAnchor.OnTrackingChanged += (NRWorldAnchor worldAnchor, TrackingState state) =>
                    {
                        switch (state)
                        {
                            case TrackingState.Tracking:
                                m_Material.color = Color.green;
                                break;
                            case TrackingState.Paused:
                                m_Material.color = Color.white;
                                break;
                            case TrackingState.Stopped:
                                m_Material.color = Color.red;
                                break;
                        }
                    };
                }
            }
        }

        public void Save()
        {
            if (m_NRWorldAnchor != null)
                m_NRWorldAnchor.SaveAnchor();
        }

        public void Erase()
        {
            if (m_NRWorldAnchor != null)
                m_NRWorldAnchor.EraseAnchor();
        }

        public void Destory()
        {
            if (m_NRWorldAnchor != null)
                m_NRWorldAnchor.DestroyAnchor();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnAnchorItemClick?.Invoke(key, gameObject);
        }
    }
}
