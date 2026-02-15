# EAI AC/AR Flow Extension Plan Document

> Version: 1.0.0 | Created: 2026-02-10 | Status: Draft

## 1. Executive Summary
Expand the EAI PoC to include Accounting (AC) and Accounts Receivable (AR) flows with specific post-processing requirements: URL notification for AC and multi-hop triggering for AR.

## 2. Goals and Objectives
- Implement **AC Flow**: Real-time capture -> Transformation -> Success -> **Webhook Call**.
- Implement **AR Flow**: Real-time capture -> Transformation -> Success -> **Trigger Next Step (Producer/Chain)**.
- Demonstrate multi-protocol and multi-step orchestration in EAI.

## 3. Scope
### In Scope
- **DB**: Create `SOURCE_AC_DATA` and `SOURCE_AR_DATA` tables with triggers.
- **Producer**: Expand to handle `AC` and `AR` database events.
- **Transformer**: 
    - Implement mock Webhook calling for AC.
    - Implement subsequent message triggering for AR (Chained flow).
- **Dashboard**: Visualize different flow types.

### Out of Scope
- Real external system integration (using Mock/Logging).
- Complex error recovery for Webhooks.

## 4. Success Criteria
| Criterion | Metric | Target |
|-----------|--------|--------|
| AC Webhook | Log entry for URL call | Verified in Transformer logs |
| AR Chain | Subsequent message creation | Verified in Audit Log (New TraceId or status) |

## 5. Timeline
1. DB Schema & Trigger Setup
2. Producer Expansion
3. Transformer Logic Implementation (Webhook & Chaining)
4. Verification & Dashboard Update
