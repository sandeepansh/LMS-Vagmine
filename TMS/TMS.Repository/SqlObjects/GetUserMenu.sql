/*
-- =============================================
-- Author:		Atul Kumar
-- Create date: 20-Jan-2022
-- Description:	Get user's Menues
-- =============================================
*/
CREATE OR ALTER PROCEDURE GetUserMenu 
@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @UserRoleId INT, @IndexLoop INT=1, @IndexMax INT=0, @LoopFormId INT;
	DECLARE @ViewPermission BIT=0, @AddPermission BIT=0, @EditPermission BIT=0;
	DECLARE @Roles TABLE (Id INT);
	DECLARE @AllowedRoles TABLE (Id INT, RowNum INT);
	DECLARE @UniqueForms TABLE ([Id] INT, RowNum INT);
	DECLARE @AllowedForms TABLE ([Id] INT NOT NULL, [Name] NVARCHAR(50) NOT NULL, [Description] NVARCHAR(100) NULL, [Area] NVARCHAR(50) NULL,
		[Controller] NVARCHAR(50) NOT NULL, [Action] NVARCHAR(50) NOT NULL, [IconClass] NVARCHAR(50) NULL, [Sequence] INT NOT NULL, [MenuId] INT NULL,
		[IsAdmin] BIT NOT NULL,[View] BIT NOT NULL,[Add] BIT NOT NULL,[Edit] BIT NOT NULL);

	SELECT @UserRoleId=RoleId FROM UserMaster WHERE Id=@UserId;

	IF(ISNULL(@UserRoleId,0)>0)
	BEGIN
		INSERT @Roles (Id) VALUES (@UserRoleId);
	END

	INSERT @Roles(Id) 
	SELECT RoleId FROM UserRoles WHERE UserId=@UserId;

	INSERT @AllowedRoles(Id, RowNum)
	SELECT Id,ROW_NUMBER() OVER(ORDER BY Id) AS RowNum FROM @Roles;

	INSERT @UniqueForms(Id, RowNum)
	SELECT FormId,ROW_NUMBER() OVER(ORDER BY FormId) AS RowNum FROM (
	SELECT DISTINCT FormId FROM RolePermissions 
						WHERE RoleId IN (SELECT Id FROM @AllowedRoles)
								AND ([View]=1 OR [Add]=1 OR [Edit]=1)) AA;

	SELECT @IndexMax=MAX(RowNum) FROM @UniqueForms;


	WHILE (@IndexLoop<= @IndexMax)
	BEGIN
		SELECT @ViewPermission =0, @AddPermission =0, @EditPermission =0;
		SELECT @LoopFormId=Id FROM @UniqueForms WHERE RowNum=@IndexLoop;
		--View
		IF EXISTS(SELECT 1 FROM RolePermissions WHERE FormId=@LoopFormId AND [View]=1 AND RoleId IN (SELECT Id FROM @AllowedRoles))
		BEGIN
			SELECT @ViewPermission=1;
		END
		--Add
		IF EXISTS(SELECT 1 FROM RolePermissions WHERE FormId=@LoopFormId AND [Add]=1 AND RoleId IN (SELECT Id FROM @AllowedRoles))
		BEGIN
			SELECT @AddPermission=1;
		END
		--Edit
		IF EXISTS(SELECT 1 FROM RolePermissions WHERE FormId=@LoopFormId AND [Edit]=1 AND RoleId IN (SELECT Id FROM @AllowedRoles))
		BEGIN
			SELECT @EditPermission=1;
		END
		IF(@ViewPermission=1 OR @AddPermission=1 OR @EditPermission=1)
		BEGIN
			INSERT @AllowedForms(Id,[Name],[Description],Area,Controller,[Action],IconClass,[Sequence],MenuId,IsAdmin,[View],[Add],[Edit]) 
			SELECT Id,[Name],[Description],Area,Controller,[Action],IconClass,[Sequence],MenuId,IsAdmin,@ViewPermission,@AddPermission,@EditPermission FROM FormMaster 
			WHERE Id=@LoopFormId;
		END

		SELECT @IndexLoop=@IndexLoop+1;
	END

	--MENU
	SELECT Id, [Name], [Description], IconClass, [Sequence], IsAdmin FROM MenuMaster
		WHERE IsActive=1 AND Id IN (SELECT MenuId FROM @AllowedForms)
	ORDER BY [Sequence]

	-- FORMS
	SELECT *
		FROM @AllowedForms  ORDER BY [Sequence]	
	
END
GO
