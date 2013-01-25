CREATE TABLE [refViewCount](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[nodeId] [int] NOT NULL,
	[count] [int] NOT NULL,
	[lastViewed] [datetime] NOT NULL,
	[category] [nvarchar](50) NOT NULL--,
	--[hideCounter] [bit] NOT NULL,
 CONSTRAINT [PK_refViewCount] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)
)

GO

CREATE TABLE [dbo].[refViewCountHistory](
	[counterId] [int] NOT NULL,
	[updated] [datetime] NOT NULL,
	[reset] [bit] NOT NULL
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[refViewCountHistory]  WITH CHECK ADD  CONSTRAINT [FK_refViewCountHistory_refViewCount] FOREIGN KEY([counterId])
REFERENCES [dbo].[refViewCount] ([id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[refViewCountHistory] CHECK CONSTRAINT [FK_refViewCountHistory_refViewCount]
GO

CREATE TABLE [refViewCountConfig](
	[nodeId] [int] NOT NULL,
	[category] [nvarchar](100) NULL,
	[hideCounter] [bit] NOT NULL,
	[enableHistory] [bit] NOT NULL
)

GO