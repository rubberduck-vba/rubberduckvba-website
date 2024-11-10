﻿CREATE TABLE [dbo].[QuickFixes]
(
	[Id] INT NOT NULL IDENTITY,
	[DateTimeInserted] DATETIME2 NOT NULL,
	[DateTimeUpdated] DATETIME2 NULL,
	[FeatureId] INT NOT NULL,
	[TagAssetId] INT NOT NULL,
	[SourceUrl] NVARCHAR(1023) NOT NULL,
	[Name] NVARCHAR(255) NOT NULL,
	[Summary] NVARCHAR(MAX) NOT NULL,
	[Remarks] NVARCHAR(MAX) NULL,
	[CanFixMultiple] BIT NOT NULL,
	[CanFixProcedure] BIT NOT NULL,
	[CanFixModule] BIT NOT NULL,
	[CanFixProject] BIT NOT NULL,
	[CanFixAll] BIT NOT NULL,
	[Inspections] NVARCHAR(MAX) NOT NULL,
	[JsonExamples] NVARCHAR(MAX) NULL,
	CONSTRAINT [PK_QuickFixes] PRIMARY KEY CLUSTERED ([Id]),
	CONSTRAINT [NK_QuickFixes] UNIQUE ([Name]),
	CONSTRAINT [FK_QuickFixes_Features] FOREIGN KEY ([FeatureId]) REFERENCES [dbo].[Features] ([Id]),
	CONSTRAINT [FK_QuickFixes_TagAssets] FOREIGN KEY ([TagAssetId]) REFERENCES [dbo].[TagAssets] ([Id])
)
