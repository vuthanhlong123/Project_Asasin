using Game.Runtimes.Managers;
using Game.Runtimes.SlashScreen;
using UnityEngine;

namespace Game.Runtimes.LunchSpaceShip
{
    public class LunchSpaceShipController : MonoBehaviour
    {
        [SerializeField] private Transform spaceShipPlayerSpawnPoint;
        [SerializeField] private UISlashScreen slashScreenUI;
        [SerializeField] private LunchShipAnimatorController animatorController;

        [SerializeField] private GameObject spaceShipModel;
        [SerializeField] private GameObject spaceShipLunchAnimator;


        private void OnEnable()
        {
            animatorController.AnimationFinishedEvent += AnimatorController_AnimationFinishedEvent;
        }

        private void AnimatorController_AnimationFinishedEvent()
        {
            if(SpaceShipPlayerManager.instance)
                SpaceShipPlayerManager.instance.CreatePlayer(spaceShipPlayerSpawnPoint);
        }

        private void OnDisable()
        {
            animatorController.AnimationFinishedEvent -= AnimatorController_AnimationFinishedEvent;
        }

        public void DoLunch()
        {
            slashScreenUI.FadedInEvent += SlashScreenUI_FadedInEvent;
            slashScreenUI.MidTimeEndedEvent += SlashScreenUI_MidTimeEndedEvent;
            slashScreenUI.FadedOutEvent += SlashScreenUI_FadedOutEvent;

            slashScreenUI.Run();
        }

        private void SlashScreenUI_FadedInEvent()
        {
            slashScreenUI.FadedInEvent -= SlashScreenUI_FadedInEvent;

            spaceShipModel.SetActive(false);
            if(HumanPlayerManager.Instance)
            {
                HumanPlayerManager.Instance.RemovePlayer();
            }
        }

        private void SlashScreenUI_MidTimeEndedEvent()
        {
            slashScreenUI.MidTimeEndedEvent -= SlashScreenUI_MidTimeEndedEvent;

            spaceShipLunchAnimator.SetActive(true);
        }

        private void SlashScreenUI_FadedOutEvent()
        {
            slashScreenUI.FadedOutEvent -= SlashScreenUI_FadedOutEvent;
        }
    }
}


