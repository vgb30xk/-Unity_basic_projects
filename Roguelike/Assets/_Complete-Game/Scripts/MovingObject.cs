using UnityEngine;
using System.Collections;

namespace Completed
{
    // abstract 키워드를 사용하면 파생 클래스에서 구현되어야 하는 불완전한 클래스와 클래스 멤버를 생성할 수 있습니다.
    public abstract class MovingObject : MonoBehaviour
    {
        public float moveTime = 0.1f;           // 객체가 이동하는 데 걸리는 시간 (초)
        public LayerMask blockingLayer;         // 충돌이 검사될 레이어

        private BoxCollider2D boxCollider;      // 이 객체에 연결된 BoxCollider2D 컴포넌트
        private Rigidbody2D rb2D;               // 이 객체에 연결된 Rigidbody2D 컴포넌트
        private float inverseMoveTime;          // 이동을 더 효율적으로 만들기 위해 사용되는 역수
        private bool isMoving;                  // 객체가 현재 이동 중인지 여부

        // Start 함수는 상속된 클래스에서 재정의할 수 있는 보호된 가상 함수입니다.
        protected virtual void Start()
        {
            // 이 객체의 BoxCollider2D 컴포넌트에 대한 참조를 가져옵니다.
            boxCollider = GetComponent<BoxCollider2D>();

            // 이 객체의 Rigidbody2D 컴포넌트에 대한 참조를 가져옵니다.
            rb2D = GetComponent<Rigidbody2D>();

            // 이동 시간의 역수를 계산하여 나누기 대신 곱하기로 사용합니다. 이것이 더 효율적입니다.
            inverseMoveTime = 1f / moveTime;
        }

        // Move 함수는 이동이 가능한 경우 true를 반환하고 그렇지 않은 경우 false를 반환합니다.
        // Move 함수는 x 방향, y 방향 및 충돌을 검사하기 위한 RaycastHit2D를 매개변수로 사용합니다.
        protected bool Move(int xDir, int yDir, out RaycastHit2D hit)
        {
            // 현재 위치를 기준으로 시작 위치를 저장합니다.
            Vector2 start = transform.position;

            // 이동 방향 매개변수를 기반으로 종착점을 계산합니다.
            Vector2 end = start + new Vector2(xDir, yDir);

            // 이 객체 자체의 충돌체에 linecast가 충돌하지 않도록 BoxCollider2D를 비활성화합니다.
            boxCollider.enabled = false;

            // start 지점에서 end 지점까지 linecast를 사용하여 blockingLayer에서 충돌을 검사합니다.
            hit = Physics2D.Linecast(start, end, blockingLayer);

            // linecast 이후 boxCollider를 다시 활성화합니다.
            boxCollider.enabled = true;

            // 아무것도 맞히지 않았고 객체가 이미 이동 중이 아닌 경우
            if (hit.transform == null && !isMoving)
            {
                // SmoothMovement 코루틴을 시작하고 목적지로 end를 전달합니다.
                StartCoroutine(SmoothMovement(end));

                // 이동이 성공했음을 나타내기 위해 true를 반환합니다.
                return true;
            }

            // 뭔가를 맞혔으면 false를 반환하고 이동이 실패했음을 나타냅니다.
            return false;
        }

        // 유닛을 한 공간에서 다음 공간으로 이동시키는 데 사용되는 코루틴입니다.
        // end 매개변수는 이동할 위치를 지정합니다.
        protected IEnumerator SmoothMovement(Vector3 end)
        {
            // 객체가 이제 이동 중입니다.
            isMoving = true;

            // 현재 위치와 종착점 간의 차이의 제곱 크기를 기반으로 이동할 남은 거리를 계산합니다.
            // magnitude 대신 square magnitude를 사용하는 이유는 연산 비용이 더 저렴하기 때문입니다.
            float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

            // 거리가 매우 작은 양 (Epsilon, 거의 0)보다 큰 경우:
            while (sqrRemainingDistance > float.Epsilon)
            {
                // moveTime을 기준으로 end에 더 가까워지도록 새 위치를 찾습니다.
                Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);

                // 연결된 Rigidbody2D에 MovePosition을 호출하여 계산된 위치로 이동합니다.
                rb2D.MovePosition(newPosition);

                // 이동 후 남은 거리를 다시 계산합니다.
                sqrRemainingDistance = (transform.position - end).sqrMagnitude;

                // sqrRemainingDistance가 충분히 0에 가까워질 때까지 반환하고 반복합니다.
                yield return null;
            }

            // 객체를 이동의 끝에 정확하게 위치시킵니다.
            rb2D.MovePosition(end);

            // 객체는 더 이상 이동 중이 아닙니다.
            isMoving = false;
        }

        // AttemptMove 함수는 막혔을 때 상호작용할 수 있는 유닛의 유형을 지정하기 위해 제네릭 매개변수 T를 사용합니다.
        // xDir, yDir 매개변수를 사용하여 이동을 시도합니다.
        protected virtual void AttemptMove<T>(int xDir, int yDir)
            where T : Component
        {
            // Move 함수를 호출할 때 hit에 linecast 결과를 저장합니다.
            RaycastHit2D hit;

            // Move 함수가 성공한 경우 canMove를 true로 설정하고 실패한 경우 false로 설정합니다.
            bool canMove = Move(xDir, yDir, out hit);

            // linecast로 아무것도 맞히지 않은 경우
            if (hit.transform == null)
                // 아무것도 맞히지 않았으므로 더 이상의 코드를 실행하지 않고 반환합니다.
                return;

            // 충돌한 오브젝트에 연결된 T 유형의 컴포넌트에 대한 참조를 가져옵니다.
            T hitComponent = hit.transform.GetComponent<T>();

            // 만약 canMove가 false이고 hitComponent가 null이 아니라면, 이는 MovingObject가 막혀있고 상호작용 가능한 무언가에 충돌했음을 의미합니다.
            if (!canMove && hitComponent != null)

                // OnCantMove 함수를 호출하고 hitComponent를 매개변수로 전달합니다.
                OnCantMove(hitComponent);

        }

        // abstract 수정자는 수정되지 않거나 불완전한 구현을 가진 것을 나타냅니다.
        // OnCantMove는 파생 클래스에서 재정의될 것입니다.
        protected abstract void OnCantMove<T>(T component)
            where T : Component;
    }
}
