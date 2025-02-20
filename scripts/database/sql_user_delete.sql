-- Declare a variable to store the user name
DECLARE @UserName NVARCHAR(255);

-- Create a cursor to iterate through the users to be deleted
DECLARE user_cursor CURSOR FOR
SELECT name
FROM sys.database_principals
WHERE [type] = 'E'  -- External user
ORDER BY name;

-- Open the cursor
OPEN user_cursor;

-- Fetch the first user name
FETCH NEXT FROM user_cursor INTO @UserName;

-- Loop through the users and delete them
WHILE @@FETCH_STATUS = 0
BEGIN
    -- Construct the command to drop the user (using QUOTENAME for safety)
    DECLARE @SQLCommand_1 NVARCHAR(MAX) = N'ALTER ROLE [db_datareader] DROP MEMBER ' + QUOTENAME(@UserName);
    EXEC sp_executesql @SQLCommand_1;

    DECLARE @SQLCommand_2 NVARCHAR(MAX) = N'ALTER ROLE [db_datawriter] DROP MEMBER ' + QUOTENAME(@UserName);
    EXEC sp_executesql @SQLCommand_2;

    DECLARE @SQLCommand_3 NVARCHAR(MAX) = N'DROP USER ' + QUOTENAME(@UserName);
    EXEC sp_executesql @SQLCommand_3;

    -- Print a message (optional)
    PRINT N'Dropped user: ' + @UserName;

    -- Fetch the next user name
    FETCH NEXT FROM user_cursor INTO @UserName;
END;

-- Close and deallocate the cursor
CLOSE user_cursor;
DEALLOCATE user_cursor;

-- Print a completion message (optional)
PRINT N'Finished dropping external users.';
