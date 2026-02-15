# EAI Multi-DB Trigger/Notification Design Document

> Version: 1.1.0 | Created: 2026-02-10 | Status: Updated

## 1. Overview
The system supports multiple Database Change Capture (CDC) strategies. Depending on the source DB (PostgreSQL, Oracle), it uses native notification mechanisms to achieve real-time data capture without polling.

## 2. Architecture
### System Diagram
[PostgreSQL] --(Listen/Notify)--> [Eai.Producer] --(XADD)--> [Redis]
[Oracle DB]  --(DCN/CQN)      --> [Eai.Producer] --(XADD)--> [Redis]

### DB-Specific Strategies
- **PostgreSQL**: Uses `LISTEN/NOTIFY` protocol. Lightweight, session-based.
- **Oracle**: Uses **Continuous Query Notification (CQN)**. Supports transaction-based grouping and reliable delivery to external servers via TCP callbacks.

## 3. Oracle DCN/CQN Details
### Registration
The Producer registers a query (e.g., `SELECT EMP_NO FROM SOURCE_HR_DATA`) with the Oracle server.
- **QOS_RELIABLE**: Ensures notification is delivered even after a crash.
- **ROWID tracking**: Oracle provides the specific ROWIDs of changed records.

### Network Configuration (Callback)
- DB Server must be able to reach the Producer's IP on a specific port (default is often random, but can be fixed).
- **Firewall Rule**: Inbound port on Producer server must be open for DB server.

## 4. Multi-DB Implementation Strategy
The Producer implements a provider pattern:
1. **PostgresProvider**: Implements `Npgsql` listen logic.
2. **OracleProvider**: Implements `OracleDependency` (DCN) logic.
