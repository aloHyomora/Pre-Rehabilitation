using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;

namespace Unity.RenderStreaming.Samples
{
    using InputSystem = UnityEngine.InputSystem.InputSystem;

    /*static class InputReceiverExtension
    {
        public static void CalculateInputRegion(this InputReceiver reveiver, Vector2Int size)
        {
            reveiver.CalculateInputRegion(size, new Rect(0, 0, Screen.width, Screen.height));
        }
    }

    static class InputActionExtension
    {
        public static void AddListener(this InputAction action, Action<InputAction.CallbackContext> callback)
        {
            action.started += callback;
            action.performed += callback;
            action.canceled += callback;
        }
    }*/

    class BroadcastDTx : MonoBehaviour
    {
        [SerializeField] private SignalingManager renderStreaming;
        [SerializeField] private InputReceiver inputReceiver;
        [SerializeField] private SimpleVideoControllerV1 videoController;
        [SerializeField] private VideoStreamSender videoStreamSender;

        private RenderStreamingSettings settings;
        private int lastWidth;
        private int lastHeight;

        private void Awake()
        {
#if URS_USE_AR_FOUNDATION
            InputSystem.RegisterLayout<UnityEngine.XR.ARSubsystems.HandheldARInputDevice>(
                matches: new InputDeviceMatcher()
                    .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
            );
#endif
            settings = SampleManager.Instance.Settings;
            if (settings != null)
            {
                if (videoStreamSender.source != VideoStreamSource.Texture)
                {
                    videoStreamSender.width = (uint)settings.StreamSize.x;
                    videoStreamSender.height = (uint)settings.StreamSize.y;
                }
                videoStreamSender.SetCodec(settings.SenderVideoCodec);
            }
        }

        private void Start()
        {
            if (renderStreaming.runOnAwake)
                return;
            if (settings != null)
                renderStreaming.useDefaultSettings = settings.UseDefaultSettings;
            if (settings?.SignalingSettings != null)
                renderStreaming.SetSignalingSettings(settings.SignalingSettings);
            renderStreaming.Run();

            inputReceiver.OnStartedChannel += OnStartedChannel;
            var map = inputReceiver.currentActionMap;
            map["Point"].AddListener(videoController.OnPoint);
            map["Press"].AddListener(videoController.OnPress);
        }

        private void OnStartedChannel(string connectionId)
        {
            CalculateInputRegion();
        }

        // Parameters can be changed from the Unity Editor inspector when in Play Mode,
        // So this method monitors the parameters every frame and updates scene UI.
        private void Update()
        {
            // Call SetInputChange if window size is changed.
            var width = Screen.width;
            var height = Screen.height;
            if (lastWidth == width && lastHeight == height)
                return;
            lastWidth = width;
            lastHeight = height;

            CalculateInputRegion();
        }

        private void CalculateInputRegion()
        {
            if (!inputReceiver.IsConnected)
                return;
            var width = (int)(videoStreamSender.width / videoStreamSender.scaleResolutionDown);
            var height = (int)(videoStreamSender.height / videoStreamSender.scaleResolutionDown);
            inputReceiver.CalculateInputRegion(new Vector2Int(width, height));
            inputReceiver.SetEnableInputPositionCorrection(true);
        }

        
    }
}
