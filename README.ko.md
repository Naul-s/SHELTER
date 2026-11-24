# SHELTER

[English](README.md)

Meta Quest용 VR 좀비 서바이벌 게임. 하나의 쉘터를 지키며 10라운드 동안 몰려오는 좀비를 막아낸다.

- 엔진: Unity 6000.5.2f1 (URP)
- VR: OpenXR, XR Interaction Toolkit 3
- 플랫폼: Meta Quest (Android)

## 게임 방식

- 쉘터 안에서 10라운드를 버틴다. 10라운드를 클리어하면 승리, 사망하면 게임 종료.
- 좀비는 바깥에서 나타나 창문과 출입구의 판자를 뜯고 들어온다.
- 좀비를 처치하면 돈을 얻는다. 돈은 다음에 사용한다.
  - 자판기 상점에서 무기 구매: 권총, SMG, AR
  - 재장전 (탄창마다 비용 발생)
  - 준비 시간 중 바리케이드 수리
  - 구급상자에서 체력 회복
- 상점 위의 준비 완료 버튼으로 준비 시간을 생략한다.
- 난이도: Normal, Hard. Hard는 좀비 체력과 수가 늘고 처치 보상이 줄어든다.
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
