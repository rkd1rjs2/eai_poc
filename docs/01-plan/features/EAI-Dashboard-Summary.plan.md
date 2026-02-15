# [Plan] Dashboard Summary Page Implementation

## 1. Feature Overview
EAI 대시보드 메인 페이지에 당일 처리 현황을 요약하여 보여주는 기능을 추가합니다. 데이터베이스(시스템)별 및 비즈니스(데이터 타입)별로 처리 완료 건수와 대기/진행 중인 건수를 시각화합니다.

## 2. Goals and Objectives
- 당일(Today) 기준의 전체 메시지 처리 현황 파악.
- 시스템별/업무별 부하 및 지연 상태 가시화.
- 운영자가 한눈에 시스템 건강 상태를 확인할 수 있는 요약 정보 제공.

## 3. Scope
- `IAuditRepository`에 통계 쿼리 기능 추가.
- `Home.razor` 페이지 상단에 요약 카드(Summary Cards) 및 테이블/차트 추가.
- 시스템별(SourceSystem) 통계: (Success vs Ready/Processing)
- 업무별(DataType) 통계: (Success vs Ready/Processing)

## 4. Technical Stack
- Blazor Server
- Dapper (통계 쿼리 실행)
- PostgreSQL (EAI_AUDIT_LOG 테이블)

## 5. Implementation Details
### 5.1 Repository Update
- `Eai.Infrastructure`의 `SqlAuditRepository`에 당일 통계를 가져오는 메서드 추가.
- `GetSystemSummaryAsync(DateTime date)`: 시스템별 성공/대기 건수.
- `GetBusinessSummaryAsync(DateTime date)`: 업무별 성공/대기 건수.

### 5.2 UI Design
- 상단 4개 요약 카드: 총 처리건수, 성공건수, 실패건수, 대기건수.
- 중간 2개 섹션:
    - 시스템별 현황 (Table/Chart)
    - 업무별 현황 (Table/Chart)

## 6. Verification Plan
- 데이터베이스에 테스트 데이터를 입력하고 대시보드에서 정확한 숫자가 표시되는지 확인.
- 실시간으로 데이터가 유입될 때 요약 정보가 갱신되는지 확인.
