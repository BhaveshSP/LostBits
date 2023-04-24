using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoodMe;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.Collections;
using Unity.Jobs;

namespace MoodMe
{

    public class EmotionsManager : MonoBehaviour
    {
        //public MeshRenderer PreviewMR;
        //[Header("ENTER LICENSE HERE")]
        //public string Email = "";
        //public string AndroidLicense = "";
        //public string IosLicense = "";
        //public string OsxLicense = "";
        //public string WindowsLicense = "";

        [Header("Input")]
        public ManageEmotionsNetwork EmotionNetworkManager;
        public FaceDetector FaceDetectorManager;

        [Header("Performance")]
        [Range(1, 60)]
        public int ProcessEveryNFrames = 15;
        [Header("Processing")]
        public bool FilterAllZeros = true;
        [Range(0, 29f)]
        public int Smoothing;
        [Header("Emotions")]
        public bool TestMode = false;
        [Range(0, 1f)]
        public float Angry;
        [Range(0, 1f)]
        public float Disgust;
        [Range(0, 1f)]
        public float Happy;
        [Range(0, 1f)]
        public float Neutral;
        [Range(0, 1f)]
        public float Sad;
        [Range(0, 1f)]
        public float Scared;
        [Range(0, 1f)]
        public float Surprised;
        [Range(0, 1f)]

        public static float EmotionIndex;

        public static MoodMeEmotions.MDMEmotions Emotions;
        private static MoodMeEmotions.MDMEmotions CurrentEmotions;


        //Main buffer texture
        public static WebCamTexture CameraTexture;

        private EmotionsInterface _emotionNN;


        //Main buffer

      
        private byte[] _buffer;
        private bool _bufferProcessed = false;

        private int NFramePassed;

        private static DateTime timestamp;



        // Start is called before the first frame update
        void Start()
        {
            _emotionNN = new EmotionsInterface(EmotionNetworkManager, FaceDetectorManager);

            //int remainingDays = _emotionNN.SetLicense(Email == "" ? null : Email, EnvKey == "" ? null : EnvKey);

            //if (remainingDays == -1)
            //{
            //    Debug.Log("INVALID OR EMPTY LICENSE. The SDK will run in demo mode.");
            //    remainingDays = _emotionNN.SetLicense(null, EnvKey);
            //}

            //if (remainingDays < 0x7ff)
            //{
            //    Debug.Log("Remaining " + remainingDays + " days");
            //    if (remainingDays == 0)
            //    {
            //        Debug.Log("LICENSE EXPIRED. Please contact sales@mood-me.com to extend the license.");
            //    }
            //}
            //else
            //{
            //    Debug.Log("Lifetime license!");
            //}
         
        }

        void OnDestroy()
        {
            _emotionNN = null;
        }


        // Update is called once per frame
        void LateUpdate()
        {
            //If a Render Texture is provided in the VideoTexture (or just a still image), Webcam image will be ignored

            if (!TestMode)
            {
                if (CameraManager.WebcamReady)
                {

                    NFramePassed = (NFramePassed + 1) % ProcessEveryNFrames;
                    if (NFramePassed == 0)
                    {

                        try
                        {

                            _emotionNN.ProcessFrame();
                            _bufferProcessed = true;

                        }
                        catch (Exception ex)
                        {
                            Debug.Log(ex.Message);
                            _bufferProcessed = false;
                        }

                        if (_bufferProcessed)
                        {
                            _bufferProcessed = false;                         
                            if (!(_emotionNN.DetectedEmotions.AllZero && FilterAllZeros))
                            {
                                CurrentEmotions = _emotionNN.DetectedEmotions;
                                Emotions = Filter(Emotions, CurrentEmotions, Smoothing);
                                //Debug.Log("angry " + Emotions.angry);
                                //Debug.Log("disgust " + Emotions.disgust);
                                //Debug.Log("happy " + Emotions.happy);
                                //Debug.Log("neutral " + Emotions.neutral);
                                //Debug.Log("sad " + Emotions.sad);
                                //Debug.Log("scared " + Emotions.scared);
                                //Debug.Log("surprised " + Emotions.surprised);
                                Angry = Emotions.angry;
                                Disgust = Emotions.disgust;
                                Happy = Emotions.happy;
                                Neutral = Emotions.neutral;
                                Sad = Emotions.sad;
                                Scared = Emotions.scared;
                                Surprised = Emotions.surprised;
                            }

                        }
                        else
                        {
                            Emotions.Error = true;
                        }                    
                    }
                }

            }
            else
            {
                Emotions.angry = Angry;
                Emotions.disgust = Disgust;
                Emotions.happy = Happy;
                Emotions.neutral = Neutral;
                Emotions.sad = Sad;
                Emotions.scared = Scared;
                Emotions.surprised = Surprised;
            }
            EmotionIndex = (((3f * Happy + Surprised - (Sad + Scared + Disgust + Angry)) / 3f) + 1f) / 2f;

        }

        // Smoothing function
        MoodMeEmotions.MDMEmotions Filter(MoodMeEmotions.MDMEmotions target, MoodMeEmotions.MDMEmotions source, int SmoothingGrade)
        {
            float targetFactor = SmoothingGrade / 30f;
            float sourceFactor = (30 - SmoothingGrade) / 30f;
            target.angry = target.angry * targetFactor + source.angry * sourceFactor;
            target.disgust = target.disgust * targetFactor + source.disgust * sourceFactor;
            target.happy = target.happy * targetFactor + source.happy * sourceFactor;
            target.neutral = target.neutral * targetFactor + source.neutral * sourceFactor;
            target.sad = target.sad * targetFactor + source.sad * sourceFactor;
            target.scared = target.scared * targetFactor + source.scared * sourceFactor;
            target.surprised = target.surprised * targetFactor + source.surprised * sourceFactor;

            return target;
        }
    }

}