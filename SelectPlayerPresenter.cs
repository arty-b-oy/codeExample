using Code.Abstract.Enumerators;
using Code.Abstract.Interfaces;
using Code.Components.States;
using Code.Configs;
using Code.UI.View;
using Leopotam.Ecs;
using UnityEngine;
using Zenject;
using System.Collections.Generic;
using System.Linq;
using Code.Abstract;
using Code.Abstract.Extensions;
using Code.Abstract.Interfaces.Online;
using static Code.Abstract.Constant;

namespace Code.UI.Presenter
{
    public class SelectPlayerPresenter : MonoBehaviour, IReloadAvatarPresenter
    {
        private DiContainer _container;
        private IAudioService _audioService;
        private IFadeService _fadeService;
        private IScaleService _scaleService;
        [SerializeField] private SelectPlayerPanelView[] _selectPlayerPanelView;
        [SerializeField] private SelectMainPlayerView _selectMainPlayerView;
        [SerializeField] private SelectPlayerView _selectPlayerView;
        private SelectAvatar _selectAvatar;
        [SerializeField] private LoadSavedGameView _loadSavedGameView;
        private IGameParameters _gameParameters;
        private ISaveService _saveService;
        private IReactionService _reactionService;
        private ISetRandomNameServiсe _setRandomNameService;
        private IMixingServiсe _mixingService;
        private ImagesConfig _imagesConfig;
        private PlayerConfig _playerConfig;
        private EcsWorld _world = null;
        private List<Sprite> _avatars;
        //private ChooseSavedMovePresenter _chooseSavedMovePresenter;
        private IPlayerLogService _playerLogService;

        [Inject]
        private void Init(IGameParameters gameParameters,
            EcsWorld world,
            ISetRandomNameServiсe setRandomNameService,
            IMixingServiсe mixingService,
            ImagesConfig imagesConfig,
            PlayerConfig playerConfig,
            IPlayerLogService playerLogService,
            SelectAvatar selectAvatar, 
            DiContainer container,
            IAudioService audioService, 
            IFadeService fadeService,
            IReactionService reactionService,
            IScaleService scaleService)
        {
            _scaleService = scaleService;
            _fadeService = fadeService;
            _audioService = audioService;
            _container = container;
            _selectAvatar = selectAvatar;
            _reactionService = reactionService;
            SetDependencies(gameParameters, world, setRandomNameService, mixingService, imagesConfig,
                playerConfig);
            //_chooseSavedMovePresenter = chooseSavedMovePresenter;
            _playerLogService = playerLogService;
            _avatars = _imagesConfig.avatar.GetRange(0, _imagesConfig.avatar.Count);
            _avatars = _mixingService.MixingSpriteList(_avatars);
            _scaleService.Scale(Threshold2_15, Threshold2_15 - 0.15f, _selectPlayerView.BodyTransform);
            _scaleService.Scale(Threshold2_15, Threshold2_15 - 0.15f, _selectAvatar.GetComponent<RectTransform>());
            _scaleService.Scale(Threshold2_15, Threshold2_15 - 0.15f, _selectPlayerView.LoadSavedGame);
            AddListeners();
            _gameParameters.AddPlayer(0, _selectMainPlayerView.PlayerName, false);
            _selectMainPlayerView.Avatar.sprite = _avatars[0];
            _avatars.RemoveAt(0);
            SetStartPosition();
        }

        public void OnEnable()
        {
            _selectAvatar.SetParent(this);
            _saveService = _container.ResolveId<ISaveService>("Offline");
            _loadSavedGameView.Visible(_saveService.HaveSavedGame());
        }


