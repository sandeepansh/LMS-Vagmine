/*
-- =============================================
-- Author:		Atul Kumar
-- Create date: 25 Jan 2022
-- Description:	Get All user types
-- =============================================
*/
CREATE OR ALTER PROCEDURE GetAllUserTypes
AS
BEGIN
	SET NOCOUNT ON;  
	SELECT * FROM UserType ORDER BY [Name];
END
GO
