using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MoodMe
{
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    public class MorphController : MonoBehaviour
    {
        SkinnedMeshRenderer _skr;

        // Start is called before the first frame update
        void Start()
        {
            _skr = GetComponent<SkinnedMeshRenderer>();
        }

        // Update is called once per frame
        void Update()
        {

            _skr.SetBlendShapeWeight(0, EmotionsManager.Emotions.angry * 100f);
            _skr.SetBlendShapeWeight(1, EmotionsManager.Emotions.disgust * 100f);
            _skr.SetBlendShapeWeight(2, EmotionsManager.Emotions.happy * 100f);
            _skr.SetBlendShapeWeight(5, EmotionsManager.Emotions.sad * 100f);
            _skr.SetBlendShapeWeight(6, EmotionsManager.Emotions.scared * 100f);
            _skr.SetBlendShapeWeight(7, EmotionsManager.Emotions.surprised * 100f);
        }
    }
}