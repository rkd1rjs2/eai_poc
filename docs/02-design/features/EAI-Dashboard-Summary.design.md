# [Design] Dashboard Summary Page

## 1. Data Models

### 1.1 Summary Models (Eai.Shared)
```csharp
public class ProcessingSummary
{
    public string GroupName { get; set; } // SystemName or DataType
    public int SuccessCount { get; set; }
    public int WaitingCount { get; set; } // Ready + Processing
    public int FailCount { get; set; }
}
```

## 2. API / Repository Interface

### 2.1 IAuditRepository Update
```csharp
public interface IAuditRepository
{
    // ... existing ...
    Task<IEnumerable<ProcessingSummary>> GetSystemSummaryAsync(DateTime date);
    Task<IEnumerable<ProcessingSummary>> GetBusinessSummaryAsync(DateTime date);
}
```

## 3. Database Queries

### 3.1 System Summary Query
```sql
SELECT 
    SourceSystem as GroupName,
    COUNT(*) FILTER (WHERE Status = 'SUCCESS') as SuccessCount,
    COUNT(*) FILTER (WHERE Status IN ('READY', 'PROCESSING')) as WaitingCount,
    COUNT(*) FILTER (WHERE Status = 'FAIL') as FailCount
FROM EAI_AUDIT_LOG
WHERE CreatedAt >= @Date AND CreatedAt < @NextDate
GROUP BY SourceSystem;
```

### 3.2 Business Summary Query
```sql
SELECT 
    DataType as GroupName,
    COUNT(*) FILTER (WHERE Status = 'SUCCESS') as SuccessCount,
    COUNT(*) FILTER (WHERE Status IN ('READY', 'PROCESSING')) as WaitingCount,
    COUNT(*) FILTER (WHERE Status = 'FAIL') as FailCount
FROM EAI_AUDIT_LOG
WHERE CreatedAt >= @Date AND CreatedAt < @NextDate
GROUP BY DataType;
```

## 4. UI Components

### 4.1 Layout
- **Summary Cards**:
  - Total Today
  - Success
  - Waiting (READY/PROCESSING)
  - Failure
- **Split View**:
  - Left: "Database Status" table.
  - Right: "Business Status" table.

### 4.2 Auto-refresh
- `OnInitialized`에서 타이머를 사용하여 주기적으로(예: 5초) 통계 데이터를 다시 로드.
