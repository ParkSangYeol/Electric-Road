# Electric Road AI 작업 지침

이 문서는 Electric Road 저장소에서 작업하는 AI와 자동화 도구가 따라야 할 기본 지침이다.
작업 전 현재 코드와 에셋을 확인하고, 문서에 적힌 설명보다 실제 구현을 우선한다.

## 먼저 읽을 문서

작업 목적에 따라 아래 문서를 순서대로 확인한다.

1. [README.md](README.md): 프로젝트 개요, 실행 환경, 디렉터리 안내
2. [게임 규칙](docs/GAMEPLAY_RULES.md): 타일, 비용, 전력 전달, 클리어 판정
3. [아키텍처](docs/ARCHITECTURE.md): 씬, 런타임 시스템, 데이터 흐름
4. [레벨 제작](docs/LEVEL_AUTHORING.md): 스테이지 데이터와 제작·검증 절차

## 프로젝트 핵심 원칙

- Electric Road는 발전기와 모든 공장을 연결하면서 배치 비용을 최소화하는 타일 퍼즐이다.
- 런타임은 `answerMap`과 플레이어 맵을 직접 비교하지 않는다.
- 클리어 여부는 전력 경로 탐색 결과와 비용 임계값으로 결정한다.
- 비용은 플레이어가 편집 가능한 칸에 배치하거나 교체한 타일을 기준으로 계산한다.
- `StageScriptableObject.map`은 초기 맵이며 `answerMap`은 제작 참고용 정답이다.
- 정식 콘텐츠는 Village, Town, City 각 10개씩 총 30개다.

## 주요 코드 위치

- 씬 전환과 현재 스테이지 상태: `Assets/Scripts/GameManager.cs`
- 퍼즐 초기화, 비용, 정답 판정: `Assets/Scripts/Stage/StageHandler.cs`
- 드래그 전선 배치: `Assets/Scripts/WireTileHandler.cs`
- 개별 타일 배치와 팔레트: `Assets/Scripts/TileHandler.cs`
- 배치 명령과 Undo/Redo: `Assets/Scripts/Command/`
- 스테이지 데이터 모델: `Assets/Scripts/ScriptableObjects/`
- 에디터용 스테이지 제작기: `Assets/Scripts/StageBuilder/`
- 정식 퍼즐 데이터: `Assets/ScriptableObjects/PuzzleStage/`

## 작업 규칙

### 코드 변경

- 변경 전에 관련 씬, 프리팹, ScriptableObject 참조를 함께 추적한다.
- 타일 배치는 가능하면 기존 Command 흐름을 사용한다. 타일을 직접 변경하면 비용, 팔레트 수량, Undo/Redo가 서로 어긋날 수 있다.
- `StageHandler.cost`를 별도로 조작하지 말고 `CommandHistoryHandler` 이벤트를 통해 갱신한다.
- 방향 로직을 변경할 때 `WireTileHandler`, `StageBuilder`, `StageHandler.IsAvailableDirection`을 함께 검토한다.
- 새로운 `StageType`을 추가할 때 데이터 enum, 정답 판정, 레벨 제작기, 튜토리얼과 문서를 함께 갱신한다.
- 씬 이름은 코드에서 문자열로 참조된다. 이름 변경 시 Build Settings와 모든 호출부를 함께 수정한다.
- 스테이지 개수에 관한 로직을 추가할 때 하드코딩된 `10`보다 실제 `stageData.Count` 사용을 우선한다.

### 데이터와 에셋 변경

- `.asset`, `.unity`, `.prefab` 파일은 Unity 직렬화 참조가 있으므로 GUID와 `.meta` 파일을 보존한다.
- Odin Inspector가 `TileStruct[,]` 배열을 직렬화한다. 텍스트 일괄 치환으로 맵 배열을 수정하지 않는다.
- 정식 레벨 수정 시 초기 맵, 정답 맵, 발전기 위치, 공장 수, 정답 비용, 별 임계값을 함께 검증한다.
- 타일 비용 변경은 기존 30개 스테이지의 정답 비용과 별 임계값 전체에 영향을 준다.
- 외부 플러그인과 SDK 코드는 명시적인 요청이 없으면 수정하지 않는다.

### 검증

- 코드 변경 후 Unity Console의 컴파일 오류를 확인한다.
- 퍼즐 변경 시 최소한 실패 경로, 정상 연결, 1·2·3성 비용 경계를 확인한다.
- 배치 기능 변경 시 Draw, Select, Erase와 Undo/Redo를 모두 확인한다.
- 스테이지 데이터 변경 시 해당 지역의 이전·현재·다음 퍼즐 잠금 및 진행도 표시를 확인한다.
- 플랫폼 코드는 Steam, Stove, Android 조건부 컴파일 심볼의 영향을 확인한다.
- 자동화 테스트가 현재 없으므로, 수행하지 않은 Unity 수동 검증을 완료한 것처럼 기록하지 않는다.

## 현재 구현상 주의점

- `MODULATOR`와 `AMP_MOD` 판정 코드는 존재하지만 정식 30개 레벨에서는 사용되지 않는다.
- 에디터용 `StageBuilder`의 증폭기·변환기 자동 풀이 메서드는 완성되지 않았다.
- 전력 탐색의 방문 여부는 주로 좌표 단위다. 같은 칸에 서로 다른 전력 상태가 도달하는 기능을 추가할 때 탐색 상태 모델을 재검토한다.
- 프로젝트 코드용 Assembly Definition과 자동 테스트가 없다.
- 일부 런타임 스크립트에 `UnityEditor` 네임스페이스 참조가 있으므로 플레이어 빌드 변경 시 확인이 필요하다.
- 기존 워크트리 변경은 사용자 작업일 수 있다. 관련 없는 변경을 되돌리거나 덮어쓰지 않는다.

## 문서 유지

게임 규칙, 씬 흐름, 데이터 구조 또는 레벨 제작 절차가 달라지면 같은 변경에서 관련 문서도 갱신한다.
코드와 문서가 충돌할 경우 코드를 확인해 문서를 바로잡되, 의도와 구현이 다를 가능성은 작업 결과에 명시한다.
