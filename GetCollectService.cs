using Code.Abstract.CommonComponents;
using Code.Abstract.ExtensionComponents;
using Code.Abstract.Interfaces;
using Code.Components.Card;
using Code.Components.Card.Pledged;
using Code.Components.Card.Upgrade;
using Code.Components.Upgrade;
using Leopotam.Ecs;
using Code.UI.Presenter;
using UnityEngine;
using Code.Configs;
using Zenject;

namespace Code.Services
{
    public class GetCollectService:IGetCollectService
    {
        private ColorCollectionPresenter _colorCollectionPresenter=null;
        private IGetCardGroup _getCardGroup=null;
        private BuyCardConfig _cardConfig=null;
        private readonly IChangeGameplayStates _changeGameplayStates=null;
        private readonly EcsWorld _world=null;


        [Inject]
        public GetCollectService(ColorCollectionPresenter colorCollectionPresenter, 
                                 IGetCardGroup getCardGroup,
                                 BuyCardConfig cardConfig,
                                 IChangeGameplayStates changeGameplayStates,
                                 EcsWorld world)
        {
            _colorCollectionPresenter = colorCollectionPresenter;
            _getCardGroup = getCardGroup;
            _cardConfig = cardConfig;
            _changeGameplayStates = changeGameplayStates;
            _world = world;
        }

        public void ShowSetCard(Sprite plusTwoHundredBackground, Sprite avatar)
        {
            _changeGameplayStates.ClearCurrentState();
            _world.NewEntity().Get<StopCheckUpgrade>();
            _colorCollectionPresenter.GoAnimation(plusTwoHundredBackground, avatar, ()=>_changeGameplayStates.FinishBuying());
        }

        public void GetEntity(EcsEntity entity, int index)
        {
            var price = entity.Get<Price>().Value;
            var rent = entity.Get<Rent>().Value;
            var name = entity.Get<Name>().Value;
            var upgradePrice = entity.Get<UpgradePrice>().Value;
            var pledged = entity.Get<PledgedPrice>().Value;
            _getCardGroup.GetCardType(entity, out int group, out int pledgedPrice);
            _colorCollectionPresenter.SetInformationForCard(rent, upgradePrice, upgradePrice, name, price, pledged, _cardConfig.Sprites[group],index);
        }
        public bool ActiveSelf() => 
            _colorCollectionPresenter.ActiveSelf();
    }
}