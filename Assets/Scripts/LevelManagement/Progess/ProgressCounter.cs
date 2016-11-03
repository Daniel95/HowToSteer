using UnityEngine;
using System;
using System.Collections.Generic;

public class ProgressCounter : MonoBehaviour {

    [SerializeField]
    private List<int> stageLevelsAmount;

    private int stageCounter;

    public int StageCounter {
        get { return stageCounter; }
    }

    public int NextStage()
    {
        stageCounter++;
        levelCounter = UnityEngine.Random.Range(1, stageLevelsAmount[stageCounter] + 1);
        return stageCounter;
    }

    public bool CanProgressToNextStage() {
        if (stageCounter + 1 < stageLevelsAmount.Count)
            return true;
        else
            return false;
    }

    public void Reset() {
        levelCounter = 1;
        stageCounter = deadCounter = 0;
    }

    private int levelCounter = 1;

    public int LevelCounter {
        get { return levelCounter; }
    }

    public int NextLevel() {
        levelCounter++;
        if (levelCounter > stageLevelsAmount[stageCounter])
        {
            levelCounter = 1;
        }
        return levelCounter;
    }

    private int deadCounter;

    public int DeadCounter {
        get { return deadCounter; }
    }

    public Action<int> deadCounterAdded;

    public void addtoDeadCounter() {
        deadCounter++;
        if(deadCounterAdded != null)
            deadCounterAdded(deadCounter);
    }
}
