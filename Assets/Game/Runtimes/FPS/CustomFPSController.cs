using Akila.FPSFramework;
using System;
using UnityEngine;

namespace Game.Runtimes.FPS
{
    public class CustomFPSController : FirstPersonController
    {
        [Header("Custom Abilities")]
        [SerializeField] private ClimbAbility climbAbiblity;

        public ClimbAbility _ClimbAbility => climbAbiblity;

        public bool disableCameraRotation { get; set; }

        protected override void Start()
        {
            base.Start();

            climbAbiblity.Initialize(this);
            disableCameraRotation = false;
        }

        protected override void Update()
        {
            if(climbAbiblity.isActive)
            {
                climbAbiblity.HandleClimbing(Time.deltaTime);

                if(CharacterInput.JumpInput)
                {
                    climbAbiblity.TryStopAbility();
                }
            }
            else base.Update();

            if(!disableCameraRotation)
                UpdateCameraRotation();
        }

        public void RestCameraControlProperty()
        {
            yRotation = cameraTransform.eulerAngles.y;
            xRotation = cameraTransform.eulerAngles.x;
        }

        [Serializable]
        public class ClimbAbility
        {
            public bool isActive;
            public float climbingSpeed;

            public Vector3 topClimbLocation { get; private set; }
            public Vector3 topEndClimbLocation { get; private set; }
            public Vector3 bottomClimbLocation { get; private set; }

            private Vector3 ladderDirection;

            private float currentClimbNormalizeProgress;
            private CustomFPSController fpsController;
            private bool handlingExitLadderOntop;
            private bool handlingEnterLadder;

            public void Initialize(CustomFPSController fpsController)
            {
                this.fpsController = fpsController;
            }

            public void StartAbility(Vector3 topLocation, Vector3 topEndLocation, Vector3 bottomLocation, Vector3 direction)
            {
                this.topClimbLocation = topLocation;
                this.topEndClimbLocation = topEndLocation;
                this.bottomClimbLocation = bottomLocation;
                this.ladderDirection = direction;

                isActive = true;
                handlingExitLadderOntop = false;
                handlingEnterLadder = false;

                CalculateStartClimbProgress();
            }

            private void CalculateStartClimbProgress()
            {
                float currentCharacterY = fpsController.transform.position.y;
                if(currentCharacterY >= topClimbLocation.y)
                {
                    currentClimbNormalizeProgress = 1;
                    HandleEnterLadder(ladderDirection, topClimbLocation);
                }
                else if(currentCharacterY <= bottomClimbLocation.y)
                {
                    currentClimbNormalizeProgress = 0;
                    HandleEnterLadder(ladderDirection, bottomClimbLocation);
                }
                else
                {
                    float ladderDistance = Vector3.Distance(topClimbLocation, bottomClimbLocation);
                    float currentCharacterYFromLadder = currentCharacterY - bottomClimbLocation.y;
                    currentClimbNormalizeProgress = currentCharacterYFromLadder / ladderDistance;

                    HandleEnterLadder(ladderDirection, Vector3.Lerp(bottomClimbLocation, topClimbLocation, currentClimbNormalizeProgress));
                }
            }

            public void TryStopAbility()
            {
                if (handlingEnterLadder || handlingExitLadderOntop) return;

                StopAbility();
            }

            public void StopAbility()
            {
                isActive = false;
            }

            public void HandleClimbing(float deltaTime)
            {
                if (handlingExitLadderOntop || handlingEnterLadder) return;

                currentClimbNormalizeProgress += climbingSpeed * fpsController.CharacterInput.MoveInput.y * deltaTime;
                currentClimbNormalizeProgress = Mathf.Clamp01(currentClimbNormalizeProgress);
                fpsController.transform.position = Vector3.Lerp(bottomClimbLocation, topClimbLocation, currentClimbNormalizeProgress);

                if(IsShouldeExitLadderOnBottom())
                {
                    HandleExitLadderOnBottom();
                }
                else if(IsShouldeExitLadderOnTop())
                {
                    HandleExitLadderOnTop();
                }
            }

            private bool IsShouldeExitLadderOnBottom()
            {
                return fpsController.CharacterInput.MoveInput.y < 0 && fpsController.transform.position == bottomClimbLocation;
            }

            private void HandleExitLadderOnBottom()
            {
                StopAbility();
            }

            private bool IsShouldeExitLadderOnTop()
            {
                return fpsController.CharacterInput.MoveInput.y > 0 && fpsController.transform.position == topClimbLocation && !handlingExitLadderOntop;
            }

            private void HandleExitLadderOnTop()
            {
                handlingExitLadderOntop = true;
                var exitLadderHandler = fpsController.gameObject.AddComponent<EndLadderOnTopHandler>();
                exitLadderHandler.SetValue(topEndClimbLocation, 0.5f, completed: () =>
                {
                    StopAbility();
                });
            }

            private void HandleEnterLadder(Vector3 direction, Vector3 target)
            {
                if (handlingEnterLadder) return;

                fpsController.disableCameraRotation = true;
                handlingEnterLadder = true;
                var exitLadderHandler = fpsController.gameObject.AddComponent<EnterLadderHandler>();
                exitLadderHandler.SetValue(target, direction, 0.5f, fpsController, completed: () =>
                {
                    handlingEnterLadder = false;
                    fpsController.disableCameraRotation = false;
                });
            }
        }
    }
}


