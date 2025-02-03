CREATE VIEW [dbo].[HangfireJobState] 
AS 

WITH meta_xmldoc AS (
  SELECT [Field], [Value]    
  FROM [hangfire].[Hash]
  WHERE [Key] = 'recurring-job:update_xmldoc_content'
),
meta_tags AS (
  SELECT [Field], [Value]    
  FROM [hangfire].[Hash]
  WHERE [Key] = 'recurring-job:update_installer_downloads'
),
jobState AS (
  SELECT s.[Id], [JobId] = j.[Id], j.[StateName], s.[CreatedAt]
  FROM [hangfire].[Job] j
  INNER JOIN [hangfire].[State] s ON j.StateId = s.Id AND j.Id = s.JobId
  WHERE s.[Name] IN ('Succeeded', 'Failed')
),
pivoted AS (
SELECT 
     [JobName] = 'update_installer_downloads'
    ,[LastJobId] = src.[Value]
    ,[CreatedAt] = (SELECT [Value] FROM meta_tags WHERE [Field]='CreatedAt')
    ,[NextExecution] = (SELECT [Value] FROM meta_tags WHERE [Field]='NextExecution')
FROM (SELECT [Value] FROM meta_tags WHERE [Field]='LastJobId') src
UNION ALL
SELECT 
     [JobName] = 'update_xmldoc_content'
    ,[LastJobId] = src.[Value]
    ,[CreatedAt] = (SELECT [Value] FROM meta_xmldoc WHERE [Field]='CreatedAt')
    ,[NextExecution] = (SELECT [Value] FROM meta_xmldoc WHERE [Field]='NextExecution')
FROM (SELECT [Value] FROM meta_xmldoc WHERE [Field]='LastJobId') src
)
SELECT
     p.[JobName]
    ,p.[LastJobId]
    ,p.[CreatedAt]
    ,p.[NextExecution]
    ,s.[StateName]
    ,[StateTimestamp]=s.[CreatedAt]
FROM pivoted p
LEFT JOIN jobState s ON p.[LastJobId]=s.[JobId];