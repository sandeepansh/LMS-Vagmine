/*
-- =============================================
-- Author:		Atul Kumar
-- Create date: 26 Jan 2022
-- Description:	Get Roles By Company Id
-- =============================================
*/
CREATE OR ALTER PROCEDURE RolesByCompanyId
@CompanyId INT=NULL,
@UserId INT=NULL
AS
BEGIN
	SET NOCOUNT ON;
   SELECT CONVERT(VARCHAR(20),R.Id) AS Id,R.[Name] + CASE WHEN IsActive=0 THEN ' (Inactive)' ELSE '' END AS [Name] FROM  RoleMaster R
	ORDER BY R.[Name];

END
