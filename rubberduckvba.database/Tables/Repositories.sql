﻿CREATE TABLE [dbo].[Repositories]
(
	[Id] INT NOT NULL IDENTITY,
	[DateTimeInserted] DATETIME2 NOT NULL,
	[DateTimeUpdated] DATETIME2 NULL,
	[Name] NVARCHAR(255) NOT NULL,
	[Url] NVARCHAR(1023) NOT NULL,
	CONSTRAINT [PK_Repositories] PRIMARY KEY CLUSTERED ([Id]),
	CONSTRAINT [UK_Repositories_Name] UNIQUE ([Name])
)
