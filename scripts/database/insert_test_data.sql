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
        null,
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
        11112,
        'R',
        'Recall'
    ),
    (
        11113,
        'E',
        'Early recall'
    ),
    (
        11114,
        'H',
        'Higher Risk'
    ),
    (
        11115,
        'T',
        'HR ST recall'
    ),
    (
        11116,
        'G',
        'GP referral'
    ),
    (
        11117,
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
        111223,
        'CP',
        'Under care permanently'
    ),
    (
        111224,
        'CT',
        'Under care temporarily'
    ),
    (
        111225,
        'DD',
        'Deceased'
    ),
    (
        111226,
        'NS',
        'Attended, not screened'
    ),
    (
        111227,
        'NT',
        'No transport'
    ),
    (
        111228,
        'OP',
        'Opted out permanently'
    ),
    (
        111229,
        'OT',
        'Opted out temporarily'
    ),
    (
        111230,
        'RS',
        'Recently screened'
    ),
    (
        111231,
        'X',
        'Episode closed, other reason'
    ),
    (
        111232,
        'R',
        'Routine closure'
    ),
    (
        111233,
        'DE',
        'Defaulted'
    ),
    (
        111234,
        'DU',
        'Details unknown'
    ),
    (
        111235,
        'MV',
        'Moved'
    ),
    (
        111236,
        'NK',
        'Not known at this address'
    ),
    (
        111237,
        'NA',
        'Non attender'
    ),
    (
        111238,
        'HR',
        'On higher risk'
    ),
    (
        111239,
        'NR',
        'Non responder'
    ),
    (
        111240,
        'AR',
        'Randomised out'
    ),
    (
        111241,
        'FB',
        'Closed being screened'
    ),
    (
        111242,
        'FC',
        'Ceased'
    ),
    (
        111243,
        'FD',
        'Deceased'
    ),
    (
        111244,
        'FF',
        'FP69 status'
    ),
    (
        111245,
        'FM',
        'Moved away'
    ),
    (
        111246,
        'FP',
        'FPC closed prematurely'
    ),
    (
        111247,
        'FS',
        'Suspended'
    ),
    (
        111248,
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
        112234,
        'MT',
        'Medical treatment'
    ),
    (
        112235,
        'FP',
        'Follow-up'
    ),
    (
        112236,
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
    (100001, 'Breast Screening', 'AGA', 'Gateshead', null, null),
    (100002, 'Breast Screening', 'ANE', 'Newcastle', null, null),
    (100003, 'Breast Screening', 'ANT', 'North Tees', null, null),
    (100004, 'Breast Screening', 'AWC', 'North Cumbria', null, null),
    (100005, 'Breast Screening', 'BHL', 'Humberside', null, null),
    (100006, 'Breast Screening', 'BHU', 'Pennine', null, null),
    (100007, 'Breast Screening', 'BLE', 'Leeds Wakefield', null, null),
    (100008, 'Breast Screening', 'BYO', 'North Yorkshire', null, null),
    (100009, 'Breast Screening', 'CBA', 'Barnsley', null, null),
    (100010, 'Breast Screening', 'CDN', 'Chesterfield (N. Derbys)', null, null),
    (100011, 'Breast Screening', 'CDO', 'Doncaster & Bassetlaw', null, null),
    (100012, 'Breast Screening', 'CDS', 'Derby (S. Derbys)', null, null),
    (100013, 'Breast Screening', 'CLE', 'Leicester & Rutland', null, null),
    (100014, 'Breast Screening', 'CLI', 'Lincolnshire', null, null),
    (100015, 'Breast Screening', 'CNN', 'Mansfield (N. Notts)', null, null),
    (100016, 'Breast Screening', 'CNO', 'Nottingham', null, null),
    (100017, 'Breast Screening', 'CRO', 'Rotherham', null, null),
    (100018, 'Breast Screening', 'CSH', 'Sheffield', null, null),
    (100019, 'Breast Screening', 'DCB', 'Cambridge', null, null),
    (100020, 'Breast Screening', 'DGY', 'Great Yarmouth and Waveney', null, null),
    (100021, 'Breast Screening', 'DKL', 'Kings Lynn', null, null),
    (100022, 'Breast Screening', 'DNF', 'Norwich', null, null),
    (100023, 'Breast Screening', 'DPT', 'Peterborough', null, null),
    (100024, 'Breast Screening', 'DSU', 'East Suffolk', null, null),
    (100025, 'Breast Screening', 'DSW', 'West Suffolk', null, null),
    (100026, 'Breast Screening', 'EBA', 'North London', null, null),
    (100027, 'Breast Screening', 'ECX', 'West of London', null, null),
    (100028, 'Breast Screening', 'ELD', 'Beds & Herts', null, null),
    (100029, 'Breast Screening', 'FBH', 'Outer North East London', null, null),
    (100030, 'Breast Screening', 'FCO', 'Chelmsford & Colchester', null, null),
    (100031, 'Breast Screening', 'FEP', 'Epping (W. Essex)', null, null),
    (100032, 'Breast Screening', 'FLO', 'Central & East London', null, null),
    (100033, 'Breast Screening', 'FSO', 'Southend (S. Essex)', null, null),
    (100034, 'Breast Screening', 'GBR', 'East Sussex, Brighton & Hove', null, null),
    (100035, 'Breast Screening', 'GCA', 'South East London (Kings)', null, null),
    (100036, 'Breast Screening', 'GCT', 'Kent', null, null),
    (100037, 'Breast Screening', 'HGU', 'Guildford (Jarvis)', null, null),
    (100038, 'Breast Screening', 'HWA', 'South West London', null, null),
    (100039, 'Breast Screening', 'HWO', 'West Sussex', null, null),
    (100040, 'Breast Screening', 'IOM', 'Isle of Man', null, null),
    (100041, 'Breast Screening', 'JBA', 'North and Mid Hants', null, null),
    (100042, 'Breast Screening', 'JDO', 'Dorset', null, null),
    (100043, 'Breast Screening', 'JIW', 'Isle of Wight', null, null),
    (100044, 'Breast Screening', 'JPO', 'Portsmouth', null, null),
    (100045, 'Breast Screening', 'JSO', 'Southampton & Salisbury', null, null),
    (100046, 'Breast Screening', 'JSW', 'Wiltshire', null, null),
    (100047, 'Breast Screening', 'KHW', 'Aylesbury & Wycombe', null, null),
    (100048, 'Breast Screening', 'KKE', 'Kettering', null, null),
    (100049, 'Breast Screening', 'KMK', 'Milton Keynes', null, null),
    (100050, 'Breast Screening', 'KNN', 'Northampton', null, null),
    (100051, 'Breast Screening', 'KOX', 'Oxford', null, null),
    (100052, 'Breast Screening', 'KRG', 'Reading (W. Berkshire)', null, null),
    (100053, 'Breast Screening', 'KWI', 'Windsor (E. Berkshire)', null, null),
    (100054, 'Breast Screening', 'LAV', 'Avon', null, null),
    (100055, 'Breast Screening', 'LCO', 'Cornwall', null, null),
    (100056, 'Breast Screening', 'LED', 'North & East Devon', null, null),
    (100057, 'Breast Screening', 'LGL', 'Gloucestershire', null, null),
    (100058, 'Breast Screening', 'LPL', 'West Devon (Plymouth)', null, null),
    (100059, 'Breast Screening', 'LSO', 'Somerset', null, null),
    (100060, 'Breast Screening', 'LTB', 'South Devon', null, null),
    (100061, 'Breast Screening', 'MAS', 'South Staffordshire', null, null),
    (100062, 'Breast Screening', 'MBD', 'City, Sandwell & Walsall', null, null),
    (100063, 'Breast Screening', 'MBS', 'South Birmingham', null, null),
    (100064, 'Breast Screening', 'MCO', 'Warwickshire, Solihull & Coventry', null, null),
    (100065, 'Breast Screening', 'MDU', 'Dudley & Wolverhampton', null, null),
    (100066, 'Breast Screening', 'MHW', 'Hereford & Worcester', null, null),
    (100067, 'Breast Screening', 'MSH', 'Shropshire', null, null),
    (100068, 'Breast Screening', 'MST', 'North Midlands', null, null),
    (100069, 'Breast Screening', 'NCH', 'Chester', null, null),
    (100070, 'Breast Screening', 'NCR', 'Crewe', null, null),
    (100071, 'Breast Screening', 'NLI', 'Liverpool', null, null),
    (100072, 'Breast Screening', 'NMA', 'East Cheshire and Stockport', null, null),
    (100073, 'Breast Screening', 'NWA', 'Warrington, Halton, St Helens & Knowsley', null, null),
    (100074, 'Breast Screening', 'NWI', 'Wirral', null, null),
    (100075, 'Breast Screening', 'PBO', 'Bolton', null, null),
    (100076, 'Breast Screening', 'PLE', 'East Lancashire', null, null),
    (100077, 'Breast Screening', 'PLN', 'North Lancashire & South Cumbria', null, null),
    (100078, 'Breast Screening', 'PMA', 'Manchester', null, null),
    (100079, 'Breast Screening', 'PWI', 'South Lancs', null, null);
