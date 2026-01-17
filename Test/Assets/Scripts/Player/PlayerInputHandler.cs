using Component;
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
        private static readonly int Title = Animator.StringToHash("Title");

        // 参照するコンポーネント
        private Player player;
        private PlayerMove playerMove;
        private PlayerCombat playerCombat;
        
        private PlayerInput moveAction;
        private SceneButtonManager sceneButtonManager;
        private MapManager mapManager;
        
        // イベント登録状態を追跡するフラグ（重複登録防止）
        private bool isInputActionsRegistered = false;

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
            sceneButtonManager = SceneButtonManager.Instance;
            // 実行タイミングを遅らせて、moveActionがnullでないことを保証
            if (moveAction != null)
            {
                RegisterInputActions();
            }
            else
            {
                Debug.LogError("MoveAction (PlayerInput) is not assigned in Player.cs!");
            }
            
            var manager = sceneButtonManager ?? SceneButtonManager.Instance;
            if (manager == null || manager.currentState != SceneButtonManager.State.Title)
            {
                player.Animator.SetBool(Title, false);
            }
        }
        
        private void RegisterInputActions()
        {
            // 既に登録済みの場合は何もしない（重複登録防止）
            if (isInputActionsRegistered) return;
            
            moveAction.actions["Move"].performed += OnMove;
            moveAction.actions["Move"].canceled += OnMove;
            moveAction.actions["Jump"].started += OnJump;
            moveAction.actions["Jump"].canceled += OffJump;
            moveAction.actions["Shot"].started += OnShot;
            moveAction.actions["Attack"].canceled += OffAttack;
            moveAction.actions["Aim"].performed += OnQuickAttackAim;
            moveAction.actions["Aim"].canceled += OnQuickAttackAim;
            
            isInputActionsRegistered = true;
        }
        
        private void UnregisterInputActions()
        {
            if (moveAction == null) return;
            if (!isInputActionsRegistered) return;
            
            moveAction.actions["Move"].performed -= OnMove;
            moveAction.actions["Move"].canceled -= OnMove;
            moveAction.actions["Jump"].started -= OnJump;
            moveAction.actions["Jump"].canceled -= OffJump;
            moveAction.actions["Shot"].started -= OnShot;
            moveAction.actions["Attack"].canceled -= OffAttack;
            moveAction.actions["Aim"].performed -= OnQuickAttackAim;
            moveAction.actions["Aim"].canceled -= OnQuickAttackAim;
            
            isInputActionsRegistered = false;
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

        /*
         以下、入力コールバック
         実際の処理はPlayerMoveやPlayerCombatに
        */
        private void OnMove(InputAction.CallbackContext context)
        {
            var manager = sceneButtonManager ?? SceneButtonManager.Instance;
            if (manager == null || manager.currentState != SceneButtonManager.State.Gameplay) return;
            if (player == null) return; // Playerが破棄された場合

            Vector2 input = context.ReadValue<Vector2>();
            player.InputMove = input;
            playerMove.SetMoveInput(input); // PlayerMoveに伝える

            if (input != Vector2.zero)
            {
                var angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
                player.Arrow.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            var manager = sceneButtonManager ?? SceneButtonManager.Instance;
            if (manager == null) return;
            
            // タイトル状態の場合はゲームを開始
            if (manager.currentState == SceneButtonManager.State.Title)
            {
                manager.StartGame();
                return;
            }
            
            // ゲームプレイ状態の場合のみジャンプ
            if (manager.currentState != SceneButtonManager.State.Gameplay) return;
            playerMove.HandleJump(player.GetComponent<AudioSource>()); // AudioSourceはPlayerが持っている
        }

        private void OffJump(InputAction.CallbackContext context)
        {
            var manager = sceneButtonManager ?? SceneButtonManager.Instance;
            if (manager == null || manager.currentState != SceneButtonManager.State.Gameplay) return;
            playerMove.HandleJumpCanceled();
        }

        private void OnShot(InputAction.CallbackContext context)
        {
            var manager = sceneButtonManager ?? SceneButtonManager.Instance;
            if (manager == null || manager.currentState != SceneButtonManager.State.Gameplay) return;
            playerCombat.PerformShot(); // PlayerCombatに伝える
        }

        private void OnQuickAttackAim(InputAction.CallbackContext context)
        {
            var manager = sceneButtonManager ?? SceneButtonManager.Instance;
            if (manager == null || manager.currentState != SceneButtonManager.State.Gameplay) return;
            playerCombat.HandleQuickAttackAim(context.ReadValue<Vector2>()); // PlayerCombatに伝える
        }
        
        private void OffAttack(InputAction.CallbackContext context)
        {
            var manager = sceneButtonManager ?? SceneButtonManager.Instance;
            if (manager == null || manager.currentState != SceneButtonManager.State.Gameplay) return;
            playerCombat.AttackFinish(); // PlayerCombatに伝える
        }
    }
}