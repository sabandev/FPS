// using System.Runtime.InteropServices;
// using UnityEngine;
// using UnityEngine.Rendering;
// using UnityEngine.Rendering.PostProcessing;

// public class StanceVignette : MonoBehaviour
// {
//     [SerializeField] private float min = 0.1f;
//     [SerializeField] private float max = 0.35f;
//     [SerializeField] private float response = 10.0f;

//     private VolumeProfile _profile; 
    
//     private VolumeComponent _vignette;

//     public void Initialise(VolumeProfile profile)
//     {
//         _profile = profile;

//         if (!profile.TryGet<VolumeComponent>(out _vignette))
//             _vignette = profile.Add<VolumeComponent>();

//         _vignette.intensity.Override(min);
//     }

//     public void UpdateVignette(float deltaTime, Stance stance)
//     {
//         var targetIntensity = stance is Stance.Stand ? min : max;

//         _vignette.intensity.valid = Mathf.Lerp
//         (
//             a: _vignette.intensity.value,
//             b: targetIntensity,
//             t: 1.0f - Mathf.Exp(-response * deltaTime)
//         );
//     }
// }
