using UnityEngine;

namespace Managers
{
    public class AudioManager : BaseManager
    {
        private AudioSource _toneSource;
        private AudioSource _bgmSource;
        private AudioSource _sfxSource;
        private string _currentBGMFileName = string.Empty;
        private System.Collections.Generic.Dictionary<string, AudioClip> _sfxCache = new System.Collections.Generic.Dictionary<string, AudioClip>();
        private System.Collections.Generic.Dictionary<string, float> _sfxLastPlayed = new System.Collections.Generic.Dictionary<string, float>();
        private AudioClip _sineWaveClip;

        private float _volumeBGM = 1.0f;
        private float _volumeSFX = 1.0f;

        public float VolumeBGM 
        {
            get => _volumeBGM;
            set 
            {
                _volumeBGM = Mathf.Clamp01(value);
                if (_bgmSource != null) _bgmSource.volume = _volumeBGM * 0.5f;
                if (GameManager.Instance != null && GameManager.Instance.Config != null)
                {
                    GameManager.Instance.Config.VolumeBGM = _volumeBGM;
                    GameManager.Instance.SaveConfig();
                }
            }
        }

        public float VolumeSFX 
        {
            get => _volumeSFX;
            set 
            {
                _volumeSFX = Mathf.Clamp01(value);
                if (_sfxSource != null) _sfxSource.volume = _volumeSFX;
                if (_toneSource != null) _toneSource.volume = _volumeSFX;
                if (GameManager.Instance != null && GameManager.Instance.Config != null)
                {
                    GameManager.Instance.Config.VolumeSFX = _volumeSFX;
                    GameManager.Instance.SaveConfig();
                }
            }
        }

        public override void Init()
        {
            if (GameManager.Instance != null && GameManager.Instance.Config != null)
            {
                _volumeBGM = GameManager.Instance.Config.VolumeBGM;
                _volumeSFX = GameManager.Instance.Config.VolumeSFX;
            }
            else
            {
                _volumeBGM = 1.0f;
                _volumeSFX = 1.0f;
            }

            if (_toneSource == null)
            {
                _toneSource = gameObject.AddComponent<AudioSource>();
                _toneSource.playOnAwake = false;
                _toneSource.loop = true;
                _toneSource.volume = _volumeSFX;
            }

            if (_bgmSource == null)
            {
                _bgmSource = gameObject.AddComponent<AudioSource>();
                _bgmSource.playOnAwake = false;
                _bgmSource.loop = true;
                _bgmSource.volume = _volumeBGM * 0.5f;
            }

            if (_sfxSource == null)
            {
                _sfxSource = gameObject.AddComponent<AudioSource>();
                _sfxSource.playOnAwake = false;
                _sfxSource.loop = false;
                _sfxSource.volume = _volumeSFX;
            }

            if (_sineWaveClip == null)
            {
                _sineWaveClip = CreateSineWaveClip(440, 1.0f);
                _toneSource.clip = _sineWaveClip;
            }
        }

        public void PlayTone(float pitch = 1.0f)
        {
            if (_toneSource == null) Init();
            
            _toneSource.pitch = pitch;
            if (!_toneSource.isPlaying)
            {
                _toneSource.Play();
            }
        }

        public void StopTone()
        {
            if (_toneSource != null && _toneSource.isPlaying)
            {
                _toneSource.Stop();
            }
        }

        public void PlayBGM(string fileName)
        {
            if (_bgmSource == null) Init();
            if (_bgmSource == null) return;
            
            if (_bgmSource.isPlaying && _currentBGMFileName == fileName) return;
            
            _currentBGMFileName = fileName;
            StopBGM();
            StartCoroutine(LoadBGMCoroutine(fileName));
        }

        public void StopBGM()
        {
            if (_bgmSource != null && _bgmSource.isPlaying)
            {
                _bgmSource.Stop();
            }
        }

