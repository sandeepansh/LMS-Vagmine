/*-- =============================================
-- Author:		ATUL KUMAR
-- Create date: 11-JAN-2022
-- Description:	UPDATE ROLE WISE PERMISSIONS
-- =============================================
*/
CREATE OR ALTER PROCEDURE UpdateRolePermissions
@RoleId INT,
@Permissions UT_RolePermissions READONLY
AS
BEGIN
	BEGIN TRAN
	BEGIN TRY
		DELETE FROM RolePermissions WHERE RoleId=@RoleId;

		INSERT INTO RolePermissions(RoleId, FormId, [View], [Add], Edit)
		SELECT RoleId, FormId, FormView, FormAdd, FormEdit FROM @Permissions;
				
		COMMIT;
		SELECT 1 AS Result, 'Process completed' Response;
	END TRY
	BEGIN CATCH
		ROLLBACK;
		SELECT 0 AS Result, 'Process failed ' +  ERROR_MESSAGE() Response;
	END CATCH
END
GO
