using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Manager<GameManager> {

    public enum FlagModes {
        Die,
        Win
    }
    public FlagModes FlagMode = FlagModes.Die;
    public bool FlagAction;

    public bool CanControlPlayer;
    public bool CanControlPlayer2;

    public float DieTime = 2f;

    [Range(0, 2f)]
    public float LoadTime = 1.2f;
    public GameObject LoadScreen;
    public Level[] Levels;
    public int CurrentLevel;
    int prevCurrentLevel;

    public float GameTime;

    public GameObject WinScreen;
    public TMP_Text TextMessage;
    public TMP_Text TextCurrentTime;
    public TMP_Text TextBestTime;

    public TMP_Text LabelTime;


    public bool Has48;
    public bool Has48b;
    public bool Using48Level;
    public bool HasUpdate;
    public GameObject VersionNeedsUpdate;
    public GameObject CompNeedsUpdate;
    public GameObject VersionBroken;

    [System.Serializable]
    public class SceneConfig {
        public bool Requires48;
        public GameObject[] ObjectsToDisable;
    }
    public SceneConfig[] SceneConfigs;

    void Awake() {
        Load(0);
    }

    void Update() {
        GameTime += Time.deltaTime;
        if (CurrentLevel != prevCurrentLevel) {
            Load(CurrentLevel);
        }
        prevCurrentLevel = CurrentLevel;

        var minsCurrent = (int)(GameTime/60);
        var secsCurrent = (int)(GameTime%60);
        LabelTime.text = minsCurrent + ":" + secsCurrent.ToString("0#");

        VersionBroken.SetActive(HasUpdate && Has48b);
        CompNeedsUpdate.SetActive(!VersionBroken.activeSelf && Has48 && !HasUpdate);
        VersionNeedsUpdate.SetActive(!Has48 && Using48Level);
        CanControlPlayer2 = !VersionBroken.activeSelf && !CompNeedsUpdate.activeSelf && !VersionNeedsUpdate.activeSelf;
    }

    public void LevelNext() {
        CurrentLevel = (CurrentLevel + 1) % Levels.Length;
    }

    public void LevelPrev() {
        CurrentLevel = (CurrentLevel - 1 + Levels.Length) % Levels.Length;
    }

    public void SetFlagMode(int mode) {
        FlagMode = (FlagModes) mode;
    }

    public void SetFlagAction(bool toggle) {
        FlagAction = toggle;
    }

    public void Load(int level) {
        StopAllCoroutines();
        StartCoroutine(DoLoad(level));
    }

    IEnumerator DoLoad(int level) {
        LoadScreen.SetActive(true);
        Levels.ForEach(l => l.gameObject.SetActive(false));
        yield return new WaitForSeconds(LoadTime);
        Levels[level].gameObject.SetActive(true);
        Levels[level].Setup();
        LoadScreen.SetActive(false);
    }

    public void DisableControls() {
        var player = FindObjectOfType<Player>();
        player.GetComponentInSelfOrChildren<Rigidbody2D>().gravityScale = 0;
        player.GetComponentInSelfOrChildren<Rigidbody2D>().velocity = Vector2.zero;
        player.enabled = false;
    }

    public void Die() {
        StopAllCoroutines();
        StartCoroutine(DoDie());
    }

    IEnumerator DoDie() {
        var player = FindObjectOfType<Player>();
        player.GetComponentInChildren<Animator>().Play("Die");
        DisableControls();
        yield return new WaitForSeconds(DieTime);
        yield return DoLoad(CurrentLevel);
    }

    public void TouchedFlag() {
        if (!FlagAction) {
            return;
        }
        switch (FlagMode) {
            case FlagModes.Die:
                Die();
                break;
            case FlagModes.Win:
                Win();
                break;
        }
    }

    public void Win() {
        StopAllCoroutines();
        StartCoroutine(DoWin());
    }

    IEnumerator DoWin() {
        DisableControls();

        var prevBest = PlayerPrefs.GetFloat("BestTime", float.PositiveInfinity);
        var newBest = Mathf.Min(GameTime, prevBest);
        PlayerPrefs.SetFloat("BestTime", newBest);

        var minsBest = (int)(newBest/60);
        var secsBest = (int)(newBest%60);
        TextBestTime.text = minsBest + ":" + secsBest.ToString("0#");

        var minsCurrent = (int)(GameTime/60);
        var secsCurrent = (int)(GameTime%60);
        TextCurrentTime.text = minsCurrent + ":" + secsCurrent.ToString("0#");

        TextMessage.text = GameTime > 120
            ? "Oh hey, you finally jumped the gap! Something must have really gone wrong to take this long..."
            : GameTime > 30
                ? "Phew, you jumped the gap! What took you so long?!"
                : GameTime > 5
                    ? "You jumped the gap! A little slow for a tutorial level, but I'll take it."
                    : "Congratulations, you jumped the gap in a reasonable amount of time!"
                    ;

        WinScreen.SetActive(true);

        yield return null;
    }

    public void SetVersion(int version) {
        switch(version) {
            case 0:
                Has48 = false;
                Has48b = false;
                break;
            case 1:
                Has48 = false;
                Has48b = false;
                break;
            case 2:
                Has48 = true;
                Has48b = true;
                break;
            case 3:
                Has48 = true;
                Has48b = false;
                break;
            case 4:
                Has48 = true;
                Has48b = true;
                break;
        }
    }

    public void SetScene(int scene) {
        foreach (var conf in SceneConfigs) {
            conf.ObjectsToDisable.ForEach(o => o.SetActive(true));
        }
        var config = SceneConfigs[scene];
        config.ObjectsToDisable.ForEach(o => o.SetActive(false));
        GameManager.Inst.Using48Level = config.Requires48;
        Load(0);
    }
}