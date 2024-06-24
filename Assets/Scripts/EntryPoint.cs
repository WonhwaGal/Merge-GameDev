using System.Collections;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using Code.SaveLoad;
using Code.MVC;
using Code.DropLogic;
using Code.Achievements;
using GamePush;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;

public class EntryPoint : MonoBehaviour
{
    [SerializeField] private StartCanvas _startCanvas;
    [SerializeField] private AchievSO _achievSO;
    [SerializeField] private StatusSO _statusSO;
    [SerializeField] private SpriteAtlas _atlas;

    private SaveService _saveService;
    private AchievementService _achievementService;
    private LanguageHandler _languageHandler;

    private IEnumerator Start()
    {
        _startCanvas.StartNewButton.onClick.AddListener(() => LoadNewScene(withProgress: false));
        _startCanvas.ContinueButton.onClick.AddListener(() => LoadNewScene(withProgress: true));

        _saveService = ServiceLocator.Container.RegisterAndAssign(new SaveService());
        _achievementService = ServiceLocator.Container.RegisterAndAssign(new AchievementService(_achievSO, _statusSO));

        var localeInit = LocalizationSettings.InitializationOperation;
        localeInit.Completed += _ => Init();
        yield return new WaitWhile(() => _saveService == null);

        //localeInit.Completed -= InitFunction;
        var savedData = GP_Player.GetString(Constants.DropList);
        yield return new WaitWhile(() => string.IsNullOrEmpty(savedData));
        _startCanvas.ContinueButton.interactable = _saveService.LoadProgress(savedData);
        _startCanvas.SetCreditsText();
    }

    private void InitFunction(AsyncOperationHandle<LocalizationSettings> handle) => Init();

    private void Init()
    {
        _languageHandler = new LanguageHandler();
        _languageHandler.OnLanguageChanged += _startCanvas.SetTexts;
        _languageHandler.UpdateLangInfo();
    }

    public void LoadNewScene(bool withProgress)
    {
        var nextScene = Constants.GameScene;
#if UNITY_EDITOR
        nextScene = Constants.GameScene;
#else
            nextScene = GP_Device.IsMobile() 
            ? Constants.GameSceneMobile : Constants.GameScene;
#endif
        SceneManager.LoadSceneAsync(nextScene);

        if (withProgress)
            SceneManager.sceneLoaded += OnLoadWithProgress;
        else
            SceneManager.sceneLoaded += OnLoadNewGame;
    }

    private void OnLoadWithProgress(Scene scene, LoadSceneMode mode)
    {
        ServiceLocator.Container.RequestFor<DropService>().RecreateProgress(_saveService.ProgressData);
        _achievementService.SetInitialProgress(toZero: false);
        SceneManager.sceneLoaded -= OnLoadWithProgress;
    }

    private void OnLoadNewGame(Scene scene, LoadSceneMode mode)
    {
        _achievementService.SetInitialProgress(toZero: true);
        SceneManager.sceneLoaded -= OnLoadNewGame;
    }

    private void OnDestroy() 
        => _languageHandler.OnLanguageChanged -= _startCanvas.SetTexts;
}