USE ServiceInsightsDB;
GO

-- Table: EPISODE

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
      EPISODE_ID                        BIGINT               not null,
      EPISODE_ID_SYSTEM                 BIGINT               null,
      SCREENING_ID                      BIGINT               not null,
      NHS_NUMBER                        BIGINT               not null,
      EPISODE_TYPE_ID                   BIGINT               null,
      EPISODE_OPEN_DATE                 DATE                 null,
      APPOINTMENT_MADE_FLAG             SMALLINT             null,
      FIRST_OFFERED_APPOINTMENT_DATE    DATE                 null,
      ACTUAL_SCREENING_DATE             DATE                 null,
      EARLY_RECALL_DATE                 DATE                 null,
      CALL_RECALL_STATUS_AUTHORISED_BY  VARCHAR(200)         null,
      END_CODE_ID                       BIGINT               null,
      END_CODE_LAST_UPDATED             DATETIME             null,
      REASON_CLOSED_CODE_ID             BIGINT               null,
      FINAL_ACTION_CODE_ID              BIGINT               null,
      END_POINT                         VARCHAR(200)         null,
      ORGANISATION_ID                   BIGINT               null,
      BATCH_ID                          VARCHAR(100)         null,
      RECORD_INSERT_DATETIME            DATETIME             null,
      RECORD_UPDATE_DATETIME            DATETIME             null,
      constraint PK_EPISODE             primary key (EPISODE_ID)
    );
END


-- Table: END_CODE_LKP

IF NOT EXISTS
(
    SELECT *
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME = 'END_CODE_LKP'
)
BEGIN
    CREATE TABLE END_CODE_LKP
    (
      END_CODE_ID                BIGINT               not null,
      LEGACY_END_CODE            VARCHAR(10)          null,
      END_CODE                   VARCHAR(50)          null,
      END_CODE_DESCRIPTION       VARCHAR(300)         null,
      constraint PK_END_CODE_LKP primary key (END_CODE_ID)
    );
END


alter table EPISODE
    add constraint FK_EPISODE_END_CODE_LKP foreign key (END_CODE_ID)
      references END_CODE_LKP (END_CODE_ID)


-- Table: EPISODE_TYPE_LKP

IF NOT EXISTS
(
    SELECT *
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME = 'EPISODE_TYPE_LKP'
)
BEGIN
    CREATE TABLE EPISODE_TYPE_LKP
    (
      EPISODE_TYPE_ID                BIGINT               not null,
      EPISODE_TYPE                   VARCHAR(10)          null,
      EPISODE_DESCRIPTION            VARCHAR(300)         null,
      constraint PK_EPISODE_TYPE_LKP primary key (EPISODE_TYPE_ID)
    );
END


alter table EPISODE
    add constraint FK_EPISODE_EPISODE_TYPE_LKP foreign key (EPISODE_TYPE_ID)
      references EPISODE_TYPE_LKP (EPISODE_TYPE_ID)


-- Table: FINAL_ACTION_CODE_LKP

IF NOT EXISTS
(
    SELECT *
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME = 'FINAL_ACTION_CODE_LKP'
)
BEGIN
    CREATE TABLE FINAL_ACTION_CODE_LKP
    (
      FINAL_ACTION_CODE_ID BIGINT               not null,
      FINAL_ACTION_CODE    VARCHAR(50)          not null,
      FINAL_ACTION_CODE_DESCRIPTION VARCHAR(300)         null,
      constraint PK_FINAL_ACTION_CODE_LKP primary key (FINAL_ACTION_CODE_ID)
    );
END


alter table EPISODE
    add constraint FK_EPISODE_FINAL_ACTION_CODE_LKP foreign key (FINAL_ACTION_CODE_ID)
      references FINAL_ACTION_CODE_LKP (FINAL_ACTION_CODE_ID)


-- Table: REASON_CLOSED_CODE_LKP

IF NOT EXISTS
(
    SELECT *
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME = 'REASON_CLOSED_CODE_LKP'
)
BEGIN
    CREATE TABLE REASON_CLOSED_CODE_LKP
    (
      REASON_CLOSED_CODE_ID BIGINT              not null,
      REASON_CLOSED_CODE   VARCHAR(50)          not null,
      REASON_CLOSED_CODE_DESCRIPTION VARCHAR(300)         null,
      constraint PK_REASON_CLOSED_CODE_LKP primary key (REASON_CLOSED_CODE_ID)
    );
