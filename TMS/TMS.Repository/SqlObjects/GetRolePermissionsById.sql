/*
-- =============================================
-- Author:		ATUL KUMAR
-- Create date: 11-JAN-2022
-- Description:	GET ROLE WISE PERMISSIONS
-- =============================================
*/
CREATE OR ALTER PROCEDURE GetRolePermissionsById
@RoleId INT
AS
BEGIN
	SET NOCOUNT ON;
	SELECT @RoleId AS RoleId,F.Id AS FormId, CASE WHEN MM.[Name] IS NULL THEN F.[Name] ELSE CONCAT(F.[Name],' (', MM.[Name], ')') END AS FormName,
	ISNULL(RP.[View],0) [View], ISNULL(RP.[Add],0) [Add], ISNULL(RP.[Edit],0) [Edit]
	FROM FormMaster F
		LEFT JOIN RolePermissions RP ON F.Id=RP.FormId AND RP.RoleId=@RoleId
		LEFT JOIN MenuMaster MM ON F.MenuId=MM.Id
	ORDER BY ISNULL(MM.[Sequence], 0), F.[Name]
END
GO
