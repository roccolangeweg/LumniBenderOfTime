﻿using UnityEngine;
using System.Collections;

using UnityEngine.UI;
using System.Collections.Generic;

public class HUDController : MonoBehaviour {

    private Canvas myCanvas;
    private PlayerController player;
    private GameManager gameManager;

    public Canvas helpCanvas;

    public Text orbText;
    public Text scoreText;
    public Text comboText;

    public Sprite[] timebendIcons;
    public Image timebendIcon;
    public int currentTimebendImage;

    private GameObject loginWithFBButton;

    [ContextMenu ("Sort Frames By Name")]
    void DoSortFrames() {
        System.Array.Sort(timebendIcons, (a,b) => a.name.CompareTo(b.name));
    }

	// Use this for initialization
	void Start () {
        myCanvas = GetComponent<Canvas>();

        gameManager = GameManager.instance;
        player = FindObjectOfType<PlayerController>();

        if (Application.loadedLevelName == "GameScene") {
            //if(gameManager.ShowHelpOnStartup()) {
                helpCanvas.enabled = true;
                Time.timeScale = 0;
            //}

            StartCoroutine(RemoveControlsAfterStart());
        } else if (Application.loadedLevelName == "MainScene") {
            loginWithFBButton = GameObject.Find("ConnectButton");
        }


	}

    void Update() {

        if (Application.loadedLevelName == "GameScene") {

            orbText.text = gameManager.getCollectedOrbs().ToString();
            scoreText.text = gameManager.getCurrentScore().ToString();
            
            if (gameManager.RoundedCombo() >= 2) {
                comboText.text = gameManager.RoundedCombo().ToString();
            } else {
                comboText.text = "1";
            }


            if (player.GetTimebendCharge() < 100) {

                int currentImage = Mathf.RoundToInt(player.GetTimebendCharge() / 2) - 1;

                if (currentImage < 0) {
                    currentImage = 1;
                }

                if(currentTimebendImage != currentImage) {

                    timebendIcon.GetComponent<Animator>().enabled = false;
                    timebendIcon.GetComponent<Animator>().SetBool("Ready", false);
                    timebendIcon.sprite = timebendIcons [currentImage];
                
                }
            
            } else {
                timebendIcon.GetComponent<Animator>().enabled = true;
                timebendIcon.GetComponent<Animator>().SetBool("Ready", true);
            }

            if (!timebendIcon.enabled && player.GetTimebendCharge() < 100) {
                timebendIcon.enabled = true;
            }


        } else if (Application.loadedLevelName == "MainScene") {

            if(Facebook.Unity.FB.IsLoggedIn) {
                loginWithFBButton.SetActive(false);
            } else {
                loginWithFBButton.SetActive(true);
            }

        }

    }

    /* PUBLIC METHODS */
    public void PlayerJump() {
        player.Jump();
    }

    public void PlayerAttack() {
        player.Attack();
    }

    public void PlayerTimebend() {
        if (!player.IsHurt() && player.GetTimebendCharge() == 100) {
            player.Timebend();
            timebendIcon.enabled = false;
        }
    }

    public void StartGame() {
        gameManager.StartGame();
    }

    public void OpenReplays() {
        Everyplay.Show();
    }

    public void FacebookLogin() {
        FindObjectOfType<FacebookManager>().FBLogin();
    }

    public void CloseHelp() {
        helpCanvas.enabled = false;
        Time.timeScale = 1;
    }


    /* PRIVATE METHODS */
    private IEnumerator RemoveControlsAfterStart() {
        yield return new WaitForSeconds(3);
        myCanvas.transform.FindChild("Controls").GetComponent<Animator>().SetBool("FadeOut", true);
    }
}
