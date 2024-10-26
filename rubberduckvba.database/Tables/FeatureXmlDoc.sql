CREATE TABLE [dbo].[FeatureXmlDoc]
(
	[Id] INT NOT NULL IDENTITY,
	[DateTimeInserted] DATETIME2 NOT NULL,
	[DateTimeUpdated] DATETIME2 NULL,
	[FeatureId] INT NOT NULL,
	[Name] NVARCHAR(255) NOT NULL,
	[Title] NVARCHAR(255) NOT NULL,
	[Description] NVARCHAR(MAX) NOT NULL,
	[IsNew] BIT NOT NULL ,
	[IsDiscontinued] BIT NOT NULL,
	[IsHidden] BIT NOT NULL,
	[TagAssetId] INT NOT NULL,
	[SourceUrl] NVARCHAR(1023) NOT NULL,
	[Serialized] NVARCHAR(MAX) NOT NULL,
	CONSTRAINT [PK_FeatureXmlDoc] PRIMARY KEY CLUSTERED ([Id]),
	CONSTRAINT [FK_FeatureXmlDoc_Features] FOREIGN KEY ([FeatureId]) REFERENCES [dbo].[Features] ([Id]),
	CONSTRAINT [FK_FeatureXmlDoc_TagAssets] FOREIGN KEY ([TagAssetId]) REFERENCES [dbo].[TagAssets] ([Id])
)
