using System.Collections;
using UnityEngine;
using VRStandardAssets.Utils;

namespace VRStandardAssets.Menu
{
    // This script flips through a series of textures
    // whilst the user is looking at it.
    public class MenuAnimator : MonoBehaviour
    {
        [SerializeField] private int m_FrameRate = 30;                  // The number of times per second the image should change.
        [SerializeField] private MeshRenderer m_ScreenMesh;             // The mesh renderer who's texture will be changed.
        [SerializeField] private VRInteractiveItem m_VRInteractiveItem; // The VRInteractiveItem that needs to be looked at for the textures to play.
        [SerializeField] private Texture[] m_AnimTextures;              // The textures that will be looped through.


        private WaitForSeconds m_FrameRateWait;                         // The delay between frames.
        private int m_CurrentTextureIndex;                              // The index of the textures array.


        private void Awake ()
        {
            // The delay between frames is the number of seconds (one) divided by the number of frames that should play during those seconds (frame rate).
            m_FrameRateWait = new WaitForSeconds (1f / m_FrameRate);
        }


        private void OnEnable ()
        {
            m_VRInteractiveItem.OnOver += HandleOver;
            m_VRInteractiveItem.OnOut += HandleOut;
        }


        private void OnDisable ()
        {
            m_VRInteractiveItem.OnOver -= HandleOver;
            m_VRInteractiveItem.OnOut -= HandleOut;
        }


        private void HandleOver ()
        {
        }


        private void HandleOut ()
        {
        }
			
    }
}