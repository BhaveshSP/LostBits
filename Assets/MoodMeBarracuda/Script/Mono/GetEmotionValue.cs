using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MoodMe;
namespace MoodMe
{
    [RequireComponent(typeof(Slider))]
    public class GetEmotionValue : MonoBehaviour
    {
        public enum EmotionEnum
        {
            Angry, Disgust, Happy, Neutral, Sad, Scared, Surprised, EmotionIndex
        }

        public EmotionEnum Emotion;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            Slider thisSlider = GetComponent<Slider>();

            switch (Emotion)
            {
                case EmotionEnum.Angry:
                    thisSlider.value = EmotionsManager.Emotions.angry;
                    break;
                case EmotionEnum.Disgust:
                    thisSlider.value = EmotionsManager.Emotions.disgust;
                    break;
                case EmotionEnum.Happy:
                    thisSlider.value = EmotionsManager.Emotions.happy;
                    break;
                case EmotionEnum.Neutral:
                    thisSlider.value = EmotionsManager.Emotions.neutral;
                    break;
                case EmotionEnum.Sad:
                    thisSlider.value = EmotionsManager.Emotions.sad;
                    break;
                case EmotionEnum.Scared:
                    thisSlider.value = EmotionsManager.Emotions.scared;
                    break;
                case EmotionEnum.Surprised:
                    thisSlider.value = EmotionsManager.Emotions.surprised;
                    break;
                case EmotionEnum.EmotionIndex:
                    thisSlider.value = EmotionsManager.EmotionIndex;
                    break;
            }


        }
    }
}