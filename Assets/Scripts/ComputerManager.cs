using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ComputerManager : Manager<ComputerManager> {

    public Button RestartButton;
    public Animator RestartAnim;
    public Button DownloadButton;
    public Button InstallButton;

    public float DownloadSecs = 2.5f;
    public float RestartSecs = 1.5f;

    void Awake() {
        DownloadButton.onClick.AddListener(() => {
            StopAllCoroutines();
            StartCoroutine(DoDownload());
        });
        InstallButton.onClick.AddListener(() => {
            StopAllCoroutines();
            StartCoroutine(DoInstall());
        });
        RestartButton.onClick.AddListener(() => {
            StopAllCoroutines();
            StartCoroutine(DoRestart());
        });
    }

    void Update() {
    }

    IEnumerator DoDownload() {
        DownloadButton.GetComponentInChildren<TMP_Text>().text = "Downloading...";
        yield return new WaitForSeconds(DownloadSecs);
        DownloadButton.interactable = false;
        InstallButton.gameObject.SetActive(true);
    }

    IEnumerator DoInstall() {
        InstallButton.GetComponentInChildren<TMP_Text>().text = "Installing...";
        yield return new WaitForSeconds(DownloadSecs);
        InstallButton.interactable = false;
        RestartButton.gameObject.SetActive(true);
    }

    IEnumerator DoRestart() {
        RestartAnim.Play("Restart");
        yield return new WaitForSeconds(RestartSecs);
        GameManager.Inst.HasUpdate = true;
        RestartButton.interactable = false;
    }
}