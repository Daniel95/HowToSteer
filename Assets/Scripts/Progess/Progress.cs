using UnityEngine;
using System.Collections.Generic;

public class Progress : MonoBehaviour {

    private ProgressCounter progressCounter;

    void Start()
    {
        //at the start of each level, find the progressCounter, and set the maxLevel
        if (GameObject.FindGameObjectWithTag(Tags.DontDestroyOnLoad.ToString()) != null)
            progressCounter = GameObject.FindGameObjectWithTag(Tags.DontDestroyOnLoad.ToString()).GetComponent<ProgressCounter>();
    }

    void OnCollisionEnter(Collision _coll) {
        //if we hit a killer, we will load the next level in our current stage
        if (_coll.transform.tag == Tags.Killer.ToString())
        {
            if (progressCounter != null)
            {
                progressCounter.addtoDeadCounter();
                LoadStageLevel(progressCounter.StageCounter, progressCounter.NextLevel());
            }
            else
                GetComponent<SceneLoader>().LoadCurrentScene();
        }
        //if we hit the finish, we will load a level in the next stage
        else if (_coll.transform.tag == Tags.Finish.ToString())
        {
            if (progressCounter != null)
            {
                if (progressCounter.CanProgressToNextStage())
                    LoadStageLevel(progressCounter.NextStage(), progressCounter.LevelCounter);
                else
                    GetComponent<SceneLoader>().LoadNewSceneName("EndgameScreen");
            }
            else {
                GetComponent<SceneLoader>().LoadNextScene();
            }
        }
    }

    /// <summary>
    /// Loads a level a from a stage.
    /// </summary>
    private void LoadStageLevel(int _stageNumber, int _levelNumber) {
        GetComponent<SceneLoader>().LoadNewSceneName("Stage" + _stageNumber + "Level" + _levelNumber);
    }
}
