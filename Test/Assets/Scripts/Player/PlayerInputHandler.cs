using UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    // Playerの他コンポーネントに依存するため、RequireComponentで明記
    [RequireComponent(typeof(Player))]
    [RequireComponent(typeof(PlayerMove))]
    [RequireComponent(typeof(PlayerCombat))]
    public class PlayerInputHandler : MonoBehaviour
    {
        // 参照するコンポーネント
        private Player player;
        private PlayerMove playerMove;
        private PlayerCombat playerCombat;
        
        private PlayerInput moveAction;
        private SceneButtonManager sceneButtonManager;

        private void Awake()
        {
            // Player.csがAwakeでInstanceをセットするのを待たないため、GetComponentで取得
            player = GetComponent<Player>();
            playerMove = GetComponent<PlayerMove>();
            playerCombat = GetComponent<PlayerCombat>();
        }
        
        private void Start()
        {
            moveAction = player.MoveAction; // Player.csからInput Actionアセットをもらう
            sceneButtonManager = FindObjectOfType<SceneButtonManager>();
            
            // 実行タイミングを遅らせて、moveActionがnullでないことを保証
            if (moveAction != null)
            {
                RegisterInputActions();
            }
            else
            {
                Debug.LogError("MoveAction (PlayerInput) is not assigned in Player.cs!");
            }
        }
        
        private void RegisterInputActions()
        {
            moveAction.actions["Move"].performed += OnMove;
            moveAction.actions["Move"].canceled += OnMove;
            moveAction.actions["Jump"].started += OnJump;
            moveAction.actions["Jump"].canceled += OffJump;
            moveAction.actions["Shot"].started += OnShot;
            moveAction.actions["Attack"].canceled += OffAttack;
            moveAction.actions["Aim"].performed += OnQuickAttackAim;
            moveAction.actions["Aim"].canceled += OnQuickAttackAim;
        }
        
        private void UnregisterInputActions()
        {
            if (moveAction == null) return;
            moveAction.actions["Move"].performed -= OnMove;
            moveAction.actions["Move"].canceled -= OnMove;
            moveAction.actions["Jump"].started -= OnJump;
            moveAction.actions["Jump"].canceled -= OffJump;
            moveAction.actions["Shot"].started -= OnShot;
            moveAction.actions["Attack"].canceled -= OffAttack;
            moveAction.actions["Aim"].performed -= OnQuickAttackAim;
            moveAction.actions["Aim"].canceled -= OnQuickAttackAim;
        }

        // OnEnable/OnDisableで購読・解除を行う
        private void OnEnable()
        {
            // Startで登録済みの場合は、ここで再度登録しないように
            if (moveAction != null)
            {
                RegisterInputActions();
            }
        }

        private void OnDisable()
        {
            UnregisterInputActions();
        }

        // === 以下、入力コールバック ===
        // 実際の処理はPlayerMoveやPlayerCombatに「委譲」する
        
        private void OnMove(InputAction.CallbackContext context)
        {
            if (sceneButtonManager.currentState != SceneButtonManager.State.Gameplay) return;
            if (player == null) return; // Playerが破棄された場合

            Vector2 input = context.ReadValue<Vector2>();
            player.InputMove = input; // Player.csのInputMoveは残す
            playerMove.SetMoveInput(input); // PlayerMoveに伝える

            if (input != Vector2.zero)
            {
                var angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
                player.Arrow.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            if (sceneButtonManager.currentState != SceneButtonManager.State.Gameplay) return;
            playerMove.HandleJump(player.GetComponent<AudioSource>()); // AudioSourceはPlayerが持っている
        }

        private void OffJump(InputAction.CallbackContext context)
        {
            if (sceneButtonManager.currentState != SceneButtonManager.State.Gameplay) return;
            playerMove.HandleJumpCanceled();
        }

        private void OnShot(InputAction.CallbackContext context)
        {
            if (sceneButtonManager.currentState != SceneButtonManager.State.Gameplay) return;
            playerCombat.PerformShot(); // PlayerCombatに伝える
        }

        private void OnQuickAttackAim(InputAction.CallbackContext context)
        {
            if (sceneButtonManager.currentState != SceneButtonManager.State.Gameplay) return;
            playerCombat.HandleQuickAttackAim(context.ReadValue<Vector2>()); // PlayerCombatに伝える
        }
        
        private void OffAttack(InputAction.CallbackContext context)
        {
            if (sceneButtonManager.currentState != SceneButtonManager.State.Gameplay) return;
            playerCombat.AttackFinish(); // PlayerCombatに伝える
        }
        
        // OnQuickAttackは使われていなかったので削除 (もし使うならPlayerCombatを呼ぶ)
    }
}