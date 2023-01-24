using UnityEngine;
using UnityEngine.UI;
using Zenject;
using System.Collections.Generic;
using Code.Abstract;
using Code.Abstract.Enumerators;
using Code.Abstract.Extensions;
using DG.Tweening;
using Code.Abstract.Interfaces;

namespace Code.UI.Presenter
{
    public class SelectAvatar : MonoBehaviour
    {
        [SerializeField] private List<GameObject> avatarGameObject;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _saveButton;
        public List<AvatarForSelectAvatar> avatarForSelectAvatsr;
        private List<Sprite> _freeAvatars;
        private Sprite _avatarIsNow;
        private IReloadAvatarPresenter _selectPlayerPresenter;
        [SerializeField] private Image avatarInPopUp;
        private int _idPlayer;
        private IAudioService _audioService;
        
        [Inject]
        private void Init(SelectPlayerPresenter selectPlayerPresenter, IAudioService audioService)
        {
            _audioService = audioService;
            transform.SetParent(selectPlayerPresenter.transform);
            //transform.SetParent(FindObjectOfType<MainMenuPresenter>().transform);
            _selectPlayerPresenter = selectPlayerPresenter;
            _closeButton.onClick.AddListener(CloseSelectAvatar);
            _saveButton.onClick.AddListener(SaveSelectAvatar);
            _closeButton.onClick.AddListener(_audioService.ClickButton);
            _saveButton.onClick.AddListener(_audioService.ClickButton);
            for (int i = 0; i < avatarGameObject.Count; i++)
            {
                avatarForSelectAvatsr.Add(avatarGameObject[i].GetComponent<AvatarForSelectAvatar>());
            }
            for (int i = 0; i < avatarForSelectAvatsr.Count; i++)
            {
                int id = i;
                avatarForSelectAvatsr[i].Select.onClick.AddListener((() => SelectThisAvatar(id)));
                avatarForSelectAvatsr[i].Select.onClick.AddListener(_audioService.ClickButton);
            }
        }

        public void SetParent(MonoBehaviour parent)
        {
            if (parent is IReloadAvatarPresenter avatarPresenter) 
                _selectPlayerPresenter = avatarPresenter;
            transform.SetParent(parent.transform);
        }
        

        public void SetFreeAvatars(List<Sprite> avatars)
        {
            _freeAvatars = avatars;
            for (int i = 0; i < avatarForSelectAvatsr.Count; i++)
            {
                if (_freeAvatars.Contains(avatarForSelectAvatsr[i].Icon.sprite))
                    avatarForSelectAvatsr[i].Active();
                else
                    avatarForSelectAvatsr[i].InActive();
            }
        }
        public void SetAvatar(Sprite avatar, int id)
        {
            _idPlayer = id;
            _avatarIsNow = avatar;
            avatarInPopUp.sprite = avatar;
            for (int i = 0; i < avatarForSelectAvatsr.Count; i++)
            {
                if (avatarForSelectAvatsr[i].Icon.sprite== _avatarIsNow)
                    avatarForSelectAvatsr[i].SelectThis();
                else
                    avatarForSelectAvatsr[i].InSelectThis();
            }
        }
        public void CloseSelectAvatar()
        {
             GetComponent<RectTransform>().MoveTo(1000, Constant.MenuPanelVisibleDuration, Direction.Up);
            //DOTween.Sequence().Append(transform.DOLocalMoveY(1000, 0.7f)).SetEase(Ease.Linear).OnComplete(CloseThisObject);
        }
        private void CloseThisObject()
        {
            gameObject.SetActive(false);
        }

        private void SelectThisAvatar(int index)
        {
            for (int i = 0; i < avatarForSelectAvatsr.Count; i++)
            {
                    avatarForSelectAvatsr[i].InSelectThis();
            }
            avatarInPopUp.sprite= avatarForSelectAvatsr[index].Icon.sprite;
            avatarForSelectAvatsr[index].SelectThis();
        }


        private void SaveSelectAvatar()
        {
            if(_avatarIsNow!= avatarInPopUp.sprite)
            {
                _selectPlayerPresenter.ReloadAvatar(avatarInPopUp.sprite, _idPlayer);
            }
            CloseSelectAvatar();
        }
    }
}