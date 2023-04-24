using Unity;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using System;
using System.Linq;
using static MoodMe.FaceDetectorPostProcessing;

namespace MoodMe
{
    public class FaceDetector : MonoBehaviour
    {
        public NNModel Network;

        public int ImageInputWidth = 640;
        public int ImageInputHeight = 480;

        public int ImageNetworkWidth = 320;
        public int ImageNetworkHeight = 240;

        [Range(1, 4)]
        public int ChannelCount = 3;

        [Range(0, 1)]
        public float DetectionThreshold = 0.7f;

        public WorkerFactory.Device Device = WorkerFactory.Device.Auto;

        public bool Process;

        public bool GUIPreview = true;

        public GameObject PreviewTrackerPlane;
        public GameObject PreviewCropPlane;


        public bool ExportCrop = true;
        public int ExportCropWidth = 48;
        public int ExportCropHeight = 48;
        public Preprocessing.ValueType CropFormat;

        public static Color32[] OutputCrop;

        public static float[] OutputCropFloat;



        private IWorker engine;

        private Tensor scores;
        private Tensor boxes;
        private Tensor output;
        private Tensor tensor;

        private List<FaceInfo> predict_boxes;

        private Color32[] rgba;
        private float[] tensorData;
        private bool predictDone = false;
        private Texture2D _staticRectTexture;
        private GUIStyle _staticRectStyle;
        private float xScale, yScale;

        private int GUIW, GUIH;

        


        // Start is called before the first frame update
        void Start()
        {
            Model model = ModelLoader.Load(Network);
            string[] additionalOutputs = new string[] { "scores", "boxes" };
            engine = WorkerFactory.CreateWorker(WorkerFactory.Type.Compute, model, additionalOutputs, false);
            xScale = ImageInputWidth / (float)ImageNetworkWidth;
            yScale = ImageInputHeight / (float)ImageNetworkHeight;



            // _staticRectStyle = new GUIStyle();
            // _staticRectTexture = new Texture2D(1, 1);
            // _staticRectTexture.SetPixel(0, 0, new Color(1f, 0f, 0f, 0.5f));
            // _staticRectTexture.Apply();
            // _staticRectStyle.normal.background = _staticRectTexture;
        }

