using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;

//[System.Serializable]
//public class ScoreIntEvent : UnityEvent<int>
//{
//}
public class MyEventsManager : MonoBehaviour
{
    public delegate void ActionGoalFinish();
    public static event ActionGoalFinish onFinish;

    public delegate void ActionScore(int _i);
    public static event ActionScore onScore;

    public delegate void ActionCollectCoin();
    public static event ActionCollectCoin onCoin;

    public delegate void PlayerDamage();
    public static event PlayerDamage onPlayerDamaged;

    [Header("Trying UnityEvent for enemy grabbed")]
    public UnityEvent EnemyOnHook;

    public static void GoalFinish()
    {
        onFinish.Invoke();
    }

    public static void ScorePoints(int _i)
    {
        onScore.Invoke(_i);
    }

    public static void CoinCollect()
    {
        onCoin.Invoke();
    }
    public static void OnPlayerDamaged()
    {
        onPlayerDamaged.Invoke();
    }
}

public enum EnemyState
{
    Normal,
    Damaged,
    Grappled, // Can move and attack, but slowed
    Pulled,  //Can't move or attack, being pulled
    HeldByPlayer, //Can't move or attack
    Thrown, 
    Dead
}

public enum ElevatorState
{
    Stopped,
    UpOrForward,
    DownOrBack
}

public class MyGameManager : MonoBehaviour
{
    //public ScoreIntEvent m_ScoreIntEvent;
    [Header("Level & Player Values")]
    public static int levelScore = 0;
    public static int health = 0;
    public static int maxHealth = 5;
    public int currHP = 0;
    public int maxHP = 5;
    public int coinsInLevel = 0;
    public int coins = 0;
    public static float levelTime = 0; 
    public static float parTime = 120;
    int gameTotalScore = 0;
    [Header("Text for Next Level and Level Results")]
    public string nextLevelText = "";
    public string levelResultsText = "Level Results";


    [Header("Bonuses and Ranks")]

    [SerializeField] int coinsBonus = 0;
                     int coinCompleteBonus = 5000;
                      int pointsPerCoin = 50;
                      int timeBonus = 0;
                     int pointsPerSecond = 100;
                     int rankPoints = 0;
                     int rankBonus = 100000;
    //goalTime - levelTime
    [Header("Text for Level & Player Values")]
    public TextMeshProUGUI coinsText,
    scoreText,
    timeText,
    healthText,
    ratingText;

    string RatingString;
    GameObject coinToTrack;
    public TextMeshProUGUI coinsTextGoal,
                scoreTextGoal,
                timeTextGoal,
                healthTextGoal,
                ratingTextGoal;
    public GameObject goalObj;
    RectTransform  textGoalArea_Coins,
                      textGoalArea_Time,
                      textGoalArea_Health,
                      textGoalArea_Score;

    bool canUpdate = true;
    [SerializeField] int coinScoreValue = 10;

    [Header("Trying UnityEvent for enemy grabbed")]
    public UnityEvent EnemyOnHook;
    private int seconds;
    private int minutes;
    private int hours;

    static MyGameManager m_gm;

    void Start()
    {
        m_gm = new MyGameManager();
        health = maxHealth;
        maxHP = health;
        currHP = health;
        healthText.text = "Health: " + GetHealth();
        MyEventsManager.onFinish += StopUpdates;
        if (SceneManager.GetActiveScene().name == "Level Results")
        {
            //MyEventsManager.onFinish += TallyPoints;
            StopUpdates();
            LoadLevelResults();
            TallyPoints();
        }
        else
        {
            MyEventsManager.onFinish += SaveLevelResults;
            MyEventsManager.onFinish += LoadResultsScene;
            //MyEventsManager.onFinish += CallRoutineResults;

            MyEventsManager.onCoin += IncrementCoins;
            MyEventsManager.onScore += AddScore;

            

            //Count all coin Objects in level if the related coin prefab
            if (coinToTrack)
            {
                coinsInLevel = FindObjectsOfType<CoinObj>().GetLength(0);
            }
        }   
    }
    
    public void AddScore(int i)
    {
        levelScore += i;
        //possibly save score later when adding
    }

    void IncrementCoins()
    {
        coins++;
        MyEventsManager.ScorePoints(coinScoreValue);
    }

    public static TextMeshProUGUI GetHealthText()
    { 
        return m_gm.healthText;
    }

    // Health functions
    public static void ChangeHealth(int _h)
    {
        health += _h; //the vars in a static function must also be static
        m_gm.healthText.text = "Health: " + GetHealth();
        print("Health is now " + m_gm.healthText.text);
    }
    
    public void ChangeMyHealth(int _h)
    {
        currHP += _h; //the vars in a static function must also be static
        healthText.text = "Health: " + currHP;
        print("Health is now " + healthText.text);
    }
    public static int GetHealth()
    {
        return health;
    }
    
    public int GetMyHealth()
    {
        return currHP;
    }

