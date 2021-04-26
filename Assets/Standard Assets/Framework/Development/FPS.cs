
using UnityEngine;

namespace WWFramework.Development
{
    public class FPS : MonoBehaviour
    {
        [SerializeField]
        private bool _forceCloseVSync = true;
        [SerializeField, Range(1, 3)]
        private float _checkDelta = 1f;
        [SerializeField] private Vector2 _offset = new Vector2(20, 10);

        private int _frameCount;
        private float _timePassed;

        private float _fps;
        private GUIStyle _style;

        private void Start()
        {
            if (_forceCloseVSync)
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = -1;
            }

            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            _style = new GUIStyle();
            _style.normal.background = null;
            _style.normal.textColor = Color.yellow;
        }

        private void Update()
        {
            _frameCount++;
            _timePassed += Time.deltaTime;

            if (_timePassed > _checkDelta)
            {
                _fps = _frameCount / _timePassed;

                _frameCount = 0;
                _timePassed = 0f;
            }
        }

        private void OnGUI()
        {
            var fontSize = Screen.height/20;
            _style.fontSize = fontSize;

            GUI.Label(new Rect(_offset.x, _offset.y, fontSize * 3, fontSize), string.Format("{0:F0}", _fps), _style);
        }
    }
}