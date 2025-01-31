CREATE TABLE [dbo].[Features]
(
	[Id] INT NOT NULL IDENTITY,
	[DateTimeInserted] DATETIME2 NOT NULL,
	[DateTimeUpdated] DATETIME2 NULL,
	[RepositoryId] INT NOT NULL,
	[ParentId] INT NULL,
	[Name] NVARCHAR(255) NOT NULL,
	[Title] NVARCHAR(255) NOT NULL,
	[ShortDescription] NVARCHAR(1023) NOT NULL,
	[Description] NVARCHAR(MAX) NOT NULL,
	[IsHidden] BIT NOT NULL,
	[IsNew] BIT NOT NULL,
	[HasImage] BIT NOT NULL,
	[Links] NVARCHAR(MAX) NULL, 
    CONSTRAINT [PK_Features] PRIMARY KEY CLUSTERED ([Id]),
	CONSTRAINT [FK_Features_Repositories] FOREIGN KEY ([RepositoryId]) REFERENCES [dbo].[Repositories] ([Id]),
	CONSTRAINT [FK_Features_Features] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[Features] ([Id]),
	CONSTRAINT [UK_Features_Name] UNIQUE ([RepositoryId], [Name])
)
