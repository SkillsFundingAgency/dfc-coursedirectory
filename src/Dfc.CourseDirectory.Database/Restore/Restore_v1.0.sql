----------------------------------------------------------------------
------------------------------------RESTORE-------------------------------------
DECLARE @databasename as VARCHAR(150)
SET @databasename = 'https://dfcdevprovstr.blob.core.windows.net/restore/SFA_CourseDirectory_Backup_' + FORMAT (getdate(), 'yyyy_MM_dd') + '.bak'
--NEW
RESTORE DATABASE SFA_CourseDirectory 
FROM URL = @databasename
GO

