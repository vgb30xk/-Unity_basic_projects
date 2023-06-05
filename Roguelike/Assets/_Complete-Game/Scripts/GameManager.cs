using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Completed
{
    using System.Collections.Generic;    //Allows us to use Lists.
    using UnityEngine.UI;                   //Allows us to use UI.

    public class GameManager : MonoBehaviour
    {
        public float levelStartDelay = 2f;                       //시작 전 대기 시간(초).
        public float turnDelay = 0.1f;                           //플레이어 턴 간의 지연 시간.
        public int playerFoodPoints = 100;                       //플레이어 음식 포인트의 시작 값.
        public static GameManager instance = null;               //GameManager의 정적 인스턴스로, 다른 스크립트에서 액세스할 수 있게 함.
        [HideInInspector] public bool playersTurn = true;        //플레이어의 턴 여부를 확인하는 부울 변수. Inspector에는 숨겨지지만 외부에서 액세스 가능.

        private Text levelText;                                 //현재 레벨 번호를 표시하는 텍스트.
        private GameObject levelImage;                           //레벨이 설정되는 동안 레벨을 가리는 이미지, levelText의 배경.
        private BoardManager boardScript;                        //레벨을 설정하는 BoardManager에 대한 참조.
        private int level = 1;                                   //현재 레벨 번호, 게임 내에서 "Day 1"로 표시됨.
        private List<Enemy> enemies;                             //모든 적 유닛의 리스트, 이동 명령을 내릴 때 사용됨.
        private bool enemiesMoving;                              //적이 이동 중인지 확인하는 부울 변수.
        private bool doingSetup = true;                          //보드 설정 중인지 확인하는 부울 변수로, 설정 중에 플레이어가 이동하지 못하도록 함.


        //Awake는 항상 Start 함수보다 먼저 호출됩니다.
        void Awake()
        {
            //instance가 아직 존재하지 않는 경우
            if (instance == null)
                //instance를 이 스크립트로 설정합니다.
                instance = this;
            //instance가 이미 존재하고 이 스크립트와 다른 경우
            else if (instance != this)
                //이 스크립트를 파괴합니다. 이렇게 하면 싱글톤 패턴이 적용되어 GameManager의 인스턴스가 하나만 존재하게 됩니다.
                Destroy(gameObject);

            //게임 오브젝트가 씬을 다시로드할 때 파괴되지 않도록 설정합니다.
            DontDestroyOnLoad(gameObject);

            //enemies를 Enemy 객체의 새로운 리스트로 할당합니다.
            enemies = new List<Enemy>();

            //BoardManager 스크립트에 대한 참조를 가져옵니다.
            boardScript = GetComponent<BoardManager>();

            //InitGame 함수를 호출하여 첫 번째 레벨을 초기화합니다.
            InitGame();
        }

        //이 함수는 한 번만 호출되며, sceneLoaded 이벤트가 처음으로 발생할 때만 호출되도록 지정됩니다.
        //이렇게 하지 않으면 Scene Load 콜백이 가장 처음에 호출되어 버립니다. 이는 원하지 않는 동작입니다.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static public void CallbackInitialization()
        {
            //씬이 로드될 때마다 이 콜백이 호출되도록 등록합니다.
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        //이 함수는 각각의 씬이 로드될 때 호출됩니다.
        static private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            instance.level++;
            instance.InitGame();
        }


        //각 레벨마다 게임을 초기화합니다.
        void InitGame()
        {
            //doingSetup이 true인 동안 플레이어는 이동할 수 없도록 설정하여 타이틀 카드가 나타날 때 플레이어의 이동을 방지합니다.
            doingSetup = true;

            //이름으로 LevelImage를 찾아 levelImage에 대한 참조를 가져옵니다.
            levelImage = GameObject.Find("LevelImage");

            //이름으로 LevelText의 텍스트 컴포넌트를 찾아 levelText에 대한 참조를 가져옵니다.
            levelText = GameObject.Find("LevelText").GetComponent<Text>();

            //levelText의 텍스트를 "Day" 문자열에 현재 레벨 번호를 추가하여 설정합니다.
            levelText.text = "Day " + level;

            //레벨 설정 중에 게임 보드가 보이지 않도록 levelImage를 활성화합니다.
            levelImage.SetActive(true);

            //levelStartDelay초 후에 HideLevelImage 함수를 호출합니다.
            Invoke("HideLevelImage", levelStartDelay);

            //다음 레벨을 준비하기 위해 enemies List에서 Enemy 객체를 모두 제거합니다.
            enemies.Clear();

            //BoardManager 스크립트의 SetupScene 함수를 호출하여 현재 레벨 번호를 전달합니다.
            boardScript.SetupScene(level);

        }


        //레벨 간의 검은 이미지를 숨깁니다.
        void HideLevelImage()
        {
            //levelImage 게임 오브젝트를 비활성화합니다.
            levelImage.SetActive(false);

            //doingSetup을 false로 설정하여 플레이어가 다시 이동할 수 있도록 합니다.
            doingSetup = false;
        }

        //Update는 매 프레임마다 호출됩니다.
        void Update()
        {
            //playersTurn, enemiesMoving, doingSetup 중 하나라도 true인지 확인합니다.
            if (playersTurn || enemiesMoving || doingSetup)
                //위 조건 중 하나라도 true이면 MoveEnemies를 시작하지 않고 반환합니다.
                return;

            //적을 이동시킵니다.
            StartCoroutine(MoveEnemies());
        }

        //넘겨받은 Enemy를 Enemy 객체의 List에 추가합니다.
        public void AddEnemyToList(Enemy script)
        {
            //Enemy를 enemies List에 추가합니다.
            enemies.Add(script);
        }


        //플레이어가 0의 음식 포인트에 도달하면 GameOver가 호출됩니다.
        public void GameOver()
        {
            //levelText를 통해 플레이어가 통과한 레벨 수와 게임 오버 메시지를 설정합니다.
            levelText.text = "After " + level + " days, you starved.";

            //검은 배경 이미지 게임 오브젝트를 활성화합니다.
            levelImage.SetActive(true);

            //이 GameManager를 비활성화합니다.
            enabled = false;
        }

        //적을 순차적으로 이동시키는 코루틴입니다.
        IEnumerator MoveEnemies()
        {
            //enemiesMoving이 true인 동안에는 플레이어가 이동할 수 없습니다.
            enemiesMoving = true;

            //turnDelay초 동안 대기합니다. 기본값은 0.1(100ms)입니다.
            yield return new WaitForSeconds(turnDelay);

            //적이 생성되지 않은 경우(첫 번째 레벨인 경우):
            if (enemies.Count == 0)
            {
                //다음 적이 이동하기 전까지 turnDelay초 동안 기다립니다.
                yield return new WaitForSeconds(turnDelay);
            }

            //Enemy 객체의 List를 반복합니다.
            for (int i = 0; i < enemies.Count; i++)
            {
                //enemies List의 인덱스 i에 해당하는 Enemy의 MoveEnemy 함수를 호출합니다.
                enemies[i].MoveEnemy();

                //enemies[i]의 moveTime 후에 다음 Enemy가 이동하도록 기다립니다.
                yield return new WaitForSeconds(enemies[i].moveTime);
            }

            //적이 이동을 완료하면 playersTurn을 true로 설정하여 플레이어가 이동할 수 있게 합니다.
            playersTurn = true;

            //적의 이동이 완료되었으므로 enemiesMoving을 false로 설정합니다.
            enemiesMoving = false;
        }
    }
}
