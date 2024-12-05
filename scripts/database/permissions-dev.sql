CREATE USER [dev-uks-si-create-ps-episode-data] FROM EXTERNAL PROVIDER;
ALTER ROLE [db_datareader] ADD MEMBER [dev-uks-si-create-ps-episode-data];
ALTER ROLE [db_datawriter] ADD MEMBER [dev-uks-si-create-ps-episode-data];

CREATE USER [dev-uks-si-create-participant-screening-profile-data] FROM EXTERNAL PROVIDER;
ALTER ROLE [db_datareader] ADD MEMBER [dev-uks-si-create-participant-screening-profile-data];
ALTER ROLE [db_datawriter] ADD MEMBER [dev-uks-si-create-participant-screening-profile-data];

CREATE USER [dev-uks-si-create-ps-episode] FROM EXTERNAL PROVIDER;
ALTER ROLE [db_datareader] ADD MEMBER [dev-uks-si-create-ps-episode];
ALTER ROLE [db_datawriter] ADD MEMBER [dev-uks-si-create-ps-episode];

CREATE USER [dev-uks-si-create-participant-screening-profile] FROM EXTERNAL PROVIDER;
ALTER ROLE [db_datareader] ADD MEMBER [dev-uks-si-create-participant-screening-profile];
ALTER ROLE [db_datawriter] ADD MEMBER [dev-uks-si-create-participant-screening-profile];

CREATE USER [dev-uks-si-get-demographics-data] FROM EXTERNAL PROVIDER;
ALTER ROLE [db_datareader] ADD MEMBER [dev-uks-si-get-demographics-data];
ALTER ROLE [db_datawriter] ADD MEMBER [dev-uks-si-get-demographics-data];

CREATE USER [dev-uks-si-create-episode] FROM EXTERNAL PROVIDER;
ALTER ROLE [db_datareader] ADD MEMBER [dev-uks-si-create-episode];
ALTER ROLE [db_datawriter] ADD MEMBER [dev-uks-si-create-episode];

CREATE USER [dev-uks-si-get-episode] FROM EXTERNAL PROVIDER;
ALTER ROLE [db_datareader] ADD MEMBER [dev-uks-si-get-episode];
ALTER ROLE [db_datawriter] ADD MEMBER [dev-uks-si-get-episode];

CREATE USER [dev-uks-si-update-episode] FROM EXTERNAL PROVIDER;
ALTER ROLE [db_datareader] ADD MEMBER [dev-uks-si-update-episode];
ALTER ROLE [db_datawriter] ADD MEMBER [dev-uks-si-update-episode];

CREATE USER [dev-uks-si-receive-data] FROM EXTERNAL PROVIDER;
ALTER ROLE [db_datareader] ADD MEMBER [dev-uks-si-receive-data];
ALTER ROLE [db_datawriter] ADD MEMBER [dev-uks-si-receive-data];

CREATE USER [dev-uks-si-create-update-episode] FROM EXTERNAL PROVIDER;
ALTER ROLE [db_datareader] ADD MEMBER [dev-uks-si-create-update-episode];
ALTER ROLE [db_datawriter] ADD MEMBER [dev-uks-si-create-update-episode];

CREATE USER [dev-uks-si-get-episode-mgmt] FROM EXTERNAL PROVIDER;
ALTER ROLE [db_datareader] ADD MEMBER [dev-uks-si-get-episode-mgmt];
ALTER ROLE [db_datawriter] ADD MEMBER [dev-uks-si-get-episode-mgmt];

CREATE USER [dev-uks-si-retrieve-mesh-file-from-cm] FROM EXTERNAL PROVIDER;
ALTER ROLE [db_datareader] ADD MEMBER [dev-uks-si-retrieve-mesh-file-from-cm];
ALTER ROLE [db_datawriter] ADD MEMBER [dev-uks-si-retrieve-mesh-file-from-cm];

CREATE USER [dev-uks-si-get-participant] FROM EXTERNAL PROVIDER;
ALTER ROLE [db_datareader] ADD MEMBER [dev-uks-si-get-participant];
ALTER ROLE [db_datawriter] ADD MEMBER [dev-uks-si-get-participant];

CREATE USER [dev-uks-si-update-participant] FROM EXTERNAL PROVIDER;
ALTER ROLE [db_datareader] ADD MEMBER [dev-uks-si-update-participant];
ALTER ROLE [db_datawriter] ADD MEMBER [dev-uks-si-update-participant];

CREATE USER [dev-uks-si-get-organisation-data] FROM EXTERNAL PROVIDER;
ALTER ROLE [db_datareader] ADD MEMBER [dev-uks-si-get-organisation-data];
ALTER ROLE [db_datawriter] ADD MEMBER [dev-uks-si-get-organisation-data];
