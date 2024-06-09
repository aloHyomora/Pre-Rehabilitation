using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Unity.RenderStreaming.Samples
{
    class SimpleVideoControllerV1 : MonoBehaviour
    {
        [SerializeField] VideoPlayer videoPlayer;
        [SerializeField] Image pointer;
        [SerializeField] GameObject noticeTouchControl;

        private RectTransform m_rectTransform = null;


        public void SetDevice(InputDevice device, bool add = false)
        {
        }

        void Start()
        {
            m_rectTransform = GetComponent<RectTransform>();
        }

        public void OnPoint(InputAction.CallbackContext context)
        {
            if (m_rectTransform == null)
                return;
            Debug.Log("Video On Point");
            var position = context.ReadValue<Vector2>();
            var screenSize = new Vector2Int(Screen.width, Screen.height);
            position = position / screenSize * new Vector2(m_rectTransform.rect.width, m_rectTransform.rect.height);
            pointer.rectTransform.anchoredPosition = position;
        }

        public void OnPress(InputAction.CallbackContext context)
        {
            var button = context.ReadValueAsButton();
            pointer.color = button ? Color.red : Color.clear;
            Debug.Log("Video On Press");
        }
    }
}
