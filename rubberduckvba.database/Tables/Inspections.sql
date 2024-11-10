﻿CREATE TABLE [dbo].[Inspections]
(
	[Id] INT NOT NULL IDENTITY,
	[DateTimeInserted] DATETIME2 NOT NULL,
	[DateTimeUpdated] DATETIME2 NULL,
	[FeatureId] INT NOT NULL,
	[TagAssetId] INT NOT NULL,
	[SourceUrl] NVARCHAR(1023) NOT NULL,
	[Name] NVARCHAR(255) NOT NULL,
	[InspectionType] NVARCHAR(255) NOT NULL,
	[DefaultSeverity] NVARCHAR(255) NOT NULL,
	[Summary] NVARCHAR(MAX) NOT NULL,
	[Reasoning] NVARCHAR(MAX) NOT NULL,
	[Remarks] NVARCHAR(MAX) NULL,
	[HostApp] NVARCHAR(MAX) NULL,
	[References] NVARCHAR(MAX) NULL,
	[QuickFixes] NVARCHAR(MAX) NULL,
	[JsonExamples] NVARCHAR(MAX) NULL,
	CONSTRAINT [PK_Inspections] PRIMARY KEY CLUSTERED ([Id]),
	CONSTRAINT [NK_Inspections] UNIQUE ([Name]),
	CONSTRAINT [FK_Inspections_Features] FOREIGN KEY ([FeatureId]) REFERENCES [dbo].[Features] ([Id]),
	CONSTRAINT [FK_Inspections_TagAssets] FOREIGN KEY ([TagAssetId]) REFERENCES [dbo].[TagAssets] ([Id])
)
