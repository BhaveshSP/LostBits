using Unity.Burst;
using UnityEngine;

namespace MoodMe
{
    public class Preprocessing
    {
        public enum ValueType
        {
            Color32,
            Linear,
            LinearNormalized
        }

        public enum OrientationType
        {
            Source,
            CW90,
            Upsidedown,
            ACW90,
            XMirrored,
            YMirrored
        }

        public static Color32[] InputImage, OutputImage;
        public TextureFormat InputFormat, OutputFormat;



        [BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Fast)]
        private static void Preprocess32(int InputWidth, int InputHeight, int OutputWidth, int OutputHeight, OrientationType OutputOrientation)
        {
            //Check for no scale and no rotation/flip
            if ((InputWidth == OutputWidth) && (InputHeight == OutputHeight) && (OutputOrientation == OrientationType.Source)) return;

            int _square = (OutputHeight * OutputWidth);
            OutputImage = new Color32[_square];

            int i = 0;

            float _xFactor = InputWidth / (float)OutputWidth;
            float _yFactor = InputHeight / (float)OutputHeight;

            for (int y = 0; y < OutputHeight; y++)
            {
                for (int x = 0; x < OutputWidth; x++)
                {

                    switch (OutputOrientation)
                    {
                        case OrientationType.Upsidedown:
                            {
                                OutputImage[_square - i - 1] = InputImage[(Mathf.FloorToInt(x * _xFactor)) + (Mathf.FloorToInt(y * _yFactor) * InputWidth)];
                                break;
                            }
                        case OrientationType.ACW90:
                            {
                                OutputImage[(OutputWidth - x - 1) * OutputHeight + y] = InputImage[(Mathf.FloorToInt(x * _xFactor)) + (Mathf.FloorToInt(y * _yFactor) * InputWidth)];
                                break;
                            }
                        case OrientationType.CW90:
                            {
                                OutputImage[(OutputWidth - y - 1) + (x * OutputHeight)] = InputImage[(Mathf.FloorToInt(x * _xFactor)) + (Mathf.FloorToInt(y * _yFactor) * InputWidth)];
                                break;
                            }
                        default:
                            {
                                OutputImage[i] = InputImage[(Mathf.FloorToInt(x * _xFactor)) + (Mathf.FloorToInt(y * _yFactor) * InputWidth)];
                                break;
                            }
                    }
                    i++;
                }
            }
        }
        [BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Fast)]
        private static byte[] PreprocessBytes(int InputWidth, int InputHeight, int OutputWidth, int OutputHeight, TextureFormat OutputFormat, OrientationType OutputOrientation)
        {
            int _outputChannels = (OutputFormat == TextureFormat.RGBA32) ? 4 : (OutputFormat == TextureFormat.RGB24) ? 3 : 1;
            byte[] _outputArray = new byte[OutputWidth * OutputHeight * _outputChannels];
            Preprocess32(InputWidth, InputHeight, OutputWidth, OutputHeight, OutputOrientation);

            int j = 0;
            for (int i = 0; i < OutputImage.Length; i++)
            {
                switch (_outputChannels)
                {
                    case 4:
                        {
                            _outputArray[j] = OutputImage[i].r;
                            _outputArray[j + 1] = OutputImage[i].g;
                            _outputArray[j + 2] = OutputImage[i].b;
                            _outputArray[j + 3] = OutputImage[i].a;
                            break;
                        }
                    case 3:
                        {
                            _outputArray[j] = OutputImage[i].r;
                            _outputArray[j + 1] = OutputImage[i].g;
                            _outputArray[j + 2] = OutputImage[i].b;
                            break;
                        }
                    case 1:
                        {
                            _outputArray[j] = (byte)(((76 * OutputImage[i].r) + (150 * OutputImage[i].g) + (29 * OutputImage[i].b)) >> 8);
                            break;
                        }
                    default:
                        break;
                }
                j += _outputChannels;

            }
            return _outputArray;
        }
        [BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Fast)]
        private static float[] PreprocessFloats(int InputWidth, int InputHeight, int OutputWidth, int OutputHeight, TextureFormat OutputFormat, OrientationType OutputOrientation, ValueType OutputType)
        {
            int _outputChannels = (OutputFormat == TextureFormat.RGBA32) ? 4 : (OutputFormat == TextureFormat.RGB24) ? 3 : 1;
            float[] _outputArray = new float[OutputWidth * OutputHeight * _outputChannels];
            Preprocess32(InputWidth, InputHeight, OutputWidth, OutputHeight, OutputOrientation);

            float _normalizer = (OutputType == ValueType.Linear) ? 255f : 128f;
            float _offset = (OutputType == ValueType.Linear) ? 0f : 127f;

            int j = 0;
            for (int i = 0; i < OutputImage.Length; i++)
            {
                switch (_outputChannels)
                {
                    case 4:
                        {
                            _outputArray[j] = (OutputImage[i].r - _offset) / _normalizer;
                            _outputArray[j + 1] = (OutputImage[i].g - _offset) / _normalizer;
                            _outputArray[j + 2] = (OutputImage[i].b - _offset) / _normalizer;
                            _outputArray[j + 3] = (OutputImage[i].a - _offset) / _normalizer;
                            break;
                        }
                    case 3:
                        {
                            _outputArray[j] = (OutputImage[i].r - _offset) / _normalizer;
                            _outputArray[j + 1] = (OutputImage[i].g - _offset) / _normalizer;
                            _outputArray[j + 2] = (OutputImage[i].b - _offset) / _normalizer;
                            break;
                        }
                    case 1:
                        {
                            _outputArray[j] = ((float)(((76 * OutputImage[i].r) + (150 * OutputImage[i].g) + (29 * OutputImage[i].b)) >> 8) - _offset) / _normalizer;
                            break;
                        }
                    default:
                        break;
                }
                j += _outputChannels;

            }
            return _outputArray;
        }
        [BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Fast)]
        public static float[] Preprocess(int InputWidth, int InputHeight, int OutputWidth, int OutputHeight, TextureFormat OutputFormat, OrientationType OutputOrientation, ValueType OutputType)
        {
            return (PreprocessFloats(InputWidth, InputHeight, OutputWidth, OutputHeight, OutputFormat, OutputOrientation, OutputType));
        }
        [BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Fast)]
        public static byte[] Preprocess(int InputWidth, int InputHeight, int OutputWidth, int OutputHeight, TextureFormat OutputFormat, OrientationType OutputOrientation)
        {
            return (PreprocessBytes(InputWidth, InputHeight, OutputWidth, OutputHeight, OutputFormat, OutputOrientation));
        }
        [BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Fast)]
        public static Color32[] Preprocess(int InputWidth, int InputHeight, int OutputWidth, int OutputHeight, OrientationType OutputOrientation)
        {
            Preprocess32(InputWidth, InputHeight, OutputWidth, OutputHeight, OutputOrientation);
            return (OutputImage);
        }
    }
}