
using System.Runtime.InteropServices;
using UnityEngine;

namespace MoodMe
{
    public class MoodMeEmotions
    {
        public struct MDMEmotions
        {
            public float surprised;
            public float scared;
            public float disgust;
            public float happy;
            public float sad;
            public float angry;
            public float neutral;
            public long latency;
            public long latency_avg;
            public bool AllZero;
            public bool Error;
        };       
    }
}
