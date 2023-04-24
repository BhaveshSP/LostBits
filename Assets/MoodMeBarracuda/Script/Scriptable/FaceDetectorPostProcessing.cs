using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
namespace MoodMe
{
    public class FaceDetectorPostProcessing
    {
        [Serializable]
        public struct FaceInfo
        {
            public float x1;
            public float y1;
            public float x2;
            public float y2;
            public float score;

        }

        [BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Fast)]
        public static List<FaceInfo> Predict(int width, int height, float[] scores, float[] boxes, float threshold, float iou_threshold = 0.3f, int top_k = -1)
        {
            List<List<float>> picked_box_probs = new List<List<float>>();
            //List<int> picked_labels = new List<int>();
            List<FaceInfo> result_picked_box = new List<FaceInfo>();
            List<float> result_picked_probs = new List<float>();

            //foreach (int class_index in Enumerable.Range(1, scores.shape[6] - 1)) // Reminder : shape of scores is (1, 1, 1, 1, 1, 1, 2, 17640), so in this case, we only have one iteration
            //{
            List<float> probs = new List<float>();
            List<bool> mask = new List<bool>();
            List<List<float>> newBox_probs = new List<List<float>>();

            for (int i = 0; i < scores.Length; i++)
            {
                if (i % 2 == 1)
                {
                    if (scores[i] > threshold)
                    {
                        probs.Add(scores[i]);
                        mask.Add(true);
                    }
                    else
                    {
                        mask.Add(false);
                    }
                }
            }
            //if (probs.Count == 0) continue; // maybe we don't need that, because there will always be at least one, we would be extremely unlucky if we had no detection at all
            float[,] box_probs = new float[probs.Count, 5];
            int k = 0;
            for (int j = 0; j < boxes.Length / 4; j++)
            {
                if (mask[j] == true)
                {
                    box_probs[k, 0] = boxes[(j * 4) + 0];
                    box_probs[k, 1] = boxes[(j * 4) + 1];
                    box_probs[k, 2] = boxes[(j * 4) + 2];
                    box_probs[k, 3] = boxes[(j * 4) + 3];
                    box_probs[k, 4] = probs[k];
                    k++;
                }
            }

            newBox_probs = HardNMS(box_probs, iou_threshold, top_k);

            picked_box_probs.AddRange(newBox_probs);
            //for (int i = 0; i < newBox_probs.Count; i++)
            //{
            //    //picked_labels.Add(class_index);
            //    picked_labels.Add(1);
            //}
            //}
            //if (!picked_box_probs.Any()) ;

            for (int i = 0; i < picked_box_probs.Count; i++)
            {
                picked_box_probs[i][0] *= width;
                picked_box_probs[i][1] *= height;
                picked_box_probs[i][2] *= width;
                picked_box_probs[i][3] *= height;

                result_picked_box.Add(new FaceInfo
                {
                    x1 = picked_box_probs[i][0],
                    y1 = picked_box_probs[i][1],
                    x2 = picked_box_probs[i][2],
                    y2 = picked_box_probs[i][3],
                    score = picked_box_probs[i][4]
                });

                //result_picked_probs.Add(picked_box_probs[i][4]);
            }
            return (result_picked_box);
        }
        [BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Fast)]
        public static List<List<float>> HardNMS(float[,] box_probs, float iou_threshold, int top_k = -1, int candidate_size = 200)
        {
            //List<float> scores = new List<float>();
            List<float> scores = new List<float>();
            List<int> picked = new List<int>();
            List<List<float>> boxes = new List<List<float>>();
            List<List<float>> rest_boxes = new List<List<float>>();
            List<List<float>> current_box = new List<List<float>>();
            List<int> indexes = new List<int>();
            List<List<float>> result = new List<List<float>>();
            //float[,] boxes = new float[box_probs.GetLength(0), 4];
            for (int i = 0; i < box_probs.GetLength(0); i++)
            {
                boxes.Add(new List<float>
            {
                box_probs[i, 0],
                box_probs[i, 1],
                box_probs[i, 2],
                box_probs[i, 3]
            });

                scores.Add(box_probs[i, 4]);
            }
            indexes = scores
                        .Select((x, i) => new KeyValuePair<float, int>(x, i))
                        .OrderBy(x => x.Key)
                        .Select(x => x.Value)
                        .ToList();

            while (indexes.Count > 0)
            {
                List<int> newIndexes = new List<int>();
                int current = indexes[indexes.Count - 1];
                picked.Add(current);
                if ((0 < top_k) == picked.Any() || indexes.Count == 1) break;
                current_box.Clear();
                current_box.Add(new List<float> { boxes[current][0], boxes[current][1], boxes[current][2], boxes[current][3] });
                indexes.RemoveAt(indexes.Count - 1);

                rest_boxes = boxes;
                rest_boxes = Enumerable.Range(0, indexes.Count).Select(x => boxes[indexes[x]]).ToList();
                List<float> iou = IouOf(rest_boxes, current_box);
                for (int i = 0; i < indexes.Count; i++)
                {
                    if (iou[i] <= iou_threshold)
                    {
                        newIndexes.Add(indexes[i]);
                    }
                    else
                    {
                        //Debug.Log("not accepted at count " + i);
                    }
                }
                indexes.Clear();
                for (int j = 0; j < newIndexes.Count; j++)
                {
                    indexes.Add(newIndexes[j]);
                }
                //indexes = newIndexes;
            }

            for (int j = 0; j < picked.Count; j++)
            {
                result.Add(new List<float> {
                box_probs[picked[j], 0],
                box_probs[picked[j], 1],
                box_probs[picked[j], 2],
                box_probs[picked[j], 3],
                box_probs[picked[j], 4]
            });
            }
            //Debug.Log(result[0][0]);
            //Debug.Log(result[0][1]);
            //Debug.Log(result[0][2]);
            //Debug.Log(result[0][3]);
            //Debug.Log(result[0][4]);

            return result;
        }
        [BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Fast)]
        public static List<float> IouOf(List<List<float>> box0, List<List<float>> box1, float eps = 1E-5f)
        {
            List<List<float>> overlap_left_top = new List<List<float>>();
            List<List<float>> overlap_right_bottom = new List<List<float>>();
            List<float> overlap_area = new List<float>();
            List<float> area0 = new List<float>();
            List<float> area1 = new List<float>();
            List<float> result = new List<float>();

            List<List<float>> box0_0 = new List<List<float>>();
            List<List<float>> box0_1 = new List<List<float>>();
            List<List<float>> box1_0 = new List<List<float>>();
            List<List<float>> box1_1 = new List<List<float>>();

            for (int i = 0; i < box0.Count; i++)
            {
                float max0 = Math.Max(box0[i][0], box1[0][0]);
                float max1 = Math.Max(box0[i][1], box1[0][1]);
                overlap_left_top.Add(new List<float> { max0, max1 });

                float min0 = Math.Min(box0[i][2], box1[0][2]);
                float min1 = Math.Min(box0[i][3], box1[0][3]);
                overlap_right_bottom.Add(new List<float> { min0, min1 });

                box0_0.Add(new List<float> { box0[i][0], box0[i][1] });
                box0_1.Add(new List<float> { box0[i][2], box0[i][3] });
            }

            box1_0.Add(new List<float> { box1[0][0], box1[0][1] });
            box1_1.Add(new List<float> { box1[0][2], box1[0][3] });

            overlap_area = AreaOf(overlap_left_top, overlap_right_bottom);
            area0 = AreaOf(box0_0, box0_1);
            area1 = AreaOf(box1_0, box1_1);
            return Enumerable.Range(0, overlap_area.Count).Select(x => overlap_area[x] / (area0[x] + area1[0] - overlap_area[x] + eps)).ToList();
        }

        public static List<float> AreaOf(List<List<float>> left_top, List<List<float>> right_bottom)
        {
            float value0, value1;
            List<float> hw = new List<float>();
            for (int i = 0; i < left_top.Count; i++)
            {
                value0 = right_bottom[i][0] - left_top[i][0];
                if (value0 < 0.0f) value0 = 0.0f;

                value1 = right_bottom[i][1] - left_top[i][1];
                if (value1 < 0.0f) value1 = 0.0f;

                hw.Add(value0 * value1);
            }
            return hw;
        }
    }
}