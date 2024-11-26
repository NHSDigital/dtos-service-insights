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
        null
        'SC',
        'Screening complete'
    ),
    (
        1001,
        null,
        'DNR',
        'Did not respond'
    ),
    (
        1002,
        null,
        'DNA',
        'Did not attend'
    ),
    (
        1003,
        null,
        'PC',
        'Premature closure of episode'
    ),
    (
        1004,
        null,
        'WB',
        'Withdrawn (already being screened)'
    ),
    (
        1005,
        null,
        'WC',
        'Withdrawn (ceased from call/recall system)'
    ),
    (
        1006,
        null,
        'WD',
        'Withdrawn (died since included in batch)'
    ),
    (
        1007,
        null,
        'WF',
        'Withdrawn (FP69 status)'
    ),
    (
        1008,
        null,
        'WM',
        'Withdrawn (moved)'
    ),
    (
        1009,
        null,
        'WO',
        'Withdrawn (other reason)'
    ),
    (
        1010,
        null,
        'WS',
        'Withdrawn (randomised out/prev. suspended)'
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
        'C',
        'Call'
    ),
    (
        22222,
        'R',
        'Recall'
    ),
    (
        33333,
        'E',
        'Early recall'
    ),
    (
        44444,
        'H',
        'Higher Risk'
    ),
    (
        55555,
        'T',
        'HR ST recall'
    ),
    (
        66666,
        'G',
        'GP referral'
    ),
    (
        77777,
        'S',
        'Self referral'
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
        'BS',
        'Being screened'
    ),
    (
        111333,
        'CP',
        'Under care permanently'
    ),
    (
        111444,
        'CT',
        'Under care temporarily'
    ),
    (
        111555,
        'DD',
        'Deceased'
    ),
    (
        111666,
        'NS',
        'Attended, not screened'
    ),
    (
        111777,
        'NT',
        'No transport'
    ),
    (
        111888,
        'OP',
        'Opted out permanently'
    ),
    (
        111999,
        'OT',
        'Opted out temporarily'
    ),
    (
        112000,
        'RS',
        'Recently screened'
    ),
    (
        112111,
        'X',
        'Episode closed, other reason'
    ),
    (
        112222,
        'R',
        'Routine closure'
    ),
    (
        112333,
        'DE',
        'Defaulted'
    ),
    (
        112444,
        'DU',
        'Details unknown'
    ),
    (
        112555,
        'MV',
        'Moved'
    ),
    (
        112666,
        'NK',
        'Not known at this address'
    ),
    (
        112777,
        'NA',
        'Non attender'
    ),
    (
        112888,
        'HR',
        'On higher risk'
    ),
    (
        112999,
        'NR',
        'Non responder'
    ),
    (
        113000,
        'AR',
        'Randomised out'
    ),
    (
        113111,
        'FB',
        'Closed being screened'
    ),
    (
        113222,
        'FC',
        'Ceased'
    ),
    (
        113333,
        'FD',
        'Deceased'
    ),
    (
        113444,
        'FF',
        'FP69 status'
    ),
    (
        113555,
        'FM',
        'Moved away'
    ),
    (
        113666,
        'FP',
        'FPC closed prematurely'
    ),
    (
        113777,
        'FS',
        'Suspended'
    ),
    (
        113888,
        'FX',
        'WO withdrawn'
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


-- Table: ORGANISATION_LKP

INSERT INTO ORGANISATION_LKP (
    ORGANISATION_ID,
    SCREENING_NAME,
    ORGANISATION_CODE,
    ORGANISATION_NAME,
    ORGANISATION_TYPE,
    IS_ACTIVE
)
VALUES
    (100001, 'Breast Screening', 'AGA', 'Gateshead', 'Clinic', 'Yes'),
    (100002, 'Breast Screening', 'ANE', 'Newcastle', 'Clinic', 'Yes'),
    (100003, 'Breast Screening', 'ANT', 'North Tees', 'Clinic', 'Yes'),
    (100004, 'Breast Screening', 'AWC', 'North Cumbria', 'Clinic', 'Yes'),
    (100005, 'Breast Screening', 'BHL', 'Humberside', 'Clinic', 'Yes'),
    (100006, 'Breast Screening', 'BHU', 'Pennine', 'Clinic', 'Yes'),
    (100007, 'Breast Screening', 'BLE', 'Leeds Wakefield', 'Clinic', 'Yes'),
    (100008, 'Breast Screening', 'BYO', 'North Yorkshire', 'Clinic', 'Yes'),
    (100009, 'Breast Screening', 'CBA', 'Barnsley', 'Clinic', 'Yes'),
    (100010, 'Breast Screening', 'CDN', 'Chesterfield (N. Derbys)', 'Clinic', 'Yes'),
    (100011, 'Breast Screening', 'CDO', 'Doncaster & Bassetlaw', 'Clinic', 'Yes'),
    (100012, 'Breast Screening', 'CDS', 'Derby (S. Derbys)', 'Clinic', 'Yes'),
    (100013, 'Breast Screening', 'CLE', 'Leicester & Rutland', 'Clinic', 'Yes'),
    (100014, 'Breast Screening', 'CLI', 'Lincolnshire', 'Clinic', 'Yes'),
    (100015, 'Breast Screening', 'CNN', 'Mansfield (N. Notts)', 'Clinic', 'Yes'),
    (100016, 'Breast Screening', 'CNO', 'Nottingham', 'Clinic', 'Yes'),
    (100017, 'Breast Screening', 'CRO', 'Rotherham', 'Clinic', 'Yes'),
    (100018, 'Breast Screening', 'CSH', 'Sheffield', 'Clinic', 'Yes'),
    (100019, 'Breast Screening', 'DCB', 'Cambridge', 'Clinic', 'Yes'),
    (100020, 'Breast Screening', 'DGY', 'Great Yarmouth and Waveney', 'Clinic', 'Yes'),
    (100021, 'Breast Screening', 'DKL', 'Kings Lynn', 'Clinic', 'Yes'),
    (100022, 'Breast Screening', 'DNF', 'Norwich', 'Clinic', 'Yes'),
    (100023, 'Breast Screening', 'DPT', 'Peterborough', 'Clinic', 'Yes'),
    (100024, 'Breast Screening', 'DSU', 'East Suffolk', 'Clinic', 'Yes'),
    (100025, 'Breast Screening', 'DSW', 'West Suffolk', 'Clinic', 'Yes'),
    (100026, 'Breast Screening', 'EBA', 'North London', 'Clinic', 'Yes'),
    (100027, 'Breast Screening', 'ECX', 'West of London', 'Clinic', 'Yes'),
    (100028, 'Breast Screening', 'ELD', 'Beds & Herts', 'Clinic', 'Yes'),
    (100029, 'Breast Screening', 'FBH', 'Outer North East London', 'Clinic', 'Yes'),
    (100030, 'Breast Screening', 'FCO', 'Chelmsford & Colchester', 'Clinic', 'Yes'),
    (100031, 'Breast Screening', 'FEP', 'Epping (W. Essex)', 'Clinic', 'Yes'),
    (100032, 'Breast Screening', 'FLO', 'Central & East London', 'Clinic', 'Yes'),
    (100033, 'Breast Screening', 'FSO', 'Southend (S. Essex)', 'Clinic', 'Yes'),
    (100034, 'Breast Screening', 'GBR', 'East Sussex, Brighton & Hove', 'Clinic', 'Yes'),
    (100035, 'Breast Screening', 'GCA', 'South East London (Kings)', 'Clinic', 'Yes'),
    (100036, 'Breast Screening', 'GCT', 'Kent', 'Clinic', 'Yes'),
    (100037, 'Breast Screening', 'HGU', 'Guildford (Jarvis)', 'Clinic', 'Yes'),
    (100038, 'Breast Screening', 'HWA', 'South West London', 'Clinic', 'Yes'),
    (100039, 'Breast Screening', 'HWO', 'West Sussex', 'Clinic', 'Yes'),
    (100040, 'Breast Screening', 'IOM', 'Isle of Man', 'Clinic', 'Yes'),
    (100041, 'Breast Screening', 'JBA', 'North and Mid Hants', 'Clinic', 'Yes'),
    (100042, 'Breast Screening', 'JDO', 'Dorset', 'Clinic', 'Yes'),
    (100043, 'Breast Screening', 'JIW', 'Isle of Wight', 'Clinic', 'Yes'),
    (100044, 'Breast Screening', 'JPO', 'Portsmouth', 'Clinic', 'Yes'),
    (100045, 'Breast Screening', 'JSO', 'Southampton & Salisbury', 'Clinic', 'Yes'),
    (100046, 'Breast Screening', 'JSW', 'Wiltshire', 'Clinic', 'Yes'),
    (100047, 'Breast Screening', 'KHW', 'Aylesbury & Wycombe', 'Clinic', 'Yes'),
    (100048, 'Breast Screening', 'KKE', 'Kettering', 'Clinic', 'Yes'),
    (100049, 'Breast Screening', 'KMK', 'Milton Keynes', 'Clinic', 'Yes'),
    (100050, 'Breast Screening', 'KNN', 'Northampton', 'Clinic', 'Yes'),
    (100051, 'Breast Screening', 'KOX', 'Oxford', 'Clinic', 'Yes'),
    (100052, 'Breast Screening', 'KRG', 'Reading (W. Berkshire)', 'Clinic', 'Yes'),
    (100053, 'Breast Screening', 'KWI', 'Windsor (E. Berkshire)', 'Clinic', 'Yes'),
    (100054, 'Breast Screening', 'LAV', 'Avon', 'Clinic', 'Yes'),
    (100055, 'Breast Screening', 'LCO', 'Cornwall', 'Clinic', 'Yes'),
    (100056, 'Breast Screening', 'LED', 'North & East Devon', 'Clinic', 'Yes'),
    (100057, 'Breast Screening', 'LGL', 'Gloucestershire', 'Clinic', 'Yes'),
    (100058, 'Breast Screening', 'LPL', 'West Devon (Plymouth)', 'Clinic', 'Yes'),
    (100059, 'Breast Screening', 'LSO', 'Somerset', 'Clinic', 'Yes'),
    (100060, 'Breast Screening', 'LTB', 'South Devon', 'Clinic', 'Yes'),
    (100061, 'Breast Screening', 'MAS', 'South Staffordshire', 'Clinic', 'Yes'),
    (100062, 'Breast Screening', 'MBD', 'City, Sandwell & Walsall', 'Clinic', 'Yes'),
    (100063, 'Breast Screening', 'MBS', 'South Birmingham', 'Clinic', 'Yes'),
    (100064, 'Breast Screening', 'MCO', 'Warwickshire, Solihull & Coventry', 'Clinic', 'Yes'),
    (100065, 'Breast Screening', 'MDU', 'Dudley & Wolverhampton', 'Clinic', 'Yes'),
    (100066, 'Breast Screening', 'MHW', 'Hereford & Worcester', 'Clinic', 'Yes'),
    (100067, 'Breast Screening', 'MSH', 'Shropshire', 'Clinic', 'Yes'),
    (100068, 'Breast Screening', 'MST', 'North Midlands', 'Clinic', 'Yes'),
    (100069, 'Breast Screening', 'NCH', 'Chester', 'Clinic', 'Yes'),
    (100070, 'Breast Screening', 'NCR', 'Crewe', 'Clinic', 'Yes'),
    (100071, 'Breast Screening', 'NLI', 'Liverpool', 'Clinic', 'Yes'),
    (100072, 'Breast Screening', 'NMA', 'East Cheshire and Stockport', 'Clinic', 'Yes'),
    (100073, 'Breast Screening', 'NWA', 'Warrington, Halton, St Helens & Knowsley', 'Clinic', 'Yes'),
    (100074, 'Breast Screening', 'NWI', 'Wirral', 'Clinic', 'Yes'),
    (100075, 'Breast Screening', 'PBO', 'Bolton', 'Clinic', 'Yes'),
    (100076, 'Breast Screening', 'PLE', 'East Lancashire', 'Clinic', 'Yes'),
    (100077, 'Breast Screening', 'PLN', 'North Lancashire & South Cumbria', 'Clinic', 'Yes'),
    (100078, 'Breast Screening', 'PMA', 'Manchester', 'Clinic', 'Yes'),
    (100079, 'Breast Screening', 'PWI', 'South Lancs', 'Clinic', 'Yes');

