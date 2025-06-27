using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // 引入 SceneManager 命名空间

public class GameManager : MonoBehaviour
{
    // 单例模式：确保只有一个 GameManager 实例存在
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("GameManager");
                _instance = go.AddComponent<GameManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// 跳转到指定场景
    /// </summary>
    /// <param name="sceneName">目标场景名称</param>
    public void LoadScene(string sceneName)
    {
        if (SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            Debug.LogWarning($"场景 {sceneName} 已加载");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// 异步加载场景（推荐用于大场景或需要进度条时）
    /// </summary>
    /// <param name="sceneName">目标场景名称</param>
    public void LoadSceneAsync(string sceneName)
    {
        StartCoroutine(LoadSceneAsyncCoroutine(sceneName));
    }

    private IEnumerator LoadSceneAsyncCoroutine(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f); // 归一化进度
            Debug.Log($"加载进度: {progress * 100}%");

            yield return null;
        }
    }
}
