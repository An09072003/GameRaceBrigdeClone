using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LevelManager : Singleton<LevelManager>
{
    readonly List<ColorType> colorTypes = new List<ColorType>()
    {
        ColorType.Black, ColorType.Red, ColorType.Blue,
        ColorType.Green, ColorType.Yellow, ColorType.Orange,
        ColorType.Brown, ColorType.Violet
    };

    [SerializeField] private Level[] levelPrefabs;
    [SerializeField] private Bot botPrefab;
    [SerializeField] private Player player;

    private List<Bot> bots = new List<Bot>();
    private Level currentLevel;
    private int levelIndex;

    public Vector3 FinishPoint => currentLevel?.finishPoint?.position ?? Vector3.zero;
    public int CharacterAmount => currentLevel != null ? currentLevel.botAmount + 1 : 0;

    private void Awake()
    {
        levelIndex = PlayerPrefs.GetInt("Level", 0);
    }

    private void Start()
    {
        LoadLevel(levelIndex);

        if (currentLevel != null)
        {
            OnInit();
            UIManager.Instance.OpenUI<MainMenu>();
        }
        else
        {
            Debug.LogError("Không thể khởi tạo Level. Hãy kiểm tra lại levelPrefabs trong Inspector.");
        }
    }

    public void LoadLevel(int level)
    {
        if (currentLevel != null)
        {
            Destroy(currentLevel.gameObject);
        }

        if (level < levelPrefabs.Length && levelPrefabs[level] != null)
        {
            currentLevel = Instantiate(levelPrefabs[level]);
            currentLevel.OnInit();
        }
        else
        {
            Debug.LogError($"Level {level} bị null hoặc vượt quá số lượng thiết kế. Reset về level 0.");
            levelIndex = 0;
            PlayerPrefs.SetInt("Level", levelIndex);

            if (levelPrefabs.Length > 0 && levelPrefabs[0] != null)
            {
                currentLevel = Instantiate(levelPrefabs[0]);
                currentLevel.OnInit();
            }
            else
            {
                Debug.LogError("Level 0 cũng null. Hãy kiểm tra lại mảng levelPrefabs.");
                currentLevel = null;
            }
        }
    }

    public void OnInit()
    {
        if (currentLevel == null)
        {
            Debug.LogError("currentLevel is null.");
            return;
        }

        if (currentLevel.startPoint == null || currentLevel.finishPoint == null || currentLevel.navMeshData == null)
        {
            Debug.LogError("Level thiếu startPoint, finishPoint hoặc navMeshData. Hãy gán đầy đủ trong prefab.");
            return;
        }

        Vector3 index = currentLevel.startPoint.position;
        float space = 2f;
        Vector3 leftPoint = ((CharacterAmount / 2f) + (CharacterAmount % 2) * 0.5f - 0.5f) * space * Vector3.left + index;

        List<Vector3> startPoints = new List<Vector3>();
        for (int i = 0; i < CharacterAmount; i++)
        {
            startPoints.Add(leftPoint + space * Vector3.right * i);
        }

        NavMesh.RemoveAllNavMeshData();
        NavMesh.AddNavMeshData(currentLevel.navMeshData);

        List<ColorType> colorDatas = Utilities.SortOrder(colorTypes, CharacterAmount);
        int rand = Random.Range(0, CharacterAmount);

        player.TF.position = startPoints[rand];
        player.TF.rotation = Quaternion.identity;
        player.ChangeColor(colorDatas[rand]);
        startPoints.RemoveAt(rand);
        colorDatas.RemoveAt(rand);
        player.OnInit();

        for (int i = 0; i < CharacterAmount - 1; i++)
        {
            Bot bot = SimplePool.Spawn<Bot>(PoolType.Bot, startPoints[i], Quaternion.identity);
            bot.ChangeColor(colorDatas[i]);
            bot.OnInit();
            bots.Add(bot);
        }
    }

    public void OnStartGame()
    {
        GameManager.Instance.ChangeState(GameState.Gameplay);
        foreach (var bot in bots)
        {
            bot.ChangeState(new PatrolState());
        }
    }

    public void OnFinishGame()
    {
        foreach (var bot in bots)
        {
            bot.ChangeState(null);
            bot.MoveStop();
        }
    }

    public void OnReset()
    {
        SimplePool.CollectAll();
        bots.Clear();
    }

    public void OnRetry()
    {
        OnReset();
        // Reset player trước khi load level mới
        if (player != null)
        {
            player.ClearBrick();
        }
        LoadLevel(levelIndex);

        if (currentLevel != null)
        {
            OnInit();
            UIManager.Instance.OpenUI<MainMenu>();
        }
    }

    public void OnNextLevel()
    {
        levelIndex++;
        PlayerPrefs.SetInt("Level", levelIndex);
        OnReset();
        // Reset player trước khi load level mới
        if (player != null)
        {
            player.ClearBrick();
        }
        LoadLevel(levelIndex);

        if (currentLevel != null)
        {
            OnInit();
            UIManager.Instance.OpenUI<MainMenu>();
        }
    }
}
