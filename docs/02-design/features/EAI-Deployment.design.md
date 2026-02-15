# EAI CI/CD 배포 자동화 설계서 (GitLab + Jenkins + Docker)

> Version: 1.0.0 | Created: 2026-02-10 | Status: Draft

## 1. 개요
본 문서는 GitLab 소스 제어, Jenkins 빌드 자동화, Docker 컨테이너 기술을 결합하여 EAI 플랫폼의 안정적이고 신속한 배포를 위한 CI/CD 파이프라인 설계를 정의한다.

## 2. 배포 아키텍처 (Deployment Architecture)
쿠버네티스 도입 전 단계에서 가장 효율적인 **"Image-based CD"** 방식을 채택한다.

```text
[개발자 PC] --(Push)--> [GitLab Repository]
                            |
                            v (Webhook Trigger)
[Jenkins Server] <----------+
  - Step 1: 소스 체크아웃
  - Step 2: Docker Multi-stage 빌드 (SDK 설치 불필요)
  - Step 3: 도커 이미지 생성 및 레지스트리 Push
                            |
                            v (SSH / Remote Deploy)
[운영 서버 (Production)]
  - Docker Compose 기반 서비스 갱신 (Zero-downtime 지향)
```

## 3. 파이프라인 상세 설계 (CI/CD Pipeline)

### 3.1 CI (Continuous Integration): Jenkins 역할
1.  **빌드 자동화**: 컨테이너 내부에서 `.NET build`를 수행하여 호스트 서버의 환경 의존성을 제거한다.
2.  **정적 분석**: 배포 전 유닛 테스트 및 코드 품질 검사를 병행한다.
3.  **이미지 태깅**: `v1.0.1`, `build-45`와 같이 버전을 명시하여 롤백 가능성을 확보한다.

### 3.2 CD (Continuous Deployment): 배포 전략
1.  **Docker Compose 활용**: 앱(Producer, Transformer, Dashboard)과 인프라(DB, Redis)를 하나의 서비스 그룹으로 관리한다.
2.  **원격 배포**: Jenkins가 운영 서버에 SSH로 접속하여 아래 명령을 수행하도록 자동화한다.
    ```bash
    docker-compose pull  # 최신 이미지 수신
    docker-compose up -d # 서비스 무중단 갱신
    ```

## 4. Docker 전략: Multi-stage Build
애플리케이션의 보안과 크기 최적화를 위해 빌드 환경과 실행 환경을 분리한다.

- **Build Stage**: `.NET 10 SDK` 이미지를 사용하여 소스 컴파일 및 빌드 결과물 생성.
- **Run Stage**: 가벼운 `ASP.NET Core Runtime` 이미지만 사용하여 최종 이미지 크기 최소화 및 보안 강화.

## 5. 인프라 및 보안 요구사항
- **네트워크**: Jenkins 서버와 운영 서버 간의 SSH(22번) 통신 허용.
- **방화벽**: 
    - DB/Redis 포트는 내부망에서만 접근 허용.
    - Dashboard(5160) 포트는 필요 시 외부 오픈.
- **비밀번호 관리**: DB 접속 정보 등 민감 정보는 Jenkins Credentials 또는 환경 변수(`.env`)로 격리 관리.

## 6. 기대 효과
- **배포 속도**: 수동 배포 대비 80% 이상의 시간 단축.
- **안정성**: "내 컴퓨터에선 되는데 서버에선 안 돼요" 문제(환경 차이) 원천 차단.
- **롤백**: 장애 발생 시 단 1분 만에 이전 정상 버전으로 복구 가능.
