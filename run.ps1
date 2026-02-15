# EAI PoC 통합 실행 스크립트 (All-in-Docker)

Write-Host ">>> [1/2] Starting All EAI Services (5 Containers) via Docker Compose..." -ForegroundColor Cyan
# --build 플래그를 추가하여 코드 변경사항이 즉시 이미지에 반영되도록 합니다.
docker-compose up -d --build

if ($LASTEXITCODE -ne 0) {
    Write-Host "!!! Docker Compose Failed. Please check if Docker Desktop is running." -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host ">>> [2/2] Checking Container Status..." -ForegroundColor Cyan
docker ps --filter "name=eai-"

Write-Host ""
Write-Host ">>> All 5 services are starting up in Docker:" -ForegroundColor Green
Write-Host "  1. eai-db          (PostgreSQL: 5432)"
Write-Host "  2. eai-redis       (Redis: 6380)"
Write-Host "  3. eai-producer    (Producer)"
Write-Host "  4. eai-transformer (Transformer)"
Write-Host "  5. eai-dashboard   (Dashboard: 5160)"
Write-Host ""
Write-Host ">>> Dashboard URL: http://localhost:5160" -ForegroundColor Yellow
Write-Host ">>> To see logs: docker-compose logs -f" -ForegroundColor Gray
