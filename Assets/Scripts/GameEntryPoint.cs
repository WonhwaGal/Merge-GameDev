using UnityEngine;
using Code.MVC;
using Code.SaveLoad;
using Code.DropLogic;
using Code.Sounds;
using Code.Achievements;
using Code.Views;

public class GameEntryPoint : MonoBehaviour
{
    [SerializeField] private DropContainer _container;
    [SerializeField] private DropObjectSO _so;
    [SerializeField] private EffectList _fxList;
    [SerializeField] private CanvasView _canvasView;
    [SerializeField] private SoundSO _soundSO;
    [SerializeField] private AchievSO _achievSO;

    private SaveService _saveService;
    private UIService _uiService;
    private DropService _dropService;
    private SoundManager _soundManager;

    private void Awake()
    {
        InitServices();
        SetUpConnections();
    }

    private void InitServices()
    {
        _saveService = ServiceLocator.Container.RequestFor<SaveService>();
        _uiService = ServiceLocator.Container.RegisterAndAssign(new UIService(_so, _canvasView, _achievSO));
        _dropService = ServiceLocator.Container.RegisterAndAssign(new DropService(_so, _fxList));
        _soundManager = new SoundManager(_soundSO);
    }

    private void SetUpConnections()
    {
        _container.OnObjectDrop += _dropService.CreateDropObject;
        _container.CurrentDrop = _dropService.CreateDropObject(_container.transform, true);
    }

    private void Start()
    {
        if(_dropService.KeySpawned)
            GameEventSystem.Send(new KeyEvent(_dropService.KeySpawned));
    }

    private void OnDestroy()
    {
        _container.OnObjectDrop -= _dropService.CreateDropObject;
        _uiService.Dispose();
        _saveService.Dispose();
        _soundManager.Dispose();
        _dropService.Dispose();
        ServiceLocator.Container.RequestFor<AchievementService>().Dispose();
    }
}