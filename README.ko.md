# SHELTER

[English](README.md)

Meta Quest용 VR 좀비 서바이벌 게임. 하나의 쉘터를 지키며 10라운드 동안 몰려오는 좀비를 막아낸다.

- 엔진: Unity 6000.5.2f1 (URP)
- VR: OpenXR, XR Interaction Toolkit 3
- 플랫폼: Meta Quest (Android)

## 프로젝트 소개

- 3인 팀 프로젝트, 2026년 7월~11월.
- 개발이 종료되었다. 저장소는 기록용으로 보관한다.
- 현재 빌드는 싱글 플레이다. Photon Fusion 2 기반 1~4인 협동을 계획했으나 구현하지 않았다.

## 게임 방식

- 쉘터 안에서 10라운드를 버틴다. 10라운드를 클리어하면 승리, 사망하면 게임 종료.
- 좀비는 바깥에서 나타나 창문과 출입구의 판자를 뜯고 들어온다.
- 좀비를 처치하면 돈을 얻는다. 돈은 다음에 사용한다.
  - 자판기 상점에서 무기 구매: 권총, SMG, AR
  - 재장전 (탄창마다 비용 발생)
  - 준비 시간 중 바리케이드 수리
  - 구급상자에서 체력 회복
- 상점 위의 준비 완료 버튼으로 준비 시간을 생략한다.
- 난이도: 타이틀 화면에서 게임 시작을 누른 뒤 Normal 또는 Hard를 고른다.
- 10라운드를 클리어하거나 사망하면 다시 하기와 타이틀 버튼이 있는 패널이 뜬다.
- 튜토리얼에서 이동, 구매, 사격, 재장전, 무기 장착, 수리를 익힌다.

## 조작

| 입력 | 동작 |
|---|---|
| 왼쪽 스틱 | 이동 |
| 오른쪽 스틱 | 스냅 회전 |
| 트리거 | 잡기, 선택, 구매, 사격 |
| A / X | 재장전 |
| B / Y | 단발 / 연사 전환 |
| 왼쪽 메뉴 버튼 | 일시정지 메뉴 |
| 무기를 허리 옆에서 놓기 | 무기 장착 |

체력과 돈은 손등에 표시된다.

## 시스템

| 시스템 | 구현 |
|---|---|
| 라운드 | `RoundManager` 상태 머신: 준비 시간, 진행, 클리어, 승리, 패배. 다른 시스템은 이벤트를 구독한다. |
| 웨이브 | 라운드마다 `WaveDefinition` 에셋 하나가 좀비 종류와 수를 정한다. `WaveManager`가 태그된 스폰 포인트에 나눠 스폰하고 동시 생존 수를 제한한다. |
| 좀비 | `NavMeshAgent`로 이동한다. 밖에서는 배정된(없으면 가장 가까운) 바리케이드를 공격하고, 안에 들어오면 가장 가까운 플레이어를 쫓는다. 창문을 넘어 들어오고, 죽으면 래그돌이 되며, 오브젝트 풀로 재사용한다. |
| 바리케이드 | 창문과 출입구마다 판자가 있다. 좀비가 한 번 칠 때마다 판자가 하나 떨어진다. 플레이어는 수리 버튼에서 돈을 내고 판자를 복구한다. |
| 경제 | 지갑, 처치 보상, 자판기 상점, 유료 재장전, 회복 스테이션. |
| 전투 | 레이캐스트 총기, 단발·연사, 탄창, 진동 피드백, 허리 홀스터. |
| 플레이어 | 체력과 피격 연출. 아바타 몸은 헤드셋을 따라가고, 팔 IK는 컨트롤러를 따라간다. |
| 난이도 | `DifficultyConfig`가 좀비 체력, 스폰 수, 동시 생존 수, 처치 보상에 배율을 곱한다. |
| UI / 오디오 | 플레이어 앞에 배치되는 월드 스페이스 메뉴, 손목 표시, 라운드 음악, 좀비 공간 음향. |

밸런스 수치는 `Assets/_Project/ScriptableObjects`의 ScriptableObject로 관리한다.

## 밸런스

좀비

| 종류 | 체력 | 속도 (m/s) | 보상 | 등장 |
|---|---|---|---|---|
| Walker | 100 | 1.5 | 80 | 1라운드부터 |
| Runner | 80 | 3.2 | 100 | 4라운드부터 |

무기

| 무기 | 가격 | 데미지 | 탄약 |
|---|---|---|---|
| 권총 | 300 | 37 | 15 |
| SMG | 900 | 22 | 40 |
| AR | 1800 | 28 | 60 |

- 시작 자금: 800
- 회복: 400, 체력 전부 회복
- 라운드별 좀비 수 (Normal): 6, 8, 10, 12, 15, 18, 21, 24, 28, 32
- 라운드 사이 준비 시간: 60초

Hard 모드 배율

| 좀비 체력 | 스폰 수 | 동시 생존 수 | 처치 보상 |
|---|---|---|---|
| ×1.75 | ×1.5 | ×1.2 | ×0.6 |

## 프로젝트 실행

1. Unity 6000.5.2f1을 Android Build Support와 함께 설치한다.
2. 저장소를 클론해 Unity로 연다.
3. 아래 서드파티 에셋을 표의 폴더에 임포트한다.
4. `Assets/Scenes/Main.unity`(타이틀) 또는 `Assets/Scenes/TestScene.unity`(게임)를 연다.

서드파티 에셋이 없어도 코드는 컴파일된다. 없으면 모델, 애니메이션, 사운드가 비어 있다.

## 서드파티 에셋 (미포함)

재배포가 허용되지 않는 라이선스라 저장소에서 제외했다.

| 폴더 | 내용 |
|---|---|
| `Assets/Plugins/RootMotion` | Final IK (RootMotion) — 플레이어 몸 IK |
| `Assets/Survivalist` | 플레이어 캐릭터 모델 (`StarterAssets`는 포함) |
| `Assets/ZombieMale_AAB` | 좀비 캐릭터 모델 |
| `Assets/Kevin Iglesias` | Human Basic Motions FREE, Human Melee Animations FREE |
| `Assets/Infima Games` | Low Poly Shooter Pack - Free Sample |
| `Assets/Weapon/Low Poly Pistol Weapon Pack 3` | 무기 모델 |
| `Assets/Weapon/Low Poly SMG Weapon Pack 2` | 무기 모델 |
| `Assets/Weapon/LowPoly_Tactical_Dagger` | 단검 모델 |
| `Assets/PBR_Materials_AL8310` | PBR 재질 |
| `Assets/YughuesFreeBushes2018` | 덤불 |
| `Assets/Map`, `Assets/Plank` | 쉘터 맵, 판자 모델 |
| `Assets/Resources/Animations` | 좀비 애니메이션 (Mixamo) |
| `Assets/Resources/SFX`, `Assets/Sounds` | 효과음, 음악 |

## 프로젝트 구조

| 경로 | 내용 |
|---|---|
| `Assets/_Project/Scripts` | 게임 코드(`Shelter.*`): 라운드, 웨이브, 좀비, 바리케이드, 경제, 플레이어, 난이도, UI, 오디오 |
| `Assets/_Project/ScriptableObjects` | 밸런스 데이터: 라운드, 웨이브, 좀비, 경제, 난이도 |
| `Assets/_Project/Prefabs` | 무기, UI, 플레이어 |
| `Assets/Scenes` | `Main`(타이틀), `TestScene`(게임) |
