using UnityEngine;
using TMPro;
using System;
using Component;
using Player;

namespace Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        public enum TutorialStep
        {
            None,
            Loop,
            Jump,
            Shoot,
            Reflect,
            Completed
        }

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private UI.Tutorial tutorialUI;

        [Header("Settings")]
        [SerializeField] private float stepDelay = 1.0f;

        private TutorialStep currentStep = TutorialStep.None;
        [SerializeField] private PlayerMove playerMove;
        private PlayerCombat playerCombat;
        private Loop playerLoop;

        private void Awake()
        {
            if (playerMove != null)
            {
                playerCombat = playerMove.GetComponent<PlayerCombat>();
                playerLoop = playerMove.GetComponent<Loop>();
            }

            if (playerMove == null || playerCombat == null || playerLoop == null)
            {
                Debug.LogError("Player components not found!");
            }
        }

        private void Start()
        {
            // Start Tutorial
            SetStep(TutorialStep.Loop);
        }

        private void OnEnable()
        {
            if (playerLoop != null) playerLoop.OnLoop += HandleLoop;
            if (playerMove != null) playerMove.OnJump += HandleJump;
            if (playerCombat != null)
            {
                playerCombat.OnShoot += HandleShoot;
                playerCombat.OnReflect += HandleReflect;
            }
        }

        private void OnDisable()
        {
            if (playerLoop != null) playerLoop.OnLoop -= HandleLoop;
            if (playerMove != null) playerMove.OnJump -= HandleJump;
            if (playerCombat != null)
            {
                playerCombat.OnShoot -= HandleShoot;
                playerCombat.OnReflect -= HandleReflect;
            }
        }

        private void SetStep(TutorialStep step)
        {
            currentStep = step;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (instructionText == null) return;

            switch (currentStep)
            {
                case TutorialStep.Loop:
                    instructionText.text = "画面端に行き自身がループをする";
                    break;
                case TutorialStep.Jump:
                    instructionText.text = "ジャンプ";
                    break;
                case TutorialStep.Shoot:
                    instructionText.text = "射撃";
                    break;
                case TutorialStep.Reflect:
                    instructionText.text = "反射";
                    break;
                case TutorialStep.Completed:
                    instructionText.text = "チュートリアル完了！";
                    break;
            }
        }

        private void HandleLoop()
        {
            if (currentStep == TutorialStep.Loop)
            {
                Debug.Log("Loop Completed!");
                if (tutorialUI != null) tutorialUI.NextTutorial();
                SetStep(TutorialStep.Jump);
            }
        }

        private void HandleJump()
        {
            if (currentStep == TutorialStep.Jump)
            {
                Debug.Log("Jump Completed!");
                if (tutorialUI != null) tutorialUI.NextTutorial();
                SetStep(TutorialStep.Shoot);
            }
        }

        private void HandleShoot()
        {
            if (currentStep == TutorialStep.Shoot)
            {
                Debug.Log("Shoot Completed!");
                if (tutorialUI != null) tutorialUI.NextTutorial();
                SetStep(TutorialStep.Reflect);
            }
        }

        private void HandleReflect()
        {
            if (currentStep == TutorialStep.Reflect)
            {
                Debug.Log("Reflect Completed!");
                if (tutorialUI != null) tutorialUI.NextTutorial();
                SetStep(TutorialStep.Completed);
            }
        }
    }
}