        // Update is called once per frame
        void Update()
        {
            if (!Process) return;
            Texture2D previewTexture;
            
            rgba = CameraManager.GetPixels;
            if (rgba.Length == (ImageInputHeight * ImageInputWidth))
            {
                //GameObject _preview = GameObject.Find("PreviewTracker");
              

                

                Preprocessing.InputImage = rgba;
//#if UNITY_EDITOR||UNITY_STANDALONE_OSX||UNITY_STANDALONE_WIN
                previewTexture = new Texture2D(ImageNetworkWidth, ImageNetworkHeight, TextureFormat.RGBA32, false);
                tensorData = Preprocessing.Preprocess(ImageInputWidth, ImageInputHeight, ImageNetworkWidth, ImageNetworkHeight, TextureFormat.RGB24, Preprocessing.OrientationType.Upsidedown, Preprocessing.ValueType.LinearNormalized);
//#else
//                previewTexture = new Texture2D(ImageNetworkHeight, ImageNetworkWidth, TextureFormat.RGBA32, false);
//                tensorData = Preprocessing.Preprocess(ImageInputWidth, ImageInputHeight, ImageNetworkWidth, ImageNetworkHeight, TextureFormat.RGB24, Preprocessing.OrientationType.ACW90, Preprocessing.ValueType.LinearNormalized);
//#endif
                previewTexture.SetPixels32(Preprocessing.OutputImage);
                previewTexture.Apply();

                if (PreviewTrackerPlane != null) PreviewTrackerPlane.GetComponent<MeshRenderer>().material.mainTexture = previewTexture;

                tensor = new Tensor(1, ImageNetworkHeight, ImageNetworkWidth, ChannelCount, tensorData);
                DateTime timestamp;
                timestamp = DateTime.Now;
                output = engine.ExecuteAndWaitForCompletion(tensor);
                //Debug.Log("FACE DETECTOR INFERENCE TIME: " + (DateTime.Now - timestamp).TotalMilliseconds + " ms");
                scores = engine.PeekOutput("scores");
                boxes = engine.PeekOutput("boxes");
                predictDone = false;
                predict_boxes = Predict(ImageNetworkWidth, ImageNetworkHeight, scores.ToReadOnlyArray(), boxes.ToReadOnlyArray(), DetectionThreshold);
                predictDone = true;

                FaceInfo _bestBox = GetBestBox(predict_boxes);
                _bestBox = GetBiggestSquare(_bestBox);

                int _boxSide = Mathf.CeilToInt(_bestBox.x2 - _bestBox.x1);

                if ((_boxSide > 0) && ExportCrop)
                {
                    try
                    {
                        int _xCrop = ImageInputWidth - (int)(_bestBox.x2 * xScale);
                        int _yCrop = ImageInputHeight - (int)(_bestBox.y2 * yScale);

                        int _xBoxSide = (int)(_boxSide * xScale);
                        int _yBoxSide = (int)(_boxSide * yScale);

                        Vector2 _cropStart = new Vector2(_xCrop, _yCrop);
                        Vector2 _cropEnd = new Vector2(_xCrop + _xBoxSide, _yCrop + _yBoxSide);

                        Rect _souceRec = new Rect(0, 0, ImageInputWidth, ImageInputHeight);


                        if (_souceRec.Contains(_cropStart) && _souceRec.Contains(_cropEnd))
                        {
                            OutputCrop = new Color32[_boxSide * _boxSide];
                            Texture2D _cropTexture = new Texture2D((int)(_boxSide * xScale), (int)(_boxSide * yScale), TextureFormat.RGBA32, false);
                            _cropTexture.SetPixels(0, 0, (int)(_boxSide * xScale), (int)(_boxSide * yScale), CameraManager.GetTexture.GetPixels(_xCrop, _yCrop, _xBoxSide, _yBoxSide));
                            _cropTexture.Apply();


                            if (PreviewCropPlane != null) PreviewCropPlane.GetComponent<MeshRenderer>().material.mainTexture = _cropTexture;


                            OutputCrop = _cropTexture.GetPixels32();
                            Preprocessing.InputImage = OutputCrop;
                            OutputCrop = Preprocessing.Preprocess((int)(_boxSide * xScale), (int)(_boxSide * yScale), ExportCropWidth, ExportCropHeight, Preprocessing.OrientationType.Source);
                            if (CropFormat != Preprocessing.ValueType.Color32)
                            {
                                OutputCropFloat = Preprocessing.Preprocess(ExportCropWidth, ExportCropHeight, ExportCropWidth, ExportCropHeight, TextureFormat.R8, Preprocessing.OrientationType.Source, Preprocessing.ValueType.Linear);
                            }
                        }

                    }
                    catch (Exception ex)
                    {

                        Debug.Log("CROP EXCEPTION:" + ex.Message);
                    }

                }




                //foreach (FaceInfo faceInfo in predict_boxes)
                //{
                //    Vector3 UL = new Vector3(faceInfo.x1, faceInfo.y1, 0);
                //    Vector3 UR = new Vector3(faceInfo.x2, faceInfo.y1, 0);
                //    Vector3 DL = new Vector3(faceInfo.x1, faceInfo.y2, 0);
                //    Vector3 DR = new Vector3(faceInfo.x2, faceInfo.y2, 0);
                //    Debug.Log("BOX " + faceInfo.x1 + "," + faceInfo.y1 + "," + faceInfo.x2 + "," + faceInfo.y2 + "=" + faceInfo.score);
                //}

                scores.Dispose();
                boxes.Dispose();
                output.Dispose();
                tensor.Dispose();
                Process = false;
            }

        }

        private FaceInfo GetBestBox(List<FaceInfo> predict_boxes)
        {
            FaceInfo _bestBox = new FaceInfo()
            {
                x1 = 0,
                x2 = 0,
                y1 = 0,
                y2 = 0,
                score = 0
            };

            foreach (FaceInfo box in predict_boxes)
            {
                if (box.score > _bestBox.score)
                {
                    _bestBox = box;
                }
            }

            return _bestBox;
        }

