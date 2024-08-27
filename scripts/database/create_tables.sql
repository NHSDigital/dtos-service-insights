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