        private void SetStartPosition()
        {
            transform.SetParent(FindObjectOfType<CanvasTag>().transform);
            var rect = GetComponent<RectTransform>();
            rect.position = new Vector3(0, 0, 0);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private void SetDependencies(IGameParameters gameParameters, EcsWorld world,
            ISetRandomNameServiсe setRandomNameServiсe, IMixingServiсe mixingServiсe, ImagesConfig imagesConfig,
            PlayerConfig playerConfig)
        {
            _world = world;
            _gameParameters = gameParameters;
            _setRandomNameService = setRandomNameServiсe;
            _mixingService = mixingServiсe;
            _imagesConfig = imagesConfig;
            _playerConfig = playerConfig;
            _playerConfig = playerConfig;
        }

        private void AddListeners()
        {
            AddListenersForEachPlayer();
            _selectPlayerView.StartButton.onClick.AddListener(StartGame);
            _loadSavedGameView.LoadButton.onClick.AddListener(ShowLoadGame);
            _selectPlayerView.CloseSelectPlayerButton.onClick.AddListener(CloseSelectPlayer);
            _selectMainPlayerView.SelectAvatar.onClick.AddListener(() => ShowSelectAvatar(10));
            AddAudioForButtons();
        }

        private void ShowLoadGame()
        {
            _loadSavedGameView.Visible(false);
            LoadGame();
            //_chooseSavedMovePresenter.Show(LoadGame);
        }

        private void AddAudioForButtons()
        {
            _loadSavedGameView.BtnBack.onClick.AddListener(_audioService.ClickButton);
            _loadSavedGameView.LoadButton.onClick.AddListener(_audioService.ClickButton);
            _selectPlayerView.StartButton.onClick.AddListener(_audioService.ClickButton);
            _selectPlayerView.CloseSelectPlayerButton.onClick.AddListener(_audioService.ClickButton);
            _selectMainPlayerView.SelectAvatar.onClick.AddListener(_audioService.ClickButton);
        }

        private void AddListenersForEachPlayer()
        {
            for (int i = 0; i < _selectPlayerPanelView.Length; i++)
            {
                int id = i;
                _selectPlayerPanelView[i].SelectBot.onClick.AddListener((() => SelectBot(id)));
                _selectPlayerPanelView[i].SelectHuman.onClick.AddListener((() => SelectHuman(id)));
                _selectPlayerPanelView[i].DeletePlayer.onClick.AddListener((() => DeletePlayer(id)));
                _selectPlayerPanelView[i].SelectAvatar.onClick.AddListener((() => ShowSelectAvatar(id)));
                _selectPlayerPanelView[i].NameChanged += delegate { ChangeName(id); };
                AddAudioForEachPlayer(i);
            }
        }

        private void AddAudioForEachPlayer(int i)
        {
            _selectPlayerPanelView[i].SelectBot.onClick.AddListener(_audioService.ClickButton);
            _selectPlayerPanelView[i].SelectHuman.onClick.AddListener(_audioService.ClickButton);
            _selectPlayerPanelView[i].DeletePlayer.onClick.AddListener(_audioService.ClickButton);
            _selectPlayerPanelView[i].SelectAvatar.onClick.AddListener(_audioService.ClickButton);
        }




        private void ChangeName(int id)
        {
            _gameParameters.EditPlayer(id + 1, _selectPlayerPanelView[id].PlayerName);
        }

        private void LoadGame()
        {
            var indexes = _gameParameters.GetPlayers().Select(i => i.Key).ToArray();
            for (int i = 1; i < indexes.Count(); i++)
                DeletePlayer(indexes[i] - 1);
            var players = _saveService.LoadPlayers();
            var playersName = _saveService.SavedPlayerNames;
            foreach (var player in players)
            {
                if (player.Player == Players.Player1)
                    continue;
                var names = playersName.First(i => i.Player == player.Player).Name;
                _gameParameters.AddPlayer((int) player.Player, names, false);
            }
            _playerLogService.LoadLog();
            var indexAvatar = _saveService.LoadIndexAvatar();
            for(int i = 0; i < indexAvatar.Count; i++)
            {
                _playerConfig.Players[i].Avatar = _imagesConfig.avatar[indexAvatar[i]];
                _playerConfig.Players[i].PawnPrefab = _playerConfig.Pawn[indexAvatar[i]];
            }
            _container.ResolveId<IInitializable>("Startup").Initialize();
            _world.NewEntity().Get<ChangeStateToLoad>();
            RemoveListenerFromPlayers();
        }

        private void RemoveListenerFromPlayers()
        {
            for (int i = 0; i < _selectPlayerPanelView.Length; i++)
            {
                _selectPlayerPanelView[i].SelectBot.onClick.RemoveAllListeners();
                _selectPlayerPanelView[i].SelectHuman.onClick.RemoveAllListeners();
                _selectPlayerPanelView[i].DeletePlayer.onClick.RemoveAllListeners();
            }
        }

        private void StartGame()
        {
            _reactionService.ReactionsFeatureEnable(false);
            if (_gameParameters.GetPlayers().Count < 2)
                return;
            _playerConfig.Players[0].Avatar = _selectMainPlayerView.Avatar.sprite;
            _playerConfig.Players[0].PawnPrefab =
                _playerConfig.Pawn[_imagesConfig.avatar.IndexOf(_selectMainPlayerView.Avatar.sprite)];
            List<int> indexAvatarForSave = new List<int>();
            indexAvatarForSave.Add(_imagesConfig.avatar.IndexOf(_selectMainPlayerView.Avatar.sprite));
            for (int i = 0; i < _selectPlayerPanelView.Length; i++)
            {
                if (_selectPlayerPanelView[i].PlayerParameterPanel.activeSelf)
                {
                    _playerConfig.Players[i + 1].Avatar = _selectPlayerPanelView[i].GetAvatar();
                    _playerConfig.Players[i + 1].PawnPrefab =
                        _playerConfig.Pawn[_imagesConfig.avatar.IndexOf(_selectPlayerPanelView[i].GetAvatar())];
                    indexAvatarForSave.Add(_imagesConfig.avatar.IndexOf(_selectPlayerPanelView[i].GetAvatar()));
                }
            }
            _saveService.Save(indexAvatarForSave);
            /*if (_saveService.HaveSavedGame())
            {
                _saveService.DeleteSavedGame();
            }*/
            InitializeGame();
        }

        private void InitializeGame()
        {
            //_sendDataOnServerService.CreateUUID();
            _container.ResolveId<IInitializable>("Startup").Initialize();
            _world.NewEntity().Get<ChangeStateToStart>();
            RemoveListenerFromPlayers();
            ClearServices();
            Destroy(gameObject);
        }

        private void ClearServices()
        {
            _setRandomNameService.Clear();
        }

        private void CloseSelectPlayer()
        {
            GetComponent<RectTransform>().MoveTo(1000, Constant.MenuPanelVisibleDuration, Direction.Up);
            _fadeService.Fade(_selectPlayerView.Background, 0, 0.5f);
        }

        private void DeletePlayer(int id)
        {
            _avatars.Insert(0, _selectPlayerPanelView[id].GetAvatar());
            _avatars = _mixingService.MixingSpriteList(_avatars);
            _selectPlayerPanelView[id].PlayerParameterPanel.SetActive(false);
            _selectPlayerPanelView[id].ButtonHolder.SetActive(true);
            _gameParameters.DeletePlayer(id + 1);
        }

        private void SelectHuman(int id)
        {
            _selectPlayerPanelView[id].PlayerIconVariable.sprite = _selectPlayerPanelView[id].HumanIcon;
            _selectPlayerPanelView[id].PlayerParameterPanel.SetActive(true);
            _selectPlayerPanelView[id].ButtonHolder.SetActive(false);
            _selectPlayerPanelView[id].SetName(_setRandomNameService.GetName());
            _selectPlayerPanelView[id].SetAvatar(_avatars[0]);
            _avatars.RemoveAt(0);
            _gameParameters.AddPlayer(id + 1, _selectPlayerPanelView[id].PlayerName, false);
        }

        private void SelectBot(int id)
        {
            _selectPlayerPanelView[id].PlayerIconVariable.sprite = _selectPlayerPanelView[id].BotIcon;
            _selectPlayerPanelView[id].PlayerParameterPanel.SetActive(true);
            _selectPlayerPanelView[id].ButtonHolder.SetActive(false);
            _selectPlayerPanelView[id].SetName(_setRandomNameService.GetName());
            _selectPlayerPanelView[id].SetAvatar(_avatars[0]);
            _avatars.RemoveAt(0);
            _gameParameters.AddPlayer(id + 1, _selectPlayerPanelView[id].PlayerName, true);
        }

        private void ShowSelectAvatar(int id)
        {
            _selectAvatar.gameObject.SetActive(true);
            _selectAvatar.SetFreeAvatars(_avatars);
            if (id == 10)
                _selectAvatar.SetAvatar(_selectMainPlayerView.Avatar.sprite, 0);
            else
                _selectAvatar.SetAvatar(_selectPlayerPanelView[id].GetAvatar(), id + 1);
            _selectAvatar.GetComponent<RectTransform>().MoveFrom(1000, Constant.MenuPanelVisibleDuration, Direction.Up);
            //DOTween.Sequence().Append(_selectAvatar.gameObject.transform.DOLocalMoveY(0, 0.7f)).SetEase(Ease.Linear);
            //rename branch, delete unusing branches
        }

        public void RenameFirstPlayer(string name)
        {
            _selectMainPlayerView.Rename(name);
            _gameParameters.EditPlayer(0, name);
        }

        public void ReloadAvatar(Sprite newAvatar, int idPlayer)
        {
            if (idPlayer == 0)
            {
                _avatars.Insert(0, _selectMainPlayerView.Avatar.sprite);
                _selectMainPlayerView.Avatar.sprite = newAvatar;
            }
            else
            {
                idPlayer--;
                _avatars.Insert(0, _selectPlayerPanelView[idPlayer].GetAvatar());
                _selectPlayerPanelView[idPlayer].SetAvatar(newAvatar);
            }

            _avatars.Remove(newAvatar);
            _avatars = _mixingService.MixingSpriteList(_avatars);
        }

        public void ShowBackground()
        {
            _fadeService.Fade(_selectPlayerView.Background,0.8f,0.5f);
        }
    }
}
