# EAI Multi-Database Architecture Design Document

> Version: 1.0.0 | Created: 2026-02-10 | Status: Draft

## 1. Overview
The database layer is divided into a centralized EAI core and decentralized domain databases to ensure modularity and scalability.

## 2. Architecture
### Database Map
- **EAI Core (`eai_core_db`)**: Central repository for EAI metadata and logs.
- **HR Domain (`hr_domain_db`)**: HR Master data.
- **AC Domain (`ac_domain_db`)**: Accounting Master data.
- **AR Domain (`ar_domain_db`)**: Accounts Receivable Master data.

## 3. Data Model
### EAI Core Database
- `EAI_MANAGEMENT_TARGET`: List of systems managed by EAI.
    - `SYSTEM_ID`, `SYSTEM_NAME`, `DB_CONN_INFO`, `STATUS`
- `EAI_AUDIT_LOG`: Transaction logs.
- `EAI_CODE_MAPPING`: Cross-reference table.

### Domain Master Tables (`_MST`)
- `SOURCE_HR_MST`: (EMP_NO, NAME, DEPT_CODE, SALARY, MOD_DATE)
- `SOURCE_AC_MST`: (AC_ID, AMOUNT, DESCRIPTION, MOD_DATE)
- `SOURCE_AR_MST`: (AR_ID, CUSTOMER, AMOUNT, MOD_DATE)

## 4. Producer Connection Strategy
The Producer will maintain multiple persistent connections:
1. `EaiCoreConnection`: For reading management targets and writing logs.
2. `DomainConnections[]`: Dynamic list of connections to domain DBs for `LISTEN/NOTIFY`.

## 5. Implementation Steps
### DB Initialization
```sql
CREATE DATABASE eai_core_db;
CREATE DATABASE hr_domain_db;
CREATE DATABASE ac_domain_db;
CREATE DATABASE ar_domain_db;
```
*(Note: Triggers must be recreated in each domain database.)*
