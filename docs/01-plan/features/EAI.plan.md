# EAI System Plan

## 1. Feature Overview
본 시스템은 분산된 다수의 엔터프라이즈 서비스(인사, 재무, 영업 등) 간의 데이터를 실시간으로 중계하는 중앙 집중형 허브(Hub-and-Spoke) 역할을 수행합니다.

이벤트 기반 메시징: 소스 시스템에서 발생한 트랜잭션을 실시간으로 포착하여 전파합니다.

데이터 변환 및 매핑: 타겟 시스템(BIMATRIX 등)의 요구 규격에 맞춰 데이터를 실시간으로 가공합니다.

배달 보장 및 추적: 메시지 수신 확인(ACK) 및 처리 로그를 통해 데이터 전송의 정합성을 보장합니다.

## 2. Goals and Objectives
Batch-to-Realtime 전환: 기존의 정기적 배치 처리 방식을 이벤트 기반 실시간 처리로 전환하여 데이터 시차를 최소화합니다.

제로-침습 연계: 오라클 10g/11g 등 레거시 시스템의 소스 코드 수정 없이 트리거나 로그 분석(CDC)을 통해 연계를 실시간화합니다.

운영 가시성 확보: 전체 서비스 간의 데이터 흐름과 지연 현황(Lag)을 실시간 대시보드로 시각화합니다.

## 3. Scope
포함 범위:

인사, 재무, 영업, POS, 회원, 회원 모바일, 데이터플랫폼 간 데이터 연계.

Redis Streams 기반의 메시지 브로커 구축 및 컨슈머 그룹 관리.

C# Worker Service 기반의 변환 처리기(Transformer) 및 Webhook 연동.

Blazor 기반의 실시간 모니터링 대시보드.

제외 범위:

각 서비스 내부의 비즈니스 로직 재구축.

레거시 DB(Oracle)의 스키마 변경 및 물리적 이전.

## 4. Technical Stack
Language/Framework: C# (.NET 10), ASP.NET Core Blazor.

Message Broker: Redis Streams (StackExchange.Redis).

ORM/DB Access: Dapper (High-performance SQL Mapping).

Legacy Connectivity: Oracle Managed Data Access (Oracle 10g/11g 호환 및 19g 사용).

Infrastructure: Docker (Redis 컨테이너), Windows/Linux 하이브리드 환경.

## 5. Interconnected Services
인사 시스템 : 인사 정보, 급여 정보
재무 시스템	: 전표 데이터	Oracle → Redis → 타겟 시스템
영업 시스템	: 실시간 매출, 수주, 정산 정보	영업 → Redis → 재무/데이터플랫폼
POS	(검토) : 매장 결제, 재고 변동	POS → Redis → 영업/재무
회원/모바일 : 고객 프로필, 앱 활동 로그	모바일 Webhook → Redis → 회원
데이터플랫폼: 전사 통합 분석 데이터	Redis → 데이터플랫폼 (Bulk Ingestion)

## 6. High-Level Requirements
기능 요구사항
트랜잭션 벌크 처리: 대량 데이터 유입 시 트랜잭션 단위로 묶어 Redis와 타겟 DB에 일괄 반영합니다.

상태 콜백(Webhook): 전송 완료 후 지정된 URL을 호출하여 타겟 시스템에 처리 완료를 실시간 통보합니다.

재처리 메커니즘: 실패한 메시지는 자동으로 재시도하며, 최종 실패 시 운영자 알림을 생성합니다.

비기능 요구사항
확장성: 컨슈머 그룹(Consumer Group)을 통해 처리 서버를 수평적으로 확장 가능해야 합니다.

영속성: Redis AOF/RDB 설정을 통해 시스템 장애 시에도 스트림 데이터를 보존합니다.

가용성: 데이터 유실 방지를 위해 ACK 프로세스를 엄격히 준수합니다.

## 7. Future Considerations
Dead Letter Queue (DLQ) 관리: 실패 메시지만을 위한 별도의 관리 화면 및 수동 재처리 기능.

AI 기반 이상 탐지: 데이터 전송 패턴을 분석하여 평상시와 다른 대량 데이터 유출이나 전송 지연을 감지(Agentic AI 활용 가능성).

Sovereign AI 연동: 기업 내부 데이터를 학습시킨 로컬 AI 모델을 연동하여 데이터 정제 품질 고도화.