    void SaveLevelResults()
    {
        //For this level:

        //Save coins collected and coins added in level
        PlayerPrefs.SetInt("coinsCollected", coins);
        PlayerPrefs.SetInt("coinsInLevel", coinsInLevel);
        //Save time and par time
        PlayerPrefs.SetFloat("time", levelTime);
        PlayerPrefs.SetFloat("parTime", parTime);
        //Save level score and rank(pre-bonus)
        PlayerPrefs.SetInt("levelScore", levelScore);
        PlayerPrefs.SetString("Next Level", nextLevelText);
    }

    void SaveGamewideStats()
    {
        //Save total score
        PlayerPrefs.SetInt("gameTotalScore", gameTotalScore);     
    }

    void LoadLevelResults()
    {
        //For this level:
        //Load coins collected and coins added in level
        coins = PlayerPrefs.GetInt("coinsCollected", coins);
        coinsInLevel = PlayerPrefs.GetInt("coinsInLevel", coinsInLevel);
        //Load time and par time
        levelTime = PlayerPrefs.GetFloat("time", levelTime);
        parTime = PlayerPrefs.GetFloat("parTime", parTime);
        //Load level score and rank(pre-bonus)
        levelScore = PlayerPrefs.GetInt("levelScore", levelScore);
        
        nextLevelText = PlayerPrefs.GetString("Next Level", nextLevelText);
    }

    void LoadGamewideStats()
    {
        //Save total score
        PlayerPrefs.GetInt("gameTotalScore", gameTotalScore);
    }

    //public void Load()
    //{
    //    //doubles
    //    coins = double.Parse(PlayerPrefs.GetString("coins", "0"));
    //    coinsClickValue = double.Parse(PlayerPrefs.GetString("coinsClickValue", "1"));
    //    //clickUpgrade1Cost = double.Parse(PlayerPrefs.GetString("clickUpgrade1Cost", "10")); //Don't need to save or load this because it's dependent on others
    //    clickUpgrade2Cost = double.Parse(PlayerPrefs.GetString("clickUpgrade2Cost", "100"));
    //    productionUpgrade1Cost = double.Parse(PlayerPrefs.GetString("productionUpgrade1Cost", "25"));
    //    productionUpgrade2Cost = double.Parse(PlayerPrefs.GetString("productionUpgrade2Cost", "250"));
    //    productionUpgrade2Power = double.Parse(PlayerPrefs.GetString("productionUpgrade2Power", "5"));

    //    gems = double.Parse(PlayerPrefs.GetString("gems", "0"));

    //    //ints
    //    productionUpgrade1Level = PlayerPrefs.GetInt("productionUpgrade1Level", 0);
    //    productionUpgrade2Level = PlayerPrefs.GetInt("productionUpgrade2Level", 0);
    //    clickUpgrade1Level = PlayerPrefs.GetInt("clickUpgrade1Level", 0);
    //    clickUpgrade2Level = PlayerPrefs.GetInt("clickUpgrade2Level", 0);

    //}
    //public void Save()
    //{
    //    PlayerPrefs.SetString("coins", coins.ToString());
    //    PlayerPrefs.SetString("coinsClickValue", coinsClickValue.ToString());
    //    //PlayerPrefs.SetString("clickUpgrade1Cost", clickUpgrade1Cost.ToString());
    //    PlayerPrefs.SetString("clickUpgrade2Cost", clickUpgrade2Cost.ToString());
    //    PlayerPrefs.SetString("productionUpgrade1Cost", productionUpgrade1Cost.ToString());
    //    PlayerPrefs.SetString("productionUpgrade2Cost", productionUpgrade2Cost.ToString());
    //    PlayerPrefs.SetString("productionUpgrade2Power", productionUpgrade2Power.ToString());
    //    PlayerPrefs.SetString("gems", gems.ToString());

    //    //__Doubles__
    //    //coins = double.Parse(PlayerPrefs.GetString("coins", "0"));
    //    //coinsClickValue = double.Parse(PlayerPrefs.GetString("coinsClickValue", "1"));
    //    //clickUpgrade1Cost = double.Parse(PlayerPrefs.GetString("clickUpgrade1Cost", "10"));
    //    //clickUpgrade2Cost = double.Parse(PlayerPrefs.GetString("clickUpgrade2Cost", "100"));
    //    //productionUpgrade1Cost = double.Parse(PlayerPrefs.GetString("productionUpgrade1Cost", "25"));
    //    //productionUpgrade2Cost = double.Parse(PlayerPrefs.GetString("productionUpgrade2Cost", "250"));
    //    //productionUpgrade2Power = double.Parse(PlayerPrefs.GetString("productionUpgrade2Power", "5"));



    //    //ints
    //    PlayerPrefs.SetInt("productionUpgrade1Level", productionUpgrade1Level);
    //    PlayerPrefs.SetInt("productionUpgrade2Level", productionUpgrade2Level);
    //    PlayerPrefs.SetInt("clickUpgrade1Level", clickUpgrade1Level);
    //    PlayerPrefs.SetInt("clickUpgrade2Level", clickUpgrade2Level);
    //}

