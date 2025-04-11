Feature: DTOS Application Insights Smoke tests

Smoke tests to check the framework

    Background:
        Given the database is cleaned of all records for NHS Numbers: 9990007068
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
