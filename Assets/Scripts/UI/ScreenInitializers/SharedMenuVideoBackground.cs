using UnityEngine;
using UnityEngine.Video;

namespace UI.ScreenInitializers
{
    public static class SharedMenuVideoBackground
    {
        private static VideoPlayer _videoPlayer;
        private static RenderTexture _renderTexture;
        private static GameObject _videoPlayerObj;

        public static RenderTexture GetTexture()
        {
            if (_videoPlayerObj == null)
            {
                _videoPlayerObj = new GameObject("SharedMenuVideoPlayer");
                Object.DontDestroyOnLoad(_videoPlayerObj);
                
                _videoPlayer = _videoPlayerObj.AddComponent<VideoPlayer>();
                _videoPlayer.source = VideoSource.Url;
                _videoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, "Video/MainMenuBackground.mp4");
                _videoPlayer.playOnAwake = true;
                _videoPlayer.isLooping = true;
                _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
                _videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

                _renderTexture = new RenderTexture(1920, 1080, 0);
                _videoPlayer.targetTexture = _renderTexture;
            }
            
            if (Managers.GameManager.Instance != null && Managers.GameManager.Instance.AudioManager != null)
            {
                Managers.GameManager.Instance.AudioManager.PlayBGM("Music/cryptex-main-theme.wav");
            }

            _videoPlayer.targetTexture = _renderTexture;
            _videoPlayer.Play();

            return _renderTexture;
        }

        public static void StopVideo()
        {
            if (_videoPlayer != null)
            {
                _videoPlayer.Stop();
            }
        }
    }
}
