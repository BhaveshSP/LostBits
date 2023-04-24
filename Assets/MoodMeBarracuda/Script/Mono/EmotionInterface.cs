using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;
using AOT;

namespace MoodMe
{
    public class EmotionsInterface
    {
        private IntPtr handler;
        private static MoodMeEmotions.MDMEmotions _detectedemotions;
        private float[] buff;
        private DateTime timestamp;

        public ManageEmotionsNetwork EmotionNetworkManager { get; set; }
        public FaceDetector FaceDetectorManager { get; set; }

        
        public EmotionsInterface(ManageEmotionsNetwork emotionNetworkManager, FaceDetector faceDetector)
        {
            EmotionNetworkManager = emotionNetworkManager;
            FaceDetectorManager = faceDetector;
        }


        public bool ProcessFrame()
        {

            //bool res = false;
            EmotionNetworkManager.Process = true;
            FaceDetectorManager.Process = true;
            return true;
        }


        public MoodMeEmotions.MDMEmotions DetectedEmotions
        {
            get
            {
                buff = EmotionNetworkManager.GetCurrentEmotionValues;
                if (buff != null)
                {
                    _detectedemotions = new MoodMeEmotions.MDMEmotions()
                    {
                        angry = buff[0],
                        disgust = buff[1],
                        scared = buff[2],
                        happy = buff[3],
                        sad = buff[4],
                        surprised = buff[5],
                        neutral = buff[6],
                        latency = 0,
                        latency_avg = 0,
                        AllZero = (buff[0] + buff[1] + buff[2] + buff[3] + buff[4] + buff[5] + buff[6]) == 0,
                        Error = false
                    };

                }
                return _detectedemotions;
            }
        }

        private static string GetLastTrackerError()
        {
            string s = "X";
            return (s);
        }
        public int SetLicense(string email, string key)
        {
            return 0x7fff;
        }
    }
}