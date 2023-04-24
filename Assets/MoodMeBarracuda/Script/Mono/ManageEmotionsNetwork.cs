using Unity;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using System;
using System.Linq;
using static MoodMe.FaceDetectorPostProcessing;
namespace MoodMe
{
    public class ManageEmotionsNetwork : MonoBehaviour
    {

        public NNModel EmotionsNNetwork;

        public int ImageNetworkWidth = 48;
        public int ImageNetworkHeight = 48;

        [Range(1, 4)]
        public int ChannelCount = 1;

        public WorkerFactory.Device Device = WorkerFactory.Device.CPU;

        public bool Process;

        public GameObject PreviewEmotionsPlane;

        public float[] GetCurrentEmotionValues {
            get {
                    return DetectedEmotions.Values.ToArray();
                }
        }


        private static Dictionary<string,float> DetectedEmotions;

        //private static MoodMeEmotions.MDMEmotions CurrentEmotions;

        private IWorker engine;

        private string[] EmotionsLabelFull = { "Angry", "Disgusted", "Scared", "Happy", "Sad", "Surprised", "Neutral" };
        //private string[] EmotionsLabel = { "Angry", "Disgusted", "Scared", "Happy", "Sad", "Surprised", "Neutral" };             
        private string[] EmotionsLabel = { "Neutral", "Surprised", "Sad" };


        private Tensor tensor;
        private Tensor output;

        private Color32[] rgba;
        private float[] tensorData;

        // Start is called before the first frame update
        void Start()
        {

            Model model = ModelLoader.Load(EmotionsNNetwork);
            engine = WorkerFactory.CreateWorker(model, Device);

            DetectedEmotions = new Dictionary<string, float>();

            foreach(string key in EmotionsLabelFull)
            {
                DetectedEmotions.Add(key, 0);
            }
            
        }

        // Update is called once per frame
        void Update()
        {
            if (!Process) return;
            if (FaceDetector.OutputCrop == null) return;
            if (!(FaceDetector.OutputCrop.Length == (ImageNetworkWidth * ImageNetworkHeight))) return;
            Texture2D previewTexture;
            previewTexture = new Texture2D(ImageNetworkWidth, ImageNetworkHeight, TextureFormat.R8, false);


            rgba = FaceDetector.OutputCrop;

            previewTexture.SetPixels32(rgba);
            previewTexture.Apply();


            if (PreviewEmotionsPlane != null) PreviewEmotionsPlane.GetComponent<MeshRenderer>().material.mainTexture = previewTexture;
           
            tensor = new Tensor(previewTexture);
            DateTime timestamp;

            timestamp = DateTime.Now;
            output = engine.ExecuteAndWaitForCompletion(tensor);
            //Debug.Log("EMOTIONS INFERENCE TIME: " + (DateTime.Now - timestamp).TotalMilliseconds + " ms");
            float[] results = output.data.Download(output.shape);

            for (int i = 0; i < results.Length; i++)
            {
                DetectedEmotions[EmotionsLabel[i]] = results[i];
                //Debug.Log(EmotionsLabel[i] + " = " + results[i]);
            }
            //Debug.Log("-------------------------------------------");

            output.Dispose();
            tensor.Dispose();
            Process = false;
        }

        void OnDestroy()
        {
            output.Dispose();
            tensor.Dispose();
        }

       



    }

}