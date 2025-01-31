CREATE TABLE [dbo].[Annotations]
(
	[Id] INT NOT NULL IDENTITY,
	[DateTimeInserted] DATETIME2 NOT NULL,
	[DateTimeUpdated] DATETIME2 NULL,
	[FeatureId] INT NOT NULL,
	[TagAssetId] INT NOT NULL,
	[SourceUrl] NVARCHAR(1023) NOT NULL,
	[Name] NVARCHAR(255) NOT NULL,
	[Summary] NVARCHAR(MAX) NOT NULL DEFAULT(''),
	[Remarks] NVARCHAR(MAX) NULL,
	[JsonParameters] NVARCHAR(MAX) NULL,
	[JsonExamples] NVARCHAR(MAX) NULL,
	[IsNew] BIT NOT NULL DEFAULT(0),
	[IsDiscontinued] BIT NOT NULL DEFAULT(0),
	[IsHidden] BIT NOT NULL DEFAULT(0),
	CONSTRAINT [PK_Annotations] PRIMARY KEY CLUSTERED ([Id]),
	CONSTRAINT [NK_Annotations] UNIQUE ([Name]),
	CONSTRAINT [FK_Annotations_Features] FOREIGN KEY ([FeatureId]) REFERENCES [dbo].[Features] ([Id]),
	CONSTRAINT [FK_Annotations_TagAssets] FOREIGN KEY ([TagAssetId]) REFERENCES [dbo].[TagAssets] ([Id])
)