        private FaceInfo GetBiggestSquare(FaceInfo Box)
        {
            float boxWidth = Box.x2 - Box.x1;
            float boxHeight = Box.y2 - Box.y1;

            float bigEdge = boxWidth > boxHeight ? boxWidth : boxHeight;
            float smallEdge = boxWidth < boxHeight ? boxWidth : boxHeight;

            float bigX0 = boxWidth > boxHeight ? (Box.x1) : (Box.x1 - (bigEdge - smallEdge) / 2);
            float bigY0 = boxWidth < boxHeight ? (Box.y1) : (Box.y1 - (bigEdge - smallEdge) / 2);

            return new FaceInfo()
            {
                x1 = bigX0,
                y1 = bigY0,
                x2 = bigX0 + bigEdge,
                y2 = bigY0 + bigEdge
            };

        }

        private void OnGUI()
        {


            if (predictDone && GUIPreview)
            {
                GUIW = Screen.width;
                GUIH = Screen.height;
                float ratio = (GUIW>=GUIH?GUIW:GUIH) / ImageNetworkWidth;
                int centerSquareWidth = Mathf.FloorToInt((GUIW / 2) - ((ImageNetworkWidth / 2) * ratio));
                int centerSquareHeight = Mathf.FloorToInt((GUIH / 2) - ((ImageNetworkHeight / 2) * ratio));
                for (int i = 0; i < predict_boxes.Count; i++)
                {
                    float boxWidth = predict_boxes[i].x2 - predict_boxes[i].x1;
                    float boxHeight = predict_boxes[i].y2 - predict_boxes[i].y1;

                    float bigEdge = boxWidth > boxHeight ? boxWidth : boxHeight;
                    float smallEdge = boxWidth < boxHeight ? boxWidth : boxHeight;

                    float bigX0 = boxWidth > boxHeight ? (predict_boxes[i].x1 * ratio) : ((predict_boxes[i].x1 - (bigEdge - smallEdge) / 2) * ratio);
                    float bigY0 = boxWidth < boxHeight ? (predict_boxes[i].y1 * ratio) : ((predict_boxes[i].y1 - (bigEdge - smallEdge) / 2) * ratio);

                    //float bigX1 = boxWidth > boxHeight ? (predict_boxes[i][2] * ratio) : ((predict_boxes[i][2] + (bigEdge - smallEdge) / 2) * ratio);
                    //float bigY1 = boxWidth < boxHeight ? (predict_boxes[i][3] * ratio) : ((predict_boxes[i][3] + (bigEdge - smallEdge) / 2) * ratio);

                    //float smallX0 = boxWidth < boxHeight ? (predict_boxes[i].x1 * ratio) : ((predict_boxes[i].x1 + (bigEdge - smallEdge) / 2) * ratio);
                    //float smallY0 = boxWidth > boxHeight ? (predict_boxes[i].y1 * ratio) : ((predict_boxes[i].y1 + (bigEdge - smallEdge) / 2) * ratio);

                    //float smallX1 = boxWidth < boxHeight ? (predict_boxes[i][2] * ratio) : ((predict_boxes[i][2] - (bigEdge - smallEdge) / 2) * ratio);
                    //float smallY1 = boxWidth > boxHeight ? (predict_boxes[i][3] * ratio) : ((predict_boxes[i][3] - (bigEdge - smallEdge) / 2) * ratio);

                    // Rect rectInst = new Rect(centerSquareWidth + bigX0,
                    //                         centerSquareHeight + bigY0,
                    //                         bigEdge * ratio,
                    //                         bigEdge * ratio);

                    // GUI.Box(rectInst, GUIContent.none, _staticRectStyle);

                    //rectInst = new Rect(centerSquareWidth + smallX0,
                    //                                        centerSquareHeight + smallY0,
                    //                                        smallEdge * ratio,
                    //                                        smallEdge * ratio);

                    //GUI.Box(rectInst, GUIContent.none, _staticRectStyle);


                }

            }
        }

        void OnDestroy()
        {
            scores.Dispose();
            boxes.Dispose();
            output.Dispose();
            tensor.Dispose();
        }

    }
}