-- Table: END_CODE_LKP

INSERT INTO dbo.END_CODE_LKP (
    END_CODE_ID,
    LEGACY_END_CODE,
    END_CODE,
    END_CODE_DESCRIPTION
)
VALUES
    (
        1,
        null,
        'SC',
        'Screening complete'
    ),
    (
        2,
        null,
        'DNR',
        'Did not respond'
    ),
    (
        3,
        null,
        'DNA',
        'Did not attend'
    ),
    (
        4,
        null,
        'PC',
        'Premature closure of episode'
    ),
    (
        5,
        null,
        'WB',
        'Withdrawn (already being screened)'
    ),
    (
        6,
        null,
        'WC',
        'Withdrawn (ceased from call/recall system)'
    ),
    (
        7,
        null,
        'WD',
        'Withdrawn (died since included in batch)'
    ),
    (
        8,
        null,
        'WF',
        'Withdrawn (FP69 status)'
    ),
    (
        9,
        null,
        'WM',
        'Withdrawn (moved)'
    ),
    (
        10,
        null,
        'WO',
        'Withdrawn (other reason)'
    ),
    (
        11,
        null,
        'WS',
        'Withdrawn (randomised out/prev. suspended)'
    );




-- Table: EPISODE_TYPE_LKP

INSERT INTO dbo.EPISODE_TYPE_LKP (
    EPISODE_TYPE_ID,
    EPISODE_TYPE,
    EPISODE_DESCRIPTION
)
VALUES
    (
        1,
        'C',
        'Call'
    ),
    (
        2,
        'R',
        'Recall'
    ),
    (
        3,
        'E',
        'Early recall'
    ),
    (
        4,
        'H',
        'Higher Risk'
    ),
    (
        5,
        'T',
        'HR ST recall'
    ),
    (
        6,
        'G',
        'GP referral'
    ),
    (
        7,
        'S',
        'Self referral'
    );




-- Table: REASON_CLOSED_CODE_LKP

INSERT INTO dbo.REASON_CLOSED_CODE_LKP (
    REASON_CLOSED_CODE_ID,
    REASON_CLOSED_CODE,
    REASON_CLOSED_CODE_DESCRIPTION
)
VALUES
    (
        1,
        'BS',
        'Being screened'
    ),
    (
        2,
        'CP',
        'Under care permanently'
    ),
    (
        3,
        'CT',
        'Under care temporarily'
    ),
    (
        4,
        'DD',
        'Deceased'
    ),
    (
        5,
        'NS',
        'Attended, not screened'
    ),
    (
        6,
        'NT',
        'No transport'
    ),
    (
        7,
        'OP',
        'Opted out permanently'
    ),
    (
        8,
        'OT',
        'Opted out temporarily'
    ),
    (
        9,
        'RS',
        'Recently screened'
    ),
    (
        10,
        'X',
        'Episode closed, other reason'
    ),
    (
        11,
        'R',
        'Routine closure'
    ),
    (
        12,
        'DE',
        'Defaulted'
    ),
    (
        13,
        'DU',
        'Details unknown'
    ),
    (
        14,
        'MV',
        'Moved'
    ),
    (
        15,
        'NK',
        'Not known at this address'
    ),
    (
        16,
        'NA',
        'Non attender'
    ),
    (
        17,
        'HR',
        'On higher risk'
    ),
    (
        18,
        'NR',
        'Non responder'
    ),
    (
        19,
        'AR',
        'Randomised out'
    ),
    (
        20,
        'FB',
        'Closed being screened'
    ),
    (
        21,
        'FC',
        'Ceased'
    ),
    (
        22,
        'FD',
        'Deceased'
    ),
    (
        23,
        'FF',
        'FP69 status'
    ),
    (
        24,
        'FM',
        'Moved away'
    ),
    (
        25,
        'FP',
        'FPC closed prematurely'
    ),
    (
        26,
        'FS',
        'Suspended'
    ),
    (
        27,
        'FX',
        'WO withdrawn'
    );




-- Table: FINAL_ACTION_CODE_LKP