        public void PlaySFX(string fileName, bool preventOverlap = false)
        {
            if (_sfxSource == null) Init();
            if (_sfxSource == null) return;
            
            if (_sfxCache.TryGetValue(fileName, out AudioClip clip))
            {
                if (preventOverlap)
                {
                    if (_sfxLastPlayed.TryGetValue(fileName, out float lastTime))
                    {
                        if (Time.unscaledTime - lastTime < clip.length) return;
                    }
                    _sfxLastPlayed[fileName] = Time.unscaledTime;
                }
                _sfxSource.PlayOneShot(clip);
            }
            else
            {
                StartCoroutine(LoadSFXCoroutine(fileName, true, preventOverlap));
            }
        }

        public void PreloadSFX(string fileName)
        {
            if (!_sfxCache.ContainsKey(fileName))
            {
                StartCoroutine(LoadSFXCoroutine(fileName, false, false));
            }
        }

        private System.Collections.IEnumerator LoadSFXCoroutine(string fileName, bool playOnLoad, bool preventOverlap)
        {
            string url = System.IO.Path.Combine(Application.streamingAssetsPath, fileName).Replace("\\", "/");
            if (!url.Contains("://"))
            {
                if (url.StartsWith("/"))
                    url = "file://" + url;
                else
                    url = "file:///" + url;
            }
            
            UnityEngine.AudioType audioType = UnityEngine.AudioType.UNKNOWN;
            string lowerUrl = url.ToLower();
            if (lowerUrl.EndsWith(".ogg")) audioType = UnityEngine.AudioType.OGGVORBIS;
            else if (lowerUrl.EndsWith(".mp3")) audioType = UnityEngine.AudioType.MPEG;
            else if (lowerUrl.EndsWith(".wav")) audioType = UnityEngine.AudioType.WAV;

            using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(url, audioType))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    AudioClip clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);
                    if (clip != null)
                    {
                        _sfxCache[fileName] = clip;
                        if (playOnLoad && _sfxSource != null)
                        {
                            bool shouldPlay = true;
                            if (preventOverlap)
                            {
                                if (_sfxLastPlayed.TryGetValue(fileName, out float lastTime))
                                {
                                    if (Time.unscaledTime - lastTime < clip.length)
                                        shouldPlay = false;
                                }
                                if (shouldPlay)
                                    _sfxLastPlayed[fileName] = Time.unscaledTime;
                            }
                            if (shouldPlay)
                                _sfxSource.PlayOneShot(clip);
                        }
                    }
                }
            }
        }

        private System.Collections.IEnumerator LoadBGMCoroutine(string fileName)
        {
            string url = System.IO.Path.Combine(Application.streamingAssetsPath, fileName).Replace("\\", "/");
            if (!url.Contains("://"))
            {
                if (url.StartsWith("/"))
                    url = "file://" + url;
                else
                    url = "file:///" + url;
            }
            
            Debug.Log($"[AudioManager] Loading BGM from url: {url}");
            
            UnityEngine.AudioType audioType = UnityEngine.AudioType.UNKNOWN;
            string lowerUrl = url.ToLower();
            if (lowerUrl.EndsWith(".ogg")) audioType = UnityEngine.AudioType.OGGVORBIS;
            else if (lowerUrl.EndsWith(".mp3")) audioType = UnityEngine.AudioType.MPEG;
            else if (lowerUrl.EndsWith(".wav")) audioType = UnityEngine.AudioType.WAV;

            using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(url, audioType))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityEngine.Networking.UnityWebRequest.Result.ConnectionError || www.result == UnityEngine.Networking.UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"Error loading BGM {fileName}: {www.error} URL: {url}");
                }
                else
                {
                    AudioClip clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);
                    if (clip != null)
                    {
                        Debug.Log($"[AudioManager] Successfully loaded {fileName}, length: {clip.length}");
                        _bgmSource.clip = clip;
                        _bgmSource.Play();
                    }
                    else
                    {
                        Debug.LogError("[AudioManager] BGM AudioClip is null after loading.");
                    }
                }
            }
        }

        private AudioClip CreateSineWaveClip(int frequency, float durationSec)
        {
            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * durationSec);
            float[] data = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                data[i] = Mathf.Sin(2 * Mathf.PI * frequency * t);
            }

            AudioClip clip = AudioClip.Create("SineWave", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
