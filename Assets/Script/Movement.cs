using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("이동 설정 (왼손)")]
    public float moveSpeed = 3.0f;
    public OVRInput.Controller moveController = OVRInput.Controller.LTouch;

    [Header("회전 설정 (오른손)")]
    [Tooltip("체크하면 스냅 턴(각도별 회전), 해제하면 스무스 턴(부드러운 회전)을 사용합니다.")]
    public bool useSnapTurn = false; // 토글 변수

    [Header("스무스 턴 옵션")]
    public float smoothTurnSpeed = 100.0f;

    [Header("스냅 턴 옵션")]
    [Tooltip("한 번 튕길 때 회전할 각도입니다 (보통 30, 45, 90).")]
    public float snapAngle = 45.0f;
    [Tooltip("스냅 턴이 작동할 스틱의 민감도입니다 (0.1 ~ 1.0).")]
    public float snapThreshold = 0.7f;

    [Header("참조")]
    public Transform headCamera;

    // 내부 변수
    private Rigidbody rb;
    private bool _isSnapReady = true; // 스틱을 놓았는지 확인하는 변수

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = false;
            rb.drag = 5f;
        }
    }

    void Update()
    {
        // 1. 모드 전환 (A 버튼 누르면 토글)
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
        {
            useSnapTurn = !useSnapTurn;
            Debug.Log($"회전 모드 변경: {(useSnapTurn ? "스냅 턴" : "스무스 턴")}");
        }

        // 2. 이동 및 회전 실행
        Move();

        if (useSnapTurn)
        {
            SnapTurn();
        }
        else
        {
            SmoothTurn();
        }
    }

    void Move()
    {
        Vector2 input = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, moveController);
        if (input.magnitude < 0.1f) return;

        Vector3 forward = headCamera.forward;
        Vector3 right = headCamera.right;

        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * input.y) + (right * input.x);
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }

    // 부드럽게 계속 회전하는 방식
    void SmoothTurn()
    {
        float turnInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x;

        if (Mathf.Abs(turnInput) > 0.1f)
        {
            transform.Rotate(Vector3.up * turnInput * smoothTurnSpeed * Time.deltaTime);
        }
    }

    // 특정 각도만큼 딱딱 끊어서 회전하는 방식
    void SnapTurn()
    {
        float turnInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x;

        // 스틱을 충분히 꺾었는지 확인 (Threshold)
        if (Mathf.Abs(turnInput) > snapThreshold)
        {
            // 스틱을 꺾은 상태에서 한 번만 실행되도록 _isSnapReady 체크
            if (_isSnapReady)
            {
                // 오른쪽(+)이면 1, 왼쪽(-)이면 -1
                float direction = turnInput > 0 ? 1 : -1;

                // 즉시 회전
                transform.Rotate(Vector3.up * direction * snapAngle);

                // 스틱을 다시 놓을 때까지 회전 잠금
                _isSnapReady = false;
            }
        }
        else
        {
            // 스틱이 중앙으로 돌아오면 다시 회전할 준비
            _isSnapReady = true;
        }
    }
}