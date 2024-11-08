-- Table: END_CODE_LKP

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



-- Table: EPISODE_TYPE_LKP

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



-- Table: REASON_CLOSED_CODE_LKP

INSERT INTO REASON_CLOSED_CODE_LKP (
    REASON_CLOSED_CODE_ID,
    REASON_CLOSED_CODE,
    REASON_CLOSED_CODE_DESCRIPTION
)
VALUES
    (
        111222,
        'INFORMED_SUBJECT_CHOICE',
        'Informed Choice'
    ),
    (
        111333,
        'BILATERAL_MASTECTOMY',
        'Bilateral Mastectomy'
    ),
    (
        111444,
        'MENTAL_CAPACITY_ACT',
        'Mental Capacity Act'
    ),
    (
        111555,
        'PERSONAL_WELFARE',
        'Personal Welfare'
    );



-- Table: FINAL_ACTION_CODE_LKP

INSERT INTO FINAL_ACTION_CODE_LKP (
    FINAL_ACTION_CODE_ID,
    FINAL_ACTION_CODE,
    FINAL_ACTION_CODE_DESCRIPTION
)
VALUES
    (
        112233,
        'EC',
        'Short term recall (early clinic)'
    ),
    (
        223344,
        'MT',
        'Medical treatment'
    ),
    (
        334455,
        'FP',
        'Follow-up'
    ),
    (
        445566,
        'RR',
        'Routine recall'
    );
