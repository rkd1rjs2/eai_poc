-- 1. 데이터베이스 생성 (이미 존재하지 않는 경우에만 실행되도록 처리하거나, 초기 빌드 시 실행)
-- 주의: psql에서 단일 스크립트로 여러 DB를 생성하는 것은 제한이 있을 수 있어 쉘 스크립트 방식으로 처리하는 것이 정석입니다.
-- 여기서는 각 도메인 테이블 생성을 위한 기본 틀을 정의합니다.

-- [eai_core_db]용 테이블
\c eai_core_db;

CREATE TABLE EAI_MANAGEMENT_TARGET (
    SYSTEM_ID VARCHAR(50) PRIMARY KEY,
    SYSTEM_NAME VARCHAR(100),
    DATABASE_TYPE VARCHAR(50),
    STATUS VARCHAR(20) DEFAULT 'ACTIVE',
    DESCRIPTION TEXT
);

INSERT INTO EAI_MANAGEMENT_TARGET (SYSTEM_ID, SYSTEM_NAME, DATABASE_TYPE, DESCRIPTION)
VALUES 
('HR', 'Human Resources System', 'PostgreSQL', 'Employee master data management'),
('AC', 'Accounting System', 'PostgreSQL', 'Accounting and Webhook integration'),
('AR', 'Accounts Receivable System', 'PostgreSQL', 'AR and multi-hop orchestration');

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

CREATE TABLE EAI_CODE_MAPPING (
    SourceSystem VARCHAR(50),
    SourceCode VARCHAR(50),
    TargetSystem VARCHAR(50),
    TargetCode VARCHAR(50),
    PRIMARY KEY (SourceSystem, SourceCode, TargetSystem)
);

INSERT INTO EAI_CODE_MAPPING (SourceSystem, SourceCode, TargetSystem, TargetCode)
VALUES ('HR', 'DEPT01', 'FINANCE', 'ACC_01');

-- [hr_domain_db]용 테이블
\c hr_domain_db;

CREATE TABLE SOURCE_HR_MST (
    EMP_NO VARCHAR(20) PRIMARY KEY,
    NAME VARCHAR(50),
    DEPT_CODE VARCHAR(10),
    SALARY NUMERIC(15, 2),
    MOD_DATE TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE OR REPLACE FUNCTION notify_hr_mst_change() RETURNS TRIGGER AS $$
BEGIN
    PERFORM pg_notify('hr_mst_changed', json_build_object('action', TG_OP, 'id', NEW.EMP_NO)::text);
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER hr_mst_change_trg AFTER INSERT OR UPDATE ON SOURCE_HR_MST FOR EACH ROW EXECUTE FUNCTION notify_hr_mst_change();

-- [ac_domain_db]용 테이블
\c ac_domain_db;

CREATE TABLE SOURCE_AC_MST (
    AC_ID VARCHAR(20) PRIMARY KEY,
    AMOUNT NUMERIC(15, 2),
    DESCRIPTION TEXT,
    MOD_DATE TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE OR REPLACE FUNCTION notify_ac_mst_change() RETURNS TRIGGER AS $$
BEGIN
    PERFORM pg_notify('ac_mst_changed', json_build_object('action', TG_OP, 'id', NEW.AC_ID)::text);
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER ac_mst_change_trg AFTER INSERT OR UPDATE ON SOURCE_AC_MST FOR EACH ROW EXECUTE FUNCTION notify_ac_mst_change();

-- [ar_domain_db]용 테이블
\c ar_domain_db;

CREATE TABLE SOURCE_AR_MST (
    AR_ID VARCHAR(20) PRIMARY KEY,
    CUSTOMER VARCHAR(100),
    AMOUNT NUMERIC(15, 2),
    MOD_DATE TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE OR REPLACE FUNCTION notify_ar_mst_change() RETURNS TRIGGER AS $$
BEGIN
    PERFORM pg_notify('ar_mst_changed', json_build_object('action', TG_OP, 'id', NEW.AR_ID)::text);
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER ar_mst_change_trg AFTER INSERT OR UPDATE ON SOURCE_AR_MST FOR EACH ROW EXECUTE FUNCTION notify_ar_mst_change();
