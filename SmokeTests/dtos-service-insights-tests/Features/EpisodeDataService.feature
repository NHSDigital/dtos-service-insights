Feature: DTOS Application Insights Smoke tests

Smoke tests to check the framework

    Background:
        Given the database is cleaned of all records for Episode Ids: 837413, 849095, 837864
        And the application is properly configured

@smoketest1
Scenario: 01. Verify new episode is created
    Given file <FileName> exists in the configured location for "Add" with Episode Ids : <EpisodeIds>
    When the file is uploaded to the Blob Storage container
    Then the Episode Ids in the database should match the file data
    And the episode data from file should be inserted or updated in the database

      Examples:
        | FileName                     | RecordType | EpisodeIds |
        | bss_episodes_add_one_row.csv | Add        | 837413     |

@smoketest2
Scenario: 02. Verify episode is updated in the database
    Given file <AddFileName> exists in the configured location for "Add" with Episode Ids : <EpisodeIds>
    And the file is uploaded to the Blob Storage container
    And the Episode Ids in the database should match the file data
    Given file <AmendedFileName> exists in the configured location for "Amended" with Episode Ids : <EpisodeIds>
    When the file is uploaded to the Blob Storage container
    Then there should be 1 records for the Episode Id in the database
    And the database should match the amended <AmendedEpisodeDateValue> for the Episode Id
    And the episode data from file should be inserted or updated in the database

    Examples:
        | AddFileName                  |  AmendedFileName                | EpisodeIds | AmendedEpisodeDateValue |
        | bss_episodes_add_one_row.csv | bss_episodes_update_one_row.csv | 837413     | 01/03/2021              |

@smoketest3
Scenario: 03. Episode record with invalid header is processed
  Given file <FileName> exists in the configured location for "Add" with Episode Ids : <EpisodeIds>
  When the file is uploaded to the Blob Storage container
  Then there should be 0 records for the Episode Id in the database

      Examples:
        | FileName                               | RecordType | EpisodeIds |
        | bss_episodes_invalidheader_one_row.csv | Add        | 837413     |

@smoketest4
Scenario: 04. Episode record with invalid episode type is processed
  Given file <FileName> exists in the configured location for "Add" with Episode Ids : <EpisodeIds>
  When the file is uploaded to the Blob Storage container
  Then there should be 0 records for the Episode Id in the database

      Examples:
        | FileName                                    | RecordType | EpisodeIds |
        | bss_episodes_invalidepisodetype_one_row.csv | Add        | 837413     |

@smoketest5
Scenario: 05. Mixed Data Quality and Partial Failure on CSV Upload - DB Check
  Given file <FileName> exists in the configured location for "Add" with Episode Ids : <EpisodeIds>
  When the file is uploaded to the Blob Storage container
  Then there should be 1 records for the Episode Id "837864" in the database
  And there should be 0 records for the Episode Id "849095" in the database

      Examples:
        | FileName                                    | RecordType | EpisodeIds     |
        | bss_episodes_mixedquality_2_rows.csv        | Add        | 837864, 849095 |

@smoketest6
Scenario: 06. Mixed Data Quality and Partial Failure on CSV Upload - API Check
  Given file <FileName> exists in the configured location for "Add" with Episode Ids : <EpisodeIds>
  When the file is uploaded to the Blob Storage container
  And the GET Participant Screening Episode API request is made
  Then GET Participant Screening Episode API returns 1 records for the Episode Id "837864"
  And GET Participant Screening Episode API returns 0 records for the Episode Id "849095"

      Examples:
        | FileName                                    | RecordType | EpisodeIds     |
        | bss_episodes_mixedquality_2_rows.csv        | Add        | 837864, 849095 |

@smoketest7
Scenario: 07. Successful Retrieval of Episode Data from API
  Given file <FileName> exists in the configured location for "Add" with Episode Ids : <EpisodeIds>
  When the file is uploaded to the Blob Storage container
  And the GET Participant Screening Episode API request is made
  Then the response status is 200
  And the correct episode data is returned

      Examples:
        | FileName                     | RecordType | EpisodeIds |
        | bss_episodes_psepisode_api_1row.csv | Add        | 837864     |

@smoketest8
Scenario: 08. Successful Retrieval of Participant Data from API
  Given file <FileName> exists in the configured location for "Add" with Episode Ids : <EpisodeIds>
  When the file is uploaded to the Blob Storage container
  And the GET Participant Screening Profile API request is made
  Then the response status is 200
  And there should be 1 records for the participant in the API response

      Examples:
        | FileName                     | RecordType | EpisodeIds |
        | bss_episodes_add_one_row.csv | Add        | 837413     |


@smoketest9
Scenario: 09. Populate Reference Data Successfully for Episode & Participant
    Given file <FileName> exists in the configured location for "Add" with Episode Ids : <EpisodeIds>
    When the file is uploaded to the Blob Storage container
    Then description for "end_code" in "PARTICIPANT_SCREENING_PROFILE" table is populated
    Then codes for "EPISODE_TYPE_ID" in "EPISODE" table is populated from reference data

      Examples:
        | FileName                     | RecordType | EpisodeIds |
        | bss_episodes_add_one_row.csv | Add        | 837413     |


@smoketest10
Scenario: 10. Handling Multiple Record Changes for a Single Episode
    Given file <FileName> exists in the configured location for "Add" with Episode Ids : <EpisodeIds>
    When the file is uploaded to the Blob Storage container
    Then latest changes to the episode are loaded into the Episode Manager
    And there should be 2 records in BI and Analytics data store

      Examples:
        | FileName                                        | RecordType | EpisodeIds    |
        | bss_episodes_singleepisode_differentupdates.csv | Add        | 837413,837413 |
