using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

    public class MenuControllerStudio : MonoBehaviour
    {

        private int menuNumber;
        #region Menu Dialogs
        [Header("Main Menu Components")]
        [SerializeField] private GameObject menuDefaultCanvas;
        [SerializeField] private GameObject MainPage;
        [SerializeField] private GameObject AboutMenu;
        [SerializeField] private GameObject HistoryMenu;
        //[Space(10)]
        [Header("Secondary UI")]
        [SerializeField] private GameObject ScanReminder;
        public CanvasGroup ScanReminderCanvas;
        
        #endregion

        #region Initialisation - Button Selection & Menu Order
        private void Start()
        {
            menuNumber = 1;
        }
        #endregion

        //MAIN SECTION
        public IEnumerator ConfirmationBox()
        {
            MainPage.SetActive(true);
            yield return new WaitForSeconds(2);
            MainPage.SetActive(false);

        }

    public IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float lerpTime = 0.5f)
    {
            //float srAlpha = Mathf.Lerp(1.0f, 0.0f, 0.8f);
            float _TimeStartedLerp = Time.time;
            float timeSinceStart = Time.time - _TimeStartedLerp;
            float percentageComplete = timeSinceStart / lerpTime;


        while (true)
        {
            timeSinceStart = Time.time - _TimeStartedLerp;
            percentageComplete = timeSinceStart / lerpTime;

            float currentValue = Mathf.Lerp(start, end, percentageComplete);

            cg.alpha = currentValue;
            print("srAlhpa is " + currentValue);

            if (percentageComplete >= 1) break;

            yield return new WaitForEndOfFrame();
        }
        print("done");
    }


    public IEnumerator ScanReminderVisible()
    {
        ScanReminder.SetActive(true);
        ScanReminderCanvas.alpha = 1f;

        yield return new WaitForSeconds(2);
        StartCoroutine(FadeCanvasGroup(ScanReminderCanvas, ScanReminderCanvas.alpha, 0));
        //ScanReminder.SetActive(false);
    }

    private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (menuNumber == 1 || menuNumber == 2 || menuNumber == 3)
                {
                    GoBackToMainMenu();
                    ClickSound();
                }
            }
        }

        private void ClickSound()
        {
            GetComponent<AudioSource>().Play();
        }

        #region Menu Mouse Clicks
        public void MouseClick(string buttonType)
        {
            if (buttonType == "Main")
            {
                menuDefaultCanvas.SetActive(true);
                HistoryMenu.SetActive(false);
                AboutMenu.SetActive(false);
            //   ScanReminder.SetActive(false);
            StartCoroutine(ScanReminderVisible());
            menuNumber = 1;
            }

            if (buttonType == "About")
            {
                menuDefaultCanvas.SetActive(false);
                AboutMenu.SetActive(true);
                HistoryMenu.SetActive(false);
                menuNumber = 2;
            }

            if (buttonType == "History")
            {
                menuDefaultCanvas.SetActive(false);
                HistoryMenu.SetActive(true);
                AboutMenu.SetActive(false);
                menuNumber = 3;
            }
        }
        #endregion
       
        #region Back to Menus
        public void GoBackToMainMenu()
        {
            menuDefaultCanvas.SetActive(true);
            HistoryMenu.SetActive(false);
            AboutMenu.SetActive(false);
        }
        #endregion
    }