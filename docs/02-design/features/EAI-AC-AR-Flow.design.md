# EAI AC/AR Flow Design Document

> Version: 1.0.0 | Created: 2026-02-10 | Status: Draft

## 1. Overview
The system is extended to handle specialized workflows for AC (Accounting) and AR (Accounts Receivable) domains.

## 2. Architecture
### Flow: AC (Accounting)
[SOURCE_AC_DATA] -> (Trigger) -> [Producer] -> (Redis) -> [Transformer] -> **(HTTP POST Webhook)**

### Flow: AR (Accounts Receivable)
[SOURCE_AR_DATA] -> (Trigger) -> [Producer] -> (Redis) -> [Transformer] -> **(Publish to NEXT_STREAM)**

## 3. Data Model
### New Tables
- `SOURCE_AC_DATA`: (AC_ID, AMOUNT, DESCRIPTION, ...)
- `SOURCE_AR_DATA`: (AR_ID, CUSTOMER, AMOUNT, ...)

## 4. Implementation Details
### Transformer Logic
- **IF DataType == 'AC_INFO'**: After `UpdateStatusAsync(SUCCESS)`, call `HttpClient.PostAsync("http://mock-target/api/ac", ...)`.
- **IF DataType == 'AR_INFO'**: After `UpdateStatusAsync(SUCCESS)`, create a new `EaiMessage` for the "Next Step" (e.g., Debt Collection or Invoice Generation) and publish it back to Redis.

## 5. Test Plan
| Test Case | Expected Result |
|-----------|-----------------|
| AC Insert | Transformer logs "Calling Webhook..." after success |
| AR Insert | Transformer logs "Triggering next hop..." and a new message appears in the audit log |
