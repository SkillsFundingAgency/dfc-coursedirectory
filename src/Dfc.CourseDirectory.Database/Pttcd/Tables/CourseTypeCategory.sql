CREATE TABLE [Pttcd].[CourseTypeCategory]
(
	[CourseType] INT NOT NULL , 
    [CategoryRef] NVARCHAR(50) NOT NULL FOREIGN KEY REFERENCES [LARS].[Category] ([CategoryRef]), 
    PRIMARY KEY ([CourseType], [CategoryRef])
)
