/*
-- =============================================
-- Author:		ATUL KUMAR
-- Create date: 04 FEB 2022
-- Description:	GET FORM CONTROL PERMISSION BY ROLE
-- =============================================
*/
CREATE OR ALTER PROCEDURE FormControlPermissionByRole
@FormId INT,
@RoleId INT
AS
BEGIN
	SET NOCOUNT ON;
	IF @FormId=52
	BEGIN
		SELECT QRM.ColumnId AS ControlId, QRM.ColumnName AS ControlName, @FormId as FormId, COALESCE(QCR.IsVisible,QRM.IsVisible) AS IsVisible FROM QuotationColumnsRoleMaster QRM
			LEFT JOIN QuotationColumnsRole QCR ON QRM.ColumnId=QCR.ColumnId AND QCR.RoleId=@RoleId
			ORDER BY ColumnName;
	END
	ELSE
	BEGIN
	   SELECT FC.ControlId, FC.ControlName, FC.FormId, COALESCE(FCR.IsVisible, FC.IsVisible) AS IsVisible FROM FormControlMaster FC 
			LEFT JOIN FormControlRoleMaster FCR ON FC.FormId=FCR.FormId AND FC.ControlId=FCR.ControlId AND FCR.RoleId=@RoleId
	   WHERE FC.FormId=@FormId;
   END
END
GO
