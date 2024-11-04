/*==============================================================*/
/* Table: EPISODE                                               */
/*==============================================================*/

INSERT INTO dbo.EPISODE (
    episode_id,
    participant_id,
    screening_id,
    nhs_number,
    episode_type_id,
    episode_open_date,
    appointment_made_flag,
    first_offered_appointment_date,
    actual_screening_date,
    early_recall_date,
    call_recall_status_authorised_by,
    end_code_id,
    end_code_last_updated,
    organisation_id,
    batch_id,
    record_insert_datetime,
    record_update_datetime
)
VALUES
    (
        245395,
        NULL,
        NULL,
        1111111112,
        'C',
        '2000-01-01',
        'TRUE',
        '2000-01-01',
        '2000-01-01',
        NULL,
        'SCREENING_OFFICE',
        'SC',
        '2000-01-01',
        'PBO',
        'ECHO',
        NULL,
        NULL
    ),
    (
        656047,
        NULL,
        NULL,
        1111111110,
        'E',
        '2017-08-25',
        'TRUE',
        '2017-08-25',
        NULL,
        NULL,
        'SCREENING_OFFICE',
        'WF',
        '2017-08-25',
        'LED',
        'WFONCHANGE',
        NULL,
        NULL
    );


/*==============================================================*/
/* Table: END_CODE_LKP                                          */
/*==============================================================*/

INSERT INTO END_CODE_LKP (
    END_CODE_ID,
    LEGACY_END_CODE,
    END_CODE,
    END_CODE_DESCRIPTION
)
VALUES
    (
        1000,
        'LEGACY_001',
        'END_CODE_001',
        'Description for End Code 001'
    ),
    (
        2000,
        'LEGACY_002',
        'END_CODE_002',
        'Description for End Code 002'
    ),
    (
        3000,
        'LEGACY_003',
        'END_CODE_003',
        'Description for End Code 003'
    );


/*==============================================================*/
/* Table: EPISODE_TYPE_LKP                                      */
/*==============================================================*/

INSERT INTO EPISODE_TYPE_LKP (
    EPISODE_TYPE_ID,
    EPISODE_TYPE,
    EPISODE_DESCRIPTION
)
VALUES
    (
        11111,
        'A',
        'Description for Episode Type A'
    ),
    (
        22222,
        'B',
        'Description for Episode Type B'
    ),
    (
        33333,
        'C',
        'Description for Episode Type C'
    );


/*==============================================================*/
/* Table: ORGANISATION_LKP                                      */
/*==============================================================*/

INSERT INTO ORGANISATION_LKP (
    ORGANISATION_ID,
    SCREENING_NAME,
    ORGANISATION_CODE,
    ORGANISATION_NAME,
    ORGANISATION_TYPE,
    IS_ACTIVE
)
VALUES
    (
        1010,
        'Screening 1',
        'AGA',
        'Organisation 1',
        'Type A',
        1
    ),
    (
        2020,
        'Screening 2',
        'ANE',
        'Organisation 2',
        'Type B',
        1
    ),
    (
        3030,
        'Screening 3',
        'AZA',
        'Organisation 3',
        'Type C',
        0
    );
