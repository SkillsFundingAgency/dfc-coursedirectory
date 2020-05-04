
DROP CREDENTIAL [https://dfcdevprovstr.blob.core.windows.net/restore]
CREATE CREDENTIAL [https://dfcdevprovstr.blob.core.windows.net/restore]
WITH IDENTITY = 'SHARED ACCESS SIGNATURE',
SECRET = 'xxxxxxxxxxxxxxxx'
