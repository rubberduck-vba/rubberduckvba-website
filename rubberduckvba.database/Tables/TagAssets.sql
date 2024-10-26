﻿CREATE TABLE [dbo].[TagAssets]
(
	[Id] INT NOT NULL IDENTITY,
	[DateTimeInserted] DATETIME2 NOT NULL,
	[DateTimeUpdated] DATETIME2 NULL,
	[TagId] INT NOT NULL,
	[Name] NVARCHAR(255) NOT NULL,
	[DownloadUrl] NVARCHAR(1023) NOT NULL,
	CONSTRAINT [PK_TagAssets] PRIMARY KEY CLUSTERED ([Id]),
	CONSTRAINT [FK_TagAssets_Tags] FOREIGN KEY ([TagId]) REFERENCES [dbo].[Tags] ([Id])
)
