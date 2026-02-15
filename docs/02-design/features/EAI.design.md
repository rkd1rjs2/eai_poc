# EAI 시스템 설계 문서: [기능명 - EAI]

## 1. 서론
### 1.1 목적
본 문서는 EAI(Enterprise Application Integration) 시스템의 아키텍처, 구성 요소 및 상호 작용을 상세히 설명하는 설계 문서입니다.

### 1.2 범위 (설계)
이 섹션은 EAI 시스템 설계 문서가 다루는 범위를 명시합니다.

## 2. 상위 수준 아키텍처
### 2.1 시스템 개요 다이어그램
[EAI 시스템과 소스 및 타겟 시스템(Redis Streams, C# 구성 요소 포함) 간의 상호 작용을 보여주는 상위 수준 다이어그램을 삽입하세요.]

### 2.2 구성 요소 개요
EAI 시스템의 주요 구성 요소(예: Producer, Redis Streams, Transformer, Consumer, Dashboard)에 대해 간략하게 설명합니다.

## 3. 상세 설계
### 3.1 데이터 흐름 설계
소스 시스템에서 타겟 시스템까지 특정 트랜잭션의 End-to-End 데이터 흐름을 각 단계 및 변환을 상세히 설명합니다.

#### 3.1.1 Producer 설계
소스 시스템에서 데이터를 추출(예: CDC, Trigger)하여 Redis Streams에 발행하는 방법입니다.
- **기술**: Oracle Managed Data Access, CDC/Trigger
- **구현**: C# Producer 애플리케이션

#### 3.1.2 Redis Stream 설계
스트림 이름, 메시지 구조, 컨슈머 그룹 및 영속성 설정을 포함한 Redis Streams 사용에 대한 세부 정보입니다.
- **Idempotency Key 정의**: Redis Stream 메시지 생성 시 소스 시스템의 PK + 트랜잭션ID를 조합한 고유 키를 포함해야 합니다.
- **메시지 보존 정책 (Retention Policy)**:
    - **MAXLEN 설정**: XADD 시 스트림의 최대 길이 유지 정책 (예: 최근 10만 건 또는 최근 3일치).
    - **Archive 전략**: 오래된 데이터를 별도의 Cold Storage(예: 데이터플랫폼의 파일 저장소)로 옮길 것인지에 대한 검토.
- **스트림 명명 규칙**:
- **메시지 구조 (JSON/Protobuf)**:
- **컨슈머 그룹 전략**: 데이터 폭증 시 Transformer 인스턴스를 늘렸을 때 Redis가 어떻게 분산 배정하는지 전략 명시 (Scale-out 전략).
- **영속성 (AOF/RDB)**:

#### 3.1.3 Transformer 설계
Redis Streams에서 데이터를 소비하고, 변환하여 타겟 시스템 또는 다른 스트림에 발행하는 C# Worker Service의 설계입니다.
- **입력**: Redis Stream 메시지
- **변환 로직**: 매핑 규칙, 데이터 보강, 유효성 검사
- **출력**: 타겟 시스템 API/Webhook, 기타 Redis Streams
- **기술**: C# Worker Service, Dapper

#### 3.1.4 Consumer/Webhook 설계
타겟 시스템이 데이터를 소비하거나 알림을 받는 방법(예: Webhook, 직접 DB 쓰기)입니다.
- **Webhook 통합**: 콜백 메커니즘, 페이로드 구조, **Idempotency Key를 활용한 중복 체크 로직 포함**
- **타겟 시스템 통합**: API, DB 커넥터

### 3.2 오류 처리 및 재시도 메커니즘
메시지 실패, 재시도 및 잠재적인 Dead Letter Queue (DLQ) 구현에 대한 상세 설계입니다.
- **서킷 브레이커 (Circuit Breaker)**: 타겟 시스템의 상태를 보고 요청을 일시적으로 차단하여 부하를 줄이고 시스템 회복 시간을 제공하는 지능적인 재시도 패턴을 적용합니다.
- **백프레셔 (Backpressure)**: 타겟 시스템의 처리 속도가 느려질 경우, Transformer의 메시지 소비 속도를 자동으로 조절하여 타겟 시스템에 가해지는 부하를 제어합니다.

### 3.3 모니터링 및 알림 설계
데이터 흐름, 지연(Lag) 및 시스템 상태를 실시간으로 모니터링하기 위한 Blazor 대시보드 설계입니다.
- **End-to-End 추적**: 대시보드에서 특정 Trace ID만 검색하면 전 구간의 처리 상태를 볼 수 있도록 설계하여 운영 단계에서 강력한 도구를 제공합니다.
- **대시보드 메트릭**: 지연, 메시지 수, 오류율
- **알림**: 임계값, 알림 채널

### 3.4 분산 트레이싱 설계
인사에서 보낸 데이터가 재무를 거쳐 모바일까지 가는 등 복잡한 데이터 흐름에서 문제가 발생했을 때 추적을 용이하게 하기 위한 설계입니다.
- **Trace ID 도입**: 데이터가 Producer에서 생성될 때 전역 고유 ID(GUID)를 부여하고, 이를 모든 로그와 Redis 메시지에 포함하도록 합니다.
- **Trace ID 전파**: EAI 시스템의 모든 구성 요소(Producer, Transformer, Consumer)는 수신한 Trace ID를 다음 단계로 전파해야 합니다.

### 3.5 데이터 보정(Reconciliation) 프로세스
실시간 연계의 한계를 보완하고 데이터 정합성을 확보하기 위한 보정 프로세스 설계입니다.
- **주기적 비교**: 하루에 한 번 또는 정기적으로 소스 DB와 타겟 DB의 데이터 건수 및 핵심 필드를 비교하는 로직을 구현합니다.
- **누락분 처리**: 실시간 연계 중 발생할 수 있는 누락분을 배치(Batch) 방식으로 보정하는 구조를 포함합니다.

## 4. 메타데이터 및 데이터 관리 설계
EAI 시스템의 운영 환경 설정, 데이터 변환 규칙, 처리 상태를 관리하기 위한 설계입니다.

### 4.1 메타데이터 분류 및 저장 전략
| 분류 | 주요 내용 | 저장소 |
| :--- | :--- | :--- |
| **코드 매핑 (X-Ref)** | 시스템 간 코드 변환(예: A01 -> DEPT_MGR), 데이터 형식 정의 | RDBMS (SQL Server/Oracle) |
| **인터페이스 설정** | 엔드포인트 URL, DB 접속 정보, API Key, Webhook 주소 | RDBMS / HashiCorp Vault |
| **메시지 상태 (Audit)** | 트랜잭션 로그, Trace ID별 처리 상태(READY/SUCCESS/FAIL), 오류 로그 | RDBMS (최근) / Data Lake (이력) |
| **스트림 위치 (Offset)** | Last Processed ID, 소비자 그룹별 읽기 위치 | Redis (Hash/String) |
| **성능 메트릭** | 처리 지연(Lag), Throughput, 성공/실패율 | Prometheus / Redis |

### 4.2 상세 관리 항목
#### 4.2.1 매핑 및 코드 변환 (Cross-Reference)
- **시스템 간 매핑**: 소스/타겟 시스템 간의 상속 관계 및 코드 변환 규칙을 테이블화하여 Transformer가 런타임에 참조합니다.
- **규격 정의**: 필드 타입, 길이, 필수 여부 등을 정의하여 변환 시 유효성 검사 도구로 활용합니다.

#### 4.2.2 메시지 처리 상태 및 이력 (Audit & Tracking)
- **트랜잭션 로그**: Trace ID를 중심으로 전 구간(Producer -> Redis -> Transformer -> Target)의 시작/종료 시간을 기록합니다.
- **상태 전이**: `READY` -> `PROCESSING` -> `SUCCESS` / `FAIL` 단계를 추적합니다.
- **오류 상세**: 실패 시 Stack Trace와 재시도 횟수를 기록하여 대시보드에서 분석 가능하게 합니다.

#### 4.2.3 스트림 위치 정보 관리
- **Offset 관리**: Redis에 `EAI:OFFSET:{ConsumerGroup}:{StreamName}` 키를 생성하여 마지막 처리 메시지 ID를 저장, 재시작 시 누락을 방지합니다.

## 5. 보안 고려사항
데이터 보안, 접근 제어 및 통신 암호화에 대한 설계 고려사항입니다.
- **PII (개인정보) 마스킹**: 인사/회원 정보 중 주민번호, 연락처 등 민감 정보의 암호화 및 마스킹 처리 방안을 명시합니다.
- **인가 및 보안**: 시스템 간 통신 시 API Key 및 Bearer 토큰 인증을 필수 적용합니다.

## 6. 배포 및 인프라 구성
EAI 구성 요소를 배포하고 건강 상태를 관리하는 방법입니다.
- **Health Check**: 각 Worker Service의 생존 여부를 L4 로드밸런서나 대시보드가 감지하는 전략.
- **인프라 구성**: Docker Compose를 활용하여 Redis, RDBMS(PostgreSQL/SQL Server), EAI 모듈을 통합 관리합니다.

## 7. 미해결 질문 / 향후 작업

운영 고도화 및 시스템 안정성 향상을 위해 향후 검토가 필요한 항목입니다.

- **메시지 순서 보장 (Ordering)**: 특정 키(예: 사원번호) 기준의 순차 처리를 보장하기 위한 파티셔닝 전략 연구.

- **스키마 버전 관리 (Schema Versioning)**: 인터페이스 규격 변경 시 하위 호환성 유지를 위한 메시지 버전닝 도입.

- **구성 정보 동적 반영 (Hot Reload)**: Redis Pub/Sub을 활용하여 RDBMS의 설정 변경 사항을 각 Worker에 실시간 전파.