END


alter table EPISODE
    add constraint FK_EPISODE_REASON_CLOSED_CODE_LKP foreign key (REASON_CLOSED_CODE_ID)
      references REASON_CLOSED_CODE_LKP (REASON_CLOSED_CODE_ID)


-- Table: PARTICIPANT_SCREENING_PROFILE

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
      NHS_NUMBER                          BIGINT NOT NULL,
      SCREENING_NAME                      VARCHAR(200) NULL,
      PRIMARY_CARE_PROVIDER               VARCHAR(50) NULL,
      PREFERRED_LANGUAGE                  VARCHAR(50) NULL,
      REASON_FOR_REMOVAL                  VARCHAR(50) NULL,
      REASON_FOR_REMOVAL_DT               DATE NULL,
      NEXT_TEST_DUE_DATE                  DATE NULL,
      NEXT_TEST_DUE_DATE_CALC_METHOD      VARCHAR(100) NULL,
      PARTICIPANT_SCREENING_STATUS        VARCHAR(100) NULL,
      SCREENING_CEASED_REASON             VARCHAR(100) NULL,
      IS_HIGHER_RISK                      SMALLINT NULL,
      IS_HIGHER_RISK_ACTIVE               SMALLINT NULL,
      HIGHER_RISK_NEXT_TEST_DUE_DATE      DATE NULL,
      HIGHER_RISK_REFERRAL_REASON_CODE    VARCHAR(100) NULL,
      HR_REASON_CODE_DESCRIPTION          VARCHAR(200) NULL,
      DATE_IRRADIATED                     DATE NULL,
      GENE_CODE                           VARCHAR(100) NULL,
      GENE_CODE_DESCRIPTION               VARCHAR(200) NULL,
      RECORD_INSERT_DATETIME              DATETIME NULL
    );
END


-- Table: PARTICIPANT_SCREENING_EPISODE

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
      EPISODE_ID                        BIGINT NOT NULL,
      NHS_NUMBER                        BIGINT NOT NULL,
      SCREENING_NAME                    VARCHAR(200) NULL,
      EPISODE_TYPE                      VARCHAR(50) NULL,
      EPISODE_TYPE_DESCRIPTION          VARCHAR(300) NULL,
      EPISODE_OPEN_DATE                 DATE NULL,
      APPOINTMENT_MADE_FLAG             SMALLINT NULL,
      FIRST_OFFERED_APPOINTMENT_DATE    DATE NULL,
      ACTUAL_SCREENING_DATE             DATE NULL,
      EARLY_RECALL_DATE                 DATE NULL,
      CALL_RECALL_STATUS_AUTHORISED_BY  VARCHAR(200) NULL,
      END_CODE                          VARCHAR(50) NULL,
      END_CODE_DESCRIPTION              VARCHAR(300) NULL,
      END_CODE_LAST_UPDATED             DATETIME NULL,
      REASON_CLOSED_CODE                VARCHAR(50) NULL,
      REASON_CLOSED_CODE_DESCRIPTION    VARCHAR(300) NULL,
      FINAL_ACTION_CODE                 VARCHAR(50) NULL,
      FINAL_ACTION_CODE_DESCRIPTION     VARCHAR(300) NULL,
      END_POINT                         VARCHAR(200) NULL,
      ORGANISATION_CODE                 VARCHAR(50) NULL,
      ORGANISATION_NAME                 VARCHAR(300) NULL,
      BATCH_ID                          VARCHAR(100) NULL,
      RECORD_INSERT_DATETIME            DATETIME NULL
    );
END


-- Table: ORGANISATION_LKP

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
      ORGANISATION_ID BIGINT NOT NULL,
        CONSTRAINT PK_ORGANISATION_ID
          PRIMARY KEY (ORGANISATION_ID),
      SCREENING_NAME                  VARCHAR(50) NULL,
      ORGANISATION_CODE               VARCHAR(50) NULL,
      ORGANISATION_NAME               VARCHAR(50) NULL,
      ORGANISATION_TYPE               VARCHAR(50) NULL,
      IS_ACTIVE                       VARCHAR(50) NULL,
    );
END
