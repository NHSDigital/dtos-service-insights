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
    CREATE TABLE EPISODE
    (
      EPISODE_ID BIGINT NOT NULL,
        CONSTRAINT PK_EPISODE
          PRIMARY KEY (EPISODE_ID),
      EPISODE_TYPE                     NVARCHAR (50) NULL,
      BSO_ORGANISATION_CODE            NVARCHAR (50) NULL,
      BSO_BATCH_ID                     NVARCHAR (50) NULL,
      EPISODE_DATE                     NVARCHAR (50) NULL,
      END_CODE                         NVARCHAR (50) NULL,
      DATE_OF_FOA                      NVARCHAR (50) NULL,
      DATE_OF_AS                       NVARCHAR (50) NULL,
      APPOINTMENT_MADE                 NVARCHAR (50) NULL,
      CALL_RECALL_STATUS_AUTHORISED_BY NVARCHAR (50) NULL,
      EARLY_RECALL_DATE                NVARCHAR (50) NULL,
      END_CODE_LAST_UPDATED            NVARCHAR (50) NULL
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
