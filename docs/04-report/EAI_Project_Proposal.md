# 차세대 실시간 이벤트 기반 EAI 플랫폼 구축 제안서

**문서 번호:** EAI-PROP-2026-001  
**작성일:** 2026년 2월 10일  
**작성자:** EAI TF팀  

---

## 1. 프로젝트 개요 (Executive Summary)
기업 내 산재된 이기종 시스템(HR, 재무, 영업 등) 간의 데이터 동기화를 기존의 **배치(Batch) 방식에서 실시간 이벤트(Event-Driven) 방식**으로 전환하여, 데이터 지연 시간을 제로화하고 비즈니스 민첩성을 확보하기 위한 차세대 연계 통합 플랫폼을 구축합니다.

## 2. 추진 배경 및 필요성
- **데이터 적시성 부족**: 기존 폴링(Polling) 방식은 데이터 변경 후 전송까지 시차 발생(1분~1시간).
- **시스템 부하 가중**: 변경 사항이 없어도 지속적인 DB 조회로 인한 리소스 낭비.
- **통합 관제 부재**: 시스템별로 로그가 분산되어 장애 발생 시 원인 파악에 장시간 소요.
- **확장성 한계**: 새로운 업무 시스템 추가 시 복잡한 Point-to-Point 연결 증가.

## 3. 목표 시스템 아키텍처
**"Core와 Domain의 분리를 통한 유연하고 안전한 구조"**

### 3.1 아키텍처 구성도
```mermaid
[Domain DBs (HR/AC/AR)] --(Real-time Trigger)--> [EAI Producer] --(Redis Stream)--> [EAI Transformer] --(API/DB)--> [Target Systems]
                                                        ^
                                                        |
                                                 [EAI Core DB]
                                           (Management / Audit / Mapping)
```

### 3.2 핵심 기술 요소
| 구분 | 적용 기술 | 특징 및 기대효과 |
| :--- | :--- | :--- |
| **Real-time CDC** | **PostgreSQL Listen/Notify**<br>**Oracle CQN** | 폴링 없는 실시간 데이터 감지, DB 부하 최소화 |
| **Message Broker** | **Redis Streams** | **신뢰성 기반 전송 인프라**:<br>- **수신 확인(ACK)**: 처리 완료 확인 후 메시지 삭제로 유실 방지<br>- **미처리 관리(PEL)**: 장애 시 미처리 메시지 자동 추적 및 재처리<br>- **충격 완화**: 급격한 트래픽 증가 시 버퍼링을 통한 타겟 보호 |
| **Engine** | **.NET 10 (C#)** | 고성능 비동기 처리, 멀티 플랫폼 지원 |
| **Data Isolation** | **Multi-Database** | EAI Core(설정/로그)와 업무 데이터(HR/AC/AR)의 물리적 격리 |
| **Monitoring** | **Blazor Dashboard** | 트랜잭션 전 구간 실시간 흐름 시각화 및 추적 |

## 4. 주요 기능 상세
1.  **다중 DB 리스너 (Multi-DB Listener)**
    *   단일 엔진에서 HR, 회계, 영업 등 여러 도메인 DB의 변경 사항을 동시 감지.
    *   시스템 추가 시 설정만으로 즉시 연동 가능 (Zero-Code 지향).

2.  **전송 무결성 보장 (Guaranteed Delivery)**
    *   **Ack/Retry 메커니즘**: 서비스 장애 시에도 Redis Pending List를 통한 데이터 복구.
    *   **영속성(Persistence)**: 메모리 데이터를 디스크에 실시간 기록하여 인프라 장애 대비.

3.  **유연한 워크플로우 (Flexible Workflow)**
    *   **Standard Flow**: 단순 데이터 이동 및 변환 (HR 인사정보 동기화).
    *   **Webhook Flow**: 처리 완료 후 외부 시스템 API 자동 호출 (회계 전표 처리).
    *   **Chaining Flow**: 1차 처리 후 2차, 3차 프로세스 자동 트리거 (매출 발생 -> 정산 -> 세금계산서).

4.  **통합 거버넌스 (Centralized Governance)**
    *   **EAI Core DB**: 연계 대상 시스템, 표준 코드 매핑, 로그 정책을 중앙에서 통합 관리.
    *   **End-to-End 추적**: `TraceId`를 발급하여 생성부터 최종 타겟 반영까지 전 구간 이력 추적.

## 5. 기대 효과 (ROI)
- **속도 혁신**: 데이터 반영 시간 **10초 → 0.1초 미만** 단축 (Real-time).
- **안정성 확보**: 브로커 도입으로 타겟 시스템 장애 시에도 데이터 유실 방지 및 재처리 가능.
- **운영 효율화**: 직관적인 모니터링 대시보드를 통해 장애 인지 및 대응 시간 50% 단축.
- **확장 용이성**: 표준 아키텍처 기반으로 신규 시스템 연동 공수 70% 절감.

## 6. 향후 추진 로드맵
- **Phase 1 (금회 PoC 완료)**: 핵심 아키텍처 검증, HR/AC/AR 주요 시나리오 구현.
- **Phase 2 (고도화)**: 장애 자동 복구(Circuit Breaker), 대용량 분산 처리 클러스터링.
- **Phase 3 (전사 확산)**: 레거시 시스템 전면 전환 및 Hybrid Cloud 연계.