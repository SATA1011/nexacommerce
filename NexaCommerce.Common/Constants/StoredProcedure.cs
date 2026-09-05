namespace NexaCommerce.Common.Constants;

public static class StoredProcedure
{
    // Users
    public const string UsersGet = "User_Get";
    public const string UsersGetByEmail = "User_GetByEmail";
    public const string UsersGetAll = "User_GetAll";
    public const string UsersInsertUpdate = "User_InsertUpdate";
    public const string UsersSoftDelete = "User_SoftDelete";
    public const string UsersIncrementAccessFailed = "User_IncrementAccessFailed";
    public const string UsersResetAccessFailed = "User_ResetAccessFailed";

    // Roles
    public const string RolesGet = "Role_Get";
    public const string RolesGetByName = "Role_GetByName";
    public const string RolesGetAll = "Role_GetAll";
    public const string RolesInsertUpdate = "Role_InsertUpdate";
    public const string RolesSoftDelete = "Role_SoftDelete";

    // UserRoles
    public const string UserRolesAssign = "UserRole_Assign";
    public const string UserRolesRemove = "UserRole_Remove";
    public const string UserRolesGetByUserId = "UserRole_GetByUserId";

    // RefreshTokens
    public const string RefreshTokensGet = "RefreshToken_Get";
    public const string RefreshTokensInsertUpdate = "RefreshToken_InsertUpdate";
    public const string RefreshTokensRevoke = "RefreshToken_Revoke";
    public const string RefreshTokensDeleteExpired = "RefreshToken_DeleteExpired";

    // UserSessions
    public const string UserSessionsGet = "UserSession_Get";
    public const string UserSessionsInsertUpdate = "UserSession_InsertUpdate";
    public const string UserSessionsRevokeAll = "UserSession_RevokeAll";

    // AuditLogs
    public const string AuditLogsGetAll = "AuditLog_GetAll";
    public const string AuditLogsInsertUpdate = "AuditLog_InsertUpdate";

    // SecurityEvents
    public const string SecurityEventsInsertUpdate = "SecurityEvent_InsertUpdate";

    // OutboxMessages
    public const string OutboxMessagesGet = "OutboxMessage_Get";
    public const string OutboxMessagesInsertUpdate = "OutboxMessage_InsertUpdate";

    // IdempotencyRecords
    public const string IdempotencyRecordsGet = "IdempotencyRecord_Get";
    public const string IdempotencyRecordsInsertUpdate = "IdempotencyRecord_InsertUpdate";

    // Customers (Sellers / Stores)
    public const string CustomersGet = "Customer_Get";
    public const string CustomersGetById = "Customer_Get";
    public const string CustomersGetByUserId = "Customer_GetByUserId";
    public const string CustomersGetAll = "Customer_GetAll";
    public const string CustomersInsertUpdate = "Customer_InsertUpdate";
    public const string CustomersUpdateStatus = "Customer_UpdateStatus";
}
