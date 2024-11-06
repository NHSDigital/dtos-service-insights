USE ServiceInsightsDB;
GO

IF NOT EXISTS
(
    SELECT *
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME = 'EPISODE'
)
BEGIN
    CREATE TABLE dbo.[EPISODE]
    (
      EPISODE_ID NVARCHAR (50) NOT NULL,
        CONSTRAINT PK_EPISODE
          PRIMARY KEY (EPISODE_ID),
      PARTICIPANT_ID                       NVARCHAR (50) NULL,
      SCREENING_ID                         NVARCHAR (50) NULL,
      NHS_NUMBER                           NVARCHAR (50) NULL,
      EPISODE_TYPE_ID                      NVARCHAR (50) NULL,
      EPISODE_OPEN_DATE                    NVARCHAR (50) NULL,
      APPOINTMENT_MADE_FLAG                NVARCHAR (50) NULL,
      FIRST_OFFERED_APPOINTMENT_DATE       NVARCHAR (50) NULL,
      ACTUAL_SCREENING_DATE                NVARCHAR (50) NULL,
      EARLY_RECALL_DATE                    NVARCHAR (50) NULL,
      CALL_RECALL_STATUS_AUTHORISED_BY     NVARCHAR (50) NULL,
      END_CODE_ID                          NVARCHAR (50) NULL,
      END_CODE_LAST_UPDATED                NVARCHAR (50) NULL,
      ORGANISATION_ID                      NVARCHAR (50) NULL,
      BATCH_ID                             NVARCHAR (50) NULL,
      RECORD_INSERT_DATETIME               NVARCHAR (50) NULL,
      RECORD_UPDATE_DATETIME               NVARCHAR (50) NULL,
);
END

IF NOT EXISTS
(
    SELECT *
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME = 'PARTICIPANT_SCREENING_PROFILE'
)
BEGIN
    CREATE TABLE PARTICIPANT_SCREENING_PROFILE
    (
      ID BIGINT IDENTITY(1,1) PRIMARY KEY,
      NHS_NUMBER                             NVARCHAR (50) NOT NULL,
      SCREENING_NAME                         VARCHAR(50) NULL,
      PRIMARY_CARE_PROVIDER                  VARCHAR(50) NULL,
      PREFERRED_LANGUAGE                     VARCHAR(50) NULL,
      REASON_FOR_REMOVAL                     VARCHAR(50) NULL,
      REASON_FOR_REMOVAL_DT                  VARCHAR(50) NULL,
      NEXT_TEST_DUE_DATE                     VARCHAR(50) NULL,
      NEXT_TEST_DUE_DATE_CALCULATION_METHOD  VARCHAR(50) NULL,
      PARTICIPANT_SCREENING_STATUS           VARCHAR(50) NULL,
      SCREENING_CEASED_REASON                VARCHAR(50) NULL,
      IS_HIGHER_RISK                         VARCHAR(10) NULL,
      IS_HIGHER_RISK_ACTIVE                  VARCHAR(10) NULL,
      HIGHER_RISK_NEXT_TEST_DUE_DATE         VARCHAR(50) NULL,
      HIGHER_RISK_REFERRAL_REASON_CODE       VARCHAR(50) NULL,
      HR_REASON_CODE_DESCRIPTION             VARCHAR(50) NULL,
      DATE_IRRADIATED                        VARCHAR(50) NULL,
      GENE_CODE                              VARCHAR(50) NULL,
      GENE_CODE_DESCRIPTION                  VARCHAR(50) NULL,
      RECORD_INSERT_DATETIME                 VARCHAR(50) NULL
    );
END

IF NOT EXISTS
(
    SELECT *
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME = 'PARTICIPANT_SCREENING_EPISODE'
)
BEGIN
    CREATE TABLE PARTICIPANT_SCREENING_EPISODE
    (
      ID BIGINT IDENTITY(1,1) PRIMARY KEY,
      EPISODE_ID                       NVARCHAR (50) NOT NULL,
      SCREENING_NAME                   VARCHAR(50) NULL,
      NHS_NUMBER                       VARCHAR(50) NULL,
      EPISODE_TYPE                     VARCHAR(50) NULL,
      EPISODE_TYPE_DESCRIPTION         VARCHAR(50) NULL,
      EPISODE_OPEN_DATE                VARCHAR(50) NULL,
      APPOINTMENT_MADE_FLAG            VARCHAR(10) NULL,
      FIRST_OFFERED_APPOINTMENT_DATE   VARCHAR(50) NULL,
      ACTUAL_SCREENING_DATE            VARCHAR(50) NULL,
      EARLY_RECALL_DATE                VARCHAR(50) NULL,
      CALL_RECALL_STATUS_AUTHORISED_BY VARCHAR(50) NULL,
      END_CODE                         VARCHAR(50) NULL,
      END_CODE_DESCRIPTION             VARCHAR(50) NULL,
      END_CODE_LAST_UPDATED            VARCHAR(50) NULL,
      ORGANISATION_CODE                VARCHAR(50) NULL,
      ORGANISATION_NAME                VARCHAR(50) NULL,
      BATCH_ID                         VARCHAR(50) NULL,
      RECORD_INSERT_DATETIME           VARCHAR(50) NULL
    );
END

IF NOT EXISTS
(
    SELECT *
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME = 'ORGANISATION_LKP'
)
BEGIN
    CREATE TABLE ORGANISATION_LKP
    (
      ORGANISATION_ID NVARCHAR (50) NOT NULL,
        CONSTRAINT PK_ORGANISATION_ID
          PRIMARY KEY (ORGANISATION_ID),
      SCREENING_NAME                  VARCHAR(50) NULL,
      ORGANISATION_CODE               VARCHAR(50) NULL,
      ORGANISATION_NAME               VARCHAR(50) NULL,
      ORGANISATION_TYPE               VARCHAR(50) NULL,
      IS_ACTIVE                       VARCHAR(50) NULL,
    );
END

INSERT INTO ORGANISATION_LKP
(ORGANISATION_ID, SCREENING_NAME, ORGANISATION_CODE, ORGANISATION_NAME, ORGANISATION_TYPE, IS_ACTIVE)
VALUES
('ORG001', 'Cancer Screening', 'C001', 'Health Screening Center A', 'Clinic', 'Yes'),
('ORG002', 'Diabetes Screening', 'D001', 'Diabetes Care Center', 'Specialty Clinic', 'Yes'),
('ORG003', 'Cardiac Screening', 'C002', 'Heart Health Hospital', 'Hospital', 'Yes'),
('ORG004', 'Lung Screening', 'L001', 'Lung Screening Facility', 'Clinic', 'No'),
('ORG005', 'Blood Pressure Screening', 'BP001', 'Primary Care Clinic B', 'Primary Care', 'Yes'),
('ORG006', 'Cancer Screening', 'C003', 'Oncology Associates', 'Specialty Clinic', 'Yes'),
('ORG007', 'Hypertension Screening', 'H001', 'Hypertension Specialty Clinic', 'Specialty Clinic', 'No'),
('ORG008', 'Cardiac Screening', 'C004', 'Cardiac Care Clinic', 'Clinic', 'Yes'),
('ORG009', 'Cancer Screening', 'C005', 'Cancer Center C', 'Hospital', 'Yes'),
('ORG010', 'General Health Screening', 'G001', 'Community Health Clinic', 'Primary Care', 'Yes');
