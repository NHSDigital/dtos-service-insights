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
    AND TABLE_NAME = 'ANALYTICS'
)
BEGIN
    CREATE TABLE ANALYTICS
    (
      ID BIGINT NOT NULL IDENTITY(1, 1),
        CONSTRAINT PK_ID
          PRIMARY KEY (ID),
      EPISODE_ID                        NVARCHAR (50) NULL,
      EPISODE_TYPE                      NVARCHAR (50) NULL,
      EPISODE_DATE                      NVARCHAR (50) NULL,
      APPOINTMENT_MADE                  NVARCHAR (50) NULL,
      DATE_OF_FOA                       NVARCHAR (50) NULL,
      DATE_OF_AS                        NVARCHAR (50) NULL,
      EARLY_RECALL_DATE                 NVARCHAR (50) NULL,
      CALL_RECALL_STATUS_AUTHORISED_BY  NVARCHAR (50) NULL,
      END_CODE                          NVARCHAR (50) NULL,
      END_CODE_LAST_UPDATED             NVARCHAR (50) NULL,
      BSO_ORGANISATION_CODE             NVARCHAR (50) NULL,
      BSO_BATCH_ID                      NVARCHAR (50) NULL,
      NHS_NUMBER                        NVARCHAR (50) NULL,
      GP_PRACTICE_ID                    NVARCHAR (50) NULL,
      BSO_ORGANISATION_ID               NVARCHAR (50) NULL,
      NEXT_TEST_DUE_DATE                NVARCHAR (50) NULL,
      SUBJECT_STATUS_CODE               NVARCHAR (50) NULL,
      LATEST_INVITATION_DATE            NVARCHAR (50) NULL,
      REMOVAL_REASON                    NVARCHAR (50) NULL,
      REMOVAL_DATE                      NVARCHAR (50) NULL,
      CEASED_REASON                     NVARCHAR (50) NULL,
      REASON_FOR_CEASED_CODE            NVARCHAR (50) NULL,
      REASON_DEDUCTED                   NVARCHAR (50) NULL,
      IS_HIGHER_RISK                    NVARCHAR (50) NULL,
      HIGHER_RISK_NEXT_TEST_DUE_DATE    NVARCHAR (50) NULL,
      HIGHER_RISK_REFERRAL_REASON_CODE  NVARCHAR (50) NULL,
      DATE_IRRADIATED                   NVARCHAR (50) NULL,
      IS_HIGHER_RISK_ACTIVE             NVARCHAR (50) NULL,
      GENE_CODE                         NVARCHAR (50) NULL,
      NTDD_CALCULATION_METHOD           NVARCHAR (50) NULL,
      PREFERRED_LANGUAGE                NVARCHAR (50) NULL,
);
END
