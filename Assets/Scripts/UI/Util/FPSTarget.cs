using System;
using UnityEngine;

namespace UI.Util
{
    public class FPSTarget : MonoBehaviour
    {
        public int targetFrameRate = 30;
        
        private void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFrameRate;
        }
    }
}