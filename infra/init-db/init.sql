-- EAI Audit Log Table
CREATE TABLE EAI_AUDIT_LOG (
    TraceId VARCHAR(50) PRIMARY KEY,
    IdempotencyKey VARCHAR(100),
    SourceSystem VARCHAR(50),
    TargetSystem VARCHAR(50),
    DataType VARCHAR(50),
    Status VARCHAR(20),
    ErrorMessage TEXT,
    CreatedAt TIMESTAMP WITHOUT TIME ZONE,
    UpdatedAt TIMESTAMP WITHOUT TIME ZONE
);

-- Simulation Source Table (인사 시스템 예시)
CREATE TABLE SOURCE_HR_DATA (
    EMP_NO VARCHAR(20) PRIMARY KEY,
    NAME VARCHAR(50),
    DEPT_CODE VARCHAR(10),
    SALARY NUMERIC(15, 2),
    MOD_DATE TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Simulation Target System Code Mapping (X-Ref 예시)
CREATE TABLE EAI_CODE_MAPPING (
    SourceSystem VARCHAR(50),
    SourceCode VARCHAR(50),
    TargetSystem VARCHAR(50),
    TargetCode VARCHAR(50),
    PRIMARY KEY (SourceSystem, SourceCode, TargetSystem)
);

-- 초기 매핑 데이터 삽입
INSERT INTO EAI_CODE_MAPPING (SourceSystem, SourceCode, TargetSystem, TargetCode)
VALUES ('HR', 'DEPT01', 'FINANCE', 'ACC_01');
