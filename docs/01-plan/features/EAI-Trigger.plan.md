# EAI Trigger-based Data Capture Plan Document

> Version: 1.0.0 | Created: 2026-02-10 | Status: Draft

## 1. Executive Summary
Transition from a polling-based data capture mechanism to a real-time, event-driven approach using PostgreSQL Triggers and `LISTEN/NOTIFY`.

## 2. Goals and Objectives
- Achieve real-time data processing (reduce latency from 10s to near-zero).
- Reduce unnecessary database load caused by periodic polling.
- Improve system responsiveness and efficiency.

## 3. Scope
### In Scope
- Database: Create a Trigger function and a Trigger on the `SOURCE_HR_DATA` table.
- Producer: Modify `Eai.Producer` to use `Npgsql`'s `LISTEN` feature to wait for database events.
- Message Flow: Ensure the existing Redis stream publishing logic remains intact.

### Out of Scope
- Changing the Transformer or Dashboard logic.
- Implementing a full Outbox pattern (using a separate event table).

## 4. Success Criteria
| Criterion | Metric | Target |
|-----------|--------|--------|
| Processing Latency | Time from DB Insert to Redis Publish | < 1 second |
| System Resource Usage | CPU/IO on idle | Lower than polling method |

## 5. Timeline
- Phase 1: DB Schema update (Triggers)
- Phase 2: Producer code modification
- Phase 3: Integration testing

## 6. Risks
| Risk | Impact | Mitigation |
|------|--------|------------|
| Persistent Connection Loss | Missed events | Implement a fallback polling or reconnection logic |
| High Frequency Events | Connection overhead | Ensure trigger function is lightweight |