INSERT INTO dbo.FINAL_ACTION_CODE_LKP (
    FINAL_ACTION_CODE_ID,
    FINAL_ACTION_CODE,
    FINAL_ACTION_CODE_DESCRIPTION
)
VALUES
    (
        1,
        'EC',
        'Short term recall (early clinic)'
    ),
    (
        2,
        'MT',
        'Medical treatment'
    ),
    (
        3,
        'FP',
        'Follow-up'
    ),
    (
        4,
        'RR',
        'Routine recall'
    );


-- Table: ORGANISATION_LKP

INSERT INTO dbo.ORGANISATION_LKP (
    ORGANISATION_ID,
    SCREENING_NAME,
    ORGANISATION_CODE,
    ORGANISATION_NAME,
    ORGANISATION_TYPE,
    IS_ACTIVE
)
VALUES
    (1, 'Breast Screening', 'AGA', 'Gateshead', null, null),
    (2, 'Breast Screening', 'ANE', 'Newcastle', null, null),
    (3, 'Breast Screening', 'ANT', 'North Tees', null, null),
    (4, 'Breast Screening', 'AWC', 'North Cumbria', null, null),
    (5, 'Breast Screening', 'BHL', 'Humberside', null, null),
    (6, 'Breast Screening', 'BHU', 'Pennine', null, null),
    (7, 'Breast Screening', 'BLE', 'Leeds Wakefield', null, null),
    (8, 'Breast Screening', 'BYO', 'North Yorkshire', null, null),
    (9, 'Breast Screening', 'CBA', 'Barnsley', null, null),
    (10, 'Breast Screening', 'CDN', 'Chesterfield (N. Derbys)', null, null),
    (11, 'Breast Screening', 'CDO', 'Doncaster & Bassetlaw', null, null),
    (12, 'Breast Screening', 'CDS', 'Derby (S. Derbys)', null, null),
    (13, 'Breast Screening', 'CLE', 'Leicester & Rutland', null, null),
    (14, 'Breast Screening', 'CLI', 'Lincolnshire', null, null),
    (15, 'Breast Screening', 'CNN', 'Mansfield (N. Notts)', null, null),
    (16, 'Breast Screening', 'CNO', 'Nottingham', null, null),
    (17, 'Breast Screening', 'CRO', 'Rotherham', null, null),
    (18, 'Breast Screening', 'CSH', 'Sheffield', null, null),
    (19, 'Breast Screening', 'DCB', 'Cambridge', null, null),
    (20, 'Breast Screening', 'DGY', 'Great Yarmouth and Waveney', null, null),
    (21, 'Breast Screening', 'DKL', 'Kings Lynn', null, null),
    (22, 'Breast Screening', 'DNF', 'Norwich', null, null),
    (23, 'Breast Screening', 'DPT', 'Peterborough', null, null),
    (24, 'Breast Screening', 'DSU', 'East Suffolk', null, null),
    (25, 'Breast Screening', 'DSW', 'West Suffolk', null, null),
    (26, 'Breast Screening', 'EBA', 'North London', null, null),
    (27, 'Breast Screening', 'ECX', 'West of London', null, null),
    (28, 'Breast Screening', 'ELD', 'Beds & Herts', null, null),
    (29, 'Breast Screening', 'FBH', 'Outer North East London', null, null),
    (30, 'Breast Screening', 'FCO', 'Chelmsford & Colchester', null, null),
    (31, 'Breast Screening', 'FEP', 'Epping (W. Essex)', null, null),
    (32, 'Breast Screening', 'FLO', 'Central & East London', null, null),
    (33, 'Breast Screening', 'FSO', 'Southend (S. Essex)', null, null),
    (34, 'Breast Screening', 'GBR', 'East Sussex, Brighton & Hove', null, null),
    (35, 'Breast Screening', 'GCA', 'South East London (Kings)', null, null),
    (36, 'Breast Screening', 'GCT', 'Kent', null, null),
    (37, 'Breast Screening', 'HGU', 'Guildford (Jarvis)', null, null),
    (38, 'Breast Screening', 'HWA', 'South West London', null, null),
    (39, 'Breast Screening', 'HWO', 'West Sussex', null, null),
    (40, 'Breast Screening', 'IOM', 'Isle of Man', null, null),
    (41, 'Breast Screening', 'JBA', 'North and Mid Hants', null, null),
    (42, 'Breast Screening', 'JDO', 'Dorset', null, null),
    (43, 'Breast Screening', 'JIW', 'Isle of Wight', null, null),
    (44, 'Breast Screening', 'JPO', 'Portsmouth', null, null),
    (45, 'Breast Screening', 'JSO', 'Southampton & Salisbury', null, null),
    (46, 'Breast Screening', 'JSW', 'Wiltshire', null, null),
    (47, 'Breast Screening', 'KHW', 'Aylesbury & Wycombe', null, null),
    (48, 'Breast Screening', 'KKE', 'Kettering', null, null),
    (49, 'Breast Screening', 'KMK', 'Milton Keynes', null, null),
    (50, 'Breast Screening', 'KNN', 'Northampton', null, null),
    (51, 'Breast Screening', 'KOX', 'Oxford', null, null),
    (52, 'Breast Screening', 'KRG', 'Reading (W. Berkshire)', null, null),
    (53, 'Breast Screening', 'KWI', 'Windsor (E. Berkshire)', null, null),
    (54, 'Breast Screening', 'LAV', 'Avon', null, null),
    (55, 'Breast Screening', 'LCO', 'Cornwall', null, null),
    (56, 'Breast Screening', 'LED', 'North & East Devon', null, null),
    (57, 'Breast Screening', 'LGL', 'Gloucestershire', null, null),
    (58, 'Breast Screening', 'LPL', 'West Devon (Plymouth)', null, null),
    (59, 'Breast Screening', 'LSO', 'Somerset', null, null),
    (60, 'Breast Screening', 'LTB', 'South Devon', null, null),
    (61, 'Breast Screening', 'MAS', 'South Staffordshire', null, null),
    (62, 'Breast Screening', 'MBD', 'City, Sandwell & Walsall', null, null),
    (63, 'Breast Screening', 'MBS', 'South Birmingham', null, null),
    (64, 'Breast Screening', 'MCO', 'Warwickshire, Solihull & Coventry', null, null),
    (65, 'Breast Screening', 'MDU', 'Dudley & Wolverhampton', null, null),
    (66, 'Breast Screening', 'MHW', 'Hereford & Worcester', null, null),
    (67, 'Breast Screening', 'MSH', 'Shropshire', null, null),
    (68, 'Breast Screening', 'MST', 'North Midlands', null, null),
    (69, 'Breast Screening', 'NCH', 'Chester', null, null),
    (70, 'Breast Screening', 'NCR', 'Crewe', null, null),
    (71, 'Breast Screening', 'NLI', 'Liverpool', null, null),
    (72, 'Breast Screening', 'NMA', 'East Cheshire and Stockport', null, null),
    (73, 'Breast Screening', 'NWA', 'Warrington, Halton, St Helens & Knowsley', null, null),
    (74, 'Breast Screening', 'NWI', 'Wirral', null, null),
    (75, 'Breast Screening', 'PBO', 'Bolton', null, null),
    (76, 'Breast Screening', 'PLE', 'East Lancashire', null, null),
    (77, 'Breast Screening', 'PLN', 'North Lancashire & South Cumbria', null, null),
    (78, 'Breast Screening', 'PMA', 'Manchester', null, null),
    (79, 'Breast Screening', 'PWI', 'South Lancs', null, null);

-- Table: SCREENING_LKP

INSERT INTO dbo.SCREENING_LKP (
    SCREENING_ID,
    SCREENING_NAME,
    SCREENING_TYPE,
    SCREENING_ACRONYM,
    SCREENING_WORKFLOW_ID
)
VALUES
    (
        1,
        'Breast Screening',
        'Breast Screening Program',
        'BSS',
        'CAAS_BREAST_SCREENING_COHORT'
    );
