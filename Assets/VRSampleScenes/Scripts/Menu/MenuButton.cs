using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRStandardAssets.Utils;
using System.IO;


namespace VRStandardAssets.Menu
{
    // This script is for loading scenes from the main menu.
    // Each 'button' will be a rendering showing the scene
    // that will be loaded and use the SelectionRadial.
    public class MenuButton : MonoBehaviour
    {
        public event Action<MenuButton> OnButtonSelected;                   // This event is triggered when the selection of the button has finished.


        [SerializeField] private string m_SceneToLoad;                      // The name of the scene to load.
        [SerializeField] private VRCameraFade m_CameraFade;                 // This fades the scene out when a new scene is about to be loaded.
        [SerializeField] private SelectionRadial m_SelectionRadial;         // This controls when the selection is complete.
        [SerializeField] private VRInteractiveItem m_InteractiveItem;       // The interactive item for where the user should click to load the level.


        private bool m_GazeOver;                                            // Whether the user is looking at the VRInteractiveItem currently.
		private bool lectureChosen = false;

        private void OnEnable ()
        {
            m_InteractiveItem.OnOver += HandleOver;
            m_InteractiveItem.OnOut += HandleOut;
            m_SelectionRadial.OnSelectionComplete += HandleSelectionComplete;
        }


        private void OnDisable ()
        {
            m_InteractiveItem.OnOver -= HandleOver;
            m_InteractiveItem.OnOut -= HandleOut;
            m_SelectionRadial.OnSelectionComplete -= HandleSelectionComplete;
        }
        

        private void HandleOver()
        {
            // When the user looks at the rendering of the scene, show the radial.
            m_SelectionRadial.Show();

            m_GazeOver = true;
        }


        private void HandleOut()
        {
            // When the user looks away from the rendering of the scene, hide the radial.
            m_SelectionRadial.Hide();

            m_GazeOver = false;
        }


        private void HandleSelectionComplete()
        {
            // If the user is looking at the rendering of the scene when the radial's selection finishes, activate the button.
            if(m_GazeOver)
                StartCoroutine (ActivateButton());
        }


        private IEnumerator ActivateButton()
        {
            // If the camera is already fading, ignore.
            if (m_CameraFade.IsFading)
                yield break;

            // If anything is subscribed to the OnButtonSelected event, call it.
            if (OnButtonSelected != null)
                OnButtonSelected(this);

			// update static variable lectureNum
			LectureChooser.lectureNum = Int32.Parse(m_SceneToLoad);
			lectureChosen = true;

			// check if lecture is downloaded
			// get path of lecture

			// Wait for the camera to fade out.
			yield return StartCoroutine(m_CameraFade.BeginFadeOut(true));
			SceneManager.LoadScene ("GearStereo360", LoadSceneMode.Single);

//			string lecturePath = Application.persistentDataPath + "/" + "week5_360_TB.mp4";
//			if (System.IO.File.Exists (lecturePath)) 
//			{
//				// lecture file exists already, play lecture
//				// TODO transition scene and play lecture
//				Debug.Log("lecture exists!");
//				// Wait for the camera to fade out.
//				yield return StartCoroutine(m_CameraFade.BeginFadeOut(true));
//				SceneManager.LoadScene ("GearStereo360", LoadSceneMode.Single);
//			} 
//			else 
//			{
//				// TODO prompt user to download lecture
//				// StartCoroutine(DownloadLecture());
//
//			}

            // Load the playLecture scene.
        }

		private IEnumerator DownloadLecture()
		{
			string link = "http://cdn.cs50.net/2016/fall/lectures/5/week5_360_TB.mp4.download";
			WWW www = new WWW(link);
			yield return www;
			while (!www.isDone)
			{
				Debug.Log(www.progress);
				yield return null;
			}

			string savePath = Application.persistentDataPath + "/" + "week5_360_TB.mp4";
			byte[] bytes = www.bytes;

			File.WriteAllBytes(savePath, bytes);
			Debug.Log("saved the lecture file to " + savePath);
		}
    }
}