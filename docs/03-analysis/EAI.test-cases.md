# EAI 시스템 테스트 관리 문서

## 1. 테스트 개요
본 문서는 Redis Streams와 C# 기반 EAI 시스템의 기능적/비기능적 요구사항을 검증하기 위한 테스트 케이스를 정의합니다.

## 2. 테스트 환경
- **Message Broker**: Redis (Port: 6380)
- **Database**: PostgreSQL (Audit 및 시뮬레이션 소스)
- **Applications**: Eai.Producer, Eai.Transformer, Eai.Dashboard

## 3. 상세 테스트 케이스 (Test Cases)

### 3.1 기능 테스트 (Functional)
| ID | 테스트 항목 | 테스트 절차 | 예상 결과 | 상태 |
| :--- | :--- | :--- | :--- | :--- |
| FT-01 | 데이터 추출 및 발행 | `SOURCE_HR_DATA`에 데이터 insert | Producer가 감지하여 Redis에 발행 및 Audit에 `READY` 기록 | 대기 |
| FT-02 | 메시지 소비 및 변환 | Transformer 실행 | Redis 메시지를 읽어 `PROCESSING` 거쳐 `SUCCESS`로 업데이트 | 대기 |
| FT-03 | 코드 매핑(X-Ref) 적용 | 특정 부서코드(`DEPT01`) 데이터 전송 | 타겟 코드(`ACC_01`)로 정상 변환되어 로그에 출력됨 | 대기 |
| FT-04 | 멱등성(중복방지) 검증 | 동일한 `IdempotencyKey` 메시지 재전송 | 타겟 시스템(또는 로그)에서 중복 건으로 판단하여 무시 | 대기 |

### 3.2 비기능/안정성 테스트 (Non-Functional)
| ID | 테스트 항목 | 테스트 절차 | 예상 결과 | 상태 |
| :--- | :--- | :--- | :--- | :--- |
| NFT-01 | 장애 복구 (Recovery) | Transformer 중지 후 메시지 발행 -> Transformer 재시작 | 중지 기간 동안 쌓인 메시지를 누락 없이 순차 처리함 | 대기 |
| NFT-02 | 보존 정책 (Retention) | 대량 메시지(10만건+) 발행 | Redis 메모리가 설정된 `MAXLEN`을 초과하지 않고 유지됨 | 대기 |
| NFT-03 | 트레이싱 (Tracing) | 특정 `TraceId`로 검색 | Producer부터 Transformer까지 전체 이력이 대시보드에 조회됨 | 대기 |

## 4. 데이터 정합성 체크리스트
- [ ] 소스 DB 건수와 Audit 로그의 `SUCCESS` 건수가 일치하는가?
- [ ] 실패(`FAIL`)한 메시지에 대해 에러 내용이 정확히 기록되었는가?
- [ ] 모든 메시지에 고유한 `TraceId`가 부여되었는가?

## 5. 결함 관리 (Issue Tracking)
*발견된 결함은 여기에 기록합니다.*
- (예시) 이슈 1: Redis 포트 충돌 문제 -> (조치) 6380으로 변경 완료.
