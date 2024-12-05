CREATE USER [int-uks-si-create-part-screen-episode-data] FROM EXTERNAL PROVIDER;
ALTER ROLE [db_datareader] ADD MEMBER [int-uks-si-create-part-screen-episode-data];
ALTER ROLE [db_datawriter] ADD MEMBER [int-uks-si-create-part-screen-episode-data];
<<<<<<< HEAD

CREATE USER [int-uks-si-create-participant-screening-episode-data] FROM EXTERNAL PROVIDER;
ALTER ROLE [db_datareader] ADD MEMBER [int-uks-si-create-participant-screening-episode-data];
ALTER ROLE [db_datawriter] ADD MEMBER [int-uks-si-create-participant-screening-episode-data];
=======
>>>>>>> 8d8a680 (rename create-participant-screening-episode due to length limit)

CREATE USER [int-uks-si-create-participant-screening-profile-data] FROM EXTERNAL PROVIDER;
ALTER ROLE [db_datareader] ADD MEMBER [int-uks-si-create-participant-screening-profile-data];
ALTER ROLE [db_datawriter] ADD MEMBER [int-uks-si-create-participant-screening-profile-data];

CREATE USER [int-uks-si-create-part-screen-episode] FROM EXTERNAL PROVIDER;
ALTER ROLE [db_datareader] ADD MEMBER [int-uks-si-create-part-screen-episode];
ALTER ROLE [db_datawriter] ADD MEMBER [int-uks-si-create-part-screen-episode];
<<<<<<< HEAD

CREATE USER [int-uks-si-create-participant-screening-episode] FROM EXTERNAL PROVIDER;
ALTER ROLE [db_datareader] ADD MEMBER [int-uks-si-create-participant-screening-episode];
ALTER ROLE [db_datawriter] ADD MEMBER [int-uks-si-create-participant-screening-episode];
=======
>>>>>>> 8d8a680 (rename create-participant-screening-episode due to length limit)

CREATE USER [int-uks-si-create-participant-screening-profile] FROM EXTERNAL PROVIDER;
ALTER ROLE [db_datareader] ADD MEMBER [int-uks-si-create-participant-screening-profile];
ALTER ROLE [db_datawriter] ADD MEMBER [int-uks-si-create-participant-screening-profile];

CREATE USER [int-uks-si-get-demographics-data] FROM EXTERNAL PROVIDER;
ALTER ROLE [db_datareader] ADD MEMBER [int-uks-si-get-demographics-data];
ALTER ROLE [db_datawriter] ADD MEMBER [int-uks-si-get-demographics-data];

CREATE USER [int-uks-si-create-episode] FROM EXTERNAL PROVIDER;
ALTER ROLE [db_datareader] ADD MEMBER [int-uks-si-create-episode];
ALTER ROLE [db_datawriter] ADD MEMBER [int-uks-si-create-episode];

CREATE USER [int-uks-si-get-episode] FROM EXTERNAL PROVIDER;
ALTER ROLE [db_datareader] ADD MEMBER [int-uks-si-get-episode];
ALTER ROLE [db_datawriter] ADD MEMBER [int-uks-si-get-episode];

CREATE USER [int-uks-si-update-episode] FROM EXTERNAL PROVIDER;
ALTER ROLE [db_datareader] ADD MEMBER [int-uks-si-update-episode];
ALTER ROLE [db_datawriter] ADD MEMBER [int-uks-si-update-episode];

CREATE USER [int-uks-si-receive-data] FROM EXTERNAL PROVIDER;
ALTER ROLE [db_datareader] ADD MEMBER [int-uks-si-receive-data];
ALTER ROLE [db_datawriter] ADD MEMBER [int-uks-si-receive-data];

CREATE USER [int-uks-si-create-update-episode] FROM EXTERNAL PROVIDER;
ALTER ROLE [db_datareader] ADD MEMBER [int-uks-si-create-update-episode];
ALTER ROLE [db_datawriter] ADD MEMBER [int-uks-si-create-update-episode];

CREATE USER [int-uks-si-get-episode-mgmt] FROM EXTERNAL PROVIDER;
ALTER ROLE [db_datareader] ADD MEMBER [int-uks-si-get-episode-mgmt];
ALTER ROLE [db_datawriter] ADD MEMBER [int-uks-si-get-episode-mgmt];

CREATE USER [int-uks-si-retrieve-mesh-file-from-cm] FROM EXTERNAL PROVIDER;
ALTER ROLE [db_datareader] ADD MEMBER [int-uks-si-retrieve-mesh-file-from-cm];
ALTER ROLE [db_datawriter] ADD MEMBER [int-uks-si-retrieve-mesh-file-from-cm];

CREATE USER [int-uks-si-get-participant] FROM EXTERNAL PROVIDER;
ALTER ROLE [db_datareader] ADD MEMBER [int-uks-si-get-participant];
ALTER ROLE [db_datawriter] ADD MEMBER [int-uks-si-get-participant];

CREATE USER [int-uks-si-update-participant] FROM EXTERNAL PROVIDER;
ALTER ROLE [db_datareader] ADD MEMBER [int-uks-si-update-participant];
ALTER ROLE [db_datawriter] ADD MEMBER [int-uks-si-update-participant];

CREATE USER [int-uks-si-get-organisation-data] FROM EXTERNAL PROVIDER;
ALTER ROLE [db_datareader] ADD MEMBER [int-uks-si-get-organisation-data];
ALTER ROLE [db_datawriter] ADD MEMBER [int-uks-si-get-organisation-data];
