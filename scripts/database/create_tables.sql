USE ServiceInsightsDB;
GO

/*==============================================================*/
/* Table: EPISODE                                               */
/*==============================================================*/

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
      EPISODE_ID                          BIGINT               not null,
      EPISODE_ID_SYSTEM                   BIGINT               null,
      SCREENING_ID                        BIGINT               not null,
      NHS_NUMBER                          BIGINT               not null,
      EPISODE_TYPE_ID                     BIGINT               null,
      EPISODE_OPEN_DATE                   DATE                 null,
      APPOINTMENT_MADE_FLAG               VARCHAR(10)          null,
      FIRST_OFFERED_APPOINTMENT_DATE      DATE                 null,
      ACTUAL_SCREENING_DATE               DATE                 null,
      EARLY_RECALL_DATE                   DATE                 null,
      CALL_RECALL_STATUS_AUTHORISED_BY    VARCHAR(200)         null,
      END_CODE_ID                         BIGINT               null,
      END_CODE_LAST_UPDATED               DATETIME             null,
      ORGANISATION_ID                     BIGINT               null,
      BATCH_ID                            VARCHAR(100)         null,
      RECORD_INSERT_DATETIME              DATETIME             null,
      RECORD_UPDATE_DATETIME              DATETIME             null,
      constraint PK_EPISODE primary key (EPISODE_ID)
);
END


/*==============================================================*/
/* Table: END_CODE_LKP                                          */
/*==============================================================*/

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
    add constraint FK_EPISODE_STATUS_OF_END_CODE foreign key (END_CODE_ID)
      references END_CODE_LKP (END_CODE_ID)


/*==============================================================*/
/* Table: EPISODE_TYPE_LKP                                      */
/*==============================================================*/

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
    add constraint FK_EPISODE_TYPE_OF_E_EPISODE_ foreign key (EPISODE_TYPE_ID)
      references EPISODE_TYPE_LKP (EPISODE_TYPE_ID)


/*==============================================================*/
/* Table: PARTICIPANT_SCREENING_PROFILE                         */
/*==============================================================*/

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


/*==============================================================*/
/* Table: PARTICIPANT_SCREENING_EPISODE                         */
/*==============================================================*/

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