    void StopUpdates()
    {
        canUpdate = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (canUpdate)
        {
            //levelTime += Time.deltaTime;

            //timeText.text = "Time: " + levelTime;
            StopwatchCalc();
            scoreText.text = "Score : " + levelScore;
            coinsText.text = "Coins: " + coins;
            healthText.text = "Health: " + health;
        }

        if (SceneManager.GetActiveScene().name == "Level Results")
        {
            if(Input.anyKeyDown)
            {
                LoadNextLevel(nextLevelText);
            }
        }
    }
    void StopwatchCalc()
    {
        levelTime += Time.deltaTime;
        seconds = (int)(levelTime % 60);
        minutes = (int)(levelTime / 60);
        hours = (int)(levelTime / 3600);

        timeText.text = "Time: " + minutes + " : " + seconds;
    }
    void TallyPoints()
    {
        goalObj.SetActive(true);
        //multiply coins by coin bonus
        coinsBonus = coins * pointsPerCoin;
        //provide completion bonus if all collected
        coinsTextGoal.text = "Coins: " + coins + " x " + pointsPerCoin + "\n Bonus: " + coinsBonus;
        AddScore(coinsBonus);
        //subtract time from timeGoal, if 0 or less, set to 0
        var timeRemaining = parTime - levelTime;
        if (timeRemaining <= 0)
            timeRemaining = 0;
        //multiply time difference by time bonus
        timeBonus = (int)(pointsPerSecond * timeRemaining); //NOTE: CHECK FOR EXAMPLES

        timeTextGoal.text = "Time remaining: " + timeRemaining + " \nPar: " + parTime + "\nTime Bonus: " + timeBonus; 
        AddScore(timeBonus);

        //combine coins and time into rating num
        rankPoints = (int)timeRemaining + coins;
        //switch to see where rating is
        if(rankPoints >= 0 && rankPoints <= 1000)
        { 
            if (rankPoints >= 1000 && rankPoints <= 2500)
                RatingString = "Rating: C-ool!";
            else if (rankPoints >= 2500 && rankPoints <= 5000)
                RatingString = "Rating: B-allin!";
            else if (rankPoints >= 5000 && rankPoints <= 7500)
                RatingString = "Rating: A-BAP!";
            else if (rankPoints <= 7500 && rankPoints <= 10000)
                RatingString = "Rating: S-uper!";
            else if (rankPoints >= 10000)
                RatingString = "Rating: SS-uper Sweet!";
            else
                RatingString = "Rating: D-erp!";                
        }
        ratingTextGoal.text = RatingString;
        //AddScore()
        //Assign and show rating and add bonus points
    }

    void CallRoutineResults()
    {
        StartCoroutine(ShowResults());
    }

    IEnumerator ShowResults()
    {
        //Show coins & bonus

        //if(coins >= coinsInLevel)
        //{
        //    //Show all coins found & completion bonus
        //    //Add completion bonus to coins found
        //}
        //Add coin bonus to score
        //Show time & bonus
        // Add time bonus to score
        //Show rating
        //Show rating bonus points
        //Add rating bonus to score

        //multiply coins by coin bonus
        coinsBonus = coins * pointsPerCoin;
        //provide completion bonus if all collected
        coinsText.text = "Coins: " + coins + " x " + pointsPerCoin + "\n Bonus: " + coinsBonus;
        AddScore(coinsBonus);
        //subtract time from timeGoal, if 0 or less, set to 0
        var timeRemaining = parTime - levelTime;
        if (timeRemaining <= 0)
            timeRemaining = 0;
        //multiply time difference by time bonus
        timeBonus = (int)(pointsPerSecond * timeRemaining); //NOTE: CHECK FOR EXAMPLES
        timeText.text = "Time remaining: " + timeRemaining + " \nPar: " + parTime + "\nTime Bonus: " + timeBonus;
        AddScore(timeBonus);

        //combine coins and time into rating num
        rankPoints = (int)timeRemaining + coins;
        //switch to see where rating is
        if (rankPoints >= 0 && rankPoints <= 1000)
        {
            if (rankPoints >= 1000 && rankPoints <= 2500)
                RatingString = "Rating: C-ool!";
            else if (rankPoints >= 2500 && rankPoints <= 5000)
                RatingString = "Rating: B-allin!";
            else if (rankPoints >= 5000 && rankPoints <= 7500)
                RatingString = "Rating: A-BAP!";
            else if (rankPoints <= 7500 && rankPoints <= 10000)
                RatingString = "Rating: S-uper!";
            else if (rankPoints >= 10000)
                RatingString = "Rating: SS-uper Sweet!";
            else
                RatingString = "Rating: D-erp!";
        }
        ratingText.text = RatingString;
        //AddScore()
        //Assign and show rating and add bonus points
        yield return 0;
    }

    public static void RestartScene()
    {
        //Reset health
        health = maxHealth;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public static void LoadNextLevel(string _next)
    {
        //Reset health
        health = maxHealth;
        SceneManager.LoadScene(_next);
    }

    public static void LoadResultsScene()
    {
        SceneManager.LoadScene("Level Results");
    }
}
