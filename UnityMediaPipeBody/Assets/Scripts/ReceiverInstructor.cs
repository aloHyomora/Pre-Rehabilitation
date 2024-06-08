using System.Collections;
using System.Collections.Generic;
using Unity.RenderStreaming;
using UnityEngine;

public class ReceiverInstructor : MonoBehaviour
{
    [SerializeField] private SignalingManager renderStreaming;
    [SerializeField] private VideoStreamReceiver receiveVideoViewer;
    [SerializeField] private AudioStreamReceiver receiveAudioViewer;
    [SerializeField] private SingleConnection connection;
    private RenderStreamingSettings settings;

    public string connectionId;
    
    // TODO : Instructor 연결 시도 connection Id, Start~
    
    /*void Awake()
    {
        startButton.onClick.AddListener(OnStart);
        stopButton.onClick.AddListener(OnStop);
        if (connectionIdInput != null)
            connectionIdInput.onValueChanged.AddListener(input => connectionId = input);

        receiveVideoViewer.OnUpdateReceiveTexture += OnUpdateReceiveTexture;
        receiveAudioViewer.OnUpdateReceiveAudioSource += source =>
        {
            source.loop = true;
            source.Play();
        };

        inputSender = GetComponent<InputSender>();
        inputSender.OnStartedChannel += OnStartedChannel;

        settings = Samp.Instance.Settings;
    }
    void Start()
    {
        if (renderStreaming.runOnAwake)
            return;

        if (settings != null)
            renderStreaming.useDefaultSettings = settings.Use;
        if (settings?.signalingSettings != null)
            renderStreaming.SetSignalingSettings(settings.SignalingSettings);
        renderStreaming.Run();
    }*/
}
