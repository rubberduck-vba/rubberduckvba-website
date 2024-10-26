CREATE TABLE [dbo].[Tags]
(
	[Id] INT NOT NULL IDENTITY,
	[DateTimeInserted] DATETIME2 NOT NULL,
	[DateTimeUpdated] DATETIME2 NULL,
	[RepositoryId] INT NOT NULL,
	[ReleaseId] INT NOT NULL,
	[Name] NVARCHAR(255) NOT NULL,
	[DateCreated] DATE NOT NULL,
	[InstallerDownloadUrl] NVARCHAR(1023) NOT NULL,
	[InstallerDownloads] INT NOT NULL,
	[IsPreRelease] BIT NOT NULL,
	CONSTRAINT [PK_Tags] PRIMARY KEY CLUSTERED ([Id]),
	CONSTRAINT [FK_Tags_Repositories] FOREIGN KEY ([RepositoryId]) REFERENCES [dbo].[Repositories] ([Id]),
	INDEX [IX_Tags_DateCreated] ([IsPreRelease] ASC, [DateCreated] ASC)
)
