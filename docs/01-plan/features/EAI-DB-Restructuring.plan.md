# EAI Database Restructuring Plan Document

> Version: 1.0.0 | Created: 2026-02-10 | Status: Draft

## 1. Executive Summary
Refactor the single-database architecture into a multi-database structure to simulate a real-world enterprise environment where EAI core and source systems are isolated.

## 2. Goals and Objectives
- Isolate EAI core infrastructure (Logging, Mapping, Management).
- Separate source system data into domain-specific databases (HR, AC, AR).
- Implement a Management Target table for EAI governance.
- Update Producer to handle multi-database listening.

## 3. Scope
### In Scope
- **Databases**: Create `eai_core_db`, `hr_domain_db`, `ac_domain_db`, `ar_domain_db`.
- **Tables**:
    - `eai_core_db`: `EAI_AUDIT_LOG`, `EAI_CODE_MAPPING`, `EAI_MANAGEMENT_TARGET`.
    - `hr_domain_db`: `SOURCE_HR_MST`.
    - `ac_domain_db`: `SOURCE_AC_MST`.
    - `ar_domain_db`: `SOURCE_AR_MST`.
- **Source Code**: Update all connection strings and Producer's connection management.

### Out of Scope
- Migrating existing test data (we will re-insert).
- Changing Transformer's core business logic (only DB connections).

## 4. Success Criteria
| Criterion | Metric | Target |
|-----------|--------|--------|
| Database Isolation | Number of databases | 4 distinct databases |
| System Functionality | End-to-end test | HR/AC/AR flows work correctly across DBs |

## 5. Timeline
1. Database & Table Creation
2. Connection String & Configuration Update
3. Producer Refactoring (Multi-connection Listener)
4. Dashboard & Transformer Update
